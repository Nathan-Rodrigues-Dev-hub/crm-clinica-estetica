using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations;

public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuarios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200).IsRequired();
        b.Property(x => x.SenhaHash).HasMaxLength(300).IsRequired();
        b.Property(x => x.Papel).HasConversion<int>();
        b.HasIndex(x => x.Email).IsUnique();

        b.HasOne(x => x.Clinica)
            .WithMany(c => c.Usuarios)
            .HasForeignKey(x => x.ClinicaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
