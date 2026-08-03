using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Common.Interfaces;

/// <summary>Abstração do DbContext usada pelas features (Clean Architecture).</summary>
public interface IAppDbContext
{
    DbSet<Clinica> Clinicas { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<Paciente> Pacientes { get; }
    DbSet<Anamnese> Anamneses { get; }
    DbSet<AnamneseLink> AnamneseLinks { get; }
    DbSet<SolicitacaoAtendimento> Solicitacoes { get; }
    DbSet<Procedimento> Procedimentos { get; }
    DbSet<Agendamento> Agendamentos { get; }
    DbSet<Midia> Midias { get; }
    DbSet<LogAuditoria> LogsAuditoria { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
