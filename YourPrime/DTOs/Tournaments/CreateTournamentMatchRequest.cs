namespace YourPrime.DTOs.Tournaments;

public class CreateTournamentMatchRequest
{
    public int RoundNumber { get; set; }

    public int MatchNumber { get; set; }

    public int? TeamAId { get; set; }

    public int? TeamBId { get; set; }

    public DateTime? MatchDate { get; set; }

    public string? Location { get; set; }
}