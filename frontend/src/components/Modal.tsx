import { useEffect, type ReactNode } from 'react'

interface ModalProps {
  aberto: boolean
  titulo: string
  onFechar: () => void
  children: ReactNode
  largura?: number
}

export function Modal({ aberto, titulo, onFechar, children, largura = 560 }: ModalProps) {
  useEffect(() => {
    if (!aberto) return
    const onKey = (e: KeyboardEvent) => e.key === 'Escape' && onFechar()
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [aberto, onFechar])

  if (!aberto) return null

  return (
    <div className="modal-overlay" onMouseDown={onFechar}>
      <div className="modal" style={{ maxWidth: largura }} onMouseDown={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>{titulo}</h3>
          <button className="icon-btn" onClick={onFechar} aria-label="Fechar">
            ✕
          </button>
        </div>
        <div className="modal-body">{children}</div>
      </div>
    </div>
  )
}
