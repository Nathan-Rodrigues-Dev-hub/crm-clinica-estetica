import { createContext, useCallback, useContext, useState, type ReactNode } from 'react'

type Tipo = 'sucesso' | 'erro' | 'info'
interface ToastItem {
  id: number
  tipo: Tipo
  texto: string
}

interface ToastApi {
  sucesso: (texto: string) => void
  erro: (texto: string) => void
  info: (texto: string) => void
}

const ToastContext = createContext<ToastApi | undefined>(undefined)

let seq = 1

export function ToastProvider({ children }: { children: ReactNode }) {
  const [itens, setItens] = useState<ToastItem[]>([])

  const push = useCallback((tipo: Tipo, texto: string) => {
    const id = seq++
    setItens((xs) => [...xs, { id, tipo, texto }])
    setTimeout(() => setItens((xs) => xs.filter((x) => x.id !== id)), 4500)
  }, [])

  const value: ToastApi = {
    sucesso: (t) => push('sucesso', t),
    erro: (t) => push('erro', t),
    info: (t) => push('info', t),
  }

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="toast-stack">
        {itens.map((t) => (
          <div key={t.id} className={`toast toast-${t.tipo}`} role="status">
            {t.texto}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useToast(): ToastApi {
  const ctx = useContext(ToastContext)
  if (!ctx) throw new Error('useToast deve ser usado dentro de <ToastProvider>.')
  return ctx
}
