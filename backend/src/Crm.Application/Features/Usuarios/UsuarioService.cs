using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Crm.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Usuarios;

public class UsuarioService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditLogger _audit;

    public UsuarioService(IAppDbContext db, ICurrentUser user, IPasswordHasher hasher, IAuditLogger audit)
    {
        _db = db;
        _user = user;
        _hasher = hasher;
        _audit = audit;
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

    // ---------------------------------------------------------- gestão de equipe (Admin)

    /// <summary>Lista os membros da equipe da clínica (Admin).</summary>
    public async Task<Result<List<EquipeUsuarioDto>>> ListarEquipeAsync(bool incluirInativos, CancellationToken ct = default)
    {
        var query = _db.Usuarios.AsNoTracking().Where(u => u.ClinicaId == _user.ClinicaId);
        if (!incluirInativos) query = query.Where(u => u.Ativo);

        var itens = await query
            .OrderBy(u => u.Nome)
            .Select(u => new EquipeUsuarioDto(u.Id, u.Nome, u.Email, u.Papel, u.Ativo))
            .ToListAsync(ct);

        return Result.Ok(itens);
    }

    /// <summary>Cria um membro da equipe com senha inicial definida pelo Admin.</summary>
    public async Task<Result<EquipeUsuarioDto>> CriarAsync(CriarUsuarioRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return Result.Falha<EquipeUsuarioDto>("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(req.Senha) || req.Senha.Length < 6)
            return Result.Falha<EquipeUsuarioDto>("A senha inicial deve ter ao menos 6 caracteres.");

        var erroEmail = NormalizarEmail(req.Email, out var email);
        if (erroEmail is not null) return Result.Falha<EquipeUsuarioDto>(erroEmail);

        if (await _db.Usuarios.AnyAsync(u => u.Email == email, ct))
            return Result.Falha<EquipeUsuarioDto>("Já existe um usuário com este e-mail.", ErrorType.Conflict);

        var usuario = new Usuario
        {
            ClinicaId = _user.ClinicaId!.Value,
            Nome = req.Nome.Trim(),
            Email = email,
            SenhaHash = _hasher.Hash(req.Senha),
            Papel = req.Papel,
            Ativo = true
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync(ct);
        await _audit.RegistrarAsync("UsuarioCriado", "Usuario", usuario.Id, $"papel={usuario.Papel}", ct);

        return Result.Ok(Map(usuario));
    }

    /// <summary>Edita nome, papel e status; opcionalmente redefine a senha.</summary>
    public async Task<Result<EquipeUsuarioDto>> AtualizarAsync(Guid id, AtualizarUsuarioRequest req, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.ClinicaId == _user.ClinicaId, ct);
        if (usuario is null) return Result.Falha<EquipeUsuarioDto>("Usuário não encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(req.Nome))
            return Result.Falha<EquipeUsuarioDto>("Nome é obrigatório.");

        var mexendoEmSiMesmo = usuario.Id == _user.UsuarioId;
        // Evita que o Admin se tranque para fora: não pode se rebaixar nem se desativar.
        if (mexendoEmSiMesmo && (req.Papel != Papel.Admin || !req.Ativo))
            return Result.Falha<EquipeUsuarioDto>("Você não pode alterar o próprio papel de Admin nem se desativar.", ErrorType.Forbidden);

        if (!string.IsNullOrEmpty(req.NovaSenha) && req.NovaSenha.Length < 6)
            return Result.Falha<EquipeUsuarioDto>("A nova senha deve ter ao menos 6 caracteres.");

        usuario.Nome = req.Nome.Trim();
        usuario.Papel = req.Papel;
        usuario.Ativo = req.Ativo;
        if (!string.IsNullOrEmpty(req.NovaSenha))
            usuario.SenhaHash = _hasher.Hash(req.NovaSenha);

        await _db.SaveChangesAsync(ct);
        await _audit.RegistrarAsync("UsuarioAtualizado", "Usuario", usuario.Id, $"papel={usuario.Papel};ativo={usuario.Ativo}", ct);

        return Result.Ok(Map(usuario));
    }

    /// <summary>Desativa (soft) um membro da equipe. Não permite desativar a si mesmo.</summary>
    public async Task<Result> DesativarAsync(Guid id, CancellationToken ct = default)
    {
        if (id == _user.UsuarioId)
            return Result.Falha("Você não pode desativar o próprio usuário.", ErrorType.Forbidden);

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.ClinicaId == _user.ClinicaId, ct);
        if (usuario is null) return Result.Falha("Usuário não encontrado.", ErrorType.NotFound);

        if (!usuario.Ativo) return Result.Ok();

        usuario.Ativo = false;
        await _db.SaveChangesAsync(ct);
        await _audit.RegistrarAsync("UsuarioDesativado", "Usuario", usuario.Id, ct: ct);

        return Result.Ok();
    }

    private static string? NormalizarEmail(string? bruto, out string email)
    {
        email = (bruto ?? string.Empty).Trim().ToLowerInvariant();
        if (email.Length == 0) return "E-mail é obrigatório.";
        // Validação simples: presença de "@" e "." — o formato completo é responsabilidade do validator.
        if (!email.Contains('@') || !email.Contains('.')) return "E-mail inválido.";
        return null;
    }

    private static EquipeUsuarioDto Map(Usuario u) => new(u.Id, u.Nome, u.Email, u.Papel, u.Ativo);
}
