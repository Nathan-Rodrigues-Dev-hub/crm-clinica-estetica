import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi } from '../api/services'
import { TOKEN_KEY, USER_KEY } from '../api/client'
import type { Papel, UsuarioLogado } from '../api/types'

interface AuthState {
  usuario: UsuarioLogado | null
  carregando: boolean
  login: (email: string, senha: string) => Promise<void>
  logout: () => void
  temPapel: (...papeis: Papel[]) => boolean
}

const AuthContext = createContext<AuthState | undefined>(undefined)

function lerUsuarioSalvo(): UsuarioLogado | null {
  try {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? (JSON.parse(raw) as UsuarioLogado) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioLogado | null>(lerUsuarioSalvo)
  const [carregando, setCarregando] = useState(true)

  // Revalida a sessão ao montar (se houver token).
  useEffect(() => {
    const token = localStorage.getItem(TOKEN_KEY)
    if (!token) {
      setCarregando(false)
      return
    }
    authApi
      .me()
      .then((res) => {
        setUsuario(res.data)
        localStorage.setItem(USER_KEY, JSON.stringify(res.data))
      })
      .catch(() => {
        localStorage.removeItem(TOKEN_KEY)
        localStorage.removeItem(USER_KEY)
        setUsuario(null)
      })
      .finally(() => setCarregando(false))
  }, [])

  const login = useCallback(async (email: string, senha: string) => {
    const res = await authApi.login(email, senha)
    localStorage.setItem(TOKEN_KEY, res.data.token)
    localStorage.setItem(USER_KEY, JSON.stringify(res.data.usuario))
    setUsuario(res.data.usuario)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    setUsuario(null)
  }, [])

  const temPapel = useCallback(
    (...papeis: Papel[]) => (usuario ? papeis.includes(usuario.papel) : false),
    [usuario],
  )

  const value = useMemo(
    () => ({ usuario, carregando, login, logout, temPapel }),
    [usuario, carregando, login, logout, temPapel],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthState {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth deve ser usado dentro de <AuthProvider>.')
  return ctx
}
