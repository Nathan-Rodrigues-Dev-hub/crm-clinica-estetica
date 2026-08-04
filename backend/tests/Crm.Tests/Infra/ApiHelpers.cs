using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crm.Tests.Infra;

/// <summary>Utilitários de request para os testes de integração.</summary>
public static class ApiHelpers
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<string> LoginAsync(HttpClient client, string email, string senha = "Admin@123")
    {
        var resp = await client.PostAsJsonAsync("/api/auth/login", new { email, senha });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Json);
        return body.GetProperty("token").GetString()!;
    }

    public static async Task<HttpClient> ClientComoAsync(this CustomWebApplicationFactory factory, string email)
    {
        var client = factory.CreateClient();
        var token = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public const string Admin = "admin@clinica.local";
    public const string Profissional = "profissional@clinica.local";
    public const string Recepcao = "recepcao@clinica.local";
}
