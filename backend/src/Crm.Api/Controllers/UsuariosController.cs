using Crm.Api.Common;
using Crm.Application.Features.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize]
public class UsuariosController : ApiControllerBase
{
    private readonly UsuarioService _service;

    public UsuariosController(UsuarioService service) => _service = service;

    /// <summary>Lista os profissionais (para selects da agenda/triagem).</summary>
    [Authorize(Roles = Roles.Interno)]
    [HttpGet("profissionais")]
    public async Task<IActionResult> Profissionais(CancellationToken ct)
        => ToResponse(await _service.ListarProfissionaisAsync(ct));
}
