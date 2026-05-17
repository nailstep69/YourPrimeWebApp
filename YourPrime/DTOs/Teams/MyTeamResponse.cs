namespace YourPrime.DTOs.Teams;

public class MyTeamResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? CaptainId { get; set; }
    public string? CaptainUsername { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public bool IsOpen { get; set; }
    public int Rating { get; set; }
    public double AverageLevel { get; set; }
    public string? AvatarUrl { get; set; }
    public List<TeamMemberResponse> Members { get; set; } = new();
}