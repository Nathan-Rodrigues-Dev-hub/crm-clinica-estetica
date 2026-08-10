import { useState } from 'react'
import { Modal } from './Modal'
import { Field } from './ui'
import { usuariosApi } from '../api/services'
import { mensagemErro } from '../api/client'
import { useToast } from './Toast'

/** Autoatendimento: qualquer papel troca a própria senha, exigindo a senha atual. */
export function TrocarSenhaModal({ aberto, onFechar }: { aberto: boolean; onFechar: () => void }) {
  const toast = useToast()
  const [senhaAtual, setSenhaAtual] = useState('')
  const [novaSenha, setNovaSenha] = useState('')
  const [confirmar, setConfirmar] = useState('')
  const [salvando, setSalvando] = useState(false)

  const limpar = () => { setSenhaAtual(''); setNovaSenha(''); setConfirmar('') }
  const fechar = () => { limpar(); onFechar() }

  const salvar = async () => {
    if (!senhaAtual) return toast.erro('Informe a senha atual.')
    if (novaSenha.length < 6) return toast.erro('A nova senha deve ter ao menos 6 caracteres.')
    if (novaSenha !== confirmar) return toast.erro('A confirmação não bate com a nova senha.')

    setSalvando(true)
    try {
      await usuariosApi.trocarMinhaSenha(senhaAtual, novaSenha)
      toast.sucesso('Senha alterada.')
      fechar()
    } catch (err) {
      toast.erro(mensagemErro(err))
    } finally {
      setSalvando(false)
    }
  }

  return (
    <Modal aberto={aberto} titulo="Trocar minha senha" onFechar={fechar}>
      <Field label="Senha atual">
        <input type="password" value={senhaAtual} onChange={(e) => setSenhaAtual(e.target.value)} autoFocus autoComplete="current-password" />
      </Field>
      <Field label="Nova senha" hint="Mínimo 6 caracteres.">
        <input type="password" value={novaSenha} onChange={(e) => setNovaSenha(e.target.value)} autoComplete="new-password" />
      </Field>
      <Field label="Confirmar nova senha">
        <input type="password" value={confirmar} onChange={(e) => setConfirmar(e.target.value)} autoComplete="new-password" />
      </Field>
      <div className="row" style={{ justifyContent: 'flex-end', marginTop: 12 }}>
        <button className="btn btn-ghost" onClick={fechar}>Cancelar</button>
        <button className="btn btn-primary" onClick={salvar} disabled={salvando}>{salvando ? 'Salvando…' : 'Salvar'}</button>
      </div>
    </Modal>
  )
}
