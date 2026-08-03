using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Crm.Domain.Enums;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Common;

public static class DbSeeder
{
    /// <summary>Aplica migrations e cria clínica + usuários de teste (um por papel) se não existirem.</summary>
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.MigrateAsync();

        var clinica = await db.Clinicas.FirstOrDefaultAsync();
        if (clinica is null)
        {
            clinica = new Clinica { Nome = config["Seed:ClinicaNome"] ?? "Clínica (MVP)" };
            db.Clinicas.Add(clinica);
            await db.SaveChangesAsync();
        }

        if (!await db.Usuarios.AnyAsync())
        {
            var senha = config["Seed:AdminSenha"] ?? "Admin@123";
            var hash = hasher.Hash(senha);

            db.Usuarios.AddRange(
                new Usuario
                {
                    ClinicaId = clinica.Id,
                    Nome = config["Seed:AdminNome"] ?? "Administrador",
                    Email = (config["Seed:AdminEmail"] ?? "admin@clinica.local").ToLowerInvariant(),
                    SenhaHash = hash,
                    Papel = Papel.Admin
                },
                new Usuario
                {
                    ClinicaId = clinica.Id,
                    Nome = "Profissional Teste",
                    Email = "profissional@clinica.local",
                    SenhaHash = hash,
                    Papel = Papel.Profissional
                },
                new Usuario
                {
                    ClinicaId = clinica.Id,
                    Nome = "Recepção Teste",
                    Email = "recepcao@clinica.local",
                    SenhaHash = hash,
                    Papel = Papel.Recepcao
                });

            await db.SaveChangesAsync();
        }
    }
}
