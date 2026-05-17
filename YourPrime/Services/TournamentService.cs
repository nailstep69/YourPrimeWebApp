using YourPrime.Data;
using YourPrime.DTOs.Tournaments;
using YourPrime.Entities;
using YourPrime.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace YourPrime.Services;

public class TournamentService : ITournamentService
{
    private readonly AppDbContext _db;

    public TournamentService(AppDbContext db)
    {
        _db = db;
    }

    
    
    
    
    // СОЗДАТЬ ТУРНИР  (админ)
    public async Task<TournamentResponse> CreateTournamentAsync(CreateTournamentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new Exception("Tournament name cannot be empty");

        if (request.MaxTeams != 8 && request.MaxTeams != 12 && request.MaxTeams != 16)
            throw new Exception("MaxTeams must be 8, 12 or 16");

        var tournament = new Tournament
        {
            Name = request.Name,
            Description = request.Description,
            MaxTeams = request.MaxTeams,
            Status = "Draft"
        };

        _db.Tournaments.Add(tournament);
        await _db.SaveChangesAsync();

        return new TournamentResponse
        {
            Id = tournament.Id,
            Name = tournament.Name,
            Description = tournament.Description,
            MaxTeams = tournament.MaxTeams,
            Status = tournament.Status,
            CreatedAt = tournament.CreatedAt,
            AcceptedTeamsCount = 0
        };
    }
    
    
    
    
    
    
    // ПОЛУЧИТЬ ВСЕ ТУРНИРЫ  (анонимус)
    public async Task<List<TournamentResponse>> GetAllTournamentsAsync()
    {
        var tournaments = await _db.Tournaments
            .Include(t => t.TournamentTeams)
            .ToListAsync();

        return tournaments.Select(t => new TournamentResponse
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            MaxTeams = t.MaxTeams,
            Status = t.Status,
            CreatedAt = t.CreatedAt,
            AcceptedTeamsCount = t.TournamentTeams.Count(x => x.Status == "Accepted")
        }).ToList();
    }
    
    
    
    
    
    
    
    // ПОДАТЬ ЗАЯВКУ В ТУРНИР (капитан)
    public async Task<string> JoinTournamentAsync(int captainId, int tournamentId)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.TournamentTeams)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
            throw new Exception("Tournament not found");

        if (tournament.Status != "RegistrationOpen")
            throw new Exception("Tournament is not open for registration");

        var team = await _db.Teams
            .FirstOrDefaultAsync(t => t.CaptainId == captainId);

        if (team == null)
            throw new Exception("You are not captain of any team");

        var alreadyRequested = await _db.TournamentTeams
            .AnyAsync(x => x.TeamId == team.Id && x.TournamentId == tournamentId);

        if (alreadyRequested)
            throw new Exception("Your team already applied");

        var acceptedCount = tournament.TournamentTeams.Count(x => x.Status == "Accepted");

        if (acceptedCount >= tournament.MaxTeams)
            throw new Exception("Tournament is full");

        var request = new TournamentTeam
        {
            TournamentId = tournamentId,
            TeamId = team.Id,
            CaptainId = captainId,
            Status = "Pending"
        };

        _db.TournamentTeams.Add(request);
        await _db.SaveChangesAsync();

        return "Request sent";
    }
    
    
    
    
    
    
    
    // ОТКРЫТЬ ТУРНИР (СТАТУС) (админ)
    public async Task<string> OpenRegistrationAsync(int tournamentId)
    {
        var tournament = await _db.Tournaments
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
            throw new Exception("Tournament not found");

        if (tournament.Status != "Draft")
            throw new Exception("Only Draft tournament can be opened");

        tournament.Status = "RegistrationOpen";

        await _db.SaveChangesAsync();

        return "Registration opened";
    }
    
    
    
    
    
    
    
    // ПОЛУЧИТЬ ВСЕ ЗАЯВКИ НА ТУРНИР
    public async Task<List<TournamentTeamRequestResponse>> GetTournamentRequestsAsync(int tournamentId)
    {
        var tournamentExists = await _db.Tournaments
            .AnyAsync(t => t.Id == tournamentId);

        if (!tournamentExists)
            throw new Exception("Tournament not found");

        var requests = await _db.TournamentTeams
            .Include(x => x.Tournament)
            .Include(x => x.Team)
            .Include(x => x.Captain)
            .Where(x => x.TournamentId == tournamentId && x.Status == "Pending")
            .ToListAsync();

        return requests.Select(x => new TournamentTeamRequestResponse
        {
            TournamentId = x.TournamentId,
            TournamentName = x.Tournament.Name,

            TeamId = x.TeamId,
            TeamName = x.Team.Name,

            CaptainId = x.CaptainId,
            CaptainUsername = x.Captain.Username,

            Status = x.Status,
            CreatedAt = x.CreatedAt
        }).ToList();
    }
    
    
    
    
    
    
    // ПРИНЯТЬ ЗАЯВКУ
    public async Task<string> AcceptTeamAsync(int tournamentId, int teamId)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.TournamentTeams)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
            throw new Exception("Tournament not found");

        var request = tournament.TournamentTeams
            .FirstOrDefault(x => x.TeamId == teamId && x.Status == "Pending");

        if (request == null)
            throw new Exception("Request not found");

        var acceptedCount = tournament.TournamentTeams.Count(x => x.Status == "Accepted");

        if (acceptedCount >= tournament.MaxTeams)
            throw new Exception("Tournament is full");

        request.Status = "Accepted";

        await _db.SaveChangesAsync();

        return "Team accepted";
    }
    
    
    
    
    
    
    // ОТКЛОНИТЬ ЗАЯВКУ
    public async Task<string> RejectTeamAsync(int tournamentId, int teamId)
    {
        var request = await _db.TournamentTeams
            .FirstOrDefaultAsync(x =>
                x.TournamentId == tournamentId &&
                x.TeamId == teamId &&
                x.Status == "Pending");

        if (request == null)
            throw new Exception("Request not found");

        request.Status = "Rejected";

        await _db.SaveChangesAsync();

        return "Team rejected";
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    // СОЗДАТЬ МАТЧ (МОЖЕТ БЫТЬ ПУСТЫМ)
    public async Task<TournamentMatchResponse> CreateMatchAsync(
    int tournamentId,
    CreateTournamentMatchRequest request)
    {
        if (request.RoundNumber <= 0)
            throw new Exception("Round number must be greater than 0");

        if (request.MatchNumber <= 0)
            throw new Exception("Match number must be greater than 0");

        if (request.TeamAId.HasValue && request.TeamBId.HasValue &&
            request.TeamAId == request.TeamBId)
            throw new Exception("Team cannot play against itself");

        var tournament = await _db.Tournaments
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
            throw new Exception("Tournament not found");

        if (tournament.Status != "RegistrationOpen" && tournament.Status != "Started")
            throw new Exception("Tournament must be open or started");

        if (request.TeamAId.HasValue)
        {
            var teamAIsAccepted = await _db.TournamentTeams
                .AnyAsync(x =>
                    x.TournamentId == tournamentId &&
                    x.TeamId == request.TeamAId.Value &&
                    x.Status == "Accepted");

            if (!teamAIsAccepted)
                throw new Exception("Team A is not accepted in this tournament");
        }

        if (request.TeamBId.HasValue)
        {
            var teamBIsAccepted = await _db.TournamentTeams
                .AnyAsync(x =>
                    x.TournamentId == tournamentId &&
                    x.TeamId == request.TeamBId.Value &&
                    x.Status == "Accepted");

            if (!teamBIsAccepted)
                throw new Exception("Team B is not accepted in this tournament");
        }

        var matchExists = await _db.TournamentMatches
            .AnyAsync(x =>
                x.TournamentId == tournamentId &&
                x.RoundNumber == request.RoundNumber &&
                x.MatchNumber == request.MatchNumber);

        if (matchExists)
            throw new Exception("Match with this round and number already exists");

        var match = new TournamentMatch
        {
            TournamentId = tournamentId,
            RoundNumber = request.RoundNumber,
            MatchNumber = request.MatchNumber,
            TeamAId = request.TeamAId,
            TeamBId = request.TeamBId,
            MatchDate = request.MatchDate,
            Location = request.Location,
            Status = "Scheduled"
        };

        _db.TournamentMatches.Add(match);
        await _db.SaveChangesAsync();

        var createdMatch = await _db.TournamentMatches
            .Include(x => x.TeamA)
            .Include(x => x.TeamB)
            .Include(x => x.WinnerTeam)
            .FirstAsync(x => x.Id == match.Id);

        return new TournamentMatchResponse
        {
            Id = createdMatch.Id,
            TournamentId = createdMatch.TournamentId,
            RoundNumber = createdMatch.RoundNumber,
            MatchNumber = createdMatch.MatchNumber,

            TeamAId = createdMatch.TeamAId,
            TeamAName = createdMatch.TeamA?.Name,

            TeamBId = createdMatch.TeamBId,
            TeamBName = createdMatch.TeamB?.Name,

            GoalsA = createdMatch.GoalsA,
            GoalsB = createdMatch.GoalsB,

            WinnerTeamId = createdMatch.WinnerTeamId,
            WinnerTeamName = createdMatch.WinnerTeam?.Name,

            MatchDate = createdMatch.MatchDate,
            Location = createdMatch.Location,
            Status = createdMatch.Status
        };
    }
    
    
    
    
    
    
    // ОПРЕДЕЛИТЬ КОМАНДУ И СЛОТ
    public async Task<TournamentMatchResponse> SetMatchTeamAsync(int matchId, SetMatchTeamRequest request)
    {
        var match = await _db.TournamentMatches
            .Include(x => x.TeamA)
            .Include(x => x.TeamB)
            .Include(x => x.WinnerTeam)
            .FirstOrDefaultAsync(x => x.Id == matchId);

        if (match == null)
            throw new Exception("Match not found");

        var teamIsAccepted = await _db.TournamentTeams
            .AnyAsync(x =>
                x.TournamentId == match.TournamentId &&
                x.TeamId == request.TeamId &&
                x.Status == "Accepted");

        if (!teamIsAccepted)
            throw new Exception("Team is not accepted in this tournament");

        if (request.Slot != "A" && request.Slot != "B")
            throw new Exception("Slot must be A or B");
        
        if (match.Status == "Finished")
            throw new Exception("Cannot change finished match");
        
        if (request.Slot == "A" && match.TeamBId == request.TeamId)
            throw new Exception("Team A and Team B cannot be the same");

        if (request.Slot == "B" && match.TeamAId == request.TeamId)
            throw new Exception("Team A and Team B cannot be the same");

        if (request.Slot == "A")
            match.TeamAId = request.TeamId;
        else
            match.TeamBId = request.TeamId;

        await _db.SaveChangesAsync();

        var updatedMatch = await _db.TournamentMatches
            .Include(x => x.TeamA)
            .Include(x => x.TeamB)
            .Include(x => x.WinnerTeam)
            .FirstAsync(x => x.Id == match.Id);

        return new TournamentMatchResponse
        {
            Id = updatedMatch.Id,
            TournamentId = updatedMatch.TournamentId,
            RoundNumber = updatedMatch.RoundNumber,
            MatchNumber = updatedMatch.MatchNumber,
            TeamAId = updatedMatch.TeamAId,
            TeamAName = updatedMatch.TeamA?.Name,
            TeamBId = updatedMatch.TeamBId,
            TeamBName = updatedMatch.TeamB?.Name,
            GoalsA = updatedMatch.GoalsA,
            GoalsB = updatedMatch.GoalsB,
            WinnerTeamId = updatedMatch.WinnerTeamId,
            WinnerTeamName = updatedMatch.WinnerTeam?.Name,
            MatchDate = updatedMatch.MatchDate,
            Location = updatedMatch.Location,
            Status = updatedMatch.Status
        };
    }
    
    
    
    
    
        
        
        
    
    // ОПРЕДЕЛИТЬ ГОЛЫ И ПОБЕДИТЕЛЯ
    public async Task<TournamentMatchResponse> FinishMatchAsync(
    int matchId,
    FinishTournamentMatchRequest request)
    {
        var match = await _db.TournamentMatches
            .Include(x => x.TeamA)
            .Include(x => x.TeamB)
            .Include(x => x.WinnerTeam)
            .FirstOrDefaultAsync(x => x.Id == matchId);

        if (match == null)
            throw new Exception("Match not found");

        if (match.Status == "Finished")
            throw new Exception("Match already finished");

        if (request.GoalsA < 0 || request.GoalsB < 0)
            throw new Exception("Goals cannot be negative");

        if (request.GoalsA == request.GoalsB)
            throw new Exception("Draw is not allowed in playoff match");

        if (request.WinnerTeamId != match.TeamAId && request.WinnerTeamId != match.TeamBId)
            throw new Exception("Winner must be Team A or Team B");

        if (request.GoalsA > request.GoalsB && request.WinnerTeamId != match.TeamAId)
            throw new Exception("Winner does not match score");

        if (request.GoalsB > request.GoalsA && request.WinnerTeamId != match.TeamBId)
            throw new Exception("Winner does not match score");

        match.GoalsA = request.GoalsA;
        match.GoalsB = request.GoalsB;
        match.WinnerTeamId = request.WinnerTeamId;
        match.Description = request.Description;
        match.Status = "Finished";

        await _db.SaveChangesAsync();

        var finishedMatch = await _db.TournamentMatches
            .Include(x => x.TeamA)
            .Include(x => x.TeamB)
            .Include(x => x.WinnerTeam)
            .FirstAsync(x => x.Id == match.Id);

        return new TournamentMatchResponse
        {
            Id = finishedMatch.Id,
            TournamentId = finishedMatch.TournamentId,
            RoundNumber = finishedMatch.RoundNumber,
            MatchNumber = finishedMatch.MatchNumber,
            TeamAId = finishedMatch.TeamAId,
            TeamAName = finishedMatch.TeamA?.Name,
            TeamBId = finishedMatch.TeamBId,
            TeamBName = finishedMatch.TeamB?.Name,
            GoalsA = finishedMatch.GoalsA,
            GoalsB = finishedMatch.GoalsB,
            WinnerTeamId = finishedMatch.WinnerTeamId,
            WinnerTeamName = finishedMatch.WinnerTeam?.Name,
            MatchDate = finishedMatch.MatchDate,
            Location = finishedMatch.Location,
            Status = finishedMatch.Status,
            Description = finishedMatch.Description,
        };
    }
    
    
    
    
    
    
    
    
    // ПОЛУЧИТЬ ВСЕ МАТЧИ ОДНОГО ТУРНИРА 
    public async Task<List<TournamentMatchResponse>> GetTournamentMatchesAsync(int tournamentId)
    {
        var tournamentExists = await _db.Tournaments
            .AnyAsync(t => t.Id == tournamentId);

        if (!tournamentExists)
            throw new Exception("Tournament not found");

        var matches = await _db.TournamentMatches
            .Include(x => x.TeamA)
            .Include(x => x.TeamB)
            .Include(x => x.WinnerTeam)
            .Where(x => x.TournamentId == tournamentId)
            .OrderBy(x => x.RoundNumber)
            .ThenBy(x => x.MatchNumber)
            .ToListAsync();

        return matches.Select(x => new TournamentMatchResponse
        {
            Id = x.Id,
            TournamentId = x.TournamentId,
            RoundNumber = x.RoundNumber,
            MatchNumber = x.MatchNumber,

            TeamAId = x.TeamAId,
            TeamAName = x.TeamA?.Name,

            TeamBId = x.TeamBId,
            TeamBName = x.TeamB?.Name,

            GoalsA = x.GoalsA,
            GoalsB = x.GoalsB,

            WinnerTeamId = x.WinnerTeamId,
            WinnerTeamName = x.WinnerTeam?.Name,

            MatchDate = x.MatchDate,
            Location = x.Location,
            Status = x.Status
        }).ToList();
    }

    
    
    
    
    
    // ПОЛУЧИТЬ ПРИНЯТЫЕ КОМАНДЫ (Для капитана)
    public async Task<List<TournamentAcceptedTeamResponse>> GetAcceptedTeamsAsync(int tournamentId)
    {
        var tournamentExists = await _db.Tournaments
            .AnyAsync(t => t.Id == tournamentId);

        if (!tournamentExists)
            throw new Exception("Tournament not found");

        var teams = await _db.TournamentTeams
            .Include(x => x.Team)
            .Include(x => x.Captain)
            .Where(x => x.TournamentId == tournamentId && x.Status == "Accepted")
            .ToListAsync();

        return teams.Select(x => new TournamentAcceptedTeamResponse
        {
            TeamId = x.TeamId,
            TeamName = x.Team.Name,
            CaptainId = x.CaptainId,
            CaptainUsername = x.Captain.Username,
            AverageLevel = x.Team.AverageLevel,
            Rating = x.Team.Rating
        }).ToList();
    }
    
    
    
    
    
    
    
    // ПОЛУЧИТЬ МАТЧ ПО ID
    public async Task<TournamentMatchResponse> GetMatchByIdAsync(int matchId)
    {
        var match = await _db.TournamentMatches
            .Include(x => x.TeamA)
            .Include(x => x.TeamB)
            .Include(x => x.WinnerTeam)
            .FirstOrDefaultAsync(x => x.Id == matchId);

        if (match == null)
            throw new Exception("Match not found");

        return new TournamentMatchResponse
        {
            Id = match.Id,
            TournamentId = match.TournamentId,
            RoundNumber = match.RoundNumber,
            MatchNumber = match.MatchNumber,

            TeamAId = match.TeamAId,
            TeamAName = match.TeamA?.Name,

            TeamBId = match.TeamBId,
            TeamBName = match.TeamB?.Name,

            GoalsA = match.GoalsA,
            GoalsB = match.GoalsB,

            WinnerTeamId = match.WinnerTeamId,
            WinnerTeamName = match.WinnerTeam?.Name,

            MatchDate = match.MatchDate,
            Location = match.Location,
            Description = match.Description,
            Status = match.Status
        };
    }
}