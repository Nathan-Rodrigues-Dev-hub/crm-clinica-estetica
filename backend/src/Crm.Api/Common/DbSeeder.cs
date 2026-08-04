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

        // Seed opcional de procedimentos de exemplo (Seed:Procedimentos = true, padrão em Development).
        var seedProcedimentos = config.GetValue<bool?>("Seed:Procedimentos") ?? true;
        if (seedProcedimentos && !await db.Procedimentos.AnyAsync())
        {
            db.Procedimentos.AddRange(
                new Procedimento { ClinicaId = clinica.Id, Nome = "Limpeza de pele profunda", Categoria = "Facial", Descricao = "Higienização, extração e hidratação.", DuracaoMinutos = 60, ValorReferencia = 180m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Peeling químico", Categoria = "Facial", Descricao = "Renovação celular com ácidos.", DuracaoMinutos = 45, ValorReferencia = 250m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Massagem modeladora", Categoria = "Corporal", Descricao = "Redução de medidas e drenagem.", DuracaoMinutos = 50, ValorReferencia = 150m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Drenagem linfática", Categoria = "Corporal", Descricao = "Estímulo do sistema linfático.", DuracaoMinutos = 60, ValorReferencia = 160m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Aplicação de toxina botulínica", Categoria = "Injetáveis", Descricao = "Atenuação de linhas de expressão.", DuracaoMinutos = 30, ValorReferencia = 900m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Microagulhamento", Categoria = "Facial", Descricao = "Indução percutânea de colágeno.", DuracaoMinutos = 60, ValorReferencia = 350m, Ativo = true });

            await db.SaveChangesAsync();
        }
    }
}
