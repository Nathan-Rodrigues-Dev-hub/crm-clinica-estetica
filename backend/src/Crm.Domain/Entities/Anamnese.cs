using Crm.Domain.Common;

namespace Crm.Domain.Entities;

/// <summary>
/// Preenchimento de anamnese (histórico datado — nunca sobrescrito).
/// Peso/Altura geram o IMC automaticamente; a pressão é aferida na clínica.
/// </summary>
public class Anamnese : BaseEntity
{
    public Guid ClinicaId { get; set; }

    public Guid PacienteId { get; set; }
    public Paciente? Paciente { get; set; }

    public DateTime DataPreenchimento { get; set; } = DateTime.UtcNow;

    /// <summary>Peso em quilogramas.</summary>
    public decimal? Peso { get; set; }
    /// <summary>Altura em metros.</summary>
    public decimal? Altura { get; set; }
    /// <summary>Índice de Massa Corporal, calculado a partir de Peso/Altura.</summary>
    public decimal? Imc { get; private set; }

    /// <summary>Pressão arterial aferida na clínica (não no preenchimento remoto).</summary>
    public string? PressaoArterial { get; set; }

    public string? MotivoEstetico { get; set; }

    public bool AceiteDeclaracao { get; set; }
    public DateTime? DataHoraAceite { get; set; }

    public RespostasAnamnese Respostas { get; set; } = new();

    /// <summary>RN02 — IMC = Peso(kg) / (Altura(m))². Recalcula e persiste em Imc.</summary>
    public void RecalcularImc()
    {
        if (Peso is > 0 && Altura is > 0)
            Imc = Math.Round(Peso.Value / (Altura.Value * Altura.Value), 2, MidpointRounding.AwayFromZero);
        else
            Imc = null;
    }
}
