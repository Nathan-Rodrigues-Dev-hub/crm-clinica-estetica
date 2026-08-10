import { useEffect, useState } from 'react'
import { procedimentosApi } from '../api/services'
import { useAuth } from '../auth/AuthContext'
import { useToast } from '../components/Toast'
import { mensagemErro } from '../api/client'
import { Badge, Card, EmptyState, Field, PageHeader } from '../components/ui'
import { Modal } from '../components/Modal'
import { Spinner } from '../components/Spinner'
import { formatarMoeda } from '../lib/format'
import type { AtualizarProcedimentoRequest, ProcedimentoDto } from '../api/types'

interface Editando {
  id?: string
  nome: string
  descricao: string
  categoria: string
  duracaoMinutos: number
  valorReferencia: number
  ativo: boolean
}

const novo: Editando = { nome: '', descricao: '', categoria: '', duracaoMinutos: 60, valorReferencia: 0, ativo: true }

export function CatalogoPage() {
  const { temPapel } = useAuth()
  const toast = useToast()
  const podeEscrever = temPapel('Admin', 'Recepcao', 'SuperAdmin')

  const [itens, setItens] = useState<ProcedimentoDto[]>([])
  const [carregando, setCarregando] = useState(true)
  const [incluirInativos, setIncluirInativos] = useState(false)
  const [edit, setEdit] = useState<Editando | null>(null)
  const [salvando, setSalvando] = useState(false)

  const carregar = (inativos: boolean) => {
    setCarregando(true)
    procedimentosApi.listar(inativos).then((r) => setItens(r.data)).finally(() => setCarregando(false))
  }

  useEffect(() => carregar(incluirInativos), [incluirInativos])

  const salvar = async () => {
    if (!edit) return
    if (!edit.nome.trim()) return toast.erro('Informe o nome.')
    if (edit.duracaoMinutos <= 0) return toast.erro('Duração deve ser maior que zero.')
    setSalvando(true)
    try {
      if (edit.id) {
        const req: AtualizarProcedimentoRequest = {
          nome: edit.nome.trim(), descricao: edit.descricao || null, categoria: edit.categoria || null,
          duracaoMinutos: edit.duracaoMinutos, valorReferencia: edit.valorReferencia, ativo: edit.ativo,
        }
        await procedimentosApi.atualizar(edit.id, req)
      } else {
        await procedimentosApi.criar({
          nome: edit.nome.trim(), descricao: edit.descricao || null, categoria: edit.categoria || null,
          duracaoMinutos: edit.duracaoMinutos, valorReferencia: edit.valorReferencia,
        })
      }
      toast.sucesso('Procedimento salvo.')
      setEdit(null)
      carregar(incluirInativos)
    } catch (err) {
      toast.erro(mensagemErro(err))
    } finally {
      setSalvando(false)
    }
  }

  return (
    <>
      <PageHeader
        titulo="Catálogo de procedimentos"
        subtitulo="Nome, categoria, duração e valor de referência."
        acao={podeEscrever && <button className="btn btn-primary" onClick={() => setEdit({ ...novo })}>+ Novo procedimento</button>}
      />

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
          <EmptyState icone="✦" titulo="Catálogo vazio" texto={podeEscrever ? 'Cadastre o primeiro procedimento.' : 'Nenhum procedimento cadastrado.'} />
        ) : (
          <div className="table-wrap">
            <table className="data">
              <thead>
                <tr>
                  <th>Procedimento</th>
                  <th>Categoria</th>
                  <th>Duração</th>
                  <th>Valor ref.</th>
                  <th>Status</th>
                  {podeEscrever && <th></th>}
                </tr>
              </thead>
              <tbody>
                {itens.map((p) => (
                  <tr key={p.id}>
                    <td>
                      <strong style={{ color: 'var(--plum)' }}>{p.nome}</strong>
                      {p.descricao && <div><small className="muted">{p.descricao}</small></div>}
                    </td>
                    <td>{p.categoria ?? '—'}</td>
                    <td>{p.duracaoMinutos} min</td>
                    <td>{formatarMoeda(p.valorReferencia)}</td>
                    <td>{p.ativo ? <Badge texto="Ativo" tom="sucesso" /> : <Badge texto="Inativo" tom="neutro" />}</td>
                    {podeEscrever && (
                      <td className="text-right">
                        <button className="link-btn" onClick={() => setEdit({ id: p.id, nome: p.nome, descricao: p.descricao ?? '', categoria: p.categoria ?? '', duracaoMinutos: p.duracaoMinutos, valorReferencia: p.valorReferencia, ativo: p.ativo })}>
                          Editar
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <Modal aberto={!!edit} titulo={edit?.id ? 'Editar procedimento' : 'Novo procedimento'} onFechar={() => setEdit(null)}>
        {edit && (
          <>
            <Field label="Nome">
              <input value={edit.nome} onChange={(e) => setEdit({ ...edit, nome: e.target.value })} autoFocus />
            </Field>
            <Field label="Descrição">
              <textarea value={edit.descricao} onChange={(e) => setEdit({ ...edit, descricao: e.target.value })} />
            </Field>
            <div className="form-row-3">
              <Field label="Categoria">
                <input value={edit.categoria} onChange={(e) => setEdit({ ...edit, categoria: e.target.value })} placeholder="Facial, Corporal…" />
              </Field>
              <Field label="Duração (min)">
                <input type="number" min={1} value={edit.duracaoMinutos} onChange={(e) => setEdit({ ...edit, duracaoMinutos: Number(e.target.value) })} />
              </Field>
              <Field label="Valor ref. (R$)">
                <input type="number" min={0} step="0.01" value={edit.valorReferencia} onChange={(e) => setEdit({ ...edit, valorReferencia: Number(e.target.value) })} />
              </Field>
            </div>
            {edit.id && (
              <label className="checkbox">
                <input type="checkbox" checked={edit.ativo} onChange={(e) => setEdit({ ...edit, ativo: e.target.checked })} />
                <span>Procedimento ativo</span>
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
