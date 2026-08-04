import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { procedimentosApi, triagemApi, usuariosApi } from '../api/services'
import { mensagemErro } from '../api/client'
import { useToast } from '../components/Toast'
import { Badge, Card, Field, PageHeader } from '../components/ui'
import { Modal } from '../components/Modal'
import { Spinner } from '../components/Spinner'
import { AnamneseView } from '../components/AnamneseView'
import { formatarCpf, formatarTelefone, linkWhatsApp, localParaUtcIso } from '../lib/format'
import type { DecisaoResultado, ProcedimentoDto, ProfissionalListItem, SolicitacaoDetalhe } from '../api/types'

export function TriagemDetalhePage() {
  const { id = '' } = useParams()
  const navigate = useNavigate()
  const toast = useToast()

  const [sol, setSol] = useState<SolicitacaoDetalhe | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [profs, setProfs] = useState<ProfissionalListItem[]>([])
  const [procs, setProcs] = useState<ProcedimentoDto[]>([])

  const [modalAgendar, setModalAgendar] = useState(false)
  const [modalRecusar, setModalRecusar] = useState(false)
  const [resultado, setResultado] = useState<DecisaoResultado | null>(null)

  const carregar = () => {
    setCarregando(true)
    triagemApi.detalhe(id).then((r) => setSol(r.data)).catch((e) => toast.erro(mensagemErro(e))).finally(() => setCarregando(false))
  }

  useEffect(() => {
    carregar()
    usuariosApi.profissionais().then((r) => setProfs(r.data)).catch(() => {})
    procedimentosApi.listar(false).then((r) => setProcs(r.data)).catch(() => {})
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  const assumir = async () => {
    try {
      await triagemApi.assumir(id)
      toast.sucesso('Solicitação assumida (em triagem).')
      carregar()
    } catch (e) {
      toast.erro(mensagemErro(e))
    }
  }

  if (carregando) return <Spinner full label="Carregando solicitação…" />
  if (!sol) return null

  const pendente = sol.status === 'Recebida' || sol.status === 'EmTriagem'

  return (
    <>
      <PageHeader
        titulo={sol.pacienteNome}
        subtitulo={`Interesse: ${sol.procedimentoInteresse}`}
        acao={<button className="btn btn-ghost" onClick={() => navigate('/triagem')}>← Voltar</button>}
      />

      <div className="grid grid-2">
        <div>
          <Card>
            <div className="spread" style={{ marginBottom: 12 }}>
              <h3>Dados da paciente</h3>
              <Badge texto={sol.status} tom={sol.status === 'Agendada' ? 'sucesso' : sol.status === 'Recusada' ? 'erro' : sol.status === 'EmTriagem' ? 'alerta' : 'info'} />
            </div>
            <dl className="kv">
              <dt>Nome</dt><dd>{sol.pacienteNome}</dd>
              <dt>CPF</dt><dd>{formatarCpf(sol.cpf)}</dd>
              <dt>Telefone</dt><dd>{formatarTelefone(sol.telefone)}</dd>
              <dt>Interesse</dt><dd>{sol.procedimentoInteresse}</dd>
            </dl>

            {sol.status === 'Recusada' && sol.motivoRecusa && (
              <div className="alert alert-err" style={{ marginTop: 12 }}><strong>Recusada:</strong> {sol.motivoRecusa}</div>
            )}
            {sol.status === 'Agendada' && (
              <div className="alert alert-info" style={{ marginTop: 12 }}>Solicitação agendada. Veja na <a href="/agenda">agenda</a>.</div>
            )}

            {pendente && (
              <div className="row" style={{ marginTop: 16 }}>
                {sol.status === 'Recebida' && <button className="btn btn-soft" onClick={assumir}>Assumir triagem</button>}
                <button className="btn btn-success" onClick={() => setModalAgendar(true)}>Agendar</button>
                <button className="btn btn-danger" onClick={() => setModalRecusar(true)}>Recusar</button>
              </div>
            )}
          </Card>
        </div>

        <Card>
          <h3 style={{ marginBottom: 14 }}>Anamnese</h3>
          <AnamneseView a={sol.anamnese} />
        </Card>
      </div>

      {modalAgendar && (
        <AgendarModal
          sol={sol}
          profs={profs}
          procs={procs}
          onFechar={() => setModalAgendar(false)}
          onDecidido={(r) => { setResultado(r); setModalAgendar(false); carregar() }}
        />
      )}
      {modalRecusar && (
        <RecusarModal
          sol={sol}
          onFechar={() => setModalRecusar(false)}
          onDecidido={(r) => { setResultado(r); setModalRecusar(false); carregar() }}
        />
      )}
      {resultado && <MensagemModal sol={sol} resultado={resultado} onFechar={() => setResultado(null)} />}
    </>
  )
}

// ---------------------------------------------------------------- Agendar
function AgendarModal({ sol, profs, procs, onFechar, onDecidido }: {
  sol: SolicitacaoDetalhe
  profs: ProfissionalListItem[]
  procs: ProcedimentoDto[]
  onFechar: () => void
  onDecidido: (r: DecisaoResultado) => void
}) {
  const toast = useToast()
  const sugestaoProc = useMemo(
    () => procs.find((p) => p.nome.toLowerCase() === sol.procedimentoInteresse.toLowerCase())?.id ?? '',
    [procs, sol.procedimentoInteresse],
  )
  const [profId, setProfId] = useState(profs[0]?.id ?? '')
  const [procId, setProcId] = useState(sugestaoProc)
  const [inicio, setInicio] = useState('')
  const [valor, setValor] = useState('')
  const [enviando, setEnviando] = useState(false)

  useEffect(() => { if (!profId && profs[0]) setProfId(profs[0].id) }, [profs, profId])
  useEffect(() => { setProcId(sugestaoProc) }, [sugestaoProc])

  const confirmar = async () => {
    if (!profId) return toast.erro('Selecione o profissional.')
    if (!procId) return toast.erro('Selecione o procedimento.')
    if (!inicio) return toast.erro('Informe a data e hora.')
    setEnviando(true)
    try {
      const res = await triagemApi.agendar(sol.id, {
        usuarioProfissionalId: profId,
        procedimentoId: procId,
        inicio: localParaUtcIso(inicio),
        valorCobrado: valor ? Number(valor) : null,
      })
      toast.sucesso('Agendado com sucesso.')
      onDecidido(res.data)
    } catch (e) {
      toast.erro(mensagemErro(e))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <Modal aberto titulo="Agendar atendimento" onFechar={onFechar}>
      <Field label="Profissional">
        <select value={profId} onChange={(e) => setProfId(e.target.value)}>
          <option value="">Selecione…</option>
          {profs.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
        </select>
      </Field>
      <Field label="Procedimento" hint={sugestaoProc ? 'Pré-selecionado pelo interesse informado.' : 'O interesse era texto livre — escolha o procedimento do catálogo.'}>
        <select value={procId} onChange={(e) => setProcId(e.target.value)}>
          <option value="">Selecione…</option>
          {procs.map((p) => <option key={p.id} value={p.id}>{p.nome} ({p.duracaoMinutos} min)</option>)}
        </select>
      </Field>
      <div className="form-row">
        <Field label="Data e hora">
          <input type="datetime-local" value={inicio} onChange={(e) => setInicio(e.target.value)} />
        </Field>
        <Field label="Valor cobrado (R$)" hint="Vazio = valor de referência.">
          <input type="number" min={0} step="0.01" value={valor} onChange={(e) => setValor(e.target.value)} />
        </Field>
      </div>
      <div className="row" style={{ justifyContent: 'flex-end', marginTop: 8 }}>
        <button className="btn btn-ghost" onClick={onFechar}>Cancelar</button>
        <button className="btn btn-success" onClick={confirmar} disabled={enviando}>{enviando ? 'Agendando…' : 'Confirmar agendamento'}</button>
      </div>
    </Modal>
  )
}

// ---------------------------------------------------------------- Recusar
function RecusarModal({ sol, onFechar, onDecidido }: {
  sol: SolicitacaoDetalhe
  onFechar: () => void
  onDecidido: (r: DecisaoResultado) => void
}) {
  const toast = useToast()
  const [motivo, setMotivo] = useState('')
  const [enviando, setEnviando] = useState(false)

  const confirmar = async () => {
    if (!motivo.trim()) return toast.erro('Informe o motivo da recusa.')
    setEnviando(true)
    try {
      const res = await triagemApi.recusar(sol.id, motivo.trim())
      toast.sucesso('Solicitação recusada.')
      onDecidido(res.data)
    } catch (e) {
      toast.erro(mensagemErro(e))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <Modal aberto titulo="Recusar solicitação" onFechar={onFechar}>
      <Field label="Motivo da recusa" hint="Será comunicado à paciente.">
        <textarea value={motivo} onChange={(e) => setMotivo(e.target.value)} placeholder="Ex.: contraindicação clínica…" autoFocus />
      </Field>
      <div className="row" style={{ justifyContent: 'flex-end' }}>
        <button className="btn btn-ghost" onClick={onFechar}>Cancelar</button>
        <button className="btn btn-danger" onClick={confirmar} disabled={enviando}>{enviando ? 'Enviando…' : 'Confirmar recusa'}</button>
      </div>
    </Modal>
  )
}

// ---------------------------------------------------------------- Mensagem
function MensagemModal({ sol, resultado, onFechar }: {
  sol: SolicitacaoDetalhe
  resultado: DecisaoResultado
  onFechar: () => void
}) {
  const toast = useToast()
  const copiar = async () => {
    await navigator.clipboard.writeText(resultado.mensagemParaPaciente)
    toast.info('Mensagem copiada.')
  }
  return (
    <Modal aberto titulo="Mensagem para a paciente" onFechar={onFechar}>
      <p className="muted" style={{ marginBottom: 12 }}>No MVP, o envio é manual. Copie ou abra direto no WhatsApp:</p>
      <div className="card" style={{ background: 'var(--rose-bg)', marginBottom: 14 }}>{resultado.mensagemParaPaciente}</div>
      <div className="row">
        <a className="btn btn-success" href={linkWhatsApp(sol.telefone, resultado.mensagemParaPaciente)} target="_blank" rel="noreferrer">Abrir no WhatsApp</a>
        <button className="btn btn-soft" onClick={copiar}>Copiar mensagem</button>
        <button className="btn btn-ghost" onClick={onFechar}>Fechar</button>
      </div>
    </Modal>
  )
}
