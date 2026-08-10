import { useEffect, useState } from 'react'
import { Modal } from './Modal'
import { Spinner } from './Spinner'
import { EmptyState } from './ui'
import { AnamneseView } from './AnamneseView'
import { AnamneseManualForm } from './AnamneseManualForm'
import { pacientesApi } from '../api/services'
import { mensagemErro } from '../api/client'
import { useToast } from './Toast'
import { formatarDataHora } from '../lib/format'
import type { AnamneseDetalhe } from '../api/types'

/** Histórico de anamneses de uma paciente — dado clínico sensível (Admin/Profissional). */
export function DetalheAnamnese({ pacienteId, aberto, onFechar }: { pacienteId: string; aberto: boolean; onFechar: () => void }) {
  const toast = useToast()
  const [itens, setItens] = useState<AnamneseDetalhe[]>([])
  const [carregando, setCarregando] = useState(true)
  const [selecionadaId, setSelecionadaId] = useState<string | null>(null)
  const [modalManual, setModalManual] = useState(false)

  const carregar = () => {
    setCarregando(true)
    pacientesApi.anamneses(pacienteId)
      .then((r) => {
        setItens(r.data)
        setSelecionadaId((atual) => atual ?? r.data[0]?.id ?? null)
      })
      .catch((e) => toast.erro(mensagemErro(e)))
      .finally(() => setCarregando(false))
  }

  useEffect(() => {
    if (!aberto) return
    setSelecionadaId(null)
    carregar()
  }, [aberto, pacienteId])

  const selecionada = itens.find((a) => a.id === selecionadaId) ?? null

  return (
    <Modal aberto={aberto} titulo="Anamneses da paciente" onFechar={onFechar} largura={820}>
      <div className="row spread" style={{ marginBottom: 16 }}>
        <p className="muted" style={{ margin: 0, fontSize: 13 }}>Histórico completo, mais recente primeiro.</p>
        <button className="btn btn-soft btn-sm" onClick={() => setModalManual(true)}>+ Nova ficha</button>
      </div>

      {carregando ? (
        <Spinner label="Carregando anamneses…" />
      ) : itens.length === 0 ? (
        <EmptyState titulo="Nenhuma anamnese preenchida" texto="Gere um link de anamnese ou clique em '+ Nova ficha' para digitalizar uma ficha em papel." />
      ) : (
        <div>
          {itens.length > 1 && (
            <div className="row" style={{ gap: 8, marginBottom: 16, flexWrap: 'wrap' }}>
              {itens.map((a) => (
                <button
                  key={a.id}
                  className={`btn btn-sm ${a.id === selecionadaId ? 'btn-primary' : 'btn-ghost'}`}
                  onClick={() => setSelecionadaId(a.id)}
                >
                  {formatarDataHora(a.dataPreenchimento)}
                </button>
              ))}
            </div>
          )}
          {selecionada && <AnamneseView a={selecionada} />}
        </div>
      )}

      <AnamneseManualForm
        pacienteId={pacienteId}
        aberto={modalManual}
        onFechar={() => setModalManual(false)}
        onCriada={() => { setModalManual(false); setSelecionadaId(null); carregar() }}
      />
    </Modal>
  )
}
