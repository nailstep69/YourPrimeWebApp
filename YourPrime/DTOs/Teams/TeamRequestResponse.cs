namespace YourPrime.DTOs.Teams;

public class TeamRequestResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public int Age { get; set; }
    public int Level { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }

    public int TeamId { get; set; }
    public string TeamName { get; set; } = null!;

    public string Role { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}