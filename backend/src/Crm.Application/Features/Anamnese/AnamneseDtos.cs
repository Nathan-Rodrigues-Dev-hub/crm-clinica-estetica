using Crm.Domain.Entities;

namespace Crm.Application.Features.Anamnese;

// ---- Interno (recepção gera o link) ----
public record GerarLinkRequest(string? TelefoneDestino, int? ValidadeHoras);

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
