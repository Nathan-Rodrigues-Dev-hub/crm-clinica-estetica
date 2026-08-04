using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Crm.Tests.Infra;
using FluentAssertions;
using Xunit;

namespace Crm.Tests.Integration;

[Collection("Integração")]
public class FluxoIntegracaoTests
{
    private readonly CustomWebApplicationFactory _factory;

    public FluxoIntegracaoTests(CustomWebApplicationFactory factory) => _factory = factory;

    private static readonly JsonSerializerOptions J = ApiHelpers.Json;

    // ----------------------------------------------------------------- helpers

    private async Task<Guid> CriarProcedimentoAsync(HttpClient admin, string nome, int duracao = 60)
    {
        var resp = await admin.PostAsJsonAsync("/api/procedimentos",
            new { nome, descricao = "teste", categoria = "Facial", duracaoMinutos = duracao, valorReferencia = 200m });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(J);
        return body.GetProperty("id").GetGuid();
    }

    private async Task<string> GerarLinkAsync(HttpClient recepcao)
    {
        var resp = await recepcao.PostAsJsonAsync("/api/anamnese/links", new { telefoneDestino = "11999990000", validadeHoras = 48 });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(J);
        return body.GetProperty("token").GetString()!;
    }

    private static object IntakePayload(Guid procedimentoId, string? cpf) => new
    {
        procedimentoInteresseId = procedimentoId,
        procedimentoInteresseTexto = (string?)null,
        paciente = new { nome = "Maria Teste", cpf, telefone = "11988887777", dataNascimento = "1990-05-10", sexo = "F" },
        anamnese = new
        {
            peso = 70m,
            altura = 1.75m,
            motivoEstetico = "Melhorar a pele",
            aceiteDeclaracao = true,
            respostas = new { tabagismo = false, gestante = false, possuiAlergia = false }
        }
    };

    private async Task<Guid> ProfissionalIdAsync(HttpClient interno)
    {
        var lista = await interno.GetFromJsonAsync<JsonElement>("/api/usuarios/profissionais", J);
        return lista[0].GetProperty("id").GetGuid();
    }

    // ----------------------------------------------------------------- testes

    [Fact]
    public async Task Intake_calcula_imc_e_queima_token_uso_unico()
    {
        var admin = await _factory.ClientComoAsync(ApiHelpers.Admin);
        var recepcao = await _factory.ClientComoAsync(ApiHelpers.Recepcao);
        var publico = _factory.CreateClient();

        var procId = await CriarProcedimentoAsync(admin, "Limpeza (imc)");
        var token = await GerarLinkAsync(recepcao);

        // Abre o link (sem login) e vê o catálogo.
        var abertura = await publico.GetFromJsonAsync<JsonElement>($"/api/public/anamnese/{token}", J);
        abertura.GetProperty("valido").GetBoolean().Should().BeTrue();

        // Submete o intake.
        var post = await publico.PostAsJsonAsync($"/api/public/anamnese/{token}", IntakePayload(procId, "529.982.247-25"));
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        var res = await post.Content.ReadFromJsonAsync<JsonElement>(J);
        res.GetProperty("imc").GetDecimal().Should().Be(22.86m);

        // Reenvio com o mesmo token → 409 (uso único).
        var reenvio = await publico.PostAsJsonAsync($"/api/public/anamnese/{token}", IntakePayload(procId, "529.982.247-25"));
        reenvio.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Intake_rejeita_cpf_invalido_com_400()
    {
        var admin = await _factory.ClientComoAsync(ApiHelpers.Admin);
        var recepcao = await _factory.ClientComoAsync(ApiHelpers.Recepcao);
        var publico = _factory.CreateClient();

        var procId = await CriarProcedimentoAsync(admin, "Proc cpf");
        var token = await GerarLinkAsync(recepcao);

        var post = await publico.PostAsJsonAsync($"/api/public/anamnese/{token}", IntakePayload(procId, "529.982.247-24"));
        post.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Fluxo_intake_triagem_agendar_e_rbac()
    {
        var admin = await _factory.ClientComoAsync(ApiHelpers.Admin);
        var recepcao = await _factory.ClientComoAsync(ApiHelpers.Recepcao);
        var publico = _factory.CreateClient();

        var procId = await CriarProcedimentoAsync(admin, "Proc fluxo");
        var token = await GerarLinkAsync(recepcao);

        var post = await publico.PostAsJsonAsync($"/api/public/anamnese/{token}", IntakePayload(procId, null));
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        var res = await post.Content.ReadFromJsonAsync<JsonElement>(J);
        var solicitacaoId = res.GetProperty("solicitacaoId").GetGuid();

        // A fila é visível para a recepção...
        var fila = await recepcao.GetFromJsonAsync<JsonElement>("/api/triagem", J);
        fila.EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).Should().Contain(solicitacaoId);

        // ...mas o detalhe clínico NÃO (RBAC — recepção recebe 403).
        var detalheRecepcao = await recepcao.GetAsync($"/api/triagem/{solicitacaoId}");
        detalheRecepcao.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Admin/Profissional veem o detalhe clínico.
        var detalheAdmin = await admin.GetAsync($"/api/triagem/{solicitacaoId}");
        detalheAdmin.StatusCode.Should().Be(HttpStatusCode.OK);

        // Agenda a partir da triagem.
        var profId = await ProfissionalIdAsync(admin);
        var inicio = new DateTime(2035, 1, 10, 13, 0, 0, DateTimeKind.Utc);
        var agendarResp = await admin.PostAsJsonAsync($"/api/triagem/{solicitacaoId}/agendar",
            new { usuarioProfissionalId = profId, procedimentoId = procId, inicio, valorCobrado = 200m, observacao = (string?)null });
        agendarResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var decisao = await agendarResp.Content.ReadFromJsonAsync<JsonElement>(J);
        decisao.GetProperty("status").GetString().Should().Be("Agendada");
        decisao.GetProperty("agendamentoId").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Agenda_bloqueia_colisao_de_horario_rn03()
    {
        var admin = await _factory.ClientComoAsync(ApiHelpers.Admin);

        var procId = await CriarProcedimentoAsync(admin, "Proc colisão", duracao: 60);
        var profId = await ProfissionalIdAsync(admin);

        // Cria um paciente direto (recepção).
        var pacienteResp = await admin.PostAsJsonAsync("/api/pacientes",
            new { nome = "Paciente Colisão", cpf = (string?)null, telefone = "11970001111" });
        pacienteResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var pacienteId = (await pacienteResp.Content.ReadFromJsonAsync<JsonElement>(J)).GetProperty("id").GetGuid();

        var inicio = new DateTime(2035, 2, 20, 14, 0, 0, DateTimeKind.Utc);

        var primeiro = await admin.PostAsJsonAsync("/api/agenda",
            new { pacienteId, usuarioProfissionalId = profId, procedimentoId = procId, inicio, valorCobrado = (decimal?)null, observacao = (string?)null });
        primeiro.StatusCode.Should().Be(HttpStatusCode.Created);

        // Mesmo profissional, horário sobreposto → 409.
        var segundo = await admin.PostAsJsonAsync("/api/agenda",
            new { pacienteId, usuarioProfissionalId = profId, procedimentoId = procId, inicio = inicio.AddMinutes(30), valorCobrado = (decimal?)null, observacao = (string?)null });
        segundo.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
