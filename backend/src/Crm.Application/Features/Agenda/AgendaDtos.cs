using Crm.Domain.Enums;

namespace Crm.Application.Features.Agenda;

public record CriarAgendamentoRequest(
    Guid PacienteId, Guid UsuarioProfissionalId, Guid ProcedimentoId,
    DateTime Inicio, decimal? ValorCobrado, string? Observacao);

public record AlterarStatusRequest(StatusAgendamento Status, string? MotivoCancelamento);

public record AtualizarValorRequest(decimal ValorCobrado);

public record AgendamentoDto(
    Guid Id, Guid PacienteId, string PacienteNome,
    Guid UsuarioProfissionalId, string ProfissionalNome,
    Guid ProcedimentoId, string ProcedimentoNome,
    DateTime Inicio, DateTime Fim, StatusAgendamento Status,
    decimal? ValorCobrado, string? Observacao, string? MotivoCancelamento,
    Guid? SolicitacaoAtendimentoId);
