import { Link } from 'react-router-dom'

export function NaoEncontradaPage() {
  return (
    <div className="spinner-full" style={{ display: 'grid', placeItems: 'center', textAlign: 'center' }}>
      <div>
        <div style={{ fontSize: 54, opacity: 0.4 }}>✿</div>
        <h1 style={{ fontSize: 28 }}>Página não encontrada</h1>
        <p className="muted" style={{ margin: '8px 0 20px' }}>O endereço acessado não existe ou você não tem acesso.</p>
        <Link className="btn btn-primary" to="/">Voltar ao painel</Link>
      </div>
    </div>
  )
}
