using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Api.Common;

/// <summary>
/// Usado apenas em tempo de design (dotnet-ef). Evita executar o Program.cs (seed/conexão)
/// ao gerar/aplicar migrations. Em runtime, o DbContext vem da DI (AddInfrastructure).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var conn = Environment.GetEnvironmentVariable("CRM_DB_CONNECTION")
                   ?? "Host=localhost;Port=5432;Database=crm;Username=crm;Password=crm_dev_password";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(conn, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
