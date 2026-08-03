using System.Security.Cryptography;
using Crm.Application.Common.Interfaces;

namespace Crm.Infrastructure.Services;

public class TokenGenerator : ITokenGenerator
{
    /// <summary>Token aleatório url-safe. Padrão 16 bytes = 128 bits.</summary>
    public string GerarUrlSafe(int bytes = 16)
    {
        var buffer = RandomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(buffer)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
