using Crm.Application.Common.Interfaces;
using Crm.Infrastructure.Auth;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Services;
using Crm.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npg => npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));

        services.AddSingleton<IClock, Clock>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditLogger, AuditLogger>();

        return services;
    }
}
