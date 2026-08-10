using Crm.Domain.Enums;

namespace Crm.Application.Features.Midia;

public record UploadMidiaRequest(
    Guid PacienteId, EtiquetaMidia Etiqueta, Guid? ProcedimentoId,
    bool ConsentimentoClinico, bool ConsentimentoPublico);

public record RenomearMidiaRequest(string NomeArquivo);

public record MidiaDto(
    Guid Id, Guid PacienteId, Guid? ProcedimentoId, EtiquetaMidia Etiqueta, DateTime Data,
    string NomeArquivo, string ContentType, long TamanhoBytes,
    bool ConsentimentoClinico, bool ConsentimentoPublico, string Url);
