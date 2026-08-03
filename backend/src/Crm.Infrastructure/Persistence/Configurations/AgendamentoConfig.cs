using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations;

public class AgendamentoConfig : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> b)
    {
        b.ToTable("agendamentos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.ValorCobrado).HasPrecision(10, 2);
        b.Property(x => x.Observacao).HasMaxLength(1000);
        b.Property(x => x.MotivoCancelamento).HasMaxLength(1000);

        b.HasOne(x => x.Paciente)
            .WithMany(p => p.Agendamentos)
            .HasForeignKey(x => x.PacienteId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.UsuarioProfissional)
            .WithMany()
            .HasForeignKey(x => x.UsuarioProfissionalId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Procedimento)
            .WithMany()
            .HasForeignKey(x => x.ProcedimentoId)
            .OnDelete(DeleteBehavior.Restrict);

        // RN03 — apoio à checagem de colisão por profissional.
        b.HasIndex(x => new { x.UsuarioProfissionalId, x.Inicio, x.Fim });
    }
}
