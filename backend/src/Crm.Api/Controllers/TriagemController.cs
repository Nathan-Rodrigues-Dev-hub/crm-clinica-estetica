using Crm.Api.Common;
using Crm.Application.Features.Triagem;
using Crm.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize(Roles = Roles.Interno)]
public class TriagemController : ApiControllerBase
{
    private readonly TriagemService _service;

    public TriagemController(TriagemService service) => _service = service;

    /// <summary>Fila de triagem (por padrão, solicitações pendentes).</summary>
    [HttpGet]
    public async Task<IActionResult> Fila([FromQuery] StatusSolicitacao? status, CancellationToken ct)
        => ToResponse(await _service.ListarFilaAsync(status, ct));

    /// <summary>Detalhe com a anamnese completa (dado clínico sensível — Admin/Profissional).</summary>
    [Authorize(Roles = Roles.AdminOuProfissional)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detalhe(Guid id, CancellationToken ct)
        => ToResponse(await _service.ObterAsync(id, ct));

    [HttpPost("{id:guid}/assumir")]
    public async Task<IActionResult> Assumir(Guid id, CancellationToken ct)
        => ToResponse(await _service.AssumirAsync(id, ct));

    /// <summary>Aprova e agenda (cria o agendamento e retorna a mensagem para a paciente).</summary>
    [HttpPost("{id:guid}/agendar")]
    public async Task<IActionResult> Agendar(Guid id, [FromBody] AgendarDecisaoRequest req, CancellationToken ct)
        => ToResponse(await _service.AgendarAsync(id, req, ct));

    /// <summary>Recusa com motivo (retorna a mensagem para a paciente).</summary>
    [HttpPost("{id:guid}/recusar")]
    public async Task<IActionResult> Recusar(Guid id, [FromBody] RecusarDecisaoRequest req, CancellationToken ct)
        => ToResponse(await _service.RecusarAsync(id, req, ct));
}
