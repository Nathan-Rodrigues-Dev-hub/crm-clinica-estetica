using Crm.Api.Common;
using Crm.Application.Features.Auth;
using Crm.Application.Features.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly AuthService _auth;
    private readonly UsuarioService _usuarios;

    public AuthController(AuthService auth, UsuarioService usuarios)
    {
        _auth = auth;
        _usuarios = usuarios;
    }

    /// <summary>Login por e-mail e senha. Retorna um JWT com o papel do usuário.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        => ToResponse(await _auth.LoginAsync(req, ct));

    /// <summary>Dados do usuário autenticado (revalida a sessão no front).</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
        => ToResponse(await _usuarios.MeAsync(ct));
}
