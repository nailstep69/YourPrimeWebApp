namespace YourPrime.DTOs.Tournaments;

public class SetMatchTeamRequest
{
    public int TeamId { get; set; }

    public string Slot { get; set; } = null!; 
    // A / B
}