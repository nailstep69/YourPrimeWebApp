namespace YourPrime.DTOs.Tournaments;

public class TournamentResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int MaxTeams { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int AcceptedTeamsCount { get; set; }
}