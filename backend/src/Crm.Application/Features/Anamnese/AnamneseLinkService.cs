using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Crm.Application.Features.Anamnese;

/// <summary>Geração do link tokenizado de anamnese (uso interno da clínica).</summary>
public class AnamneseLinkService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;
    private readonly ITokenGenerator _tokens;
    private readonly IClock _clock;
    private readonly IConfiguration _config;

    public AnamneseLinkService(IAppDbContext db, ICurrentUser user, ITokenGenerator tokens, IClock clock, IConfiguration config)
    {
        _db = db;
        _user = user;
        _tokens = tokens;
        _clock = clock;
        _config = config;
    }

    public async Task<Result<LinkGeradoDto>> GerarAsync(GerarLinkRequest req, CancellationToken ct = default)
    {
        var validadeHoras = req.ValidadeHoras is > 0 ? req.ValidadeHoras.Value : 168; // 7 dias
        var expiracao = _clock.UtcNow.AddHours(validadeHoras);

        var link = new AnamneseLink
        {
            ClinicaId = _user.ClinicaId!.Value,
            Token = _tokens.GerarUrlSafe(16), // 128 bits
            TelefoneDestino = req.TelefoneDestino?.Trim(),
            Expiracao = expiracao,
            Usado = false
        };
        _db.AnamneseLinks.Add(link);
        await _db.SaveChangesAsync(ct);

        var baseUrl = _config["PublicApp:AnamneseUrlBase"]?.TrimEnd('/') ?? "http://localhost:5173/anamnese";
        var url = $"{baseUrl}/{link.Token}";

        return Result.Ok(new LinkGeradoDto(link.Token, url, link.Expiracao));
    }
}
