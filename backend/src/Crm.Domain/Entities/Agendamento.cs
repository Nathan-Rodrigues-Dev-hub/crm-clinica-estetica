using Crm.Domain.Common;
using Crm.Domain.Enums;

namespace Crm.Domain.Entities;

/// <summary>Agendamento de um procedimento para um paciente com um profissional.</summary>
public class Agendamento : BaseEntity
{
    public Guid ClinicaId { get; set; }

    public Guid PacienteId { get; set; }
    public Paciente? Paciente { get; set; }

    public Guid UsuarioProfissionalId { get; set; }
    public Usuario? UsuarioProfissional { get; set; }

    public Guid ProcedimentoId { get; set; }
    public Procedimento? Procedimento { get; set; }

    /// <summary>Origem na triagem, quando o agendamento nasceu de uma solicitação.</summary>
    public Guid? SolicitacaoAtendimentoId { get; set; }

    public DateTime Inicio { get; set; }
    /// <summary>Derivado da duração do procedimento.</summary>
    public DateTime Fim { get; set; }

    public StatusAgendamento Status { get; set; } = StatusAgendamento.Agendado;

    /// <summary>Valor efetivamente cobrado (pode diferir do valor de referência).</summary>
    public decimal? ValorCobrado { get; set; }

    public string? Observacao { get; set; }
    public string? MotivoCancelamento { get; set; }
}
