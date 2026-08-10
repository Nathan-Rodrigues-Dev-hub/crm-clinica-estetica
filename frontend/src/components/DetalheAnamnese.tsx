import { useEffect, useState } from 'react'
import { Modal } from './Modal'
import { Spinner } from './Spinner'
import { EmptyState } from './ui'
import { AnamneseView } from './AnamneseView'
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

  useEffect(() => {
    if (!aberto) return
    setCarregando(true)
    pacientesApi.anamneses(pacienteId)
      .then((r) => {
        setItens(r.data)
        setSelecionadaId(r.data[0]?.id ?? null)
      })
      .catch((e) => toast.erro(mensagemErro(e)))
      .finally(() => setCarregando(false))
  }, [aberto, pacienteId])

  const selecionada = itens.find((a) => a.id === selecionadaId) ?? null

  return (
    <Modal aberto={aberto} titulo="Anamneses da paciente" onFechar={onFechar} largura={820}>
      {carregando ? (
        <Spinner label="Carregando anamneses…" />
      ) : itens.length === 0 ? (
        <EmptyState titulo="Nenhuma anamnese preenchida" texto="Gere um link de anamnese para a paciente preencher." />
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
    </Modal>
  )
}
