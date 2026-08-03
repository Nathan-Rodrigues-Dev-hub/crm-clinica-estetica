using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Enums;

namespace Crm.Api.Common;

/// <summary>Lê o usuário autenticado a partir das claims do JWT no request atual.</summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public bool Autenticado => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UsuarioId
    {
        get
        {
            var v = User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(v, out var id) ? id : null;
        }
    }

    public Guid? ClinicaId
    {
        get
        {
            var v = User?.FindFirstValue("clinicaId");
            return Guid.TryParse(v, out var id) ? id : null;
        }
    }

    public Papel? Papel
    {
        get
        {
            var v = User?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<Papel>(v, out var p) ? p : null;
        }
    }
}
