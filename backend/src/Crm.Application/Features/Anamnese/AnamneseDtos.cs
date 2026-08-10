using Crm.Domain.Entities;

namespace Crm.Application.Features.Anamnese;

// ---- Interno (recepção gera o link) ----
// PacienteId opcional: quando informado, a paciente já está cadastrada e o link nasce
// vinculado a ela — o preenchimento remoto grava a anamnese direto nesse paciente,
// sem depender de bater CPF (RN — link.PacienteId tem prioridade em IntakeService).
public record GerarLinkRequest(string? TelefoneDestino, int? ValidadeHoras, Guid? PacienteId);

public record LinkGeradoDto(string Token, string UrlPreenchimento, DateTime Expiracao);

// ---- Público (paciente abre e preenche, sem login) ----
public record ProcedimentoResumo(Guid Id, string Nome, string? Categoria, int DuracaoMinutos);

public record AberturaLinkDto(bool Valido, string? Motivo, List<ProcedimentoResumo> Procedimentos);

public record PacienteIntake(
    string Nome, string? Cpf, string? Telefone, DateOnly? DataNascimento,
    string? Sexo, string? Endereco, string? RedeSocial);

public record AnamneseIntake(
    decimal? Peso, decimal? Altura, string? MotivoEstetico,
    bool AceiteDeclaracao, RespostasAnamnese? Respostas);

public record IntakeRequest(
    Guid? ProcedimentoInteresseId, string? ProcedimentoInteresseTexto,
    PacienteIntake Paciente, AnamneseIntake Anamnese);

public record IntakeResultado(Guid SolicitacaoId, Guid PacienteId, decimal? Imc, string Mensagem);

// ---- Consulta (histórico por paciente) ----
public record AnamneseDetalhe(
    Guid Id, DateTime DataPreenchimento, decimal? Peso, decimal? Altura, decimal? Imc,
    string? PressaoArterial, string? MotivoEstetico, bool AceiteDeclaracao, RespostasAnamnese Respostas);

// ---- Registro manual (recepção transcrevendo ficha em papel, ou fallback sem internet) ----
public record CriarAnamneseManualRequest(
    DateTime? DataPreenchimento, decimal? Peso, decimal? Altura, string? PressaoArterial,
    string? MotivoEstetico, bool AceiteDeclaracao, RespostasAnamnese? Respostas);
