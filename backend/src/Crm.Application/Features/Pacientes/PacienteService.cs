using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Anamnese;
using Crm.Domain.Entities;
using Crm.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Pacientes;

public class PacienteService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IClock _clock;

    public PacienteService(IAppDbContext db, ICurrentUser user, IClock clock)
    {
        _db = db;
        _user = user;
        _clock = clock;
    }

    public async Task<Result<PacienteDto>> CriarAsync(CriarPacienteRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return Result.Falha<PacienteDto>("Nome é obrigatório.");

        var cpf = Cpf.Normalizar(req.Cpf);
        if (cpf is not null && !Cpf.Valido(cpf))
            return Result.Falha<PacienteDto>("CPF inválido (verifique os dígitos).");

        if (cpf is not null && await _db.Pacientes.AnyAsync(p => p.ClinicaId == _user.ClinicaId && p.Cpf == cpf, ct))
            return Result.Falha<PacienteDto>("Já existe um paciente com este CPF.", ErrorType.Conflict);

        var paciente = new Paciente
        {
            ClinicaId = _user.ClinicaId!.Value,
            Nome = req.Nome.Trim(),
            Cpf = cpf,
            CpfPendente = cpf is null,
            Telefone = req.Telefone?.Trim(),
            DataNascimento = req.DataNascimento,
            Sexo = req.Sexo,
            Endereco = req.Endereco,
            RedeSocial = req.RedeSocial,
            Origem = OrigemPaciente.Recepcao
        };
        _db.Pacientes.Add(paciente);
        await _db.SaveChangesAsync(ct);
        return Result.Ok(Map(paciente));
    }

    public async Task<Result<PacienteDto>> AtualizarAsync(Guid id, AtualizarPacienteRequest req, CancellationToken ct = default)
    {
        var paciente = await _db.Pacientes.FirstOrDefaultAsync(p => p.Id == id && p.ClinicaId == _user.ClinicaId, ct);
        if (paciente is null) return Result.Falha<PacienteDto>("Paciente não encontrado.", ErrorType.NotFound);
        if (string.IsNullOrWhiteSpace(req.Nome)) return Result.Falha<PacienteDto>("Nome é obrigatório.");

        var cpf = Cpf.Normalizar(req.Cpf);
        if (cpf is not null && !Cpf.Valido(cpf))
            return Result.Falha<PacienteDto>("CPF inválido (verifique os dígitos).");
        if (cpf is not null && await _db.Pacientes.AnyAsync(p => p.ClinicaId == _user.ClinicaId && p.Cpf == cpf && p.Id != id, ct))
            return Result.Falha<PacienteDto>("Já existe outro paciente com este CPF.", ErrorType.Conflict);

        paciente.Nome = req.Nome.Trim();
        paciente.Cpf = cpf;
        paciente.CpfPendente = cpf is null;
        paciente.Telefone = req.Telefone?.Trim();
        paciente.DataNascimento = req.DataNascimento;
        paciente.Sexo = req.Sexo;
        paciente.Endereco = req.Endereco;
        paciente.RedeSocial = req.RedeSocial;
        paciente.AtualizadoEm = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Result.Ok(Map(paciente));
    }

    public async Task<Result<PacienteDto>> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _db.Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.ClinicaId == _user.ClinicaId, ct);
        return p is null
            ? Result.Falha<PacienteDto>("Paciente não encontrado.", ErrorType.NotFound)
            : Result.Ok(Map(p));
    }

    public async Task<Result<List<PacienteListItem>>> BuscarAsync(string? termo, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _db.Pacientes.AsNoTracking().Where(p => p.ClinicaId == _user.ClinicaId);

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var t = termo.Trim();
            var tl = t.ToLower();
            var digitos = Cpf.Normalizar(t);
            query = query.Where(p =>
                p.Nome.ToLower().Contains(tl)
                || (digitos != null && p.Cpf != null && p.Cpf.Contains(digitos))
                || (p.Telefone != null && p.Telefone.Contains(t)));
        }

        var itens = await query
            .OrderBy(p => p.Nome)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PacienteListItem(p.Id, p.Nome, p.Cpf, p.Telefone, p.CpfPendente))
            .ToListAsync(ct);

        return Result.Ok(itens);
    }

    /// <summary>Histórico de anamneses da paciente, mais recente primeiro (dado clínico sensível — controlar acesso no controller).</summary>
    public async Task<Result<List<AnamneseDetalhe>>> ListarAnamnesesAsync(Guid pacienteId, CancellationToken ct = default)
    {
        var existe = await _db.Pacientes.AsNoTracking()
            .AnyAsync(p => p.Id == pacienteId && p.ClinicaId == _user.ClinicaId, ct);
        if (!existe) return Result.Falha<List<AnamneseDetalhe>>("Paciente não encontrado.", ErrorType.NotFound);

        var itens = await _db.Anamneses.AsNoTracking()
            .Where(a => a.PacienteId == pacienteId && a.ClinicaId == _user.ClinicaId)
            .OrderByDescending(a => a.DataPreenchimento)
            .Select(a => new AnamneseDetalhe(a.Id, a.DataPreenchimento, a.Peso, a.Altura, a.Imc,
                a.PressaoArterial, a.MotivoEstetico, a.AceiteDeclaracao, a.Respostas))
            .ToListAsync(ct);

        return Result.Ok(itens);
    }

    /// <summary>RF08 — avisar sobre possível duplicidade por CPF ou telefone antes de criar.</summary>
    public async Task<Result<DuplicidadeResponse>> VerificarDuplicidadeAsync(string? cpf, string? telefone, CancellationToken ct = default)
    {
        var cpfNorm = Cpf.Normalizar(cpf);
        var tel = telefone?.Trim();

        var porCpf = cpfNorm is null ? new List<PacienteListItem>() : await _db.Pacientes.AsNoTracking()
            .Where(p => p.ClinicaId == _user.ClinicaId && p.Cpf == cpfNorm)
            .Select(p => new PacienteListItem(p.Id, p.Nome, p.Cpf, p.Telefone, p.CpfPendente)).ToListAsync(ct);

        var porTelefone = string.IsNullOrWhiteSpace(tel) ? new List<PacienteListItem>() : await _db.Pacientes.AsNoTracking()
            .Where(p => p.ClinicaId == _user.ClinicaId && p.Telefone == tel)
            .Select(p => new PacienteListItem(p.Id, p.Nome, p.Cpf, p.Telefone, p.CpfPendente)).ToListAsync(ct);

        return Result.Ok(new DuplicidadeResponse(porCpf, porTelefone));
    }

    private static PacienteDto Map(Paciente p) => new(
        p.Id, p.Nome, p.Cpf, p.CpfPendente, p.Telefone, p.DataNascimento,
        p.Sexo, p.Endereco, p.RedeSocial, p.Origem, p.CriadoEm);
}
