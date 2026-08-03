namespace Crm.Infrastructure.Auth;

/// <summary>Configuração do JWT (seção "Jwt" do appsettings).</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Crm.Api";
    public string Audience { get; set; } = "Crm.Client";
    public string Key { get; set; } = string.Empty;
    public int ExpiraMinutos { get; set; } = 480;
}
