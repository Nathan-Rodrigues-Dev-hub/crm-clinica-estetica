using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations;

public class AnamneseLinkConfig : IEntityTypeConfiguration<AnamneseLink>
{
    public void Configure(EntityTypeBuilder<AnamneseLink> b)
    {
        b.ToTable("anamnese_links");
        b.HasKey(x => x.Id);
        b.Property(x => x.Token).HasMaxLength(64).IsRequired();
        b.Property(x => x.TelefoneDestino).HasMaxLength(20);
        b.HasIndex(x => x.Token).IsUnique();

        b.HasOne(x => x.Paciente)
            .WithMany()
            .HasForeignKey(x => x.PacienteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
