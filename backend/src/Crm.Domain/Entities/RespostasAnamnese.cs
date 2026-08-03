namespace Crm.Domain.Entities;

/// <summary>
/// Respostas do modelo fixo de anamnese (Sim/Não com detalhe onde indicado).
/// Persistido como coluna JSON (owned type) dentro de Anamnese.
/// </summary>
public class RespostasAnamnese
{
    public bool? UsaLentesContato { get; set; }
    public bool? EpilepsiaConvulsoes { get; set; }
    public bool? FuncionamentoIntestinalRegular { get; set; }
    public bool? TratamentoFacialAnterior { get; set; }
    public bool? IngereAgua { get; set; }
    public bool? IngereBebidaAlcoolica { get; set; }

    public bool? ExposicaoSol { get; set; }
    public string? ExposicaoSolFrequencia { get; set; }

    public bool? UsaFiltroSolar { get; set; }
    public bool? PeriodoMenstrual { get; set; }

    public string? QualidadeSono { get; set; }
    public int? HorasSono { get; set; }

    public bool? ProteseCorporalFacial { get; set; }
    public bool? Tabagismo { get; set; }
    public bool? AlteracoesCardiacas { get; set; }
    public bool? Marcapasso { get; set; }

    public bool? Gestante { get; set; }
    public int? SemanasGestacao { get; set; }

    public bool? UsaCremesLocoes { get; set; }
    public string? CremesLocoesDetalhe { get; set; }

    public bool? PraticaAtividadeFisica { get; set; }
    public string? AtividadeFisicaDetalhe { get; set; }

    public bool? UsaAnticoncepcional { get; set; }
    public string? AnticoncepcionalQual { get; set; }

    public bool? PossuiAlergia { get; set; }
    public string? AlergiaDetalhe { get; set; }

    public bool? BoaAlimentacao { get; set; }
    public bool? ProblemasDePele { get; set; }
    public bool? EmTratamentoMedico { get; set; }
    public bool? TumorLesaoPreCancerosa { get; set; }
}
