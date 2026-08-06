import { useState } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import type { Papel } from '../api/types'
import { iniciais } from '../lib/format'

interface ItemMenu {
  para: string
  rotulo: string
  icone: string
  papeis?: Papel[]
}

const MENU: ItemMenu[] = [
  { para: '/', rotulo: 'Painel', icone: '▦' },
  { para: '/triagem', rotulo: 'Triagem', icone: '⌛' },
  { para: '/agenda', rotulo: 'Agenda', icone: '📅' },
  { para: '/pacientes', rotulo: 'Pacientes', icone: '👤' },
  { para: '/catalogo', rotulo: 'Catálogo', icone: '✦' },
]

const PAPEL_ROTULO: Record<Papel, string> = {
  Admin: 'Administrador',
  Profissional: 'Profissional',
  Recepcao: 'Recepção',
}

export function Layout() {
  const { usuario, logout, temPapel } = useAuth()
  const navigate = useNavigate()
  const [menuAberto, setMenuAberto] = useState(false)

  const sair = () => {
    logout()
    navigate('/login')
  }

  const itensVisiveis = MENU.filter((i) => !i.papeis || temPapel(...i.papeis))

  return (
    <div className="app-shell">
      <aside className={`sidebar ${menuAberto ? 'sidebar-open' : ''}`}>
        <div className="brand">
          <span className="brand-mark">GA</span>
          <div>
            <strong>GA Clinic &amp; Academy</strong>
            <small>CRM · Estética</small>
          </div>
        </div>

        <nav className="nav">
          {itensVisiveis.map((i) => (
            <NavLink
              key={i.para}
              to={i.para}
              end={i.para === '/'}
              className={({ isActive }) => `nav-item ${isActive ? 'nav-item-active' : ''}`}
              onClick={() => setMenuAberto(false)}
            >
              <span className="nav-icon">{i.icone}</span>
              {i.rotulo}
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-foot">
          <div className="user-chip">
            <span className="avatar">{usuario ? iniciais(usuario.nome) : '?'}</span>
            <div className="user-chip-info">
              <strong>{usuario?.nome}</strong>
              <small>{usuario ? PAPEL_ROTULO[usuario.papel] : ''}</small>
            </div>
          </div>
          <button className="btn btn-ghost btn-block" onClick={sair}>
            Sair
          </button>
        </div>
      </aside>

      <div className="main">
        <header className="topbar">
          <button className="icon-btn only-mobile" onClick={() => setMenuAberto((v) => !v)} aria-label="Menu">
            ☰
          </button>
          <div className="topbar-title">Painel operacional</div>
          <div className="topbar-user only-desktop">
            <span className="avatar avatar-sm">{usuario ? iniciais(usuario.nome) : '?'}</span>
            <span>{usuario?.nome}</span>
          </div>
        </header>
        <main className="content">
          <Outlet />
        </main>
      </div>

      {menuAberto && <div className="backdrop only-mobile" onClick={() => setMenuAberto(false)} />}
    </div>
  )
}
