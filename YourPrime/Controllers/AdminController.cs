using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourPrime.DTOs.Admin;
using YourPrime.Interfaces;

namespace YourPrime.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITeamService _teamService;
    
    public AdminController(IUserService userService, ITeamService teamService)
    {
        _userService = userService;
        _teamService = teamService;
    }

    
    
    [HttpPut("users/{userId}/stats")]
    public async Task<IActionResult> UpdateUserStats(int userId, UpdateUserStatsRequest request)
    {
        try
        {
            var result = await _userService.UpdateUserStatsAsync(userId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    
    
    
    
    [HttpPut("teams/{teamId}/stats")]
    public async Task<IActionResult> UpdateTeamStats(int teamId, UpdateTeamStatsRequest request)
    {
        try
        {
            var result = await _teamService.UpdateTeamStatsAsync(teamId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}