namespace YourPrime.Entities;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public int Age { get; set; }
    
    public string? TelegramId { get; set; }
    
    public string? AvatarUrl { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = "User";
    
    public int Level { get; set; } = 0; // 0-10

    public int Goals { get; set; } = 0;

    public int Assists { get; set; } = 0;

    public ICollection<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
}