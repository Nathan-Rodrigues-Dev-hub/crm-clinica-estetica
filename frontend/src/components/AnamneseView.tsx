import type { AnamneseDetalhe, RespostasAnamnese } from '../api/types'
import { formatarDataHora } from '../lib/format'

type Campo = { chave: keyof RespostasAnamnese; label: string; detalhe?: keyof RespostasAnamnese }

const CAMPOS: Campo[] = [
  { chave: 'usaLentesContato', label: 'Usa lentes de contato' },
  { chave: 'epilepsiaConvulsoes', label: 'Epilepsia / convulsões' },
  { chave: 'funcionamentoIntestinalRegular', label: 'Funcionamento intestinal regular' },
  { chave: 'tratamentoFacialAnterior', label: 'Tratamento facial anterior' },
  { chave: 'ingereAgua', label: 'Ingere água regularmente' },
  { chave: 'ingereBebidaAlcoolica', label: 'Ingere bebida alcoólica' },
  { chave: 'exposicaoSol', label: 'Exposição ao sol', detalhe: 'exposicaoSolFrequencia' },
  { chave: 'usaFiltroSolar', label: 'Usa filtro solar' },
  { chave: 'periodoMenstrual', label: 'Período menstrual' },
  { chave: 'proteseCorporalFacial', label: 'Prótese corporal / facial' },
  { chave: 'tabagismo', label: 'Tabagismo' },
  { chave: 'alteracoesCardiacas', label: 'Alterações cardíacas' },
  { chave: 'marcapasso', label: 'Marca-passo' },
  { chave: 'gestante', label: 'Gestante', detalhe: 'semanasGestacao' },
  { chave: 'usaCremesLocoes', label: 'Usa cremes / loções', detalhe: 'cremesLocoesDetalhe' },
  { chave: 'praticaAtividadeFisica', label: 'Pratica atividade física', detalhe: 'atividadeFisicaDetalhe' },
  { chave: 'usaAnticoncepcional', label: 'Usa anticoncepcional', detalhe: 'anticoncepcionalQual' },
  { chave: 'possuiAlergia', label: 'Possui alergia', detalhe: 'alergiaDetalhe' },
  { chave: 'boaAlimentacao', label: 'Boa alimentação' },
  { chave: 'problemasDePele', label: 'Problemas de pele' },
  { chave: 'emTratamentoMedico', label: 'Em tratamento médico' },
  { chave: 'tumorLesaoPreCancerosa', label: 'Tumor / lesão pré-cancerosa' },
]

function SimNao({ v }: { v: boolean | null | undefined }) {
  if (v === true) return <span className="badge badge-alerta">Sim</span>
  if (v === false) return <span className="badge badge-sucesso">Não</span>
  return <span className="badge badge-neutro">—</span>
}

export function AnamneseView({ a }: { a: AnamneseDetalhe }) {
  const r = a.respostas
  return (
    <div>
      <div className="grid grid-4" style={{ marginBottom: 18 }}>
        <div className="stat"><div className="stat-label">Peso</div><strong>{a.peso ? `${a.peso} kg` : '—'}</strong></div>
        <div className="stat"><div className="stat-label">Altura</div><strong>{a.altura ? `${a.altura} m` : '—'}</strong></div>
        <div className="stat"><div className="stat-label">IMC</div><strong style={{ color: 'var(--rose-dark)' }}>{a.imc ?? '—'}</strong></div>
        <div className="stat"><div className="stat-label">Pressão arterial</div><strong>{a.pressaoArterial ?? 'Aferir na clínica'}</strong></div>
      </div>

      {a.motivoEstetico && (
        <div style={{ marginBottom: 14 }}>
          <div className="stat-label">Motivo estético</div>
          <p style={{ margin: '4px 0 0' }}>{a.motivoEstetico}</p>
        </div>
      )}

      <div className="section-title">Questionário</div>
      <div className="grid grid-2" style={{ gap: '2px 24px' }}>
        {CAMPOS.map((c) => {
          const detalhe = c.detalhe ? r[c.detalhe] : undefined
          return (
            <div key={c.chave} className="row spread" style={{ padding: '8px 0', borderBottom: '1px solid var(--line)' }}>
              <span>{c.label}{detalhe ? <em className="muted"> — {String(detalhe)}</em> : null}</span>
              <SimNao v={r[c.chave] as boolean | null | undefined} />
            </div>
          )
        })}
        {(r.qualidadeSono || r.horasSono != null) && (
          <div className="row spread" style={{ padding: '8px 0', borderBottom: '1px solid var(--line)' }}>
            <span>Qualidade do sono</span>
            <span className="badge badge-neutro">{r.qualidadeSono ?? '—'}{r.horasSono != null ? ` · ${r.horasSono}h` : ''}</span>
          </div>
        )}
      </div>

      <p className="muted" style={{ marginTop: 16, fontSize: 13 }}>
        {a.aceiteDeclaracao ? '✓ Declaração de veracidade aceita' : 'Declaração não aceita'} · Preenchida em {formatarDataHora(a.dataPreenchimento)}
      </p>
    </div>
  )
}
