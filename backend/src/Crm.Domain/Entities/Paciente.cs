using Crm.Domain.Common;
using Crm.Domain.Enums;

namespace Crm.Domain.Entities;

/// <summary>Paciente/cliente da clínica. CPF é a chave canônica quando presente.</summary>
public class Paciente : BaseEntity
{
    public Guid ClinicaId { get; set; }
    public Clinica? Clinica { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>CPF (apenas dígitos). Único quando informado.</summary>
    public string? Cpf { get; set; }

    /// <summary>True quando o paciente ainda não informou o CPF.</summary>
    public bool CpfPendente { get; set; } = true;

    public string? Telefone { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public string? Sexo { get; set; }
    public string? Endereco { get; set; }
    public string? RedeSocial { get; set; }

    public OrigemPaciente Origem { get; set; } = OrigemPaciente.Recepcao;
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<Anamnese> Anamneses { get; set; } = new List<Anamnese>();
    public ICollection<SolicitacaoAtendimento> Solicitacoes { get; set; } = new List<SolicitacaoAtendimento>();
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    public ICollection<Midia> Midias { get; set; } = new List<Midia>();
}
