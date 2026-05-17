namespace YourPrime.DTOs.Tournaments;

public class CreateTournamentRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int MaxTeams { get; set; }
}