import { useEffect, useState } from 'react'
import { Modal } from './Modal'
import { midiasApi } from '../api/services'
import type { MidiaDto } from '../api/types'

/** Visualização em tela cheia da mídia (foto/vídeo/documento) a partir da galeria. */
export function MidiaLightbox({ midia, onFechar }: { midia: MidiaDto; onFechar: () => void }) {
  const [url, setUrl] = useState<string | null>(null)
  const ehImagem = midia.contentType.startsWith('image/')
  const ehVideo = midia.contentType.startsWith('video/')

  useEffect(() => {
    let ativo = true
    let objUrl = ''
    midiasApi.conteudo(midia.id).then((r) => {
      if (!ativo) return
      objUrl = URL.createObjectURL(r.data)
      setUrl(objUrl)
    }).catch(() => {})
    return () => { ativo = false; if (objUrl) URL.revokeObjectURL(objUrl) }
  }, [midia.id])

  return (
    <Modal aberto titulo={midia.nomeArquivo} onFechar={onFechar} largura={1100}>
      <div style={{ display: 'grid', placeItems: 'center', minHeight: 200 }}>
        {!url ? (
          <p className="muted">Carregando…</p>
        ) : ehImagem ? (
          <img src={url} alt={midia.nomeArquivo} style={{ maxWidth: '100%', maxHeight: '78vh', borderRadius: 'var(--radius)' }} />
        ) : ehVideo ? (
          <video src={url} controls style={{ maxWidth: '100%', maxHeight: '78vh', borderRadius: 'var(--radius)' }} />
        ) : (
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontSize: 48 }}>📄</div>
            <p className="muted">Pré-visualização indisponível para este tipo de arquivo.</p>
            <a className="btn btn-primary" href={url} download={midia.nomeArquivo}>Baixar {midia.nomeArquivo}</a>
          </div>
        )}
      </div>
    </Modal>
  )
}
