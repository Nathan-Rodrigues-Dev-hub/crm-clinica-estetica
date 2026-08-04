import { Navigate, Route, Routes } from 'react-router-dom'
import { RequireAuth } from './auth/RequireAuth'
import { Layout } from './components/Layout'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { PacientesPage } from './pages/PacientesPage'
import { PacienteDetalhePage } from './pages/PacienteDetalhePage'
import { CatalogoPage } from './pages/CatalogoPage'
import { TriagemPage } from './pages/TriagemPage'
import { TriagemDetalhePage } from './pages/TriagemDetalhePage'
import { AgendaPage } from './pages/AgendaPage'
import { AnamnesePublicaPage } from './pages/public/AnamnesePublicaPage'
import { NaoEncontradaPage } from './pages/NaoEncontradaPage'

export function App() {
  return (
    <Routes>
      {/* Público (sem login, isolado da área autenticada) */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/anamnese/:token" element={<AnamnesePublicaPage />} />

      {/* Área autenticada */}
      <Route element={<RequireAuth />}>
        <Route element={<Layout />}>
          <Route index element={<DashboardPage />} />
          <Route path="/triagem" element={<TriagemPage />} />
          <Route path="/agenda" element={<AgendaPage />} />
          <Route path="/pacientes" element={<PacientesPage />} />
          <Route path="/pacientes/:id" element={<PacienteDetalhePage />} />
          <Route path="/catalogo" element={<CatalogoPage />} />
        </Route>
      </Route>

      {/* Detalhe de triagem exige Admin/Profissional (dado clínico) */}
      <Route element={<RequireAuth papeis={['Admin', 'Profissional']} />}>
        <Route element={<Layout />}>
          <Route path="/triagem/:id" element={<TriagemDetalhePage />} />
        </Route>
      </Route>

      <Route path="/404" element={<NaoEncontradaPage />} />
      <Route path="*" element={<Navigate to="/404" replace />} />
    </Routes>
  )
}
