namespace YourPrime.DTOs.Users;

public class UpdateProfileRequest
{
    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public int Age { get; set; }

    public string? TelegramId { get; set; }
}