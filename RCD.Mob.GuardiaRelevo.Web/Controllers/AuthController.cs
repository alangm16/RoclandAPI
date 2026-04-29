using Microsoft.AspNetCore.Mvc;
using RCD.Mob.GuardiaRelevo.Application.Common;
using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;

namespace RCD.Mob.GuardiaRelevo.Web.Controllers;

[ApiController]
[Route("api/mob/guardiarelevo/auth")]
[ApiExplorerSettings(GroupName = "mobile-guardia-relevo")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request.Usuario, request.Password, ct);

        if (result is null)
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Credenciales incorrectas."));

        return Ok(ApiResponse<LoginResponseDto>.Ok(result));
    }
}