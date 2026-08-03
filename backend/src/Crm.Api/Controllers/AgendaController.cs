using Crm.Api.Common;
using Crm.Application.Features.Agenda;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize(Roles = Roles.Interno)]
public class AgendaController : ApiControllerBase
{
    private readonly AgendaService _service;

    public AgendaController(AgendaService service) => _service = service;

    /// <summary>Lista agendamentos, com filtros opcionais por profissional e período (datas em UTC).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid? profissionalId, [FromQuery] DateTime? de, [FromQuery] DateTime? ate, CancellationToken ct)
        => ToResponse(await _service.ListarAsync(profissionalId, de, ate, ct));

    /// <summary>Cria um agendamento avulso (dimensiona pela duração do procedimento; bloqueia colisão).</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarAgendamentoRequest req, CancellationToken ct)
        => ToResponse(await _service.CriarAsync(req, null, ct), StatusCodes.Status201Created);

    /// <summary>Muda o status (Realizado/Faltou/Cancelado — cancelamento exige motivo).</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> AlterarStatus(Guid id, [FromBody] AlterarStatusRequest req, CancellationToken ct)
        => ToResponse(await _service.AlterarStatusAsync(id, req, ct));

    /// <summary>Edita o valor cobrado (pode diferir do valor de referência).</summary>
    [HttpPatch("{id:guid}/valor")]
    public async Task<IActionResult> AtualizarValor(Guid id, [FromBody] AtualizarValorRequest req, CancellationToken ct)
        => ToResponse(await _service.AtualizarValorAsync(id, req.ValorCobrado, ct));
}
