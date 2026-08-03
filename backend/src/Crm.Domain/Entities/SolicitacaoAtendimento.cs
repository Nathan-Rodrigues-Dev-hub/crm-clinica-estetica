using Crm.Domain.Common;
using Crm.Domain.Enums;

namespace Crm.Domain.Entities;

/// <summary>
/// Solicitação de atendimento gerada quando a paciente conclui o link (intake).
/// Entra na fila de triagem; a decisão a agenda ou recusa (com motivo).
/// </summary>
public class SolicitacaoAtendimento : BaseEntity
{
    public Guid ClinicaId { get; set; }

    public Guid PacienteId { get; set; }
    public Paciente? Paciente { get; set; }

    public Guid AnamneseId { get; set; }
    public Anamnese? Anamnese { get; set; }

    /// <summary>Procedimento de interesse escolhido no catálogo (nulo se "outro").</summary>
    public Guid? ProcedimentoInteresseId { get; set; }
    public Procedimento? ProcedimentoInteresse { get; set; }

    /// <summary>Texto livre quando a paciente escolhe "outro" procedimento.</summary>
    public string? ProcedimentoInteresseTexto { get; set; }

    public DateTime DataEnvio { get; set; } = DateTime.UtcNow;

    public StatusSolicitacao Status { get; set; } = StatusSolicitacao.Recebida;

    /// <summary>Obrigatório quando Status = Recusada.</summary>
    public string? MotivoRecusa { get; set; }

    public Guid? UsuarioTriagemId { get; set; }
    public Usuario? UsuarioTriagem { get; set; }
    public DateTime? DataDecisao { get; set; }

    /// <summary>Agendamento gerado quando a solicitação é aprovada.</summary>
    public Guid? AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }
}
