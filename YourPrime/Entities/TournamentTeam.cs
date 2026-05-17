namespace YourPrime.Entities;

public class TournamentTeam
{
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;  // к какому турниру относится заявка

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;   // какая команда подала заявку

    public int CaptainId { get; set; }
    public User Captain { get; set; } = null!;  // кто подал заявку (капитан)

    public string Status { get; set; } = "Pending";  // Pending/Accepted/Rejected

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // когда подали заявку
}