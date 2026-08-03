using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations;

public class LogAuditoriaConfig : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> b)
    {
        b.ToTable("logs_auditoria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Acao).HasMaxLength(100).IsRequired();
        b.Property(x => x.EntidadeTipo).HasMaxLength(100).IsRequired();
        b.Property(x => x.Detalhe).HasMaxLength(2000);
        b.HasIndex(x => new { x.EntidadeTipo, x.EntidadeId });
        b.HasIndex(x => x.DataHora);
    }
}
