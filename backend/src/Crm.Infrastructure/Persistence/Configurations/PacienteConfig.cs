using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations;

public class PacienteConfig : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> b)
    {
        b.ToTable("pacientes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        b.Property(x => x.Cpf).HasMaxLength(11);
        b.Property(x => x.Telefone).HasMaxLength(20);
        b.Property(x => x.Sexo).HasMaxLength(30);
        b.Property(x => x.Endereco).HasMaxLength(300);
        b.Property(x => x.RedeSocial).HasMaxLength(150);
        b.Property(x => x.Origem).HasConversion<int>();

        // RN01 — CPF único por clínica quando informado (índice parcial no Postgres).
        // Coluna é "Cpf" (PascalCase) — precisa das aspas no filtro.
        b.HasIndex(x => new { x.ClinicaId, x.Cpf })
            .IsUnique()
            .HasFilter("\"Cpf\" IS NOT NULL");

        b.HasIndex(x => new { x.ClinicaId, x.Telefone });

        b.HasOne(x => x.Clinica)
            .WithMany(c => c.Pacientes)
            .HasForeignKey(x => x.ClinicaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
