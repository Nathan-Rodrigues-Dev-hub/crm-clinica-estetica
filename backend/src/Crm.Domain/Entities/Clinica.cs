using Crm.Domain.Common;

namespace Crm.Domain.Entities;

/// <summary>Raiz do tenant. No MVP existe uma única clínica, mas o modelo já carrega ClinicaId.</summary>
public class Clinica : BaseEntity
{
    public string Nome { get; set; } = string.Empty;

    /// <summary>Vagas de profissional contratadas no plano — controla quantos usuários com papel Profissional podem ficar ativos.</summary>
    public int LimiteProfissionais { get; set; } = 5;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
    public ICollection<Procedimento> Procedimentos { get; set; } = new List<Procedimento>();
}
