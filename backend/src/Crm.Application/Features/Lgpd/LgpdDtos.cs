namespace Crm.Application.Features.Lgpd;

public record AnonimizarRequest(string? Motivo);

public record AnonimizacaoResultado(
    Guid PacienteId, int MidiasRemovidas, int AnamnesesAfetadas, DateTime Quando);
