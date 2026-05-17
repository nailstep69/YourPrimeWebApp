using YourPrime.DTOs.Tournaments;

namespace YourPrime.Interfaces;

public interface ITournamentService
{
    Task<TournamentResponse> CreateTournamentAsync(CreateTournamentRequest request);  // СОЗДАТЬ ТУРНИР  (админ)
    
    Task<List<TournamentResponse>> GetAllTournamentsAsync();  // ПОЛУЧИТЬ ВСЕ ТУРНИРЫ  (анонимус)
    
    Task<string> JoinTournamentAsync(int captainId, int tournamentId);  // ПОДАТЬ ЗАЯВКУ В ТУРНИР (капитан)
    
    Task<string> OpenRegistrationAsync(int tournamentId);  // ОТКРЫТЬ ТУРНИР (СТАТУС) (админ)
    
    Task<List<TournamentTeamRequestResponse>> GetTournamentRequestsAsync(int tournamentId);  // ПОЛУЧИТЬ ВСЕ ЗАЯВКИ НА ТУРНИР
    
    Task<string> AcceptTeamAsync(int tournamentId, int teamId);  // ПРИНЯТЬ ЗАЯВКУ
    
    Task<string> RejectTeamAsync(int tournamentId, int teamId);  // ОТКЛОНИТЬ ЗАЯВКУ
    
    Task<TournamentMatchResponse> CreateMatchAsync(int tournamentId, CreateTournamentMatchRequest request); // СОЗДАТЬ МАТЧ (МОЖЕТ БЫТЬ ПУСТЫМ)
    
    Task<TournamentMatchResponse> SetMatchTeamAsync(int matchId, SetMatchTeamRequest request);  // ОПРЕДЕЛИТЬ КОМАНДУ И СЛОТ
    
    Task<TournamentMatchResponse> FinishMatchAsync(int matchId, FinishTournamentMatchRequest request); // ОПРЕДЕЛИТЬ ГОЛЫ И ПОБЕДИТЕЛЯ
    
    Task<List<TournamentMatchResponse>> GetTournamentMatchesAsync(int tournamentId);  // ПОЛУЧИТЬ ВСЕ МАТЧИ ОДНОГО ТУРНИРА 
    
    Task<List<TournamentAcceptedTeamResponse>> GetAcceptedTeamsAsync(int tournamentId);  // ПОЛУЧИТЬ ПРИНЯТЫЕ КОМАНДЫ (Для капитана)
    
    Task<TournamentMatchResponse> GetMatchByIdAsync(int matchId);  // ПОЛУЧИТЬ МАТЧ ПО ID
}