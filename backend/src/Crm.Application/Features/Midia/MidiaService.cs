using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using DomainMidia = Crm.Domain.Entities.Midia;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Midia;

public class MidiaService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IFileStorage _storage;
    private readonly IClock _clock;

    public MidiaService(IAppDbContext db, ICurrentUser user, IFileStorage storage, IClock clock)
    {
        _db = db;
        _user = user;
        _storage = storage;
        _clock = clock;
    }

    public async Task<Result<MidiaDto>> UploadAsync(UploadMidiaRequest meta, Stream conteudo, string nomeArquivo, string contentType, CancellationToken ct = default)
    {
        var clinicaId = _user.ClinicaId!.Value;

        var paciente = await _db.Pacientes.AnyAsync(p => p.Id == meta.PacienteId && p.ClinicaId == clinicaId, ct);
        if (!paciente) return Result.Falha<MidiaDto>("Paciente não encontrado.", ErrorType.NotFound);

        if (meta.ProcedimentoId is { } procId &&
            !await _db.Procedimentos.AnyAsync(p => p.Id == procId && p.ClinicaId == clinicaId, ct))
            return Result.Falha<MidiaDto>("Procedimento não encontrado.", ErrorType.NotFound);

        var armazenado = await _storage.SalvarAsync(conteudo, nomeArquivo, contentType, ct);

        var midia = new DomainMidia
        {
            ClinicaId = clinicaId,
            PacienteId = meta.PacienteId,
            ProcedimentoId = meta.ProcedimentoId,
            UsuarioId = _user.UsuarioId!.Value,
            ChaveArmazenamento = armazenado.Chave,
            NomeArquivo = nomeArquivo,
            ContentType = contentType,
            TamanhoBytes = armazenado.TamanhoBytes,
            Etiqueta = meta.Etiqueta,
            Data = _clock.UtcNow,
            ConsentimentoClinico = meta.ConsentimentoClinico,
            ConsentimentoPublico = meta.ConsentimentoPublico
        };
        _db.Midias.Add(midia);
        await _db.SaveChangesAsync(ct);

        return Result.Ok(Map(midia));
    }

    public async Task<Result<List<MidiaDto>>> ListarPorPacienteAsync(Guid pacienteId, CancellationToken ct = default)
    {
        var itens = await _db.Midias.AsNoTracking()
            .Where(m => m.PacienteId == pacienteId && m.ClinicaId == _user.ClinicaId)
            .OrderByDescending(m => m.Data)
            .ToListAsync(ct);
        return Result.Ok(itens.Select(Map).ToList());
    }

    /// <summary>Abre o binário para download autenticado (equivalente dev da URL assinada).</summary>
    public async Task<Result<ArquivoConteudo>> AbrirConteudoAsync(Guid midiaId, CancellationToken ct = default)
    {
        var midia = await _db.Midias.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == midiaId && m.ClinicaId == _user.ClinicaId, ct);
        if (midia is null) return Result.Falha<ArquivoConteudo>("Mídia não encontrada.", ErrorType.NotFound);

        var arquivo = await _storage.AbrirAsync(midia.ChaveArmazenamento, ct);
        if (arquivo is null) return Result.Falha<ArquivoConteudo>("Arquivo não encontrado no storage.", ErrorType.NotFound);

        return Result.Ok(arquivo with { NomeArquivo = midia.NomeArquivo, ContentType = midia.ContentType });
    }

    public async Task<Result> RemoverAsync(Guid midiaId, CancellationToken ct = default)
    {
        var midia = await _db.Midias.FirstOrDefaultAsync(m => m.Id == midiaId && m.ClinicaId == _user.ClinicaId, ct);
        if (midia is null) return Result.Falha("Mídia não encontrada.", ErrorType.NotFound);

        await _storage.RemoverAsync(midia.ChaveArmazenamento, ct);
        _db.Midias.Remove(midia);
        await _db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private static MidiaDto Map(DomainMidia m) => new(
        m.Id, m.PacienteId, m.ProcedimentoId, m.Etiqueta, m.Data,
        m.NomeArquivo, m.ContentType, m.TamanhoBytes,
        m.ConsentimentoClinico, m.ConsentimentoPublico, $"/api/midias/{m.Id}/conteudo");
}
