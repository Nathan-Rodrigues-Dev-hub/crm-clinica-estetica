using Crm.Domain.Common;
using Crm.Domain.Enums;

namespace Crm.Domain.Entities;

/// <summary>Foto/vídeo do paciente (galeria "Mídia"), com dois flags de consentimento.</summary>
public class Midia : BaseEntity
{
    public Guid ClinicaId { get; set; }

    public Guid PacienteId { get; set; }
    public Paciente? Paciente { get; set; }

    /// <summary>Vínculo opcional a um procedimento.</summary>
    public Guid? ProcedimentoId { get; set; }
    public Procedimento? Procedimento { get; set; }

    /// <summary>Usuário que registrou a mídia.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Chave no storage (S3 em produção; arquivo local em dev).</summary>
    public string ChaveArmazenamento { get; set; } = string.Empty;
    public string NomeArquivo { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }

    public EtiquetaMidia Etiqueta { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;

    public bool ConsentimentoClinico { get; set; }
    public bool ConsentimentoPublico { get; set; }
}
