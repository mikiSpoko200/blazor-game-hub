using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameHubShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GameHubApi.Controllers;

[Authorize(Policy = "PolicyOfTruth")]
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "PolicyOfTruth")]
public class StatisticsController : ControllerBase
{
    private readonly DataBase.Context _context;

    public StatisticsController(DataBase.Context context)
    {
        _context = context;
    }

    public async Task<ActionResult<List<StatisticModel>>> Get()
    {
        
        var jwtHandler = new JwtSecurityTokenHandler();
        var auth = HttpContext.Request.Headers["Authorization"];
        var jwtInput = auth.ToString().Replace("Bearer ", string.Empty);
        var token = jwtHandler.ReadJwtToken(jwtInput);
        var Name = token.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;

        var user = await _context.Users.Include(u => u.Statistics).ThenInclude(s => s.Game).SingleOrDefaultAsync(p => p.UserName == Name);

        if (user == null || user.Statistics == null)
        {
            return NotFound();
        }

        return user.Statistics.Select(s => new StatisticModel(s.Date, s.Game.Name, s.Won, s.Opponent)).ToList();
    }
}
