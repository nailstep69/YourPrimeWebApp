using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YourPrime.DTOs.Teams;
using YourPrime.Interfaces;

namespace YourPrime.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    
    
    // СОЗДАТЬ КОМАНДУ
    [HttpPost("create")]
    public async Task<IActionResult> CreateTeam(CreateTeamRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _teamService.CreateTeamAsync(userId, request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    
    // ВЕРНУТЬ СПИСОК КОМАНД
    [HttpGet]
    [AllowAnonymous]   // список команд можно смотреть без токена
    public async Task<IActionResult> GetAllTeams()
    {
        var result = await _teamService.GetAllTeamsAsync();
        return Ok(result);
    }
    
    
    
    
    
    
    
    // ЗАЯВКА НА ВСТУПЛЕНИЕ
    [HttpPost("{teamId}/join")]
    public async Task<IActionResult> JoinTeam(int teamId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _teamService.JoinTeamAsync(userId, teamId);

            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    // ПОЛУЧЕНИЕ ЗАЯВОК ЛЮБЫМ УЧАСТНИКОМ КОМАНДЫ
    [HttpGet("requests")]
    public async Task<IActionResult> GetMyTeamRequests()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _teamService.GetMyTeamRequestsAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    // ПРИНЯТЬ ЗАЯВКУ (КАПИТАНОМ)
    [HttpPost("accept/{userId}")]
    public async Task<IActionResult> AcceptRequest(int userId)
    {
        try
        {
            var captainId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _teamService.AcceptRequestAsync(captainId, userId);

            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    // ОТКЛОНИТЬ ЗАЯВКУ (КАПИТАНОМ)
    [HttpPost("reject/{userId}")]
    public async Task<IActionResult> RejectRequest(int userId)
    {
        try
        {
            var captainId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _teamService.RejectRequestAsync(captainId, userId);

            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    // ПОСМОТРЕТЬ СВОЮ КОМАНДУ 
    [HttpGet("my-team")]
    public async Task<IActionResult> GetMyTeam()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _teamService.GetMyTeamAsync(userId);

        if (result == null)
            return NotFound(new { message = "You are not in any team" });

        return Ok(result);
    }
    
    
    
    
    
    
    
    // ПОСМОТРЕТЬ СВОИ ЗАПРОСЫ
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _teamService.GetMyRequestsAsync(userId);

        return Ok(result);
    }
    
    
    
    
    // НАЗНАЧИТЬ КАПИТАНОМ
    [HttpPost("make-captain/{newCaptainId}")]
    public async Task<IActionResult> MakeCaptain(int newCaptainId)
    {
        try
        {
            var currentCaptainId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _teamService.MakeCaptainAsync(currentCaptainId, newCaptainId);

            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    // ИСКЛЮЧИТЬ/КИКНУТЬ
    [HttpPost("kick/{userId}")]
    public async Task<IActionResult> KickUser(int userId)
    {
        try
        {
            var captainId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _teamService.KickUserAsync(captainId, userId);

            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    
    // ПОКИНУТЬ КОМАНДУ (капитан не сможет SOS)
    [HttpPost("leave")]
    public async Task<IActionResult> LeaveTeam()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _teamService.LeaveTeamAsync(userId);

            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    // ИЗМЕНИТЬ НАЗВАНИЕ ТИМЫ
    [HttpPut("name")]
    public async Task<IActionResult> UpdateTeamName(UpdateTeamNameRequest request)
    {
        try
        {
            var captainId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _teamService.UpdateTeamNameAsync(captainId, request);

            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    // ПОСМОТРЕТЬ ЛЮБУЮ КОМАНДУ С ЕЕ УЧАСТННИКАМИ [AllowAnonymous]
    [HttpGet("{teamId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTeamById(int teamId)
    {
        try
        {
            var result = await _teamService.GetTeamByIdAsync(teamId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    
    
    
    
    
    // ОБНОВИТЬ АВАТАРКУ ТИМЫ
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadTeamAvatar(IFormFile file)
    {
        try
        {
            var captainId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _teamService.UploadTeamAvatarAsync(captainId, file);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}