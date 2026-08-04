import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { pacientesApi } from '../api/services'
import { Card, EmptyState, PageHeader } from '../components/ui'
import { Spinner } from '../components/Spinner'
import { PacienteFormModal } from './PacienteFormModal'
import { formatarCpf, formatarTelefone } from '../lib/format'
import type { PacienteListItem } from '../api/types'

export function PacientesPage() {
  const navigate = useNavigate()
  const [termo, setTermo] = useState('')
  const [itens, setItens] = useState<PacienteListItem[]>([])
  const [carregando, setCarregando] = useState(true)
  const [modal, setModal] = useState(false)

  const buscar = useCallback((t: string) => {
    setCarregando(true)
    pacientesApi
      .buscar(t)
      .then((r) => setItens(r.data))
      .finally(() => setCarregando(false))
  }, [])

  useEffect(() => {
    const t = setTimeout(() => buscar(termo), termo ? 350 : 0)
    return () => clearTimeout(t)
  }, [termo, buscar])

  return (
    <>
      <PageHeader
        titulo="Pacientes"
        subtitulo="Busque por nome, CPF ou telefone."
        acao={<button className="btn btn-primary" onClick={() => setModal(true)}>+ Novo paciente</button>}
      />

      <div className="toolbar">
        <div className="search">
          <input value={termo} onChange={(e) => setTermo(e.target.value)} placeholder="🔍  Buscar paciente…" />
        </div>
      </div>

      <Card style={{ padding: 0 }}>
        {carregando ? (
          <Spinner label="Buscando…" />
        ) : itens.length === 0 ? (
          <EmptyState icone="👤" titulo="Nenhum paciente" texto={termo ? 'Nada encontrado para a busca.' : 'Cadastre o primeiro paciente.'} />
        ) : (
          <div className="table-wrap">
            <table className="data">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>CPF</th>
                  <th>Telefone</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {itens.map((p) => (
                  <tr key={p.id} className="row-click" onClick={() => navigate(`/pacientes/${p.id}`)}>
                    <td><strong style={{ color: 'var(--plum)' }}>{p.nome}</strong></td>
                    <td>{p.cpfPendente ? <span className="badge badge-alerta">CPF pendente</span> : formatarCpf(p.cpf)}</td>
                    <td>{formatarTelefone(p.telefone)}</td>
                    <td className="text-right"><span className="link-btn">Abrir →</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <PacienteFormModal aberto={modal} onFechar={() => setModal(false)} onSalvo={() => buscar(termo)} />
    </>
  )
}
