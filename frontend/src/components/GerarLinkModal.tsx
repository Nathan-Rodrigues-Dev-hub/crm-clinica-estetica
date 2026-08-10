import { useEffect, useState } from 'react'
import { Modal } from './Modal'
import { Field } from './ui'
import { anamneseApi, pacientesApi } from '../api/services'
import { mensagemErro } from '../api/client'
import { useToast } from './Toast'
import { linkWhatsApp } from '../lib/format'
import type { LinkGeradoDto, PacienteListItem } from '../api/types'

/** Gera um link tokenizado de anamnese e oferece copiar / abrir no WhatsApp. */
export function GerarLinkModal({ aberto, onFechar }: { aberto: boolean; onFechar: () => void }) {
  const toast = useToast()
  const [telefone, setTelefone] = useState('')
  const [validade, setValidade] = useState(48)
  const [gerando, setGerando] = useState(false)
  const [link, setLink] = useState<LinkGeradoDto | null>(null)

  const [pacienteJaExiste, setPacienteJaExiste] = useState(false)
  const [buscaPaciente, setBuscaPaciente] = useState('')
  const [resultadosBusca, setResultadosBusca] = useState<PacienteListItem[]>([])
  const [buscando, setBuscando] = useState(false)
  const [pacienteSelecionado, setPacienteSelecionado] = useState<PacienteListItem | null>(null)

  useEffect(() => {
    if (!pacienteJaExiste || pacienteSelecionado || !buscaPaciente.trim()) { setResultadosBusca([]); return }
    setBuscando(true)
    const id = setTimeout(() => {
      pacientesApi.buscar(buscaPaciente).then((r) => setResultadosBusca(r.data)).catch(() => {}).finally(() => setBuscando(false))
    }, 300)
    return () => clearTimeout(id)
  }, [buscaPaciente, pacienteJaExiste, pacienteSelecionado])

  const fechar = () => {
    setLink(null)
    setTelefone('')
    setPacienteJaExiste(false)
    setBuscaPaciente('')
    setResultadosBusca([])
    setPacienteSelecionado(null)
    onFechar()
  }

  const selecionarPaciente = (p: PacienteListItem) => {
    setPacienteSelecionado(p)
    setBuscaPaciente('')
    setResultadosBusca([])
    if (p.telefone) setTelefone(p.telefone)
  }

  const gerar = async () => {
    if (pacienteJaExiste && !pacienteSelecionado) {
      toast.erro('Selecione o paciente cadastrado no sistema.')
      return
    }
    setGerando(true)
    try {
      const res = await anamneseApi.gerarLink(telefone, validade, pacienteJaExiste ? pacienteSelecionado?.id : undefined)
      setLink(res.data)
      toast.sucesso('Link gerado.')
    } catch (err) {
      toast.erro(mensagemErro(err))
    } finally {
      setGerando(false)
    }
  }

  const urlCompleta = link ? `${location.origin}/anamnese/${link.token}` : ''

  const copiar = async () => {
    await navigator.clipboard.writeText(urlCompleta)
    toast.info('Link copiado.')
  }

  return (
    <Modal aberto={aberto} titulo="Gerar link de anamnese" onFechar={fechar}>
      {!link ? (
        <>
          <p className="muted" style={{ marginBottom: 16 }}>
            Envie um link seguro para a paciente preencher a anamnese sem precisar de login. O link expira e é de uso único.
          </p>

          <label className="checkbox" style={{ marginBottom: 14 }}>
            <input
              type="checkbox"
              checked={pacienteJaExiste}
              onChange={(e) => { setPacienteJaExiste(e.target.checked); setPacienteSelecionado(null); setBuscaPaciente('') }}
            />
            <span>Paciente já cadastrado no sistema</span>
          </label>

          {pacienteJaExiste && (
            pacienteSelecionado ? (
              <div className="alert alert-info" style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 14 }}>
                <span>Vinculado a <strong>{pacienteSelecionado.nome}</strong></span>
                <button type="button" className="link-btn" onClick={() => setPacienteSelecionado(null)}>trocar</button>
              </div>
            ) : (
              <>
                <Field label="Buscar paciente" hint="Nome, CPF ou telefone.">
                  <input value={buscaPaciente} onChange={(e) => setBuscaPaciente(e.target.value)} placeholder="Digite para buscar…" autoFocus />
                </Field>
                {buscaPaciente.trim() && (
                  <div className="card" style={{ padding: 6, marginTop: -10, marginBottom: 14, maxHeight: 180, overflowY: 'auto' }}>
                    {buscando ? (
                      <p className="muted" style={{ padding: 8 }}>Buscando…</p>
                    ) : resultadosBusca.length === 0 ? (
                      <p className="muted" style={{ padding: 8 }}>Nenhum paciente encontrado.</p>
                    ) : (
                      resultadosBusca.map((p) => (
                        <button
                          key={p.id}
                          type="button"
                          className="btn btn-ghost btn-block"
                          style={{ justifyContent: 'flex-start' }}
                          onClick={() => selecionarPaciente(p)}
                        >
                          {p.nome}{p.cpf ? ` · ${p.cpf}` : ''}
                        </button>
                      ))
                    )}
                  </div>
                )}
              </>
            )
          )}

          <Field label="Telefone / WhatsApp da paciente (opcional)" hint="Usado para abrir a conversa já com o link.">
            <input value={telefone} onChange={(e) => setTelefone(e.target.value)} placeholder="(11) 99999-0000" />
          </Field>
          <Field label="Validade (horas)">
            <input type="number" min={1} max={720} value={validade} onChange={(e) => setValidade(Number(e.target.value))} />
          </Field>
          <button className="btn btn-primary btn-block" onClick={gerar} disabled={gerando}>
            {gerando ? 'Gerando…' : 'Gerar link'}
          </button>
        </>
      ) : (
        <>
          <div className="alert alert-info">
            Link criado{pacienteSelecionado ? <> — vinculado a <strong>{pacienteSelecionado.nome}</strong></> : ''}! Compartilhe com a paciente:
          </div>
          <Field label="Endereço do link">
            <input readOnly value={urlCompleta} className="mono" onFocus={(e) => e.target.select()} />
          </Field>
          <div className="row">
            <button className="btn btn-soft" onClick={copiar}>Copiar link</button>
            <a className="btn btn-success" href={linkWhatsApp(telefone, `Olá! Preencha sua anamnese neste link seguro: ${urlCompleta}`)} target="_blank" rel="noreferrer">
              Abrir no WhatsApp
            </a>
            <a className="btn btn-ghost" href={urlCompleta} target="_blank" rel="noreferrer">Abrir link</a>
          </div>
          <hr className="divider" />
          <button className="btn btn-ghost btn-block" onClick={() => { setLink(null); setPacienteJaExiste(false); setPacienteSelecionado(null) }}>Gerar outro</button>
        </>
      )}
    </Modal>
  )
}
