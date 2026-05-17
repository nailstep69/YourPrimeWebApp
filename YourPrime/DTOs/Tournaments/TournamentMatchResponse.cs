namespace YourPrime.DTOs.Tournaments;

public class TournamentMatchResponse
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public int RoundNumber { get; set; }

    public int MatchNumber { get; set; }

    public int? TeamAId { get; set; }
    public string? TeamAName { get; set; }

    public int? TeamBId { get; set; }
    public string? TeamBName { get; set; }

    public int? GoalsA { get; set; }

    public int? GoalsB { get; set; }

    public int? WinnerTeamId { get; set; }
    public string? WinnerTeamName { get; set; }
    
    public string? Description { get; set; }

    public DateTime? MatchDate { get; set; }

    public string? Location { get; set; }

    public string Status { get; set; } = null!;
}