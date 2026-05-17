namespace YourPrime.DTOs.Tournaments;

public class TournamentTeamRequestResponse
{
    public int TournamentId { get; set; }
    public string TournamentName { get; set; } = null!;

    public int TeamId { get; set; }
    public string TeamName { get; set; } = null!;

    public int CaptainId { get; set; }
    public string CaptainUsername { get; set; } = null!;

    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}