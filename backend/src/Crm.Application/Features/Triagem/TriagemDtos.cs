using Crm.Domain.Entities;
using Crm.Domain.Enums;

namespace Crm.Application.Features.Triagem;

public record SolicitacaoListItem(
    Guid Id, Guid PacienteId, string PacienteNome, string? Telefone,
    string ProcedimentoInteresse, StatusSolicitacao Status, DateTime DataEnvio);

/// <summary>Detalhe da solicitação incluindo a anamnese (dado clínico sensível — Admin/Profissional).</summary>
public record SolicitacaoDetalhe(
    Guid Id, Guid PacienteId, string PacienteNome, string? Telefone, string? Cpf,
    string ProcedimentoInteresse, StatusSolicitacao Status, DateTime DataEnvio,
    string? MotivoRecusa, Guid? AgendamentoId,
    AnamneseDetalhe Anamnese);

public record AnamneseDetalhe(
    Guid Id, DateTime DataPreenchimento, decimal? Peso, decimal? Altura, decimal? Imc,
    string? PressaoArterial, string? MotivoEstetico, bool AceiteDeclaracao, RespostasAnamnese Respostas);

public record AgendarDecisaoRequest(
    Guid UsuarioProfissionalId, Guid? ProcedimentoId, DateTime Inicio, decimal? ValorCobrado, string? Observacao);

public record RecusarDecisaoRequest(string Motivo);

public record DecisaoResultado(
    Guid SolicitacaoId, StatusSolicitacao Status, Guid? AgendamentoId, string MensagemParaPaciente);
