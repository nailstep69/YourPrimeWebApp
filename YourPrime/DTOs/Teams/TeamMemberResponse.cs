namespace YourPrime.DTOs.Teams;

public class TeamMemberResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public int Age { get; set; }
    public int Level { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public string Role { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}