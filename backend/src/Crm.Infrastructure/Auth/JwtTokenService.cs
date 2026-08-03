using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Crm.Infrastructure.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly IClock _clock;

    public JwtTokenService(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public (string token, DateTime expiraEm) Gerar(Guid usuarioId, string nome, string email, Papel papel, Guid clinicaId)
    {
        var expiraEm = _clock.UtcNow.AddMinutes(_options.ExpiraMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("nome", nome),
            new(ClaimTypes.Role, papel.ToString()),
            new("clinicaId", clinicaId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: _clock.UtcNow,
            expires: expiraEm,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
