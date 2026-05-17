using Microsoft.EntityFrameworkCore;
using YourPrime.Data;
using YourPrime.DTOs.Admin;
using YourPrime.DTOs.Teams;
using YourPrime.Entities;
using YourPrime.Interfaces;

namespace YourPrime.Services;

public class TeamService : ITeamService
{
    private readonly AppDbContext _db;
    private readonly IBlobService _blobService;

    public TeamService(AppDbContext db, IBlobService blobService)
    {
        _db = db;
        _blobService = blobService;
    }

    
    
    // СОЗДАТЬ КОМАНДУ
    public async Task<TeamResponse> CreateTeamAsync(int userId, CreateTeamRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            throw new Exception("User not found");

        var alreadyInTeam = await _db.UserTeams
            .AnyAsync(x => x.UserId == userId && x.Status == "Accepted");

        if (alreadyInTeam)
            throw new Exception("You are already in a team");

        var teamNameExists = await _db.Teams
            .AnyAsync(x => x.Name == request.Name && !x.IsDeleted); // изменили: не учитываем удалённые команды

        if (teamNameExists)
            throw new Exception("Team name already exists");

        var team = new Team
        {
            Name = request.Name,
            CaptainId = userId,
            Wins = 0,
            Losses = 0,
            IsOpen = true,
            IsDeleted = false, // изменили: новая команда активная
            Rating = 0,
            AverageLevel = user.Level
        };

        _db.Teams.Add(team);
        await _db.SaveChangesAsync();

        var userTeam = new UserTeam
        {
            UserId = userId,
            TeamId = team.Id,
            Role = "Captain",
            Status = "Accepted"
        };

        _db.UserTeams.Add(userTeam);
        await _db.SaveChangesAsync();

        return new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            CaptainId = team.CaptainId,
            Wins = team.Wins,
            Losses = team.Losses,
            IsOpen = team.IsOpen,
            Rating = team.Rating,
            AverageLevel = team.AverageLevel,
            MembersCount = 1
        };
    }
    
    
    // ВЕРНУТЬ СПИСОК КОМАНД
    public async Task<List<TeamResponse>> GetAllTeamsAsync()
    {
        var teams = await _db.Teams
            .Include(t => t.UserTeams)
            .Where(t => !t.IsDeleted) // не показываем удалённые команды
            .ToListAsync();

        return teams.Select(team => new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            CaptainId = team.CaptainId,

            AvatarUrl = team.AvatarUrl, // добавили аватарку команды

            Wins = team.Wins,
            Losses = team.Losses,
            IsOpen = team.IsOpen,
            Rating = team.Rating,
            AverageLevel = team.AverageLevel,

            MembersCount = team.UserTeams.Count(x => x.Status == "Accepted")
        }).ToList();
    }
    
    
    
    
    // ЗАЯВКА НА ВСТУПЛЕНИЕ
    public async Task<string> JoinTeamAsync(int userId, int teamId)
    {
        var team = await _db.Teams
            .Include(t => t.UserTeams)
            .FirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

        if (team == null)
            throw new Exception("Team not found");

        if (!team.IsOpen)
            throw new Exception("Team is closed");

        var alreadyInTeam = await _db.UserTeams
            .AnyAsync(x => x.UserId == userId && x.Status == "Accepted");

        if (alreadyInTeam)
            throw new Exception("You are already in a team");

        var existingRequest = await _db.UserTeams
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TeamId == teamId);

        if (existingRequest != null)
        {
            if (existingRequest.Status == "Pending")
                throw new Exception("You already sent request to this team");

            if (existingRequest.Status == "Rejected")
            {
                existingRequest.Status = "Pending";
                existingRequest.Role = "Member";
                existingRequest.CreatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                return "Request sent successfully";
            }

            if (existingRequest.Status == "Accepted")
                throw new Exception("You are already in this team");
        }

        var membersCount = team.UserTeams.Count(x => x.Status == "Accepted");

        if (membersCount >= 6)
            throw new Exception("Team is full");

        var request = new UserTeam
        {
            UserId = userId,
            TeamId = teamId,
            Role = "Member",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.UserTeams.Add(request);
        await _db.SaveChangesAsync();

        return "Request sent successfully";
    }
    
    
    // ПОЛУЧЕНИЕ ЗАЯВОК ЛЮБЫМ УЧАСТНИКОМ КОМАНДЫ
    public async Task<List<TeamRequestResponse>> GetMyTeamRequestsAsync(int userId)
    {
        var userTeam = await _db.UserTeams
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Status == "Accepted" &&
                !x.Team.IsDeleted);

        if (userTeam == null)
            throw new Exception("You are not in any team");

        var requests = await _db.UserTeams
            .Include(x => x.User)
            .Include(x => x.Team)
            .Where(x => x.TeamId == userTeam.TeamId && x.Status == "Pending")
            .ToListAsync();

        return requests.Select(x => new TeamRequestResponse
        {
            UserId = x.UserId,
            Username = x.User.Username,
    //        AvatarUrl = x.User.AvatarUrl, // если добавил в DTO

            Age = x.User.Age,
            Level = x.User.Level,
            Goals = x.User.Goals,
            Assists = x.User.Assists,

            TeamId = x.TeamId,
            TeamName = x.Team.Name,
     //       TeamAvatarUrl = x.Team.AvatarUrl, // если добавил в DTO

            Role = x.Role,
            Status = x.Status,
            CreatedAt = x.CreatedAt
        }).ToList();
    }
    
    
    
    
    // ПРИНЯТЬ ЗАЯВКУ (АДМИНОМ)
    public async Task<string> AcceptRequestAsync(int captainId, int userId)
    {
        var team = await _db.Teams
            .Include(t => t.UserTeams)
            .FirstOrDefaultAsync(t => t.CaptainId == captainId);

        if (team == null)
            throw new Exception("You are not captain of any team");

        var request = await _db.UserTeams
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.TeamId == team.Id &&
                x.UserId == userId &&
                x.Status == "Pending");

        if (request == null)
            throw new Exception("Request not found");

        var userAlreadyInTeam = await _db.UserTeams
            .AnyAsync(x => x.UserId == userId && x.Status == "Accepted");

        if (userAlreadyInTeam)
            throw new Exception("User is already in another team");

        var membersCount = team.UserTeams.Count(x => x.Status == "Accepted");

        if (membersCount >= 6)
            throw new Exception("Team is full");

        // принимаем
        request.Status = "Accepted";

        // пересчёт среднего уровня
        var acceptedUsers = await _db.UserTeams
            .Where(x => x.TeamId == team.Id && x.Status == "Accepted")
            .Include(x => x.User)
            .ToListAsync();

        acceptedUsers.Add(request); // добавляем нового

        team.AverageLevel = acceptedUsers.Average(x => x.User.Level);

        await _db.SaveChangesAsync();

        return "User accepted to team";
    }
    
    
    
    
    
    
    // ОТКЛОНИТЬ ЗАЯВКУ (АДМИНОМ)
    public async Task<string> RejectRequestAsync(int captainId, int userId)
    {
        var team = await _db.Teams
            .FirstOrDefaultAsync(t => t.CaptainId == captainId);

        if (team == null)
            throw new Exception("You are not captain of any team");

        var request = await _db.UserTeams
            .FirstOrDefaultAsync(x =>
                x.TeamId == team.Id &&
                x.UserId == userId &&
                x.Status == "Pending");

        if (request == null)
            throw new Exception("Request not found");

        request.Status = "Rejected";

        await _db.SaveChangesAsync();

        return "Request rejected";
    }
    
    
    
    
    
    
    
    // ПОСМОТРЕТЬ СВОЮ КОМАНДУ 
    public async Task<MyTeamResponse?> GetMyTeamAsync(int userId)
    {
        var userTeam = await _db.UserTeams
            .Include(x => x.Team)
            .ThenInclude(t => t.Captain)
            .Include(x => x.Team)
            .ThenInclude(t => t.UserTeams)
            .ThenInclude(ut => ut.User)
            .FirstOrDefaultAsync(x => 
                x.UserId == userId && 
                x.Status == "Accepted" &&
                !x.Team.IsDeleted);

        if (userTeam == null)
            return null;

        var team = userTeam.Team;

        return new MyTeamResponse
        {
            Id = team.Id,
            Name = team.Name,

            AvatarUrl = team.AvatarUrl, // аватар команды

            CaptainId = team.CaptainId,
            CaptainUsername = team.Captain?.Username,

            Wins = team.Wins,
            Losses = team.Losses,
            IsOpen = team.IsOpen,
            Rating = team.Rating,
            AverageLevel = team.AverageLevel,

            Members = team.UserTeams
                .Where(x => x.Status == "Accepted")
                .Select(x => new TeamMemberResponse
                {
                    UserId = x.UserId,
                    Username = x.User.Username,

                    AvatarUrl = x.User.AvatarUrl, // аватар игрока

                    Age = x.User.Age,
                    Level = x.User.Level,
                    Goals = x.User.Goals,
                    Assists = x.User.Assists,
                    Role = x.Role
                })
                .ToList()
        };
    }
    
    
    
    
    
    
    
    
    // ПОСМОТРЕТЬ СВОИ ЗАПРОСЫ
    public async Task<List<TeamRequestResponse>> GetMyRequestsAsync(int userId)
    {
        var requests = await _db.UserTeams
            .Include(x => x.User)
            .Include(x => x.Team)
            .Where(x => x.UserId == userId && x.Status == "Pending")
            .ToListAsync();

        return requests.Select(x => new TeamRequestResponse
        {
            UserId = x.UserId,
            Username = x.User.Username,
            Age = x.User.Age,
            Level = x.User.Level,
            Goals = x.User.Goals,
            Assists = x.User.Assists,

            TeamId = x.TeamId,
            TeamName = x.Team.Name,

            Role = x.Role,
            Status = x.Status,
            CreatedAt = x.CreatedAt
        }).ToList();
    }
    
    
    
    
    
    
    // НАЗНАЧИТЬ КАПИТАНОМ
    public async Task<string> MakeCaptainAsync(int currentCaptainId, int newCaptainId)
    {
        var team = await _db.Teams
            .Include(t => t.UserTeams)
            .FirstOrDefaultAsync(t => t.CaptainId == currentCaptainId);

        if (team == null)
            throw new Exception("You are not captain of any team");

        var newCaptainUserTeam = team.UserTeams
            .FirstOrDefault(x => x.UserId == newCaptainId && x.Status == "Accepted");

        if (newCaptainUserTeam == null)
            throw new Exception("User is not accepted member of your team");

        var oldCaptainUserTeam = team.UserTeams
            .FirstOrDefault(x => x.UserId == currentCaptainId && x.Status == "Accepted");

        if (oldCaptainUserTeam == null)
            throw new Exception("Current captain membership not found");

        oldCaptainUserTeam.Role = "Member";
        newCaptainUserTeam.Role = "Captain";

        team.CaptainId = newCaptainId;

        await _db.SaveChangesAsync();

        return "Captain changed successfully";
    }
    
    
    
    
    // ИСКЛЮЧИТЬ/КИКНУТЬ
    public async Task<string> KickUserAsync(int captainId, int userId)
    {
        var team = await _db.Teams
            .Include(t => t.UserTeams)
            .ThenInclude(ut => ut.User)
            .FirstOrDefaultAsync(t => t.CaptainId == captainId);

        if (team == null)
            throw new Exception("You are not captain of any team");

        if (captainId == userId)
            throw new Exception("You cannot kick yourself");

        var userTeam = team.UserTeams
            .FirstOrDefault(x => x.UserId == userId && x.Status == "Accepted");

        if (userTeam == null)
            throw new Exception("User is not in your team");

        _db.UserTeams.Remove(userTeam);

        // пересчёт среднего уровня
        var remainingUsers = team.UserTeams
            .Where(x => x.UserId != userId && x.Status == "Accepted")
            .ToList();

        if (remainingUsers.Count == 0)
        {
            team.AverageLevel = 0;
        }
        else
        {
            team.AverageLevel = remainingUsers.Average(x => x.User.Level);
        }

        await _db.SaveChangesAsync();

        return "User kicked from team";
    }
    
    
    
    
    
    
    // ПОКИНУТЬ КОМАНДУ
    public async Task<string> LeaveTeamAsync(int userId)
    {
        var userTeam = await _db.UserTeams
            .Include(x => x.Team)
            .ThenInclude(t => t.UserTeams)
            .ThenInclude(ut => ut.User)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == "Accepted");

        if (userTeam == null)
            throw new Exception("You are not in any team");

        var team = userTeam.Team;

        var remainingUsers = team.UserTeams
            .Where(x => x.UserId != userId && x.Status == "Accepted")
            .ToList();

        // Если выходит капитан, но в команде есть другие участники — запрещаем
        if (team.CaptainId == userId && remainingUsers.Count > 0)
            throw new Exception("Captain cannot leave while team has members");

        _db.UserTeams.Remove(userTeam);

        team.AverageLevel = remainingUsers.Count == 0
            ? 0
            : remainingUsers.Average(x => x.User.Level);

        // Если после выхода никого не осталось — мягко удаляем команду
        if (remainingUsers.Count == 0)
        {
            team.CaptainId = null;
            team.IsOpen = false;
            team.IsDeleted = true;
        }

        await _db.SaveChangesAsync();

        return team.IsDeleted
            ? "You left the team. Team was deleted because no members left"
            : "You left the team";
    }
    
    
    
    
    
    
    // ИЗМЕНИТЬ НАЗВАНИЕ ТИМЫ
    public async Task<string> UpdateTeamNameAsync(int captainId, UpdateTeamNameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new Exception("Team name cannot be empty");

        var team = await _db.Teams
            .FirstOrDefaultAsync(t => t.CaptainId == captainId);

        if (team == null)
            throw new Exception("You are not captain of any team");

        var nameExists = await _db.Teams
            .AnyAsync(t => t.Name == request.Name && t.Id != team.Id);

        if (nameExists)
            throw new Exception("Team name already exists");

        team.Name = request.Name;

        await _db.SaveChangesAsync();

        return "Team name updated successfully";
    }
    
    
    
    
    
    // ПОСМОТРЕТЬ ЛЮБУЮ КОМАНДУ С ЕЕ УЧАСТННИКАМИ [AllowAnonymous]
    public async Task<MyTeamResponse> GetTeamByIdAsync(int teamId)
    {
        var team = await _db.Teams
            .Include(t => t.Captain)
            .Include(t => t.UserTeams)
            .ThenInclude(ut => ut.User)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
            throw new Exception("Team not found");

        return new MyTeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            CaptainId = team.CaptainId,
            CaptainUsername = team.Captain?.Username,
            Wins = team.Wins,
            Losses = team.Losses,
            IsOpen = team.IsOpen,
            Rating = team.Rating,
            AverageLevel = team.AverageLevel,
            Members = team.UserTeams
                .Where(x => x.Status == "Accepted")
                .Select(x => new TeamMemberResponse
                {
                    UserId = x.UserId,
                    Username = x.User.Username,
                    Age = x.User.Age,
                    Level = x.User.Level,
                    Goals = x.User.Goals,
                    Assists = x.User.Assists,
                    Role = x.Role
                })
                .ToList()
        };
    }
    
    
    
    
    
    
    
    // ОБНОВИТЬ СТАТУ ТИМЫ
    public async Task<TeamResponse> UpdateTeamStatsAsync(int teamId, UpdateTeamStatsRequest request)
    {
        var team = await _db.Teams
            .Include(t => t.UserTeams)
            .FirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

        if (team == null)
            throw new Exception("Team not found");

        if (request.Wins < 0)
            throw new Exception("Wins cannot be negative");

        if (request.Losses < 0)
            throw new Exception("Losses cannot be negative");

        team.Wins = request.Wins;
        team.Losses = request.Losses;
        team.Rating = request.Rating;

        await _db.SaveChangesAsync();

        return new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            CaptainId = team.CaptainId,
            Wins = team.Wins,
            Losses = team.Losses,
            IsOpen = team.IsOpen,
            Rating = team.Rating,
            AverageLevel = team.AverageLevel,
            MembersCount = team.UserTeams.Count(x => x.Status == "Accepted")
        };
    }
    
    
    
    
    
    
    
    // ОБНОВИТЬ АВУ ТИМЫ
    public async Task<MyTeamResponse> UploadTeamAvatarAsync(int captainId, IFormFile file)
    {
        var team = await _db.Teams
            .Include(t => t.Captain)
            .Include(t => t.UserTeams)
            .ThenInclude(ut => ut.User)
            .FirstOrDefaultAsync(t => t.CaptainId == captainId && !t.IsDeleted);

        if (team == null)
            throw new Exception("You are not captain of any team");

        if (file == null || file.Length == 0)
            throw new Exception("File is empty");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
            throw new Exception("Only jpg, jpeg, png, webp allowed");

        if (!string.IsNullOrWhiteSpace(team.AvatarUrl))
        {
            await _blobService.DeleteFileAsync(team.AvatarUrl);
        }

        var avatarUrl = await _blobService.UploadFileAsync(file);

        team.AvatarUrl = avatarUrl;

        await _db.SaveChangesAsync();

        return new MyTeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            CaptainId = team.CaptainId,
            CaptainUsername = team.Captain?.Username,
            AvatarUrl = team.AvatarUrl,
            Wins = team.Wins,
            Losses = team.Losses,
            IsOpen = team.IsOpen,
            Rating = team.Rating,
            AverageLevel = team.AverageLevel,
            Members = team.UserTeams
                .Where(x => x.Status == "Accepted")
                .Select(x => new TeamMemberResponse
                {
                    UserId = x.UserId,
                    Username = x.User.Username,
                    Age = x.User.Age,
                    Level = x.User.Level,
                    Goals = x.User.Goals,
                    Assists = x.User.Assists,
                    Role = x.Role
                })
                .ToList()
        };
    }
}