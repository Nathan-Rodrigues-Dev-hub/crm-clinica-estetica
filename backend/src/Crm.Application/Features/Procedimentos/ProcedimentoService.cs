using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Procedimentos;

public class ProcedimentoService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public ProcedimentoService(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result<ProcedimentoDto>> CriarAsync(CriarProcedimentoRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return Result.Falha<ProcedimentoDto>("Nome é obrigatório.");
        if (req.DuracaoMinutos <= 0)
            return Result.Falha<ProcedimentoDto>("Duração deve ser maior que zero.");
        if (req.ValorReferencia < 0)
            return Result.Falha<ProcedimentoDto>("Valor de referência não pode ser negativo.");

        var proc = new Procedimento
        {
            ClinicaId = _user.ClinicaId!.Value,
            Nome = req.Nome.Trim(),
            Descricao = req.Descricao,
            Categoria = req.Categoria,
            DuracaoMinutos = req.DuracaoMinutos,
            ValorReferencia = req.ValorReferencia,
            Ativo = true
        };
        _db.Procedimentos.Add(proc);
        await _db.SaveChangesAsync(ct);
        return Result.Ok(Map(proc));
    }

    public async Task<Result<ProcedimentoDto>> AtualizarAsync(Guid id, AtualizarProcedimentoRequest req, CancellationToken ct = default)
    {
        var proc = await _db.Procedimentos.FirstOrDefaultAsync(p => p.Id == id && p.ClinicaId == _user.ClinicaId, ct);
        if (proc is null) return Result.Falha<ProcedimentoDto>("Procedimento não encontrado.", ErrorType.NotFound);
        if (string.IsNullOrWhiteSpace(req.Nome)) return Result.Falha<ProcedimentoDto>("Nome é obrigatório.");
        if (req.DuracaoMinutos <= 0) return Result.Falha<ProcedimentoDto>("Duração deve ser maior que zero.");

        proc.Nome = req.Nome.Trim();
        proc.Descricao = req.Descricao;
        proc.Categoria = req.Categoria;
        proc.DuracaoMinutos = req.DuracaoMinutos;
        proc.ValorReferencia = req.ValorReferencia;
        proc.Ativo = req.Ativo;
        await _db.SaveChangesAsync(ct);
        return Result.Ok(Map(proc));
    }

    public async Task<Result<ProcedimentoDto>> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var proc = await _db.Procedimentos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicaId == _user.ClinicaId, ct);
        return proc is null
            ? Result.Falha<ProcedimentoDto>("Procedimento não encontrado.", ErrorType.NotFound)
            : Result.Ok(Map(proc));
    }

    public async Task<Result<List<ProcedimentoDto>>> ListarAsync(bool incluirInativos, CancellationToken ct = default)
    {
        var query = _db.Procedimentos.AsNoTracking().Where(p => p.ClinicaId == _user.ClinicaId);
        if (!incluirInativos) query = query.Where(p => p.Ativo);
        var itens = await query.OrderBy(p => p.Nome).Select(p => Map(p)).ToListAsync(ct);
        return Result.Ok(itens);
    }

    private static ProcedimentoDto Map(Procedimento p) =>
        new(p.Id, p.Nome, p.Descricao, p.Categoria, p.DuracaoMinutos, p.ValorReferencia, p.Ativo);
}
