using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations;

public class AnamneseConfig : IEntityTypeConfiguration<Anamnese>
{
    public void Configure(EntityTypeBuilder<Anamnese> b)
    {
        b.ToTable("anamneses");
        b.HasKey(x => x.Id);
        b.Property(x => x.Peso).HasPrecision(5, 2);
        b.Property(x => x.Altura).HasPrecision(4, 2);
        b.Property(x => x.Imc).HasPrecision(5, 2);
        b.Property(x => x.PressaoArterial).HasMaxLength(20);
        b.Property(x => x.MotivoEstetico).HasMaxLength(2000);

        // Respostas do questionário fixo persistidas como JSON (jsonb no Postgres).
        b.OwnsOne(x => x.Respostas, o => o.ToJson());

        b.HasOne(x => x.Paciente)
            .WithMany(p => p.Anamneses)
            .HasForeignKey(x => x.PacienteId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.PacienteId, x.DataPreenchimento });
    }
}
