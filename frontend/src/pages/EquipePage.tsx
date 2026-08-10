import { useEffect, useState } from 'react'
import { usuariosApi } from '../api/services'
import { useAuth } from '../auth/AuthContext'
import { useToast } from '../components/Toast'
import { mensagemErro } from '../api/client'
import { Badge, Card, EmptyState, Field, PageHeader } from '../components/ui'
import { Modal } from '../components/Modal'
import { Spinner } from '../components/Spinner'
import type { EquipeUsuario, LimiteProfissionaisDto, Papel } from '../api/types'

interface Editando {
  id?: string
  nome: string
  email: string
  papel: Papel
  senha: string
  ativo: boolean
}

const novo: Editando = { nome: '', email: '', papel: 'Recepcao', senha: '', ativo: true }

const PAPEL_ROTULO: Record<Papel, string> = {
  Admin: 'Administrador',
  Profissional: 'Profissional',
  Recepcao: 'Recepção',
  SuperAdmin: 'Suporte',
}

const PAPEIS: Papel[] = ['Admin', 'Profissional', 'Recepcao']

export function EquipePage() {
  const { usuario, temPapel } = useAuth()
  const souSuperAdmin = temPapel('SuperAdmin')
  const papeisDisponiveis = souSuperAdmin ? PAPEIS : PAPEIS.filter((p) => p !== 'Profissional')
  const toast = useToast()

  const [itens, setItens] = useState<EquipeUsuario[]>([])
  const [carregando, setCarregando] = useState(true)
  const [incluirInativos, setIncluirInativos] = useState(false)
  const [edit, setEdit] = useState<Editando | null>(null)
  const [salvando, setSalvando] = useState(false)
  const [limite, setLimite] = useState<LimiteProfissionaisDto | null>(null)

  const carregar = (inativos: boolean) => {
    setCarregando(true)
    usuariosApi.listar(inativos).then((r) => setItens(r.data)).finally(() => setCarregando(false))
  }

  const carregarLimite = () => {
    usuariosApi.limiteProfissionais().then((r) => setLimite(r.data)).catch(() => {})
  }

  useEffect(() => carregar(incluirInativos), [incluirInativos])
  useEffect(carregarLimite, [])

  const salvar = async () => {
    if (!edit) return
    if (!edit.nome.trim()) return toast.erro('Informe o nome.')
    if (!edit.id && !edit.email.trim()) return toast.erro('Informe o e-mail.')
    if (!edit.id && edit.senha.length < 6) return toast.erro('A senha inicial deve ter ao menos 6 caracteres.')
    if (edit.id && edit.senha && edit.senha.length < 6) return toast.erro('A nova senha deve ter ao menos 6 caracteres.')

    setSalvando(true)
    try {
      if (edit.id) {
        await usuariosApi.atualizar(edit.id, {
          nome: edit.nome.trim(),
          papel: edit.papel,
          ativo: edit.ativo,
          novaSenha: edit.senha ? edit.senha : null,
        })
      } else {
        await usuariosApi.criar({
          nome: edit.nome.trim(),
          email: edit.email.trim(),
          senha: edit.senha,
          papel: edit.papel,
        })
      }
      toast.sucesso('Usuário salvo.')
      setEdit(null)
      carregar(incluirInativos)
      carregarLimite()
    } catch (err) {
      toast.erro(mensagemErro(err))
    } finally {
      setSalvando(false)
    }
  }

  const desativar = async (u: EquipeUsuario) => {
    try {
      await usuariosApi.desativar(u.id)
      toast.sucesso(`${u.nome} foi desativado.`)
      carregar(incluirInativos)
      carregarLimite()
    } catch (err) {
      toast.erro(mensagemErro(err))
    }
  }

  return (
    <>
      <PageHeader
        titulo="Equipe"
        subtitulo="Cadastre e gerencie os usuários que acessam o sistema."
        acao={<button className="btn btn-primary" onClick={() => setEdit({ ...novo })}>+ Novo usuário</button>}
      />

      {limite && (
        <div className={`alert ${limite.ativos >= limite.limite ? 'alert-warn' : 'alert-info'}`} style={{ marginBottom: 16 }}>
          Vagas de profissional do plano: <strong>{limite.ativos} de {limite.limite}</strong> em uso.
          {limite.ativos >= limite.limite && ' Limite atingido — fale com o suporte para liberar mais vagas.'}
        </div>
      )}

      <div className="toolbar">
        <label className="checkbox field" style={{ marginBottom: 0 }}>
          <input type="checkbox" checked={incluirInativos} onChange={(e) => setIncluirInativos(e.target.checked)} />
          <span className="field-label" style={{ marginBottom: 0 }}>Mostrar inativos</span>
        </label>
      </div>

      <Card style={{ padding: 0 }}>
        {carregando ? (
          <Spinner label="Carregando…" />
        ) : itens.length === 0 ? (
          <EmptyState icone="👥" titulo="Nenhum usuário" texto="Cadastre o primeiro membro da equipe." />
        ) : (
          <div className="table-wrap">
            <table className="data">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>E-mail</th>
                  <th>Papel</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {itens.map((u) => {
                  const ehVoce = u.id === usuario?.id
                  const podeEditar = souSuperAdmin || u.papel !== 'Profissional'
                  return (
                    <tr key={u.id}>
                      <td>
                        <strong style={{ color: 'var(--plum)' }}>{u.nome}</strong>
                        {ehVoce && <small className="muted"> (você)</small>}
                      </td>
                      <td className="mono">{u.email}</td>
                      <td>{PAPEL_ROTULO[u.papel]}</td>
                      <td>{u.ativo ? <Badge texto="Ativo" tom="sucesso" /> : <Badge texto="Inativo" tom="neutro" />}</td>
                      <td className="text-right">
                        {podeEditar ? (
                          <button
                            className="link-btn"
                            onClick={() => setEdit({ id: u.id, nome: u.nome, email: u.email, papel: u.papel, senha: '', ativo: u.ativo })}
                          >
                            Editar
                          </button>
                        ) : (
                          <small className="muted">Gerenciado pelo suporte</small>
                        )}
                        {u.ativo && !ehVoce && (
                          <>
                            {' · '}
                            <button className="link-btn" onClick={() => desativar(u)}>Desativar</button>
                          </>
                        )}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <Modal aberto={!!edit} titulo={edit?.id ? 'Editar usuário' : 'Novo usuário'} onFechar={() => setEdit(null)}>
        {edit && (
          <>
            <Field label="Nome completo">
              <input value={edit.nome} onChange={(e) => setEdit({ ...edit, nome: e.target.value })} autoFocus placeholder="Ex.: Maria Silva" />
            </Field>

            <Field label="E-mail" hint={edit.id ? 'O e-mail de acesso não pode ser alterado.' : 'Usado para o login.'}>
              <input
                type="email"
                value={edit.email}
                onChange={(e) => setEdit({ ...edit, email: e.target.value })}
                placeholder="nome@clinica.com"
                disabled={!!edit.id}
              />
            </Field>

            <div className="form-row">
              <Field label="Papel" hint={!souSuperAdmin ? 'Contas Profissional são cadastradas pelo suporte.' : undefined}>
                <select value={edit.papel} onChange={(e) => setEdit({ ...edit, papel: e.target.value as Papel })}>
                  {papeisDisponiveis.map((p) => (
                    <option key={p} value={p}>{PAPEL_ROTULO[p]}</option>
                  ))}
                </select>
              </Field>
              {(!edit.id || souSuperAdmin) && (
                <Field label={edit.id ? 'Nova senha' : 'Senha inicial'} hint={edit.id ? 'Deixe em branco para manter a atual.' : 'Mínimo 6 caracteres.'}>
                  <input
                    type="password"
                    value={edit.senha}
                    onChange={(e) => setEdit({ ...edit, senha: e.target.value })}
                    autoComplete="new-password"
                    placeholder={edit.id ? '••••••' : ''}
                  />
                </Field>
              )}
            </div>
            {edit.id && !souSuperAdmin && (
              <p className="muted" style={{ fontSize: 13, marginTop: -8 }}>
                Para redefinir a senha desta pessoa, peça para ela trocar em "Trocar senha" — ou fale com o suporte.
              </p>
            )}

            {edit.id && (
              <label className="checkbox">
                <input type="checkbox" checked={edit.ativo} onChange={(e) => setEdit({ ...edit, ativo: e.target.checked })} />
                <span>Usuário ativo</span>
              </label>
            )}

            <div className="row" style={{ justifyContent: 'flex-end', marginTop: 12 }}>
              <button className="btn btn-ghost" onClick={() => setEdit(null)}>Cancelar</button>
              <button className="btn btn-primary" onClick={salvar} disabled={salvando}>{salvando ? 'Salvando…' : 'Salvar'}</button>
            </div>
          </>
        )}
      </Modal>
    </>
  )
}
