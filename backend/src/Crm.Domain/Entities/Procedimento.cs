using Crm.Domain.Common;

namespace Crm.Domain.Entities;

/// <summary>Item do catálogo de procedimentos.</summary>
public class Procedimento : BaseEntity
{
    public Guid ClinicaId { get; set; }
    public Clinica? Clinica { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Categoria { get; set; }

    /// <summary>Duração padrão que dimensiona o slot da agenda.</summary>
    public int DuracaoMinutos { get; set; }

    /// <summary>Valor de referência (editável no atendimento).</summary>
    public decimal ValorReferencia { get; set; }

    public bool Ativo { get; set; } = true;
}
