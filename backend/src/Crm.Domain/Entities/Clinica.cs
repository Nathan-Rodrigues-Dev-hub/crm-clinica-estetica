using Crm.Domain.Common;

namespace Crm.Domain.Entities;

/// <summary>Raiz do tenant. No MVP existe uma única clínica, mas o modelo já carrega ClinicaId.</summary>
public class Clinica : BaseEntity
{
    public string Nome { get; set; } = string.Empty;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
    public ICollection<Procedimento> Procedimentos { get; set; } = new List<Procedimento>();
}
