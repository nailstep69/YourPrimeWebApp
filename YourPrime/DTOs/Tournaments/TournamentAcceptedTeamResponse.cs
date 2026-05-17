namespace YourPrime.DTOs.Tournaments;

public class TournamentAcceptedTeamResponse
{
    public int TeamId { get; set; }

    public string TeamName { get; set; } = null!;

    public int CaptainId { get; set; }

    public string CaptainUsername { get; set; } = null!;

    public double AverageLevel { get; set; }

    public int Rating { get; set; }
}