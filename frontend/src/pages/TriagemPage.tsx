import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { triagemApi } from '../api/services'
import { useAuth } from '../auth/AuthContext'
import { Badge, Card, EmptyState, PageHeader } from '../components/ui'
import { Spinner } from '../components/Spinner'
import { formatarDataHora, formatarTelefone } from '../lib/format'
import type { SolicitacaoListItem, StatusSolicitacao } from '../api/types'

const FILTROS: { valor?: StatusSolicitacao; rotulo: string }[] = [
  { valor: undefined, rotulo: 'Pendentes' },
  { valor: 'Recebida', rotulo: 'Recebidas' },
  { valor: 'EmTriagem', rotulo: 'Em triagem' },
  { valor: 'Agendada', rotulo: 'Agendadas' },
  { valor: 'Recusada', rotulo: 'Recusadas' },
]

function tomStatus(s: StatusSolicitacao) {
  return s === 'Recebida' ? 'info' : s === 'EmTriagem' ? 'alerta' : s === 'Agendada' ? 'sucesso' : 'erro'
}

export function TriagemPage() {
  const navigate = useNavigate()
  const { temPapel } = useAuth()
  const podeVerDetalhe = temPapel('Admin', 'Profissional')

  const [filtro, setFiltro] = useState<StatusSolicitacao | undefined>(undefined)
  const [itens, setItens] = useState<SolicitacaoListItem[]>([])
  const [carregando, setCarregando] = useState(true)

  useEffect(() => {
    setCarregando(true)
    triagemApi.fila(filtro).then((r) => setItens(r.data)).finally(() => setCarregando(false))
  }, [filtro])

  return (
    <>
      <PageHeader titulo="Fila de triagem" subtitulo="Solicitações de atendimento a avaliar." />

      <div className="toolbar">
        <div className="chips">
          {FILTROS.map((f) => (
            <button
              key={f.rotulo}
              className={`chip ${filtro === f.valor ? 'chip-active' : ''}`}
              onClick={() => setFiltro(f.valor)}
            >
              {f.rotulo}
            </button>
          ))}
        </div>
      </div>

      {!podeVerDetalhe && (
        <div className="alert alert-info">Como recepção, você acompanha a fila, mas o detalhe clínico (anamnese) é restrito a Admin/Profissional.</div>
      )}

      <Card style={{ padding: 0 }}>
        {carregando ? (
          <Spinner label="Carregando fila…" />
        ) : itens.length === 0 ? (
          <EmptyState icone="⌛" titulo="Fila vazia" texto="Nenhuma solicitação neste filtro." />
        ) : (
          <div className="table-wrap">
            <table className="data">
              <thead>
                <tr>
                  <th>Paciente</th>
                  <th>Procedimento de interesse</th>
                  <th>Telefone</th>
                  <th>Recebida em</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {itens.map((s) => (
                  <tr
                    key={s.id}
                    className={podeVerDetalhe ? 'row-click' : ''}
                    onClick={() => podeVerDetalhe && navigate(`/triagem/${s.id}`)}
                  >
                    <td><strong style={{ color: 'var(--plum)' }}>{s.pacienteNome}</strong></td>
                    <td>{s.procedimentoInteresse}</td>
                    <td>{formatarTelefone(s.telefone)}</td>
                    <td>{formatarDataHora(s.dataEnvio)}</td>
                    <td><Badge texto={s.status} tom={tomStatus(s.status)} /></td>
                    <td className="text-right">{podeVerDetalhe ? <span className="link-btn">Avaliar →</span> : <span className="badge badge-neutro">🔒</span>}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </>
  )
}
