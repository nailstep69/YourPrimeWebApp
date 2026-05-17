namespace YourPrime.Entities;

public class Tournament
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; } // описание (необязательно)

    public int MaxTeams { get; set; } // 8 / 12 / 16

    public string Status { get; set; } = "Draft";  // Draft/RegistrationOpen/Started/Finished

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>(); // список команд подавшие заявку

    public ICollection<TournamentMatch> Matches { get; set; } = new List<TournamentMatch>(); // все матчи турнира (вся сетка)
}