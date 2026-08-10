using Crm.Api.Common;
using Crm.Application.Features.Lgpd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize(Roles = Roles.AdminOuSuperAdmin)]
public class LgpdController : ApiControllerBase
{
    private readonly LgpdService _service;

    public LgpdController(LgpdService service) => _service = service;

    /// <summary>Direito ao esquecimento: anonimiza o paciente e remove mídias (com trilha de auditoria).</summary>
    [HttpPost("pacientes/{id:guid}/anonimizar")]
    public async Task<IActionResult> Anonimizar(Guid id, [FromBody] AnonimizarRequest req, CancellationToken ct)
        => ToResponse(await _service.AnonimizarPacienteAsync(id, req, ct));
}
