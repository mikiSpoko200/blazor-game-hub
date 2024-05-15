using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GameHubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AnonymousController : ControllerBase
{
    private readonly DataBase.Context _context;
    private readonly IConfiguration _configuration;
    
    public AnonymousController(DataBase.Context context, IConfiguration configuration)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginModel model)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.UserName),
            new(ClaimTypes.Role, "AnonymousUser")
        };

        var tokenOptions = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: signinCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return Ok(new { Token = tokenString });
    }

    public class LoginModel
    {
        public string UserName { get; set; }
    }
}


