using Darya.Api.Models;
using Darya.Infrastructure.TokenService;
using Microsoft.AspNetCore.Mvc;

namespace Darya.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Username == "admin" && request.Password == "password") 
        {
            var token = _jwtTokenService.GenerateToken(request.Username, "Admin");
            return Ok(new {Token = token});
        }

        return Unauthorized("Invalid credentials");
    }
}