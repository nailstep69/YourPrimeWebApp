namespace YourPrime.Entities;

public class TournamentMatch
{
    public int Id { get; set; }

    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;    // к какому турниру относится матч

    public int RoundNumber { get; set; }   // раунд: 1 → четвертьфинал / 2 → полуфинал / 3 → финал (нужно для построения сетки)

    public int MatchNumber { get; set; }  // номер матча в раунде  (Round 1 → Match 1, Match 2, Match 3…)

    public int? TeamAId { get; set; }  
    public Team? TeamA { get; set; }  // первая команда

    public int? TeamBId { get; set; }
    public Team? TeamB { get; set; }  // вторая команда

    public int? GoalsA { get; set; }
    public int? GoalsB { get; set; }   // счёт матча

    public int? WinnerTeamId { get; set; }
    public Team? WinnerTeam { get; set; }   

    public DateTime? MatchDate { get; set; }  // когда игра

    public string? Location { get; set; }  // где проходит игра
    
    public string? Description { get; set; }
    public string Status { get; set; } = "Scheduled";  // Scheduled / Finished
}