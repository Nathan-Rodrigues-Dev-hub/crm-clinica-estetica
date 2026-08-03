using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations;

public class ProcedimentoConfig : IEntityTypeConfiguration<Procedimento>
{
    public void Configure(EntityTypeBuilder<Procedimento> b)
    {
        b.ToTable("procedimentos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        b.Property(x => x.Descricao).HasMaxLength(1000);
        b.Property(x => x.Categoria).HasMaxLength(100);
        b.Property(x => x.ValorReferencia).HasPrecision(10, 2);

        b.HasOne(x => x.Clinica)
            .WithMany(c => c.Procedimentos)
            .HasForeignKey(x => x.ClinicaId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.ClinicaId, x.Nome });
    }
}
