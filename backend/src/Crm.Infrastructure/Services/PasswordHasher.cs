using Crm.Application.Common.Interfaces;

namespace Crm.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);

    public bool Verificar(string senha, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(senha, hash); }
        catch { return false; }
    }
}
