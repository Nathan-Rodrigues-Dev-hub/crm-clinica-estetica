import { useCallback, useEffect, useState } from 'react'
import { agendaApi, pacientesApi, procedimentosApi, usuariosApi } from '../api/services'
import { mensagemErro } from '../api/client'
import { useToast } from '../components/Toast'
import { Badge, Card, EmptyState, Field, PageHeader } from '../components/ui'
import { Modal } from '../components/Modal'
import { Spinner } from '../components/Spinner'
import { formatarData, formatarHora, formatarMoeda, localParaUtcIso } from '../lib/format'
import type { AgendamentoDto, PacienteListItem, ProcedimentoDto, ProfissionalListItem, StatusAgendamento } from '../api/types'

function inicioDoDia(d: Date) { return new Date(d.getFullYear(), d.getMonth(), d.getDate(), 0, 0, 0) }
function fimDoDia(d: Date) { return new Date(d.getFullYear(), d.getMonth(), d.getDate(), 23, 59, 59) }
function isoData(d: Date) { return d.toISOString().slice(0, 10) }

function tomStatus(s: StatusAgendamento) {
  return s === 'Agendado' ? 'info' : s === 'Realizado' ? 'sucesso' : s === 'Faltou' ? 'alerta' : 'erro'
}

export function AgendaPage() {
  const toast = useToast()
  const hoje = new Date()

  const [profs, setProfs] = useState<ProfissionalListItem[]>([])
  const [profId, setProfId] = useState('')
  const [de, setDe] = useState(isoData(hoje))
  const [ate, setAte] = useState(isoData(hoje))
  const [itens, setItens] = useState<AgendamentoDto[]>([])
  const [carregando, setCarregando] = useState(true)
  const [modalNovo, setModalNovo] = useState(false)
  const [statusDe, setStatusDe] = useState<AgendamentoDto | null>(null)
  const [valorDe, setValorDe] = useState<AgendamentoDto | null>(null)

  useEffect(() => { usuariosApi.profissionais().then((r) => setProfs(r.data)).catch(() => {}) }, [])

  const carregar = useCallback(() => {
    setCarregando(true)
    const deIso = inicioDoDia(new Date(de + 'T00:00')).toISOString()
    const ateIso = fimDoDia(new Date(ate + 'T00:00')).toISOString()
    agendaApi.listar(profId || undefined, deIso, ateIso)
      .then((r) => setItens(r.data))
      .catch((e) => toast.erro(mensagemErro(e)))
      .finally(() => setCarregando(false))
  }, [profId, de, ate, toast])

  useEffect(() => { carregar() }, [carregar])

  return (
    <>
      <PageHeader
        titulo="Agenda"
        subtitulo="Agendamentos por profissional e período (horário local)."
        acao={<button className="btn btn-primary" onClick={() => setModalNovo(true)}>+ Novo agendamento</button>}
      />

      <Card style={{ marginBottom: 18 }}>
        <div className="form-row-3" style={{ alignItems: 'end' }}>
          <Field label="Profissional">
            <select value={profId} onChange={(e) => setProfId(e.target.value)}>
              <option value="">Todos</option>
              {profs.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
            </select>
          </Field>
          <Field label="De"><input type="date" value={de} onChange={(e) => setDe(e.target.value)} /></Field>
          <Field label="Até"><input type="date" value={ate} onChange={(e) => setAte(e.target.value)} /></Field>
        </div>
      </Card>

      <Card style={{ padding: 0 }}>
        {carregando ? (
          <Spinner label="Carregando agenda…" />
        ) : itens.length === 0 ? (
          <EmptyState icone="📅" titulo="Sem agendamentos" texto="Nenhum agendamento no período/profissional selecionado." />
        ) : (
          <div className="table-wrap">
            <table className="data">
              <thead>
                <tr>
                  <th>Data / hora</th>
                  <th>Paciente</th>
                  <th>Procedimento</th>
                  <th>Profissional</th>
                  <th>Valor</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {itens.map((a) => (
                  <tr key={a.id}>
                    <td>
                      <strong className="mono" style={{ color: 'var(--rose-dark)' }}>{formatarHora(a.inicio)}</strong>
                      <div><small className="muted">{formatarData(a.inicio)}</small></div>
                    </td>
                    <td><strong style={{ color: 'var(--plum)' }}>{a.pacienteNome}</strong></td>
                    <td>{a.procedimentoNome}</td>
                    <td>{a.profissionalNome}</td>
                    <td>{formatarMoeda(a.valorCobrado)}</td>
                    <td>
                      <Badge texto={a.status} tom={tomStatus(a.status)} />
                      {a.motivoCancelamento && <div><small className="muted">{a.motivoCancelamento}</small></div>}
                    </td>
                    <td className="text-right">
                      <div className="row" style={{ justifyContent: 'flex-end', gap: 6 }}>
                        <button className="btn btn-ghost btn-sm" onClick={() => setValorDe(a)}>Valor</button>
                        {a.status === 'Agendado' && <button className="btn btn-soft btn-sm" onClick={() => setStatusDe(a)}>Status</button>}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {modalNovo && (
        <NovoAgendamentoModal profs={profs} onFechar={() => setModalNovo(false)} onCriado={() => { setModalNovo(false); carregar() }} />
      )}
      {statusDe && <StatusModal ag={statusDe} onFechar={() => setStatusDe(null)} onFeito={() => { setStatusDe(null); carregar() }} />}
      {valorDe && <ValorModal ag={valorDe} onFechar={() => setValorDe(null)} onFeito={() => { setValorDe(null); carregar() }} />}
    </>
  )
}

// ---------------------------------------------------------------- Novo
function NovoAgendamentoModal({ profs, onFechar, onCriado }: {
  profs: ProfissionalListItem[]
  onFechar: () => void
  onCriado: () => void
}) {
  const toast = useToast()
  const [busca, setBusca] = useState('')
  const [resultados, setResultados] = useState<PacienteListItem[]>([])
  const [paciente, setPaciente] = useState<PacienteListItem | null>(null)
  const [procs, setProcs] = useState<ProcedimentoDto[]>([])
  const [profId, setProfId] = useState(profs[0]?.id ?? '')
  const [procId, setProcId] = useState('')
  const [inicio, setInicio] = useState('')
  const [valor, setValor] = useState('')
  const [enviando, setEnviando] = useState(false)

  useEffect(() => { procedimentosApi.listar(false).then((r) => setProcs(r.data)).catch(() => {}) }, [])
  useEffect(() => { if (!profId && profs[0]) setProfId(profs[0].id) }, [profs, profId])

  useEffect(() => {
    if (paciente) return
    const t = setTimeout(() => {
      if (busca.trim().length < 2) { setResultados([]); return }
      pacientesApi.buscar(busca).then((r) => setResultados(r.data)).catch(() => {})
    }, 300)
    return () => clearTimeout(t)
  }, [busca, paciente])

  const confirmar = async () => {
    if (!paciente) return toast.erro('Selecione a paciente.')
    if (!profId) return toast.erro('Selecione o profissional.')
    if (!procId) return toast.erro('Selecione o procedimento.')
    if (!inicio) return toast.erro('Informe data e hora.')
    setEnviando(true)
    try {
      await agendaApi.criar({
        pacienteId: paciente.id,
        usuarioProfissionalId: profId,
        procedimentoId: procId,
        inicio: localParaUtcIso(inicio),
        valorCobrado: valor ? Number(valor) : null,
      })
      toast.sucesso('Agendamento criado.')
      onCriado()
    } catch (e) {
      toast.erro(mensagemErro(e)) // inclui 409 de colisão (RN03)
    } finally {
      setEnviando(false)
    }
  }

  return (
    <Modal aberto titulo="Novo agendamento" onFechar={onFechar}>
      <Field label="Paciente">
        {paciente ? (
          <div className="row spread" style={{ border: '1px solid var(--line)', borderRadius: 9, padding: '8px 12px' }}>
            <strong style={{ color: 'var(--plum)' }}>{paciente.nome}</strong>
            <button className="link-btn" onClick={() => { setPaciente(null); setBusca('') }}>trocar</button>
          </div>
        ) : (
          <>
            <input value={busca} onChange={(e) => setBusca(e.target.value)} placeholder="Buscar por nome, CPF ou telefone…" autoFocus />
            {resultados.length > 0 && (
              <div className="card" style={{ padding: 6, marginTop: 6, maxHeight: 180, overflow: 'auto' }}>
                {resultados.map((p) => (
                  <button key={p.id} className="btn btn-ghost btn-block" style={{ justifyContent: 'flex-start' }} onClick={() => setPaciente(p)}>
                    {p.nome}
                  </button>
                ))}
              </div>
            )}
          </>
        )}
      </Field>

      <div className="form-row">
        <Field label="Profissional">
          <select value={profId} onChange={(e) => setProfId(e.target.value)}>
            <option value="">Selecione…</option>
            {profs.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
          </select>
        </Field>
        <Field label="Procedimento">
          <select value={procId} onChange={(e) => setProcId(e.target.value)}>
            <option value="">Selecione…</option>
            {procs.map((p) => <option key={p.id} value={p.id}>{p.nome} ({p.duracaoMinutos} min)</option>)}
          </select>
        </Field>
      </div>
      <div className="form-row">
        <Field label="Data e hora"><input type="datetime-local" value={inicio} onChange={(e) => setInicio(e.target.value)} /></Field>
        <Field label="Valor (R$)" hint="Vazio = valor de referência."><input type="number" min={0} step="0.01" value={valor} onChange={(e) => setValor(e.target.value)} /></Field>
      </div>

      <div className="row" style={{ justifyContent: 'flex-end', marginTop: 8 }}>
        <button className="btn btn-ghost" onClick={onFechar}>Cancelar</button>
        <button className="btn btn-primary" onClick={confirmar} disabled={enviando}>{enviando ? 'Salvando…' : 'Agendar'}</button>
      </div>
    </Modal>
  )
}

// ---------------------------------------------------------------- Status
function StatusModal({ ag, onFechar, onFeito }: { ag: AgendamentoDto; onFechar: () => void; onFeito: () => void }) {
  const toast = useToast()
  const [status, setStatus] = useState<StatusAgendamento>('Realizado')
  const [motivo, setMotivo] = useState('')
  const [enviando, setEnviando] = useState(false)

  const confirmar = async () => {
    if (status === 'Cancelado' && !motivo.trim()) return toast.erro('Informe o motivo do cancelamento.')
    setEnviando(true)
    try {
      await agendaApi.alterarStatus(ag.id, status, motivo || undefined)
      toast.sucesso('Status atualizado.')
      onFeito()
    } catch (e) {
      toast.erro(mensagemErro(e))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <Modal aberto titulo={`Status — ${ag.pacienteNome}`} onFechar={onFechar} largura={440}>
      <Field label="Novo status">
        <select value={status} onChange={(e) => setStatus(e.target.value as StatusAgendamento)}>
          <option value="Realizado">Realizado</option>
          <option value="Faltou">Faltou</option>
          <option value="Cancelado">Cancelado</option>
        </select>
      </Field>
      {status === 'Cancelado' && (
        <Field label="Motivo do cancelamento">
          <textarea value={motivo} onChange={(e) => setMotivo(e.target.value)} />
        </Field>
      )}
      <div className="row" style={{ justifyContent: 'flex-end' }}>
        <button className="btn btn-ghost" onClick={onFechar}>Cancelar</button>
        <button className="btn btn-primary" onClick={confirmar} disabled={enviando}>{enviando ? 'Salvando…' : 'Confirmar'}</button>
      </div>
    </Modal>
  )
}

// ---------------------------------------------------------------- Valor
function ValorModal({ ag, onFechar, onFeito }: { ag: AgendamentoDto; onFechar: () => void; onFeito: () => void }) {
  const toast = useToast()
  const [valor, setValor] = useState(String(ag.valorCobrado ?? 0))
  const [enviando, setEnviando] = useState(false)

  const confirmar = async () => {
    setEnviando(true)
    try {
      await agendaApi.atualizarValor(ag.id, Number(valor))
      toast.sucesso('Valor atualizado.')
      onFeito()
    } catch (e) {
      toast.erro(mensagemErro(e))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <Modal aberto titulo={`Valor cobrado — ${ag.pacienteNome}`} onFechar={onFechar} largura={400}>
      <Field label="Valor (R$)">
        <input type="number" min={0} step="0.01" value={valor} onChange={(e) => setValor(e.target.value)} autoFocus />
      </Field>
      <div className="row" style={{ justifyContent: 'flex-end' }}>
        <button className="btn btn-ghost" onClick={onFechar}>Cancelar</button>
        <button className="btn btn-primary" onClick={confirmar} disabled={enviando}>{enviando ? 'Salvando…' : 'Salvar'}</button>
      </div>
    </Modal>
  )
}
