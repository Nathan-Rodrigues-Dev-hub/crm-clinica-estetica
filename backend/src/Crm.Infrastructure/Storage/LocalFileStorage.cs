using Crm.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Crm.Infrastructure.Storage;

/// <summary>
/// Armazenamento local para desenvolvimento (salva em disco).
/// Em produção, trocar por implementação S3 (sa-east-1) com URLs assinadas — ver README-backend.md.
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _baseDir;

    public LocalFileStorage(IConfiguration config)
    {
        _baseDir = config["Storage:LocalPath"]
                   ?? Path.Combine(AppContext.BaseDirectory, "uploads");
        Directory.CreateDirectory(_baseDir);
    }

    public async Task<ArquivoArmazenado> SalvarAsync(Stream conteudo, string nomeArquivo, string contentType, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(nomeArquivo);
        var subpasta = DateTime.UtcNow.ToString("yyyy/MM");
        var chave = $"{subpasta}/{Guid.NewGuid():N}{ext}";

        var destino = Path.Combine(_baseDir, chave.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(destino)!);

        await using var fs = new FileStream(destino, FileMode.CreateNew, FileAccess.Write);
        await conteudo.CopyToAsync(fs, ct);

        return new ArquivoArmazenado(chave, fs.Length);
    }

    public Task<ArquivoConteudo?> AbrirAsync(string chave, CancellationToken ct = default)
    {
        var caminho = Path.Combine(_baseDir, chave.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(caminho))
            return Task.FromResult<ArquivoConteudo?>(null);

        Stream stream = new FileStream(caminho, FileMode.Open, FileAccess.Read);
        var contentType = ContentTypePorExtensao(Path.GetExtension(caminho));
        return Task.FromResult<ArquivoConteudo?>(new ArquivoConteudo(stream, contentType, Path.GetFileName(caminho)));
    }

    public Task RemoverAsync(string chave, CancellationToken ct = default)
    {
        var caminho = Path.Combine(_baseDir, chave.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(caminho)) File.Delete(caminho);
        return Task.CompletedTask;
    }

    private static string ContentTypePorExtensao(string ext) => ext.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        ".mp4" => "video/mp4",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream"
    };
}
