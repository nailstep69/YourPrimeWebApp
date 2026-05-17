namespace YourPrime.DTOs.Tournaments;

public class FinishTournamentMatchRequest
{
    public int GoalsA { get; set; }

    public int GoalsB { get; set; }

    public int WinnerTeamId { get; set; }
    
    public string? Description { get; set; }
}