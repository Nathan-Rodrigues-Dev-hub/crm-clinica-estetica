using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations;

public class SolicitacaoConfig : IEntityTypeConfiguration<SolicitacaoAtendimento>
{
    public void Configure(EntityTypeBuilder<SolicitacaoAtendimento> b)
    {
        b.ToTable("solicitacoes_atendimento");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.ProcedimentoInteresseTexto).HasMaxLength(200);
        b.Property(x => x.MotivoRecusa).HasMaxLength(1000);

        b.HasOne(x => x.Paciente)
            .WithMany(p => p.Solicitacoes)
            .HasForeignKey(x => x.PacienteId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Anamnese)
            .WithMany()
            .HasForeignKey(x => x.AnamneseId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ProcedimentoInteresse)
            .WithMany()
            .HasForeignKey(x => x.ProcedimentoInteresseId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.UsuarioTriagem)
            .WithMany()
            .HasForeignKey(x => x.UsuarioTriagemId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Agendamento)
            .WithMany()
            .HasForeignKey(x => x.AgendamentoId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => new { x.ClinicaId, x.Status });
    }
}
