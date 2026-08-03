using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;

namespace Crm.Infrastructure.Services;

public class AuditLogger : IAuditLogger
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public AuditLogger(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task RegistrarAsync(string acao, string entidadeTipo, Guid? entidadeId, string? detalhe = null, CancellationToken ct = default)
    {
        _db.LogsAuditoria.Add(new LogAuditoria
        {
            ClinicaId = _currentUser.ClinicaId,
            UsuarioId = _currentUser.UsuarioId,
            Acao = acao,
            EntidadeTipo = entidadeTipo,
            EntidadeId = entidadeId,
            Detalhe = detalhe,
            DataHora = _clock.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }
}
