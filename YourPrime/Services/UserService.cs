using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using YourPrime.Data;
using YourPrime.DTOs.Users;
using YourPrime.Interfaces;
using YourPrime.DTOs.Admin;

namespace YourPrime.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IBlobService _blobService;

    public UserService(AppDbContext db, IBlobService blobService)
    {
        _db = db;
        _blobService = blobService;
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            throw new Exception("User not found");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new Exception("Email cannot be empty");

        if (string.IsNullOrWhiteSpace(request.Username))
            throw new Exception("Username cannot be empty");

        if (request.Age <= 0)
            throw new Exception("Age must be greater than 0");

        var emailExists = await _db.Users
            .AnyAsync(x => x.Email == request.Email && x.Id != userId);

        if (emailExists)
            throw new Exception("Email already in use");

        var usernameExists = await _db.Users
            .AnyAsync(x => x.Username == request.Username && x.Id != userId);

        if (usernameExists)
            throw new Exception("Username already in use");

        user.Email = request.Email;
        user.Username = request.Username;
        user.Age = request.Age;
        user.TelegramId = request.TelegramId;

        await _db.SaveChangesAsync();

        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Age = user.Age,
            TelegramId = user.TelegramId,
            AvatarUrl = user.AvatarUrl,
            Level = user.Level,
            Goals = user.Goals,
            Assists = user.Assists,
            Role = user.Role
        };
    }

    public async Task<UserProfileResponse> UpdateUserStatsAsync(int userId, UpdateUserStatsRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            throw new Exception("User not found");

        if (request.Level < 0 || request.Level > 10)
            throw new Exception("Level must be between 0 and 10");

        if (request.Goals < 0)
            throw new Exception("Goals cannot be negative");

        if (request.Assists < 0)
            throw new Exception("Assists cannot be negative");

        user.Level = request.Level;
        user.Goals = request.Goals;
        user.Assists = request.Assists;

        var userTeam = await _db.UserTeams
            .Include(x => x.Team)
                .ThenInclude(t => t.UserTeams)
                    .ThenInclude(ut => ut.User)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == "Accepted");

        if (userTeam != null)
        {
            var team = userTeam.Team;

            var members = team.UserTeams
                .Where(x => x.Status == "Accepted")
                .ToList();

            team.AverageLevel = members.Count == 0
                ? 0
                : members.Average(x => x.User.Level);
        }

        await _db.SaveChangesAsync();

        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Age = user.Age,
            TelegramId = user.TelegramId,
            AvatarUrl = user.AvatarUrl,
            Level = user.Level,
            Goals = user.Goals,
            Assists = user.Assists,
            Role = user.Role
        };
    }

    public async Task<UserProfileResponse> GetMyProfileAsync(int userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            throw new Exception("User not found");

        var userTeam = await _db.UserTeams
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == "Accepted");

        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Age = user.Age,
            TelegramId = user.TelegramId,
            AvatarUrl = user.AvatarUrl,
            Level = user.Level,
            Goals = user.Goals,
            Assists = user.Assists,
            Role = user.Role,
            TeamName = userTeam?.Team?.Name
        };
    }

    public async Task<UserProfileResponse> UploadAvatarAsync(int userId, IFormFile file)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            throw new Exception("User not found");

        if (file == null || file.Length == 0)
            throw new Exception("File is empty");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
            throw new Exception("Only jpg, jpeg, png, webp allowed");

        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            await _blobService.DeleteFileAsync(user.AvatarUrl);
        }

        var avatarUrl = await _blobService.UploadFileAsync(file);

        user.AvatarUrl = avatarUrl;

        await _db.SaveChangesAsync();

        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Age = user.Age,
            TelegramId = user.TelegramId,
            AvatarUrl = user.AvatarUrl,
            Level = user.Level,
            Goals = user.Goals,
            Assists = user.Assists,
            Role = user.Role
        };
    }
}