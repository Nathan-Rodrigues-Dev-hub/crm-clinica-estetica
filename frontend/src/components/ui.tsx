import type { CSSProperties, ReactNode } from 'react'

export function PageHeader({ titulo, subtitulo, acao }: { titulo: string; subtitulo?: string; acao?: ReactNode }) {
  return (
    <div className="page-header">
      <div>
        <h1>{titulo}</h1>
        {subtitulo && <p className="muted">{subtitulo}</p>}
      </div>
      {acao && <div className="page-header-actions">{acao}</div>}
    </div>
  )
}

export function Card({ children, className = '', style }: { children: ReactNode; className?: string; style?: CSSProperties }) {
  return <div className={`card ${className}`} style={style}>{children}</div>
}

export function EmptyState({ titulo, texto, icone = '∅' }: { titulo: string; texto?: string; icone?: string }) {
  return (
    <div className="empty">
      <div className="empty-icon">{icone}</div>
      <strong>{titulo}</strong>
      {texto && <p className="muted">{texto}</p>}
    </div>
  )
}

export function Badge({ texto, tom = 'neutro' }: { texto: string; tom?: 'neutro' | 'sucesso' | 'alerta' | 'erro' | 'info' }) {
  return <span className={`badge badge-${tom}`}>{texto}</span>
}

export function Field({ label, children, hint }: { label: string; children: ReactNode; hint?: string }) {
  return (
    <label className="field">
      <span className="field-label">{label}</span>
      {children}
      {hint && <small className="field-hint">{hint}</small>}
    </label>
  )
}
