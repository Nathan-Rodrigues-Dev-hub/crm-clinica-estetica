using Crm.Api.Common;
using Crm.Application.Features.Midia;
using Crm.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

/// <summary>Mídia (fotos antes/depois) — dado clínico sensível (Admin/Profissional).</summary>
[Authorize(Roles = Roles.AdminOuProfissional)]
public class MidiasController : ApiControllerBase
{
    private readonly MidiaService _service;

    public MidiasController(MidiaService service) => _service = service;

    /// <summary>Upload de foto/vídeo do paciente (multipart/form-data).</summary>
    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Upload(
        [FromForm] Guid pacienteId,
        [FromForm] EtiquetaMidia etiqueta,
        [FromForm] Guid? procedimentoId,
        [FromForm] bool consentimentoClinico,
        [FromForm] bool consentimentoPublico,
        IFormFile arquivo,
        CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(new { erro = "Envie um arquivo." });

        var meta = new UploadMidiaRequest(pacienteId, etiqueta, procedimentoId, consentimentoClinico, consentimentoPublico);
        await using var stream = arquivo.OpenReadStream();
        var result = await _service.UploadAsync(meta, stream, arquivo.FileName, arquivo.ContentType, ct);
        return ToResponse(result, StatusCodes.Status201Created);
    }

    /// <summary>Lista a galeria de um paciente.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid pacienteId, CancellationToken ct)
        => ToResponse(await _service.ListarPorPacienteAsync(pacienteId, ct));

    /// <summary>Baixa o binário (acesso autenticado — equivalente à URL assinada).</summary>
    [HttpGet("{id:guid}/conteudo")]
    public async Task<IActionResult> Conteudo(Guid id, CancellationToken ct)
    {
        var result = await _service.AbrirConteudoAsync(id, ct);
        if (!result.Sucesso) return this.ToHttp(result);
        var a = result.Valor!;
        return File(a.Conteudo, a.ContentType, a.NomeArquivo);
    }

    /// <summary>Renomeia o arquivo (organização por nome para recepção/social media).</summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Renomear(Guid id, [FromBody] RenomearMidiaRequest req, CancellationToken ct)
        => ToResponse(await _service.RenomearAsync(id, req.NomeArquivo, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
        => ToResponse(await _service.RemoverAsync(id, ct));
}
