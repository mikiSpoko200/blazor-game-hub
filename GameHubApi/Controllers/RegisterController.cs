using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameHubShared.Models;


namespace GameHubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RegisterController : ControllerBase
{
    private readonly DataBase.Context _context;
    private readonly IConfiguration _configuration;

    public RegisterController(DataBase.Context context, IConfiguration configuration)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
        {
            return BadRequest("Username is already taken");
        }

        string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: model.Password,
            salt: System.Text.Encoding.UTF8.GetBytes(_configuration["Salt"]),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10,
            numBytesRequested: 256 / 8));

        var user = new DataBase.User { UserName = model.UserName, Password = passwordHash };
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok();
    }
}