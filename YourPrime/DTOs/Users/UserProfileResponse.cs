namespace YourPrime.DTOs.Users;

public class UserProfileResponse
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public int Age { get; set; }

    public string? TelegramId { get; set; }

    public int Level { get; set; }

    public int Goals { get; set; }

    public int Assists { get; set; }
    
    public string? TeamName { get; set; } // новое поле

    public string Role { get; set; } = null!;
    
    public string? AvatarUrl { get; set; }
}