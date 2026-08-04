import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from './AuthContext'
import type { Papel } from '../api/types'
import { Spinner } from '../components/Spinner'

/** Protege rotas: exige autenticação e, opcionalmente, papéis específicos. */
export function RequireAuth({ papeis }: { papeis?: Papel[] }) {
  const { usuario, carregando } = useAuth()
  const location = useLocation()

  if (carregando) return <Spinner full label="Verificando sessão…" />
  if (!usuario) return <Navigate to="/login" replace state={{ from: location }} />
  if (papeis && !papeis.includes(usuario.papel)) return <Navigate to="/" replace />

  return <Outlet />
}
