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
            clinica = new Clinica
            {
                Nome = config["Seed:ClinicaNome"] ?? "Clínica (MVP)",
                LimiteProfissionais = config.GetValue<int?>("Seed:LimiteProfissionais") ?? 5
            };
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

        // Conta SuperAdmin (suporte/dono da plataforma) — independente do bloco acima, para não
        // exigir recriar o banco em clínicas já existentes. Só cria se a senha estiver configurada
        // (sem default: não queremos um SuperAdmin com credencial previsível em produção).
        var senhaSuperAdmin = config["Seed:SuperAdminSenha"];
        if (!string.IsNullOrWhiteSpace(senhaSuperAdmin) && !await db.Usuarios.AnyAsync(u => u.Papel == Papel.SuperAdmin))
        {
            db.Usuarios.Add(new Usuario
            {
                ClinicaId = clinica.Id,
                Nome = config["Seed:SuperAdminNome"] ?? "Suporte",
                Email = (config["Seed:SuperAdminEmail"] ?? "suporte@crm.local").ToLowerInvariant(),
                SenhaHash = hasher.Hash(senhaSuperAdmin),
                Papel = Papel.SuperAdmin
            });
            await db.SaveChangesAsync();
        }

        // Catálogo inicial da clínica (Seed:Procedimentos = true, padrão). Roda uma vez,
        // em banco sem procedimentos; a clínica adiciona/edita os demais pela tela de Catálogo.
        var seedProcedimentos = config.GetValue<bool?>("Seed:Procedimentos") ?? true;
        if (seedProcedimentos && !await db.Procedimentos.AnyAsync())
        {
            db.Procedimentos.AddRange(
                // Toxina botulínica
                new Procedimento { ClinicaId = clinica.Id, Nome = "Botox - terço superior", Categoria = "Toxina Botulínica", DuracaoMinutos = 30, ValorReferencia = 1200m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Botox - Fullface", Categoria = "Toxina Botulínica", DuracaoMinutos = 45, ValorReferencia = 2500m, Ativo = true },
                // Preenchimento
                new Procedimento { ClinicaId = clinica.Id, Nome = "Preenchimento - Lábios", Categoria = "Preenchimento", DuracaoMinutos = 40, ValorReferencia = 1600m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Preenchimento - Mento", Categoria = "Preenchimento", Descricao = "Valor a partir de.", DuracaoMinutos = 40, ValorReferencia = 1800m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Preenchimento - Malar", Categoria = "Preenchimento", Descricao = "Valor a partir de.", DuracaoMinutos = 40, ValorReferencia = 1800m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Preenchimento - Têmpora", Categoria = "Preenchimento", Descricao = "Valor a partir de.", DuracaoMinutos = 40, ValorReferencia = 1800m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Preenchimento - Mandíbula", Categoria = "Preenchimento", Descricao = "Valor a partir de.", DuracaoMinutos = 40, ValorReferencia = 1800m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Rinomodelação não cirúrgica", Categoria = "Preenchimento", DuracaoMinutos = 40, ValorReferencia = 2200m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Bigode chinês", Categoria = "Preenchimento", DuracaoMinutos = 30, ValorReferencia = 1200m, Ativo = true },
                // Fios de sustentação
                new Procedimento { ClinicaId = clinica.Id, Nome = "Fios de sustentação", Categoria = "Fios de Sustentação", Descricao = "Valor a partir de.", DuracaoMinutos = 60, ValorReferencia = 2200m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Fios lisos", Categoria = "Fios de Sustentação", Descricao = "Valor a partir de.", DuracaoMinutos = 60, ValorReferencia = 1200m, Ativo = true },
                // Bioestimulador
                new Procedimento { ClinicaId = clinica.Id, Nome = "Bioestimulador - rosto", Categoria = "Bioestimulador", DuracaoMinutos = 60, ValorReferencia = 2100m, Ativo = true },
                // Facial / outros
                new Procedimento { ClinicaId = clinica.Id, Nome = "Remoção de verrugas", Categoria = "Facial", Descricao = "Valor a partir de.", DuracaoMinutos = 30, ValorReferencia = 450m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Resete Cell", Categoria = "Facial", Descricao = "Protocolo de 5 sessões.", DuracaoMinutos = 60, ValorReferencia = 2500m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Limpeza de pele", Categoria = "Facial", DuracaoMinutos = 60, ValorReferencia = 180m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Blefaroplastia sem corte", Categoria = "Facial", Descricao = "Protocolo de 3 sessões.", DuracaoMinutos = 45, ValorReferencia = 1000m, Ativo = true },
                new Procedimento { ClinicaId = clinica.Id, Nome = "Radiofrequência - rosto", Categoria = "Facial", Descricao = "1 sessão.", DuracaoMinutos = 30, ValorReferencia = 350m, Ativo = true });

            await db.SaveChangesAsync();
        }
    }
}
