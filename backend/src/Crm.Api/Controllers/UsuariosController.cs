using Crm.Api.Common;
using Crm.Application.Features.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize]
public class UsuariosController : ApiControllerBase
{
    private readonly UsuarioService _service;

    public UsuariosController(UsuarioService service) => _service = service;

    /// <summary>Lista os profissionais (para selects da agenda/triagem).</summary>
    [Authorize(Roles = Roles.Interno)]
    [HttpGet("profissionais")]
    public async Task<IActionResult> Profissionais(CancellationToken ct)
        => ToResponse(await _service.ListarProfissionaisAsync(ct));

    /// <summary>Troca a própria senha (qualquer papel autenticado).</summary>
    [HttpPut("me/senha")]
    public async Task<IActionResult> TrocarMinhaSenha([FromBody] AlterarSenhaRequest req, CancellationToken ct)
        => ToResponse(await _service.AlterarMinhaSenhaAsync(req, ct));

    // ---------------------------------------------------------- gestão de equipe (Admin / SuperAdmin)

    /// <summary>Lista os membros da equipe (Admin/SuperAdmin).</summary>
    [Authorize(Roles = Roles.AdminOuSuperAdmin)]
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInativos = false, CancellationToken ct = default)
        => ToResponse(await _service.ListarEquipeAsync(incluirInativos, ct));

    /// <summary>Vagas de profissional do plano x quantas já estão em uso (Admin/SuperAdmin).</summary>
    [Authorize(Roles = Roles.AdminOuSuperAdmin)]
    [HttpGet("limite-profissionais")]
    public async Task<IActionResult> LimiteProfissionais(CancellationToken ct)
        => ToResponse(await _service.ObterLimiteProfissionaisAsync(ct));

    /// <summary>Cria um membro da equipe com senha inicial. Contas Profissional só via SuperAdmin.</summary>
    [Authorize(Roles = Roles.AdminOuSuperAdmin)]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioRequest req, CancellationToken ct)
        => ToResponse(await _service.CriarAsync(req, ct), StatusCodes.Status201Created);

    /// <summary>Edita nome/papel/status. Dados de conta Profissional (nome, reativação, papel) e redefinir senha de terceiros só via SuperAdmin.</summary>
    [Authorize(Roles = Roles.AdminOuSuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarUsuarioRequest req, CancellationToken ct)
        => ToResponse(await _service.AtualizarAsync(id, req, ct));

    /// <summary>Desativa (soft) um membro da equipe, inclusive Profissional (Admin/SuperAdmin).</summary>
    [Authorize(Roles = Roles.AdminOuSuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
        => ToResponse(await _service.DesativarAsync(id, ct));
}
