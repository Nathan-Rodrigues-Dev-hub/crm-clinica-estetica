using Crm.Domain.Common;

namespace Crm.Domain.Entities;

/// <summary>Trilha de auditoria de acesso/alteração de prontuário (LGPD).</summary>
public class LogAuditoria : BaseEntity
{
    public Guid? ClinicaId { get; set; }
    public Guid? UsuarioId { get; set; }

    public string Acao { get; set; } = string.Empty;
    public string EntidadeTipo { get; set; } = string.Empty;
    public Guid? EntidadeId { get; set; }
    public string? Detalhe { get; set; }

    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
