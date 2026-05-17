using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourPrime.DTOs.Tournaments;
using YourPrime.Interfaces;
using System.Security.Claims;

namespace YourPrime.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentController : ControllerBase
{
    private readonly ITournamentService _tournamentService;

    public TournamentController(ITournamentService tournamentService)
    {
        _tournamentService = tournamentService;
    }
    
    
    
    
    
    
    
    // СОЗДАТЬ ТУРНИР  (админ)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTournament(CreateTournamentRequest request)
    {
        try
        {
            var result = await _tournamentService.CreateTournamentAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    // ПОЛУЧИТЬ ВСЕ ТУРНИРЫ  (анонимус)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllTournaments()
    {
        var result = await _tournamentService.GetAllTournamentsAsync();
        return Ok(result);
    }
    
    
    
    
    
    
    // ПОДАТЬ ЗАЯВКУ В ТУРНИР (капитан)
    [HttpPost("{tournamentId}/join")]
    [Authorize]
    public async Task<IActionResult> JoinTournament(int tournamentId)
    {
        try
        {
            var captainId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _tournamentService.JoinTournamentAsync(captainId, tournamentId);

            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    
    // ОТКРЫТЬ ТУРНИР (СТАТУС) (админ)
    [HttpPost("{tournamentId}/open-registration")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> OpenRegistration(int tournamentId)
    {
        try
        {
            var result = await _tournamentService.OpenRegistrationAsync(tournamentId);

            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    
    // ПОЛУЧИТЬ ВСЕ ЗАЯВКИ НА ТУРНИР
    [HttpGet("{tournamentId}/requests")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTournamentRequests(int tournamentId)
    {
        try
        {
            var result = await _tournamentService.GetTournamentRequestsAsync(tournamentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    
    // ПРИНЯТЬ ЗАЯВКУ
    [HttpPost("{tournamentId}/accept/{teamId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AcceptTeam(int tournamentId, int teamId)
    {
        try
        {
            var result = await _tournamentService.AcceptTeamAsync(tournamentId, teamId);
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    
    // ОТКЛОНИТЬ ЗАЯВКУ
    [HttpPost("{tournamentId}/reject/{teamId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectTeam(int tournamentId, int teamId)
    {
        try
        {
            var result = await _tournamentService.RejectTeamAsync(tournamentId, teamId);
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    
    // СОЗДАТЬ МАТЧ (МОЖЕТ БЫТЬ ПУСТЫМ)
    [HttpPost("{tournamentId}/matches")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateMatch(
        int tournamentId,
        CreateTournamentMatchRequest request)
    {
        try
        {
            var result = await _tournamentService.CreateMatchAsync(tournamentId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    
    // ОПРЕДЕЛИТЬ КОМАНДУ И СЛОТ
    [HttpPut("matches/{matchId}/teams")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetMatchTeam(
        int matchId,
        SetMatchTeamRequest request)
    {
        try
        {
            var result = await _tournamentService.SetMatchTeamAsync(matchId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    
    
    // ОПРЕДЕЛИТЬ ГОЛЫ И ПОБЕДИТЕЛЯ
    [HttpPost("matches/{matchId}/finish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> FinishMatch(
        int matchId,
        FinishTournamentMatchRequest request)
    {
        try
        {
            var result = await _tournamentService.FinishMatchAsync(matchId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    
    
    // ПОЛУЧИТЬ ВСЕ МАТЧИ ОДНОГО ТУРНИРА 
    [HttpGet("{tournamentId}/matches")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTournamentMatches(int tournamentId)
    {
        try
        {
            var result = await _tournamentService.GetTournamentMatchesAsync(tournamentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    // ПОЛУЧИТЬ ПРИНЯТЫЕ КОМАНДЫ (Для капитана)
    [HttpGet("{tournamentId}/accepted-teams")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAcceptedTeams(int tournamentId)
    {
        try
        {
            var result = await _tournamentService.GetAcceptedTeamsAsync(tournamentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    // ПОЛУЧИТЬ МАТЧ ПО ID
    [HttpGet("matches/{matchId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMatchById(int matchId)
    {
        try
        {
            var result = await _tournamentService.GetMatchByIdAsync(matchId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}