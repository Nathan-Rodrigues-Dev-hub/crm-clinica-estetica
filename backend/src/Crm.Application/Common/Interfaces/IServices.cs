using Crm.Domain.Enums;

namespace Crm.Application.Common.Interfaces;

/// <summary>Hash e verificação de senha (BCrypt na Infrastructure).</summary>
public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verificar(string senha, string hash);
}

/// <summary>Geração de token JWT por papel.</summary>
public interface IJwtTokenService
{
    (string token, DateTime expiraEm) Gerar(Guid usuarioId, string nome, string email, Papel papel, Guid clinicaId);
}

/// <summary>Gera tokens aleatórios seguros (ex.: link de anamnese, 128 bits).</summary>
public interface ITokenGenerator
{
    string GerarUrlSafe(int bytes = 16);
}

/// <summary>Usuário autenticado do request atual.</summary>
public interface ICurrentUser
{
    Guid? UsuarioId { get; }
    Guid? ClinicaId { get; }
    Papel? Papel { get; }
    bool Autenticado { get; }
}

/// <summary>Relógio (facilita testes).</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

/// <summary>Registro de trilha de auditoria (LGPD).</summary>
public interface IAuditLogger
{
    Task RegistrarAsync(string acao, string entidadeTipo, Guid? entidadeId, string? detalhe = null, CancellationToken ct = default);
}

/// <summary>Resultado de um upload de arquivo no storage.</summary>
public record ArquivoArmazenado(string Chave, long TamanhoBytes);

/// <summary>Conteúdo de um arquivo lido do storage.</summary>
public record ArquivoConteudo(Stream Conteudo, string ContentType, string NomeArquivo);

/// <summary>Armazenamento de arquivos (S3 em produção; local em dev).</summary>
public interface IFileStorage
{
    Task<ArquivoArmazenado> SalvarAsync(Stream conteudo, string nomeArquivo, string contentType, CancellationToken ct = default);
    /// <summary>Abre o arquivo para leitura/streaming (usado no endpoint autenticado de mídia em dev).</summary>
    Task<ArquivoConteudo?> AbrirAsync(string chave, CancellationToken ct = default);
    Task RemoverAsync(string chave, CancellationToken ct = default);
}
