using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Crm.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using DomainAnamnese = Crm.Domain.Entities.Anamnese;

namespace Crm.Application.Features.Anamnese;

/// <summary>Fluxo público (sem login): abrir o link e submeter o preenchimento (intake).</summary>
public class IntakeService
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public IntakeService(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    /// <summary>Passo 1 — abre o link e devolve o catálogo para escolher o procedimento de interesse.</summary>
    public async Task<Result<AberturaLinkDto>> AbrirAsync(string token, CancellationToken ct = default)
    {
        var link = await _db.AnamneseLinks.AsNoTracking().FirstOrDefaultAsync(l => l.Token == token, ct);
        if (link is null)
            return Result.Ok(new AberturaLinkDto(false, "Link inválido.", new()));
        if (!link.EstaValido(_clock.UtcNow))
            return Result.Ok(new AberturaLinkDto(false, "Link expirado ou já utilizado.", new()));

        var procedimentos = await _db.Procedimentos.AsNoTracking()
            .Where(p => p.ClinicaId == link.ClinicaId && p.Ativo)
            .OrderBy(p => p.Nome)
            .Select(p => new ProcedimentoResumo(p.Id, p.Nome, p.Categoria, p.DuracaoMinutos))
            .ToListAsync(ct);

        return Result.Ok(new AberturaLinkDto(true, null, procedimentos));
    }

    /// <summary>Passo 2 — grava dados + anamnese, calcula IMC, abre a solicitação e queima o token.</summary>
    public async Task<Result<IntakeResultado>> SubmeterAsync(string token, IntakeRequest req, CancellationToken ct = default)
    {
        var link = await _db.AnamneseLinks.FirstOrDefaultAsync(l => l.Token == token, ct);
        if (link is null)
            return Result.Falha<IntakeResultado>("Link inválido.", ErrorType.NotFound);
        if (!link.EstaValido(_clock.UtcNow))
            return Result.Falha<IntakeResultado>("Link expirado ou já utilizado.", ErrorType.Conflict);

        if (req.Paciente is null || string.IsNullOrWhiteSpace(req.Paciente.Nome))
            return Result.Falha<IntakeResultado>("Informe o nome do paciente.");
        if (req.Anamnese is null || !req.Anamnese.AceiteDeclaracao)
            return Result.Falha<IntakeResultado>("É necessário aceitar a declaração de veracidade.");
        if (req.ProcedimentoInteresseId is null && string.IsNullOrWhiteSpace(req.ProcedimentoInteresseTexto))
            return Result.Falha<IntakeResultado>("Informe o procedimento de interesse.");

        // Valida o procedimento de interesse (quando escolhido do catálogo).
        if (req.ProcedimentoInteresseId is { } procId)
        {
            var existe = await _db.Procedimentos.AnyAsync(p => p.Id == procId && p.ClinicaId == link.ClinicaId && p.Ativo, ct);
            if (!existe) return Result.Falha<IntakeResultado>("Procedimento de interesse inválido.");
        }

        var cpf = Cpf.Normalizar(req.Paciente.Cpf);
        if (cpf is not null && !Cpf.TamanhoValido(cpf))
            return Result.Falha<IntakeResultado>("CPF deve ter 11 dígitos.");

        // Resolve o paciente: link já vinculado > CPF existente > novo (auto-cadastro).
        Paciente? paciente = null;
        if (link.PacienteId is { } pid)
            paciente = await _db.Pacientes.FirstOrDefaultAsync(p => p.Id == pid, ct);
        if (paciente is null && cpf is not null)
            paciente = await _db.Pacientes.FirstOrDefaultAsync(p => p.ClinicaId == link.ClinicaId && p.Cpf == cpf, ct);

        var p = req.Paciente;
        if (paciente is null)
        {
            paciente = new Paciente
            {
                ClinicaId = link.ClinicaId,
                Nome = p.Nome.Trim(),
                Cpf = cpf,
                CpfPendente = cpf is null,
                Telefone = p.Telefone?.Trim(),
                DataNascimento = p.DataNascimento,
                Sexo = p.Sexo,
                Endereco = p.Endereco,
                RedeSocial = p.RedeSocial,
                Origem = OrigemPaciente.AutoCadastroLink
            };
            _db.Pacientes.Add(paciente);
        }
        else
        {
            // Atualiza dados informados no preenchimento.
            paciente.Nome = p.Nome.Trim();
            if (cpf is not null) { paciente.Cpf = cpf; paciente.CpfPendente = false; }
            paciente.Telefone = p.Telefone?.Trim() ?? paciente.Telefone;
            paciente.DataNascimento = p.DataNascimento ?? paciente.DataNascimento;
            paciente.Sexo = p.Sexo ?? paciente.Sexo;
            paciente.Endereco = p.Endereco ?? paciente.Endereco;
            paciente.RedeSocial = p.RedeSocial ?? paciente.RedeSocial;
            paciente.AtualizadoEm = _clock.UtcNow;
        }

        var anamnese = new DomainAnamnese
        {
            ClinicaId = link.ClinicaId,
            PacienteId = paciente.Id,
            DataPreenchimento = _clock.UtcNow,
            Peso = req.Anamnese.Peso,
            Altura = req.Anamnese.Altura,
            MotivoEstetico = req.Anamnese.MotivoEstetico,
            AceiteDeclaracao = true,
            DataHoraAceite = _clock.UtcNow,
            Respostas = req.Anamnese.Respostas ?? new RespostasAnamnese()
        };
        anamnese.RecalcularImc();
        _db.Anamneses.Add(anamnese);

        var solicitacao = new SolicitacaoAtendimento
        {
            ClinicaId = link.ClinicaId,
            PacienteId = paciente.Id,
            AnamneseId = anamnese.Id,
            ProcedimentoInteresseId = req.ProcedimentoInteresseId,
            ProcedimentoInteresseTexto = req.ProcedimentoInteresseTexto?.Trim(),
            DataEnvio = _clock.UtcNow,
            Status = StatusSolicitacao.Recebida
        };
        _db.Solicitacoes.Add(solicitacao);

        // Queima o token (uso único) e vincula ao paciente.
        link.Usado = true;
        link.UsadoEm = _clock.UtcNow;
        link.PacienteId = paciente.Id;

        await _db.SaveChangesAsync(ct);

        return Result.Ok(new IntakeResultado(
            solicitacao.Id, paciente.Id, anamnese.Imc,
            "Recebemos seus dados. Em breve a clínica entrará em contato para confirmar o atendimento."));
    }
}
