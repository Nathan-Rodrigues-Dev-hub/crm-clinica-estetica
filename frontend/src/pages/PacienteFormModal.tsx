import { useEffect, useState } from 'react'
import { Modal } from '../components/Modal'
import { Field } from '../components/ui'
import { pacientesApi } from '../api/services'
import { mensagemErro } from '../api/client'
import { useToast } from '../components/Toast'
import { formatarCpf, formatarTelefone } from '../lib/format'
import type { CriarPacienteRequest, DuplicidadeResponse, PacienteDto } from '../api/types'

interface Props {
  aberto: boolean
  paciente?: PacienteDto | null
  onFechar: () => void
  onSalvo: (p: PacienteDto) => void
}

const vazio: CriarPacienteRequest = { nome: '', cpf: '', telefone: '', dataNascimento: '', sexo: '', endereco: '', redeSocial: '' }

export function PacienteFormModal({ aberto, paciente, onFechar, onSalvo }: Props) {
  const toast = useToast()
  const [form, setForm] = useState<CriarPacienteRequest>(vazio)
  const [salvando, setSalvando] = useState(false)
  const [dup, setDup] = useState<DuplicidadeResponse | null>(null)

  const editando = !!paciente

  useEffect(() => {
    if (aberto) {
      setDup(null)
      setForm(
        paciente
          ? {
              nome: paciente.nome,
              cpf: paciente.cpf ?? '',
              telefone: paciente.telefone ?? '',
              dataNascimento: paciente.dataNascimento ?? '',
              sexo: paciente.sexo ?? '',
              endereco: paciente.endereco ?? '',
              redeSocial: paciente.redeSocial ?? '',
            }
          : vazio,
      )
    }
  }, [aberto, paciente])

  // Checa duplicidade (RF08) quando CPF/telefone mudam, ao criar.
  useEffect(() => {
    if (!aberto || editando) return
    const cpf = (form.cpf ?? '').replace(/\D/g, '')
    const tel = (form.telefone ?? '').replace(/\D/g, '')
    if (cpf.length < 11 && tel.length < 10) {
      setDup(null)
      return
    }
    const t = setTimeout(() => {
      pacientesApi.duplicidade(form.cpf ?? undefined, form.telefone ?? undefined).then((r) => setDup(r.data)).catch(() => {})
    }, 450)
    return () => clearTimeout(t)
  }, [form.cpf, form.telefone, aberto, editando])

  const set = (campo: keyof CriarPacienteRequest, valor: string) => setForm((f) => ({ ...f, [campo]: valor }))

  const salvar = async () => {
    if (!form.nome.trim()) {
      toast.erro('Informe o nome.')
      return
    }
    setSalvando(true)
    try {
      const payload: CriarPacienteRequest = {
        nome: form.nome.trim(),
        cpf: form.cpf?.trim() || null,
        telefone: form.telefone?.trim() || null,
        dataNascimento: form.dataNascimento || null,
        sexo: form.sexo || null,
        endereco: form.endereco?.trim() || null,
        redeSocial: form.redeSocial?.trim() || null,
      }
      const res = editando ? await pacientesApi.atualizar(paciente!.id, payload) : await pacientesApi.criar(payload)
      toast.sucesso(editando ? 'Paciente atualizado.' : 'Paciente cadastrado.')
      onSalvo(res.data)
      onFechar()
    } catch (err) {
      toast.erro(mensagemErro(err))
    } finally {
      setSalvando(false)
    }
  }

  const temDup = dup?.haPossivelDuplicidade

  return (
    <Modal aberto={aberto} titulo={editando ? 'Editar paciente' : 'Novo paciente'} onFechar={onFechar} largura={620}>
      <Field label="Nome completo">
        <input value={form.nome} onChange={(e) => set('nome', e.target.value)} autoFocus placeholder="Ex.: Maria Silva" />
      </Field>

      <div className="form-row">
        <Field label="CPF" hint="Opcional — pode ficar pendente.">
          <input value={form.cpf ?? ''} onChange={(e) => set('cpf', e.target.value)} placeholder="000.000.000-00" />
        </Field>
        <Field label="Telefone / WhatsApp">
          <input value={form.telefone ?? ''} onChange={(e) => set('telefone', e.target.value)} placeholder="(11) 99999-0000" />
        </Field>
      </div>

      {temDup && (
        <div className="alert alert-warn">
          <strong>Possível duplicidade:</strong>
          <ul style={{ margin: '6px 0 0', paddingLeft: 18 }}>
            {[...(dup?.porCpf ?? []), ...(dup?.porTelefone ?? [])].map((p) => (
              <li key={p.id}>{p.nome} — {formatarCpf(p.cpf)} · {formatarTelefone(p.telefone)}</li>
            ))}
          </ul>
        </div>
      )}

      <div className="form-row">
        <Field label="Data de nascimento">
          <input type="date" value={form.dataNascimento ?? ''} onChange={(e) => set('dataNascimento', e.target.value)} />
        </Field>
        <Field label="Sexo">
          <select value={form.sexo ?? ''} onChange={(e) => set('sexo', e.target.value)}>
            <option value="">—</option>
            <option value="F">Feminino</option>
            <option value="M">Masculino</option>
            <option value="Outro">Outro</option>
          </select>
        </Field>
      </div>

      <Field label="Endereço">
        <input value={form.endereco ?? ''} onChange={(e) => set('endereco', e.target.value)} placeholder="Rua, nº, bairro, cidade" />
      </Field>
      <Field label="Rede social">
        <input value={form.redeSocial ?? ''} onChange={(e) => set('redeSocial', e.target.value)} placeholder="@instagram" />
      </Field>

      <div className="row" style={{ justifyContent: 'flex-end', marginTop: 8 }}>
        <button className="btn btn-ghost" onClick={onFechar}>Cancelar</button>
        <button className="btn btn-primary" onClick={salvar} disabled={salvando}>
          {salvando ? 'Salvando…' : editando ? 'Salvar alterações' : 'Cadastrar'}
        </button>
      </div>
    </Modal>
  )
}
