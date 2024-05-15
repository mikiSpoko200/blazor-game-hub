using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

using GameHubShared.Models;

namespace GameHubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly DataBase.Context _context;
    private readonly IConfiguration _configuration;

    public LoginController(DataBase.Context context, IConfiguration configuration)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: model.Password,
            salt: System.Text.Encoding.UTF8.GetBytes(_configuration["Salt"]),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10,
            numBytesRequested: 256 / 8));

        // Get the user from the database
        var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == model.UserName);

        // Check if user exists and the password is correct
        if (user == null || user.Password != passwordHash)
        {
            return Unauthorized();
        }

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.UserName),
            new(ClaimTypes.Role, "LoggedInUser")
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
}
