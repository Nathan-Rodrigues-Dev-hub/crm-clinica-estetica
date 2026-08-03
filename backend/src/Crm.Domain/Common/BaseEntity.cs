namespace Crm.Domain.Common;

/// <summary>Base de todas as entidades: Id (Guid) e data de criação (UTC).</summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
