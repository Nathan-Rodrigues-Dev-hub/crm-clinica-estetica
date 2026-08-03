using Crm.Api.Common;
using Crm.Application.Features.Procedimentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize]
public class ProcedimentosController : ApiControllerBase
{
    private readonly ProcedimentoService _service;

    public ProcedimentosController(ProcedimentoService service) => _service = service;

    /// <summary>Lista o catálogo (por padrão só ativos).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInativos = false, CancellationToken ct = default)
        => ToResponse(await _service.ListarAsync(incluirInativos, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
        => ToResponse(await _service.ObterAsync(id, ct));

    [Authorize(Roles = Roles.AdminOuRecepcao)]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarProcedimentoRequest req, CancellationToken ct)
        => ToResponse(await _service.CriarAsync(req, ct), StatusCodes.Status201Created);

    [Authorize(Roles = Roles.AdminOuRecepcao)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarProcedimentoRequest req, CancellationToken ct)
        => ToResponse(await _service.AtualizarAsync(id, req, ct));
}
