using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations;

public class MidiaConfig : IEntityTypeConfiguration<Midia>
{
    public void Configure(EntityTypeBuilder<Midia> b)
    {
        b.ToTable("midias");
        b.HasKey(x => x.Id);
        b.Property(x => x.ChaveArmazenamento).HasMaxLength(500).IsRequired();
        b.Property(x => x.NomeArquivo).HasMaxLength(300).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
        b.Property(x => x.Etiqueta).HasConversion<int>();

        b.HasOne(x => x.Paciente)
            .WithMany(p => p.Midias)
            .HasForeignKey(x => x.PacienteId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Procedimento)
            .WithMany()
            .HasForeignKey(x => x.ProcedimentoId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => new { x.PacienteId, x.Etiqueta });
    }
}
