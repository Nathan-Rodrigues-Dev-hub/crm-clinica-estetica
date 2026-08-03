using Crm.Api.Common;
using Crm.Application.Features.Anamnese;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Crm.Api.Controllers;

/// <summary>
/// Fluxo público da paciente (sem login). Rate limited (RNF08).
/// A paciente informa o procedimento de interesse e preenche dados + anamnese.
/// </summary>
[ApiController]
[AllowAnonymous]
[EnableRateLimiting("intake")]
[Route("api/public/anamnese")]
public class PublicAnamneseController : ControllerBase
{
    private readonly IntakeService _service;

    public PublicAnamneseController(IntakeService service) => _service = service;

    /// <summary>Passo 1 — abre o link e retorna o catálogo para escolher o procedimento de interesse.</summary>
    [HttpGet("{token}")]
    public async Task<IActionResult> Abrir(string token, CancellationToken ct)
        => this.ToHttp(await _service.AbrirAsync(token, ct));

    /// <summary>Passo 2 — envia dados + anamnese; cria a solicitação e queima o token.</summary>
    [HttpPost("{token}")]
    public async Task<IActionResult> Submeter(string token, [FromBody] IntakeRequest req, CancellationToken ct)
        => this.ToHttp(await _service.SubmeterAsync(token, req, ct), StatusCodes.Status201Created);
}
