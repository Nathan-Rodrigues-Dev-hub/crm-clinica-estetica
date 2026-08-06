using Crm.Api.Common;
using Crm.Application.Features.Anamnese;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

/// <summary>Uso interno da clínica (recepção/profissional/admin): gera o link tokenizado de anamnese.</summary>
[Authorize(Roles = Roles.Interno)]
public class AnamneseController : ApiControllerBase
{
    private readonly AnamneseLinkService _service;

    public AnamneseController(AnamneseLinkService service) => _service = service;

    /// <summary>Gera um link de anamnese (sem login) para enviar à paciente por WhatsApp.</summary>
    [HttpPost("links")]
    public async Task<IActionResult> GerarLink([FromBody] GerarLinkRequest req, CancellationToken ct)
        => ToResponse(await _service.GerarAsync(req, ct), StatusCodes.Status201Created);
}
