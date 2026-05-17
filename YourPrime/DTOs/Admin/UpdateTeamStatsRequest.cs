namespace YourPrime.DTOs.Admin;

public class UpdateTeamStatsRequest
{
    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Rating { get; set; }
}