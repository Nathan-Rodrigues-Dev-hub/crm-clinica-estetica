using Crm.Domain.Common;

namespace Crm.Domain.Entities;

/// <summary>
/// Link tokenizado de anamnese, emitido pela clínica (nunca auto-gerado pelo público).
/// Sem login. Token de 128 bits, expira e é de uso único.
/// </summary>
public class AnamneseLink : BaseEntity
{
    public Guid ClinicaId { get; set; }

    /// <summary>Token aleatório (128 bits, url-safe). Não sequencial.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Telefone/WhatsApp para o qual a clínica enviou o link.</summary>
    public string? TelefoneDestino { get; set; }

    /// <summary>Preenchido só depois que a paciente conclui o auto-cadastro.</summary>
    public Guid? PacienteId { get; set; }
    public Paciente? Paciente { get; set; }

    public DateTime Expiracao { get; set; }
    public bool Usado { get; set; }
    public DateTime? UsadoEm { get; set; }

    public bool EstaValido(DateTime agoraUtc) => !Usado && Expiracao > agoraUtc;
}
