import { useEffect, useState } from 'react'
import { midiasApi, procedimentosApi } from '../api/services'
import { mensagemErro } from '../api/client'
import { useToast } from './Toast'
import { Modal } from './Modal'
import { Field } from './ui'
import { MidiaThumb } from './MidiaThumb'
import { MidiaLightbox } from './MidiaLightbox'
import { formatarData } from '../lib/format'
import type { EtiquetaMidia, MidiaDto, ProcedimentoDto } from '../api/types'

const ETIQUETAS: EtiquetaMidia[] = ['Antes', 'Depois', 'Durante', 'Documento']

function tomEtiqueta(e: EtiquetaMidia) {
  return e === 'Antes' ? 'info' : e === 'Depois' ? 'sucesso' : e === 'Durante' ? 'alerta' : 'neutro'
}

export function MidiaGaleria({ pacienteId }: { pacienteId: string }) {
  const toast = useToast()
  const [itens, setItens] = useState<MidiaDto[]>([])
  const [carregando, setCarregando] = useState(true)
  const [modalUpload, setModalUpload] = useState(false)
  const [visualizando, setVisualizando] = useState<MidiaDto | null>(null)
  const [renomeandoId, setRenomeandoId] = useState<string | null>(null)
  const [novoNome, setNovoNome] = useState('')
  const [salvandoNome, setSalvandoNome] = useState(false)

  const carregar = () => {
    setCarregando(true)
    midiasApi.listar(pacienteId).then((r) => setItens(r.data)).catch((e) => toast.erro(mensagemErro(e))).finally(() => setCarregando(false))
  }

  useEffect(carregar, [pacienteId])

  const iniciarRenomear = (m: MidiaDto) => {
    setRenomeandoId(m.id)
    setNovoNome(m.nomeArquivo)
  }

  const salvarNome = async (m: MidiaDto) => {
    if (!novoNome.trim()) return toast.erro('Informe um nome para o arquivo.')
    setSalvandoNome(true)
    try {
      const res = await midiasApi.renomear(m.id, novoNome.trim())
      setItens((prev) => prev.map((it) => (it.id === m.id ? res.data : it)))
      toast.sucesso('Arquivo renomeado.')
      setRenomeandoId(null)
    } catch (e) {
      toast.erro(mensagemErro(e))
    } finally {
      setSalvandoNome(false)
    }
  }

  const baixar = async (m: MidiaDto) => {
    try {
      const r = await midiasApi.conteudo(m.id)
      const url = URL.createObjectURL(r.data)
      const a = document.createElement('a')
      a.href = url
      a.download = m.nomeArquivo
      a.click()
      URL.revokeObjectURL(url)
    } catch (e) {
      toast.erro(mensagemErro(e))
    }
  }

  const remover = async (m: MidiaDto) => {
    if (!confirm(`Remover "${m.nomeArquivo}"? Esta ação não pode ser desfeita.`)) return
    try {
      await midiasApi.remover(m.id)
      toast.sucesso('Mídia removida.')
      carregar()
    } catch (e) {
      toast.erro(mensagemErro(e))
    }
  }

  return (
    <div className="card">
      <div className="spread" style={{ marginBottom: 14 }}>
        <div>
          <h3>Galeria de mídia</h3>
          <p className="muted" style={{ fontSize: 13 }}>Fotos antes/depois e documentos — dado clínico sensível.</p>
        </div>
        <button className="btn btn-primary" onClick={() => setModalUpload(true)}>+ Enviar mídia</button>
      </div>

      {carregando ? (
        <p className="muted">Carregando…</p>
      ) : itens.length === 0 ? (
        <div className="empty"><div className="empty-icon">🖼️</div><strong>Sem mídias</strong><p className="muted">Envie a primeira foto ou documento.</p></div>
      ) : (
        <div className="midia-grid">
          {itens.map((m) => (
            <div key={m.id} className="midia-card">
              <div className="midia-thumb-wrap" onClick={() => setVisualizando(m)}>
                <MidiaThumb midia={m} />
                <button className="midia-maximize" onClick={(e) => { e.stopPropagation(); setVisualizando(m) }} title="Ver em tela cheia" aria-label="Ver em tela cheia">⛶</button>
              </div>
              <div className="midia-info">
                <div className="row spread">
                  <span className={`badge badge-${tomEtiqueta(m.etiqueta)}`}>{m.etiqueta}</span>
                  <small className="muted">{formatarData(m.data)}</small>
                </div>

                {renomeandoId === m.id ? (
                  <div className="row" style={{ marginTop: 8, gap: 6 }}>
                    <input
                      value={novoNome}
                      onChange={(e) => setNovoNome(e.target.value)}
                      onKeyDown={(e) => e.key === 'Enter' && salvarNome(m)}
                      autoFocus
                      style={{ flex: 1, minWidth: 0 }}
                    />
                    <button className="btn btn-primary btn-sm" onClick={() => salvarNome(m)} disabled={salvandoNome}>OK</button>
                    <button className="btn btn-ghost btn-sm" onClick={() => setRenomeandoId(null)}>✕</button>
                  </div>
                ) : (
                  <p className="midia-nome" title={m.nomeArquivo}>{m.nomeArquivo}</p>
                )}

                <div className="pill-list" style={{ marginTop: 6 }}>
                  {m.consentimentoClinico && <span className="badge badge-neutro">Consent. clínico</span>}
                  {m.consentimentoPublico && <span className="badge badge-neutro">Consent. público</span>}
                </div>
                <div className="row" style={{ marginTop: 8, gap: 6, flexWrap: 'wrap' }}>
                  <button className="btn btn-ghost btn-sm" onClick={() => iniciarRenomear(m)}>Renomear</button>
                  <button className="btn btn-ghost btn-sm" onClick={() => baixar(m)}>Baixar</button>
                  <button className="btn btn-danger btn-sm" onClick={() => remover(m)}>Excluir</button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {modalUpload && <UploadModal pacienteId={pacienteId} onFechar={() => setModalUpload(false)} onEnviado={() => { setModalUpload(false); carregar() }} />}
      {visualizando && <MidiaLightbox midia={visualizando} onFechar={() => setVisualizando(null)} />}
    </div>
  )
}

function UploadModal({ pacienteId, onFechar, onEnviado }: { pacienteId: string; onFechar: () => void; onEnviado: () => void }) {
  const toast = useToast()
  const [arquivo, setArquivo] = useState<File | null>(null)
  const [etiqueta, setEtiqueta] = useState<EtiquetaMidia>('Antes')
  const [procId, setProcId] = useState('')
  const [clinico, setClinico] = useState(true)
  const [publico, setPublico] = useState(false)
  const [procs, setProcs] = useState<ProcedimentoDto[]>([])
  const [enviando, setEnviando] = useState(false)

  useEffect(() => { procedimentosApi.listar(false).then((r) => setProcs(r.data)).catch(() => {}) }, [])

  const enviar = async () => {
    if (!arquivo) return toast.erro('Selecione um arquivo.')
    setEnviando(true)
    try {
      const form = new FormData()
      form.append('pacienteId', pacienteId)
      form.append('etiqueta', etiqueta)
      if (procId) form.append('procedimentoId', procId)
      form.append('consentimentoClinico', String(clinico))
      form.append('consentimentoPublico', String(publico))
      form.append('arquivo', arquivo)
      await midiasApi.upload(form)
      toast.sucesso('Mídia enviada.')
      onEnviado()
    } catch (e) {
      toast.erro(mensagemErro(e))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <Modal aberto titulo="Enviar mídia" onFechar={onFechar}>
      <Field label="Arquivo (imagem, PDF…)">
        <input type="file" accept="image/*,application/pdf" onChange={(e) => setArquivo(e.target.files?.[0] ?? null)} />
      </Field>
      <div className="form-row">
        <Field label="Etiqueta">
          <select value={etiqueta} onChange={(e) => setEtiqueta(e.target.value as EtiquetaMidia)}>
            {ETIQUETAS.map((et) => <option key={et} value={et}>{et}</option>)}
          </select>
        </Field>
        <Field label="Procedimento (opcional)">
          <select value={procId} onChange={(e) => setProcId(e.target.value)}>
            <option value="">—</option>
            {procs.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
          </select>
        </Field>
      </div>
      <label className="checkbox"><input type="checkbox" checked={clinico} onChange={(e) => setClinico(e.target.checked)} /><span>Consentimento para uso clínico</span></label>
      <label className="checkbox" style={{ marginTop: 8 }}><input type="checkbox" checked={publico} onChange={(e) => setPublico(e.target.checked)} /><span>Consentimento para uso público / marketing</span></label>
      <div className="row" style={{ justifyContent: 'flex-end', marginTop: 14 }}>
        <button className="btn btn-ghost" onClick={onFechar}>Cancelar</button>
        <button className="btn btn-primary" onClick={enviar} disabled={enviando}>{enviando ? 'Enviando…' : 'Enviar'}</button>
      </div>
    </Modal>
  )
}
