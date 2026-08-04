using Crm.Application.Common.Interfaces;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Crm.Tests.Infra;

/// <summary>
/// Sobe a API real (Program) apontando para um banco Postgres de teste ISOLADO (nome único por execução).
/// A troca do banco é feita em ConfigureServices (substituindo o registro do DbContext) — à prova de
/// ordem de carregamento da configuração, para nunca tocar no banco de desenvolvimento.
/// Program aplica migrations e faz o seed dos 3 usuários automaticamente. O banco é derrubado no dispose.
///
/// Postgres via variáveis de ambiente (defaults do docker-compose de dev):
///   CRM_TEST_DB_HOST (localhost) · CRM_TEST_DB_PORT (5432) · CRM_TEST_DB_USER (crm) · CRM_TEST_DB_PASSWORD (crm_dev_password)
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory()
    {
        var host = Environment.GetEnvironmentVariable("CRM_TEST_DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("CRM_TEST_DB_PORT") ?? "5432";
        var user = Environment.GetEnvironmentVariable("CRM_TEST_DB_USER") ?? "crm";
        var pass = Environment.GetEnvironmentVariable("CRM_TEST_DB_PASSWORD") ?? "crm_dev_password";
        var db = $"crm_test_{Guid.NewGuid():N}";
        _connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Seed:Procedimentos", "false"); // catálogo controlado pelos testes

        builder.ConfigureServices(services =>
        {
            // Remove o DbContext registrado pela Infrastructure (aponta para o banco de dev)...
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(AppDbContext) ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            // ...e re-registra apontando para o banco de teste isolado.
            services.AddDbContext<AppDbContext>(o =>
                o.UseNpgsql(_connectionString, npg => npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Segurança extra: só derruba se for de fato um banco de teste.
        if (db.Database.GetDbConnection().Database.StartsWith("crm_test_", StringComparison.Ordinal))
            await db.Database.EnsureDeletedAsync();
    }
}
