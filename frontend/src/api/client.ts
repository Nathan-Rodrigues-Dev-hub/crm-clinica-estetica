import axios, { AxiosError } from 'axios'

export const TOKEN_KEY = 'crm.token'
export const USER_KEY = 'crm.user'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5240',
})

// Anexa o JWT em toda requisição autenticada.
api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Trata 401 globalmente: limpa a sessão e volta para o login (exceto no fluxo público).
api.interceptors.response.use(
  (res) => res,
  (error: AxiosError) => {
    const status = error.response?.status
    const url = error.config?.url ?? ''
    const isPublic = url.includes('/api/public/')
    if (status === 401 && !isPublic) {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(USER_KEY)
      if (!location.pathname.startsWith('/login') && !location.pathname.startsWith('/anamnese/')) {
        location.assign('/login')
      }
    }
    return Promise.reject(error)
  },
)

/** Extrai uma mensagem amigável de um erro do axios (ProblemDetails / ValidationProblemDetails). */
export function mensagemErro(error: unknown, fallback = 'Ocorreu um erro. Tente novamente.'): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as
      | { detail?: string; title?: string; errors?: Record<string, string[]> }
      | string
      | undefined

    if (typeof data === 'string' && data.trim()) return data

    if (data && typeof data === 'object') {
      if (data.errors) {
        const msgs = Object.values(data.errors).flat()
        if (msgs.length) return msgs.join(' ')
      }
      if (data.detail) return data.detail
      if (data.title) return data.title
    }
    if (!error.response) return 'Sem conexão com o servidor. Verifique se a API está no ar.'
  }
  return fallback
}
