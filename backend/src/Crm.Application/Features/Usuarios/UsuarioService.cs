using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Usuarios;

public class UsuarioService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public UsuarioService(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    /// <summary>Dados do usuário autenticado (para o front montar o cabeçalho/menu).</summary>
    public async Task<Result<UsuarioDto>> MeAsync(CancellationToken ct = default)
    {
        if (_user.UsuarioId is not { } id)
            return Result.Falha<UsuarioDto>("Não autenticado.", ErrorType.Unauthorized);

        var u = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return u is null
            ? Result.Falha<UsuarioDto>("Usuário não encontrado.", ErrorType.NotFound)
            : Result.Ok(new UsuarioDto(u.Id, u.Nome, u.Email, u.Papel, u.ClinicaId));
    }

    /// <summary>Lista os profissionais ativos (para selects da agenda/triagem).</summary>
    public async Task<Result<List<ProfissionalListItem>>> ListarProfissionaisAsync(CancellationToken ct = default)
    {
        var itens = await _db.Usuarios.AsNoTracking()
            .Where(u => u.ClinicaId == _user.ClinicaId && u.Ativo && u.Papel == Papel.Profissional)
            .OrderBy(u => u.Nome)
            .Select(u => new ProfissionalListItem(u.Id, u.Nome))
            .ToListAsync(ct);

        return Result.Ok(itens);
    }
}
