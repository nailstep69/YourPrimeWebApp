namespace YourPrime.Entities;

public class UserTeam
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public string Role { get; set; } = "Member"; // Member / Captain

    public string Status { get; set; } = "Pending";
    // Pending / Accepted / Rejected

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}