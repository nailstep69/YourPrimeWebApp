namespace YourPrime.Entities;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public int? CaptainId { get; set; }
    
    public User? Captain { get; set; }

    public int Wins { get; set; }
    public int Losses { get; set; }

    public bool IsOpen { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    
    public string? AvatarUrl { get; set; }
    public int Rating { get; set; } = 0;
    
    public double AverageLevel { get; set; } = 0;

    public ICollection<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
}