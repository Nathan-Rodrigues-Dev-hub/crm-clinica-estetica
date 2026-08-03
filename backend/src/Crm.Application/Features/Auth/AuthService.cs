using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Auth;

public class AuthService
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public AuthService(IAppDbContext db, IPasswordHasher hasher, IJwtTokenService jwt)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Senha))
            return Result.Falha<LoginResponse>("Informe e-mail e senha.");

        var email = req.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);

        // Mensagem genérica para não revelar se o e-mail existe.
        if (usuario is null || !usuario.Ativo || !_hasher.Verificar(req.Senha, usuario.SenhaHash))
            return Result.Falha<LoginResponse>("Credenciais inválidas.", ErrorType.Unauthorized);

        var (token, expiraEm) = _jwt.Gerar(usuario.Id, usuario.Nome, usuario.Email, usuario.Papel, usuario.ClinicaId);

        var resposta = new LoginResponse(
            token,
            expiraEm,
            new UsuarioLogado(usuario.Id, usuario.Nome, usuario.Email, usuario.Papel, usuario.ClinicaId));

        return Result.Ok(resposta);
    }
}
