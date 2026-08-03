using System.Reflection;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Clinica> Clinicas => Set<Clinica>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Anamnese> Anamneses => Set<Anamnese>();
    public DbSet<AnamneseLink> AnamneseLinks => Set<AnamneseLink>();
    public DbSet<SolicitacaoAtendimento> Solicitacoes => Set<SolicitacaoAtendimento>();
    public DbSet<Procedimento> Procedimentos => Set<Procedimento>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<Midia> Midias => Set<Midia>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
