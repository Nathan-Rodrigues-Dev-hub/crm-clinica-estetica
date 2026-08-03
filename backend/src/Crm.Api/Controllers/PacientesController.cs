using Crm.Api.Common;
using Crm.Application.Features.Pacientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize]
public class PacientesController : ApiControllerBase
{
    private readonly PacienteService _service;

    public PacientesController(PacienteService service) => _service = service;

    /// <summary>Busca pacientes por nome, CPF ou telefone (paginado).</summary>
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] string? termo, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => ToResponse(await _service.BuscarAsync(termo, page, pageSize, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
        => ToResponse(await _service.ObterAsync(id, ct));

    /// <summary>Verifica possível duplicidade por CPF/telefone antes de criar (RF08).</summary>
    [HttpGet("duplicidade")]
    public async Task<IActionResult> Duplicidade([FromQuery] string? cpf, [FromQuery] string? telefone, CancellationToken ct)
        => ToResponse(await _service.VerificarDuplicidadeAsync(cpf, telefone, ct));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPacienteRequest req, CancellationToken ct)
        => ToResponse(await _service.CriarAsync(req, ct), StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPacienteRequest req, CancellationToken ct)
        => ToResponse(await _service.AtualizarAsync(id, req, ct));
}
