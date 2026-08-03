using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Lgpd;

/// <summary>Direito ao esquecimento (LGPD): anonimiza os dados pessoais do paciente e remove mídias.</summary>
public class LgpdService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IFileStorage _storage;
    private readonly IAuditLogger _audit;
    private readonly IClock _clock;

    public LgpdService(IAppDbContext db, ICurrentUser user, IFileStorage storage, IAuditLogger audit, IClock clock)
    {
        _db = db;
        _user = user;
        _storage = storage;
        _audit = audit;
        _clock = clock;
    }

    public async Task<Result<AnonimizacaoResultado>> AnonimizarPacienteAsync(Guid pacienteId, AnonimizarRequest req, CancellationToken ct = default)
    {
        var paciente = await _db.Pacientes.FirstOrDefaultAsync(p => p.Id == pacienteId && p.ClinicaId == _user.ClinicaId, ct);
        if (paciente is null) return Result.Falha<AnonimizacaoResultado>("Paciente não encontrado.", ErrorType.NotFound);

        // Remove mídias (fotos são dados identificáveis) do storage e do banco.
        var midias = await _db.Midias.Where(m => m.PacienteId == pacienteId).ToListAsync(ct);
        foreach (var m in midias)
            await _storage.RemoverAsync(m.ChaveArmazenamento, ct);
        _db.Midias.RemoveRange(midias);

        // Limpa texto livre identificável das anamneses (mantém dados clínicos agregados).
        var anamneses = await _db.Anamneses.Where(a => a.PacienteId == pacienteId).ToListAsync(ct);
        foreach (var a in anamneses)
        {
            a.MotivoEstetico = null;
            a.PressaoArterial = null;
        }

        // Anonimiza os dados pessoais do paciente.
        paciente.Nome = "[ANONIMIZADO]";
        paciente.Cpf = null;
        paciente.CpfPendente = true;
        paciente.Telefone = null;
        paciente.Endereco = null;
        paciente.RedeSocial = null;
        paciente.Sexo = null;
        paciente.DataNascimento = null;
        paciente.AtualizadoEm = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _audit.RegistrarAsync(
            acao: "AnonimizacaoLGPD",
            entidadeTipo: nameof(Paciente),
            entidadeId: pacienteId,
            detalhe: req.Motivo,
            ct: ct);

        return Result.Ok(new AnonimizacaoResultado(pacienteId, midias.Count, anamneses.Count, _clock.UtcNow));
    }
}
