using YourPrime.DTOs.Teams;
using YourPrime.DTOs.Admin;
using Microsoft.AspNetCore.Http;

namespace YourPrime.Interfaces;

public interface ITeamService
{
    Task<TeamResponse> CreateTeamAsync(int userId, CreateTeamRequest request);  // СОЗДАТЬ КОМАНДУ
    
    Task<List<TeamResponse>> GetAllTeamsAsync();  // ВЕРНУТЬ СПИСОК КОМАНД [AllowAnonymous]   +++
    
    Task<string> JoinTeamAsync(int userId, int teamId);  // ЗАЯВКА НА ВСТУПЛЕНИЕ
    
    Task<List<TeamRequestResponse>> GetMyTeamRequestsAsync(int userId);  // ПОЛУЧЕНИЕ ЗАЯВОК (КАПИТАНОМ)
    
    Task<string> AcceptRequestAsync(int captainId, int userId); // ПРИНЯТЬ ЗАЯВКУ (капитаном)
    
    Task<string> RejectRequestAsync(int captainId, int userId); // ОТКЛОНИТЬ ЗАЯВКУ (капитаном)
    
    Task<MyTeamResponse?> GetMyTeamAsync(int userId); // ПОСМОТРЕТЬ СВОЮ КОМАНДУ  +++
    
    Task<List<TeamRequestResponse>> GetMyRequestsAsync(int userId); // ПОСМОТРЕТЬ СВОИ ЗАПРОСЫ
    
    Task<string> MakeCaptainAsync(int currentCaptainId, int newCaptainId); // НАЗНАЧИТЬ КАПИТАНОМ
    
    Task<string> KickUserAsync(int captainId, int userId); // ИСКЛЮЧИТЬ/КИКНУТЬ
    
    Task<string> LeaveTeamAsync(int userId); // ПОКИНУТЬ КОМАНДУ (капитан не сможет SOS)
    
    Task<string> UpdateTeamNameAsync(int captainId, UpdateTeamNameRequest request); // ИЗМЕНИТЬ НАЗВАНИЕ ТИМЫ
    
    Task<MyTeamResponse> GetTeamByIdAsync(int teamId); // ПОСМОТРЕТЬ ЛЮБУЮ КОМАНДУ С ЕЕ УЧАСТННИКАМИ [AllowAnonymous]   +++
    
    Task<TeamResponse> UpdateTeamStatsAsync(int teamId, UpdateTeamStatsRequest request);   // ОБНОВИТЬ СТАТУ ТИМЫ
    
    Task<MyTeamResponse> UploadTeamAvatarAsync(int captainId, IFormFile file); // ОБНОВИТЬ АВУ ТИМЫ
}