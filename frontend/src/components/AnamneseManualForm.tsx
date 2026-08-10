import { useState } from 'react'
import { Modal } from './Modal'
import { Field } from './ui'
import { PERGUNTAS_ANAMNESE, SimNaoToggle } from '../lib/anamneseQuestionario'
import { pacientesApi } from '../api/services'
import { mensagemErro } from '../api/client'
import { useToast } from './Toast'
import type { RespostasAnamnese } from '../api/types'

const HOJE = () => new Date().toISOString().slice(0, 10)

/**
 * Registro manual de anamnese: recepção transcrevendo uma ficha em papel (cadastro antigo)
 * ou fallback quando a internet cai durante o atendimento. Grava direto no paciente já
 * selecionado, sem passar pela fila de triagem.
 */
export function AnamneseManualForm({ pacienteId, aberto, onFechar, onCriada }: {
  pacienteId: string
  aberto: boolean
  onFechar: () => void
  onCriada: () => void
}) {
  const toast = useToast()
  const [data, setData] = useState(HOJE())
  const [peso, setPeso] = useState('')
  const [altura, setAltura] = useState('')
  const [pressao, setPressao] = useState('')
  const [motivo, setMotivo] = useState('')
  const [respostas, setRespostas] = useState<RespostasAnamnese>({})
  const [aceite, setAceite] = useState(false)
  const [enviando, setEnviando] = useState(false)

  const setResp = (chave: keyof RespostasAnamnese, valor: unknown) => setRespostas((r) => ({ ...r, [chave]: valor }))

  const limpar = () => {
    setData(HOJE()); setPeso(''); setAltura(''); setPressao(''); setMotivo(''); setRespostas({}); setAceite(false)
  }

  const fechar = () => { limpar(); onFechar() }

  const salvar = async () => {
    setEnviando(true)
    try {
      await pacientesApi.criarAnamneseManual(pacienteId, {
        dataPreenchimento: data ? new Date(data).toISOString() : null,
        peso: peso ? Number(peso) : null,
        altura: altura ? Number(altura) : null,
        pressaoArterial: pressao.trim() || null,
        motivoEstetico: motivo.trim() || null,
        aceiteDeclaracao: aceite,
        respostas,
      })
      toast.sucesso('Anamnese registrada.')
      limpar()
      onCriada()
    } catch (e) {
      toast.erro(mensagemErro(e))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <Modal aberto={aberto} titulo="Preencher anamnese manualmente" onFechar={fechar} largura={720}>
      <p className="muted" style={{ marginBottom: 16 }}>
        Use para transcrever uma ficha preenchida em papel (cadastro antigo) ou quando a internet cair durante o atendimento.
      </p>

      <div className="form-row">
        <Field label="Data do preenchimento" hint="A data que está na ficha em papel."><input type="date" value={data} onChange={(e) => setData(e.target.value)} /></Field>
        <Field label="Pressão arterial" hint="Aferida na clínica."><input value={pressao} onChange={(e) => setPressao(e.target.value)} placeholder="12/8" /></Field>
      </div>
      <div className="form-row">
        <Field label="Peso (kg)"><input type="number" min={1} max={500} step="0.1" value={peso} onChange={(e) => setPeso(e.target.value)} placeholder="68" /></Field>
        <Field label="Altura (m)"><input type="number" min={0.5} max={2.5} step="0.01" value={altura} onChange={(e) => setAltura(e.target.value)} placeholder="1.70" /></Field>
      </div>

      <div className="section-title">Questionário de saúde</div>
      {PERGUNTAS_ANAMNESE.map((q) => {
        const val = respostas[q.chave] as boolean | null | undefined
        return (
          <div key={q.chave} className="q-item">
            <div className="q-row">
              <span>{q.label}</span>
              <SimNaoToggle valor={val} onChange={(v) => setResp(q.chave, v)} />
            </div>
            {q.detalhe && val === true && (
              <div style={{ marginTop: 10 }}>
                <input
                  placeholder={q.detalheLabel}
                  value={(respostas[q.detalhe] as string | number | undefined) ?? ''}
                  onChange={(e) => setResp(q.detalhe!, q.detalhe === 'semanasGestacao' ? Number(e.target.value) : e.target.value)}
                  type={q.detalhe === 'semanasGestacao' ? 'number' : 'text'}
                />
              </div>
            )}
          </div>
        )
      })}

      <Field label="Motivo estético" hint="Conforme descrito na ficha.">
        <textarea value={motivo} onChange={(e) => setMotivo(e.target.value)} />
      </Field>

      <label className="checkbox" style={{ marginTop: 8 }}>
        <input type="checkbox" checked={aceite} onChange={(e) => setAceite(e.target.checked)} />
        <span>A declaração de veracidade foi assinada na ficha em papel.</span>
      </label>

      <div className="row" style={{ marginTop: 18, justifyContent: 'flex-end' }}>
        <button className="btn btn-ghost" onClick={fechar}>Cancelar</button>
        <button className="btn btn-primary" onClick={salvar} disabled={enviando}>{enviando ? 'Salvando…' : 'Salvar anamnese'}</button>
      </div>
    </Modal>
  )
}
