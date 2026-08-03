using Crm.Domain.Common;
using Crm.Domain.Enums;

namespace Crm.Domain.Entities;

/// <summary>Membro da equipe (Admin / Profissional / Recepção). Autentica por e-mail + senha.</summary>
public class Usuario : BaseEntity
{
    public Guid ClinicaId { get; set; }
    public Clinica? Clinica { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public Papel Papel { get; set; }
    public bool Ativo { get; set; } = true;
}
