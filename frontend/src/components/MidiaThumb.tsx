import { useEffect, useState } from 'react'
import { midiasApi } from '../api/services'
import type { MidiaDto } from '../api/types'

/** Baixa o binário autenticado e exibe imagem (ou ícone para outros tipos). */
export function MidiaThumb({ midia }: { midia: MidiaDto }) {
  const [url, setUrl] = useState<string | null>(null)
  const ehImagem = midia.contentType.startsWith('image/')

  useEffect(() => {
    if (!ehImagem) return
    let ativo = true
    let objUrl = ''
    midiasApi.conteudo(midia.id).then((r) => {
      if (!ativo) return
      objUrl = URL.createObjectURL(r.data)
      setUrl(objUrl)
    }).catch(() => {})
    return () => { ativo = false; if (objUrl) URL.revokeObjectURL(objUrl) }
  }, [midia.id, ehImagem])

  if (!ehImagem) return <div className="midia-thumb midia-thumb-file">📄</div>
  if (!url) return <div className="midia-thumb midia-thumb-file">…</div>
  return <img className="midia-thumb" src={url} alt={midia.nomeArquivo} />
}
