import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { agendaApi, procedimentosApi, triagemApi } from '../api/services'
import { useAuth } from '../auth/AuthContext'
import { Card, PageHeader } from '../components/ui'
import { GerarLinkModal } from '../components/GerarLinkModal'
import { Spinner } from '../components/Spinner'
import { formatarHora } from '../lib/format'
import type { AgendamentoDto, SolicitacaoListItem } from '../api/types'

export function DashboardPage() {
  const { usuario, temPapel } = useAuth()
  const [carregando, setCarregando] = useState(true)
  const [fila, setFila] = useState<SolicitacaoListItem[]>([])
  const [agendaHoje, setAgendaHoje] = useState<AgendamentoDto[]>([])
  const [procAtivos, setProcAtivos] = useState(0)
  const [modalLink, setModalLink] = useState(false)

  useEffect(() => {
    const hoje = new Date()
    const inicio = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate(), 0, 0, 0)
    const fim = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate(), 23, 59, 59)

    Promise.all([
      triagemApi.fila(),
      agendaApi.listar(undefined, inicio.toISOString(), fim.toISOString()),
      procedimentosApi.listar(false),
    ])
      .then(([f, a, p]) => {
        setFila(f.data)
        setAgendaHoje(a.data)
        setProcAtivos(p.data.length)
      })
      .finally(() => setCarregando(false))
  }, [])

  if (carregando) return <Spinner full label="Carregando painel…" />

  const podeGerarLink = temPapel('Admin', 'Recepcao')

  return (
    <>
      <PageHeader
        titulo={`Olá, ${usuario?.nome?.split(' ')[0]} 👋`}
        subtitulo="Aqui está o resumo do dia da clínica."
        acao={podeGerarLink && <button className="btn btn-primary" onClick={() => setModalLink(true)}>+ Link de anamnese</button>}
      />

      <div className="grid grid-4">
        <Card>
          <div className="stat">
            <div className="stat-icon">⌛</div>
            <div className="stat-value">{fila.length}</div>
            <div className="stat-label">Solicitações na fila de triagem</div>
          </div>
        </Card>
        <Card>
          <div className="stat">
            <div className="stat-icon">📅</div>
            <div className="stat-value">{agendaHoje.length}</div>
            <div className="stat-label">Agendamentos para hoje</div>
          </div>
        </Card>
        <Card>
          <div className="stat">
            <div className="stat-icon">✦</div>
            <div className="stat-value">{procAtivos}</div>
            <div className="stat-label">Procedimentos ativos</div>
          </div>
        </Card>
        <Card>
          <div className="stat">
            <div className="stat-icon">✿</div>
            <div className="stat-value">{agendaHoje.filter((a) => a.status === 'Realizado').length}</div>
            <div className="stat-label">Atendimentos concluídos hoje</div>
          </div>
        </Card>
      </div>

      <div className="grid grid-2" style={{ marginTop: 18 }}>
        <Card>
          <div className="spread" style={{ marginBottom: 12 }}>
            <h3>Fila de triagem</h3>
            <Link className="link-btn" to="/triagem">Ver tudo</Link>
          </div>
          {fila.length === 0 ? (
            <p className="muted">Nenhuma solicitação pendente. 🎉</p>
          ) : (
            <div className="stack" style={{ gap: 10 }}>
              {fila.slice(0, 5).map((s) => (
                <Link key={s.id} to="/triagem" className="row spread" style={{ textDecoration: 'none', color: 'inherit', padding: '8px 0', borderBottom: '1px solid var(--line)' }}>
                  <div className="stack">
                    <strong style={{ color: 'var(--plum)' }}>{s.pacienteNome}</strong>
                    <small className="muted">{s.procedimentoInteresse}</small>
                  </div>
                  <span className="badge badge-alerta">{s.status}</span>
                </Link>
              ))}
            </div>
          )}
        </Card>

        <Card>
          <div className="spread" style={{ marginBottom: 12 }}>
            <h3>Agenda de hoje</h3>
            <Link className="link-btn" to="/agenda">Ver agenda</Link>
          </div>
          {agendaHoje.length === 0 ? (
            <p className="muted">Nenhum agendamento para hoje.</p>
          ) : (
            <div className="stack" style={{ gap: 10 }}>
              {agendaHoje.slice(0, 6).map((a) => (
                <div key={a.id} className="row spread" style={{ padding: '8px 0', borderBottom: '1px solid var(--line)' }}>
                  <div className="row" style={{ gap: 12 }}>
                    <strong className="mono" style={{ color: 'var(--rose-dark)' }}>{formatarHora(a.inicio)}</strong>
                    <div className="stack">
                      <strong style={{ color: 'var(--plum)' }}>{a.pacienteNome}</strong>
                      <small className="muted">{a.procedimentoNome} · {a.profissionalNome}</small>
                    </div>
                  </div>
                  <span className={`badge ${a.status === 'Agendado' ? 'badge-info' : a.status === 'Realizado' ? 'badge-sucesso' : 'badge-neutro'}`}>{a.status}</span>
                </div>
              ))}
            </div>
          )}
        </Card>
      </div>

      <GerarLinkModal aberto={modalLink} onFechar={() => setModalLink(false)} />
    </>
  )
}
