import type { RespostasAnamnese } from '../api/types'

export interface PerguntaAnamnese {
  chave: keyof RespostasAnamnese
  label: string
  detalhe?: keyof RespostasAnamnese
  detalheLabel?: string
}

/** Perguntas Sim/Não do modelo fixo — usado no preenchimento público e no registro manual (recepção). */
export const PERGUNTAS_ANAMNESE: PerguntaAnamnese[] = [
  { chave: 'usaLentesContato', label: 'Usa lentes de contato?' },
  { chave: 'epilepsiaConvulsoes', label: 'Tem epilepsia ou convulsões?' },
  { chave: 'funcionamentoIntestinalRegular', label: 'Funcionamento intestinal é regular?' },
  { chave: 'tratamentoFacialAnterior', label: 'Já fez tratamento facial anteriormente?' },
  { chave: 'ingereAgua', label: 'Ingere água regularmente?' },
  { chave: 'ingereBebidaAlcoolica', label: 'Ingere bebida alcoólica?' },
  { chave: 'exposicaoSol', label: 'Exposição ao sol?', detalhe: 'exposicaoSolFrequencia', detalheLabel: 'Com que frequência?' },
  { chave: 'usaFiltroSolar', label: 'Usa filtro solar?' },
  { chave: 'periodoMenstrual', label: 'Está no período menstrual?' },
  { chave: 'proteseCorporalFacial', label: 'Possui prótese corporal ou facial?' },
  { chave: 'tabagismo', label: 'Tabagismo?' },
  { chave: 'alteracoesCardiacas', label: 'Tem alterações cardíacas?' },
  { chave: 'marcapasso', label: 'Usa marca-passo?' },
  { chave: 'gestante', label: 'Está gestante?', detalhe: 'semanasGestacao', detalheLabel: 'Semanas de gestação' },
  { chave: 'usaCremesLocoes', label: 'Usa cremes ou loções?', detalhe: 'cremesLocoesDetalhe', detalheLabel: 'Quais?' },
  { chave: 'praticaAtividadeFisica', label: 'Pratica atividade física?', detalhe: 'atividadeFisicaDetalhe', detalheLabel: 'Qual?' },
  { chave: 'usaAnticoncepcional', label: 'Usa anticoncepcional?', detalhe: 'anticoncepcionalQual', detalheLabel: 'Qual?' },
  { chave: 'possuiAlergia', label: 'Possui alergia?', detalhe: 'alergiaDetalhe', detalheLabel: 'A quê?' },
  { chave: 'boaAlimentacao', label: 'Considera ter boa alimentação?' },
  { chave: 'problemasDePele', label: 'Tem problemas de pele?' },
  { chave: 'emTratamentoMedico', label: 'Está em tratamento médico?' },
  { chave: 'tumorLesaoPreCancerosa', label: 'Possui tumor ou lesão pré-cancerosa?' },
]

export function SimNaoToggle({ valor, onChange }: { valor: boolean | null | undefined; onChange: (v: boolean) => void }) {
  return (
    <div className="sn-group">
      <button type="button" className={`sn-btn sn-btn-sim ${valor === true ? 'sn-on' : ''}`} onClick={() => onChange(true)}>Sim</button>
      <button type="button" className={`sn-btn sn-btn-nao ${valor === false ? 'sn-on' : ''}`} onClick={() => onChange(false)}>Não</button>
    </div>
  )
}
