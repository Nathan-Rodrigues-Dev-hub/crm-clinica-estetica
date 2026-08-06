import { useState, type FormEvent } from 'react'
import { useLocation, useNavigate, Navigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { useToast } from '../components/Toast'
import { mensagemErro } from '../api/client'
import { Field } from '../components/ui'

export function LoginPage() {
  const { usuario, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const toast = useToast()

  const [email, setEmail] = useState('admin@clinica.local')
  const [senha, setSenha] = useState('Admin@123')
  const [enviando, setEnviando] = useState(false)

  if (usuario) return <Navigate to="/" replace />

  const destino = (location.state as { from?: { pathname: string } } | null)?.from?.pathname ?? '/'

  const submeter = async (e: FormEvent) => {
    e.preventDefault()
    setEnviando(true)
    try {
      await login(email.trim(), senha)
      toast.sucesso('Bem-vindo(a)!')
      navigate(destino, { replace: true })
    } catch (err) {
      toast.erro(mensagemErro(err, 'Não foi possível entrar.'))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="login-wrap">
      <div className="login-hero">
        <span className="hero-mark">GA</span>
        <h2>Gestão completa da sua clínica de estética</h2>
        <p>Pacientes, anamnese online, triagem, agenda e prontuário — tudo em um só lugar, com segurança e LGPD.</p>
        <ul>
          <li>Anamnese por link, sem fricção para a paciente</li>
          <li>Triagem com decisão de agendar ou recusar</li>
          <li>Agenda por profissional, sem colisões</li>
        </ul>
      </div>

      <div className="login-panel">
        <form className="login-card" onSubmit={submeter}>
          <h1>Entrar</h1>
          <p className="muted" style={{ marginBottom: 22 }}>Acesse com suas credenciais.</p>

          <Field label="E-mail">
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoFocus />
          </Field>
          <Field label="Senha">
            <input type="password" value={senha} onChange={(e) => setSenha(e.target.value)} required />
          </Field>

          <button className="btn btn-primary btn-block" disabled={enviando} style={{ marginTop: 6 }}>
            {enviando ? 'Entrando…' : 'Entrar'}
          </button>

          <div className="login-seed">
            <strong>Usuários de demonstração</strong> (senha <code>Admin@123</code>):<br />
            <code>admin@clinica.local</code> · <code>profissional@clinica.local</code> · <code>recepcao@clinica.local</code>
          </div>
        </form>
      </div>
    </div>
  )
}
