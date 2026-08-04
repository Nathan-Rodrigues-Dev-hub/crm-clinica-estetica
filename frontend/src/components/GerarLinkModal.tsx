import { useState } from 'react'
import { Modal } from './Modal'
import { Field } from './ui'
import { anamneseApi } from '../api/services'
import { mensagemErro } from '../api/client'
import { useToast } from './Toast'
import { linkWhatsApp } from '../lib/format'
import type { LinkGeradoDto } from '../api/types'

/** Gera um link tokenizado de anamnese e oferece copiar / abrir no WhatsApp. */
export function GerarLinkModal({ aberto, onFechar }: { aberto: boolean; onFechar: () => void }) {
  const toast = useToast()
  const [telefone, setTelefone] = useState('')
  const [validade, setValidade] = useState(48)
  const [gerando, setGerando] = useState(false)
  const [link, setLink] = useState<LinkGeradoDto | null>(null)

  const fechar = () => {
    setLink(null)
    setTelefone('')
    onFechar()
  }

  const gerar = async () => {
    setGerando(true)
    try {
      const res = await anamneseApi.gerarLink(telefone, validade)
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
          <div className="alert alert-info">Link criado! Compartilhe com a paciente:</div>
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
          <button className="btn btn-ghost btn-block" onClick={() => setLink(null)}>Gerar outro</button>
        </>
      )}
    </Modal>
  )
}
