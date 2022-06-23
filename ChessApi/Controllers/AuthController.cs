using ChessApi.Controllers.Requests;
using ChessApi.Controllers.Responses;
using ChessApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequestBodyType body)
    {
        var user = new User(body.Name, body.Email, body.Password);
        await using var ctx = new ChessContext();
        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();
        return Ok(new UserResponseBodyType(user.Name, user.Id));
    }
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequestBodyType body)
    {
        await using var ctx = new ChessContext();
        var user = await ctx.Users.FirstOrDefaultAsync(x => x.Email == body.Email && x.Password == body.Password);
        if (user == null)
            return StatusCode(401);
        return Ok(new UserResponseBodyType(user.Name, user.Id));
    }
}