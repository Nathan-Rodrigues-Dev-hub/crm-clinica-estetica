import { useEffect, useRef, useState } from 'react'
import { useParams } from 'react-router-dom'
import { publicAnamneseApi } from '../../api/services'
import { mensagemErro } from '../../api/client'
import { Field } from '../../components/ui'
import { Spinner } from '../../components/Spinner'
import type { AberturaLinkDto, IntakeResultado, RespostasAnamnese } from '../../api/types'

type Etapa = 'carregando' | 'invalido' | 'procedimento' | 'formulario' | 'concluido'

interface DadosPaciente {
  nome: string
  cpf: string
  telefone: string
  dataNascimento: string
  sexo: string
  endereco: string
}

// ------------------------------------------------------------- rascunho local
// Épico C (#18): não perder o que a paciente digita se a aba fechar/cair a rede.
// Persistido no localStorage, chaveado pelo token do link.
type EtapaRascunho = 'procedimento' | 'formulario'

interface Rascunho {
  procId: string
  outro: boolean
  outroTexto: string
  dados: DadosPaciente
  peso: string
  altura: string
  motivo: string
  respostas: RespostasAnamnese
  aceite: boolean
  etapa: EtapaRascunho
  ts: number
}

const chaveRascunho = (token: string) => `crm.rascunho.${token}`

function lerRascunho(token: string): Rascunho | null {
  try {
    const raw = localStorage.getItem(chaveRascunho(token))
    return raw ? (JSON.parse(raw) as Rascunho) : null
  } catch {
    return null // localStorage indisponível ou JSON corrompido — segue sem rascunho
  }
}

function salvarRascunho(token: string, r: Rascunho): boolean {
  try {
    localStorage.setItem(chaveRascunho(token), JSON.stringify(r))
    return true
  } catch {
    return false // modo privado, cota cheia, etc. — degrada sem quebrar o fluxo
  }
}

function limparRascunho(token: string) {
  try {
    localStorage.removeItem(chaveRascunho(token))
  } catch {
    /* ignore */
  }
}

// Só recupera/mostra banner se o rascunho tiver algo de fato preenchido.
function rascunhoTemConteudo(r: Rascunho): boolean {
  const d = r.dados
  return !!(
    d.nome || d.cpf || d.telefone || d.dataNascimento || d.sexo || d.endereco ||
    r.procId || r.outroTexto || r.peso || r.altura || r.motivo || r.aceite ||
    (r.respostas && Object.keys(r.respostas).length > 0)
  )
}

// Perguntas Sim/Não do modelo fixo (com detalhe onde indicado).
const PERGUNTAS: { chave: keyof RespostasAnamnese; label: string; detalhe?: keyof RespostasAnamnese; detalheLabel?: string }[] = [
  { chave: 'usaLentesContato', label: 'Usa lentes de contato?' },
  { chave: 'epilepsiaConvulsoes', label: 'Tem epilepsia ou convulsões?' },
  { chave: 'funcionamentoIntestinalRegular', label: 'Funcionamento intestinal é regular?' },
  { chave: 'tratamentoFacialAnterior', label: 'Já fez tratamento facial anteriormente?' },
  { chave: 'ingereAgua', label: 'Ingere água regularmente?' },
  { chave: 'ingereBebidaAlcoolica', label: 'Ingere bebida alcoólica?' },
  { chave: 'exposicaoSol', label: 'Exposição ao sol?', detalhe: 'exposicaoSolFrequencia', detalheLabel: 'Com que frequência?' },
  { chave: 'usaFiltroSolar', label: 'Usa filtro solar?' },
  { chave: 'periodoMenstrual', label: 'Está no período menstrual?' },
  { chave: 'proteseCorporalFacial', label: 'Possui prótese corporal ou facial?' },
  { chave: 'tabagismo', label: 'Tabagismo?' },
  { chave: 'alteracoesCardiacas', label: 'Tem alterações cardíacas?' },
  { chave: 'marcapasso', label: 'Usa marca-passo?' },
  { chave: 'gestante', label: 'Está gestante?', detalhe: 'semanasGestacao', detalheLabel: 'Semanas de gestação' },
  { chave: 'usaCremesLocoes', label: 'Usa cremes ou loções?', detalhe: 'cremesLocoesDetalhe', detalheLabel: 'Quais?' },
  { chave: 'praticaAtividadeFisica', label: 'Pratica atividade física?', detalhe: 'atividadeFisicaDetalhe', detalheLabel: 'Qual?' },
  { chave: 'usaAnticoncepcional', label: 'Usa anticoncepcional?', detalhe: 'anticoncepcionalQual', detalheLabel: 'Qual?' },
  { chave: 'possuiAlergia', label: 'Possui alergia?', detalhe: 'alergiaDetalhe', detalheLabel: 'A quê?' },
  { chave: 'boaAlimentacao', label: 'Considera ter boa alimentação?' },
  { chave: 'problemasDePele', label: 'Tem problemas de pele?' },
  { chave: 'emTratamentoMedico', label: 'Está em tratamento médico?' },
  { chave: 'tumorLesaoPreCancerosa', label: 'Possui tumor ou lesão pré-cancerosa?' },
]

function SimNao({ valor, onChange }: { valor: boolean | null | undefined; onChange: (v: boolean) => void }) {
  return (
    <div className="sn-group">
      <button type="button" className={`sn-btn sn-btn-sim ${valor === true ? 'sn-on' : ''}`} onClick={() => onChange(true)}>Sim</button>
      <button type="button" className={`sn-btn sn-btn-nao ${valor === false ? 'sn-on' : ''}`} onClick={() => onChange(false)}>Não</button>
    </div>
  )
}

export function AnamnesePublicaPage() {
  const { token = '' } = useParams()
  const [etapa, setEtapa] = useState<Etapa>('carregando')
  const [abertura, setAbertura] = useState<AberturaLinkDto | null>(null)
  const [erro, setErro] = useState('')

  const [procId, setProcId] = useState<string>('')
  const [outro, setOutro] = useState(false)
  const [outroTexto, setOutroTexto] = useState('')

  const [dados, setDados] = useState<DadosPaciente>({ nome: '', cpf: '', telefone: '', dataNascimento: '', sexo: '', endereco: '' })
  const [peso, setPeso] = useState('')
  const [altura, setAltura] = useState('')
  const [motivo, setMotivo] = useState('')
  const [respostas, setRespostas] = useState<RespostasAnamnese>({})
  const [aceite, setAceite] = useState(false)
  const [enviando, setEnviando] = useState(false)
  const [resultado, setResultado] = useState<IntakeResultado | null>(null)

  // Autosave (Épico C): banner de recuperação, indicador "salvo" e trava até restaurar.
  const [rascunhoRecuperado, setRascunhoRecuperado] = useState(false)
  const [salvoEm, setSalvoEm] = useState<number | null>(null)
  const prontoRef = useRef(false)

  useEffect(() => {
    publicAnamneseApi.abrir(token)
      .then((r) => {
        setAbertura(r.data)
        if (!r.data.valido) {
          setEtapa('invalido')
          setErro(r.data.motivo ?? 'Link inválido.')
          return
        }
        const rasc = lerRascunho(token)
        if (rasc && rascunhoTemConteudo(rasc)) {
          setProcId(rasc.procId)
          setOutro(rasc.outro)
          setOutroTexto(rasc.outroTexto)
          setDados(rasc.dados)
          setPeso(rasc.peso)
          setAltura(rasc.altura)
          setMotivo(rasc.motivo)
          setRespostas(rasc.respostas ?? {})
          setAceite(rasc.aceite)
          setEtapa(rasc.etapa === 'formulario' ? 'formulario' : 'procedimento')
          setRascunhoRecuperado(true)
        } else {
          setEtapa('procedimento')
        }
        prontoRef.current = true // libera o autosave só depois de tentar restaurar
      })
      .catch((e) => { setEtapa('invalido'); setErro(mensagemErro(e, 'Link inválido ou expirado.')) })
  }, [token])

  // Persiste o preenchimento com debounce (~600ms) enquanto a paciente digita.
  useEffect(() => {
    if (!prontoRef.current) return
    if (etapa !== 'procedimento' && etapa !== 'formulario') return
    const id = setTimeout(() => {
      const ok = salvarRascunho(token, {
        procId, outro, outroTexto, dados, peso, altura, motivo, respostas, aceite,
        etapa: etapa as EtapaRascunho, ts: Date.now(),
      })
      if (ok) setSalvoEm(Date.now())
    }, 600)
    return () => clearTimeout(id)
  }, [token, etapa, procId, outro, outroTexto, dados, peso, altura, motivo, respostas, aceite])

  const setResp = (chave: keyof RespostasAnamnese, valor: unknown) => setRespostas((r) => ({ ...r, [chave]: valor }))

  const avancarProcedimento = () => {
    if (!outro && !procId) { setErro('Selecione um procedimento de interesse.'); return }
    if (outro && !outroTexto.trim()) { setErro('Descreva o procedimento de interesse.'); return }
    setErro('')
    setEtapa('formulario')
    window.scrollTo(0, 0)
  }

  const enviar = async () => {
    if (!dados.nome.trim()) { setErro('Informe seu nome.'); return }
    if (!aceite) { setErro('É necessário aceitar a declaração de veracidade.'); return }
    setErro('')
    setEnviando(true)
    try {
      const payload = {
        procedimentoInteresseId: outro ? null : procId,
        procedimentoInteresseTexto: outro ? outroTexto.trim() : null,
        paciente: {
          nome: dados.nome.trim(),
          cpf: dados.cpf.trim() || null,
          telefone: dados.telefone.trim() || null,
          dataNascimento: dados.dataNascimento || null,
          sexo: dados.sexo || null,
          endereco: dados.endereco.trim() || null,
          redeSocial: null,
        },
        anamnese: {
          peso: peso ? Number(peso) : null,
          altura: altura ? Number(altura) : null,
          motivoEstetico: motivo.trim() || null,
          aceiteDeclaracao: aceite,
          respostas,
        },
      }
      const res = await publicAnamneseApi.submeter(token, payload)
      limparRascunho(token) // enviado com sucesso — rascunho não é mais necessário
      setResultado(res.data)
      setEtapa('concluido')
      window.scrollTo(0, 0)
    } catch (e) {
      setErro(mensagemErro(e, 'Não foi possível enviar. O link pode ter expirado ou já ter sido usado.'))
    } finally {
      setEnviando(false)
    }
  }

  const Marca = (
    <div className="intake-brand">
      <span className="brand-mark">GA</span>
      <div><strong style={{ display: 'block', fontFamily: 'var(--font-title)', color: 'var(--plum)' }}>GA Clinic &amp; Academy</strong><small className="muted">Ficha de anamnese</small></div>
    </div>
  )

  if (etapa === 'carregando') return <div className="intake"><Spinner full label="Abrindo o link…" /></div>

  return (
    <div className="intake">
      <div className="intake-container">
        {Marca}

        {etapa !== 'invalido' && etapa !== 'concluido' && (
          <div className="intake-steps">
            <div className={`intake-step ${etapa === 'procedimento' || etapa === 'formulario' ? 'intake-step-on' : ''}`} />
            <div className={`intake-step ${etapa === 'formulario' ? 'intake-step-on' : ''}`} />
          </div>
        )}

        {rascunhoRecuperado && (etapa === 'procedimento' || etapa === 'formulario') && (
          <div className="alert alert-info" style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12 }}>
            <span>Recuperamos o preenchimento que você começou neste aparelho.</span>
            <button type="button" className="link-btn" onClick={() => setRascunhoRecuperado(false)}>ok</button>
          </div>
        )}

        {erro && <div className="alert alert-err">{erro}</div>}

        {salvoEm != null && (etapa === 'procedimento' || etapa === 'formulario') && (
          <p className="muted" style={{ textAlign: 'center', fontSize: 12, marginTop: -6, marginBottom: 14 }}>
            ✓ Rascunho salvo automaticamente neste aparelho
          </p>
        )}

        {etapa === 'invalido' && (
          <div className="card intake-ok">
            <div className="ok-mark" style={{ background: 'var(--err-bg)', color: 'var(--err)' }}>✕</div>
            <h2>Link indisponível</h2>
            <p className="muted">{abertura?.motivo ?? (erro || 'Este link é inválido, expirou ou já foi utilizado.')}</p>
            <p className="muted">Entre em contato com a clínica para receber um novo link.</p>
          </div>
        )}

        {etapa === 'procedimento' && (
          <div className="card">
            <h2 style={{ marginBottom: 4 }}>Qual procedimento você deseja?</h2>
            <p className="muted" style={{ marginBottom: 18 }}>Escolha o procedimento de interesse para começarmos.</p>
            <div className="chips" style={{ marginBottom: 16 }}>
              {abertura?.procedimentos.map((p) => (
                <button key={p.id} className={`chip ${!outro && procId === p.id ? 'chip-active' : ''}`} onClick={() => { setProcId(p.id); setOutro(false) }}>
                  {p.nome}
                </button>
              ))}
              <button className={`chip ${outro ? 'chip-active' : ''}`} onClick={() => { setOutro(true); setProcId('') }}>Outro…</button>
            </div>
            {outro && (
              <Field label="Descreva o procedimento desejado">
                <input value={outroTexto} onChange={(e) => setOutroTexto(e.target.value)} placeholder="Ex.: avaliação facial" autoFocus />
              </Field>
            )}
            <button className="btn btn-primary btn-block" onClick={avancarProcedimento}>Continuar</button>
          </div>
        )}

        {etapa === 'formulario' && (
          <>
            <div className="card">
              <h2 style={{ marginBottom: 16 }}>Seus dados</h2>
              <Field label="Nome completo"><input value={dados.nome} onChange={(e) => setDados({ ...dados, nome: e.target.value })} /></Field>
              <div className="form-row">
                <Field label="CPF (opcional)"><input value={dados.cpf} onChange={(e) => setDados({ ...dados, cpf: e.target.value })} placeholder="000.000.000-00" /></Field>
                <Field label="Telefone / WhatsApp"><input value={dados.telefone} onChange={(e) => setDados({ ...dados, telefone: e.target.value })} placeholder="(11) 99999-0000" /></Field>
              </div>
              <div className="form-row">
                <Field label="Data de nascimento"><input type="date" value={dados.dataNascimento} onChange={(e) => setDados({ ...dados, dataNascimento: e.target.value })} /></Field>
                <Field label="Sexo">
                  <select value={dados.sexo} onChange={(e) => setDados({ ...dados, sexo: e.target.value })}>
                    <option value="">—</option><option value="F">Feminino</option><option value="M">Masculino</option><option value="Outro">Outro</option>
                  </select>
                </Field>
              </div>
              <Field label="Endereço"><input value={dados.endereco} onChange={(e) => setDados({ ...dados, endereco: e.target.value })} /></Field>
            </div>

            <div className="card" style={{ marginTop: 18 }}>
              <h2 style={{ marginBottom: 6 }}>Anamnese</h2>
              <p className="muted" style={{ marginBottom: 16 }}>Peso e altura calculam seu IMC automaticamente. A pressão arterial é aferida na clínica.</p>
              <div className="form-row">
                <Field label="Peso (kg)" hint="Entre 1 e 500 kg."><input type="number" min={1} max={500} step="0.1" value={peso} onChange={(e) => setPeso(e.target.value)} placeholder="68" /></Field>
                <Field label="Altura (m)" hint="Em metros — ex.: 1,70 (não em cm)."><input type="number" min={0.5} max={2.5} step="0.01" value={altura} onChange={(e) => setAltura(e.target.value)} placeholder="1.70" /></Field>
              </div>

              <div className="section-title">Questionário de saúde</div>
              {PERGUNTAS.map((q) => {
                const val = respostas[q.chave] as boolean | null | undefined
                return (
                  <div key={q.chave} className="q-item">
                    <div className="q-row">
                      <span>{q.label}</span>
                      <SimNao valor={val} onChange={(v) => setResp(q.chave, v)} />
                    </div>
                    {q.detalhe && val === true && (
                      <div style={{ marginTop: 10 }}>
                        <input
                          placeholder={q.detalheLabel}
                          value={(respostas[q.detalhe] as string | number | undefined) ?? ''}
                          onChange={(e) => setResp(q.detalhe!, q.detalhe === 'semanasGestacao' ? Number(e.target.value) : e.target.value)}
                          type={q.detalhe === 'semanasGestacao' ? 'number' : 'text'}
                        />
                      </div>
                    )}
                  </div>
                )
              })}

              <Field label="Motivo estético" hint="Conte o que você deseja melhorar.">
                <textarea value={motivo} onChange={(e) => setMotivo(e.target.value)} />
              </Field>

              <label className="checkbox" style={{ marginTop: 8 }}>
                <input type="checkbox" checked={aceite} onChange={(e) => setAceite(e.target.checked)} />
                <span>Declaro que as informações acima são verdadeiras.</span>
              </label>

              <div className="row" style={{ marginTop: 18, justifyContent: 'space-between' }}>
                <button className="btn btn-ghost" onClick={() => { setEtapa('procedimento'); setErro('') }}>← Voltar</button>
                <button className="btn btn-primary" onClick={enviar} disabled={enviando}>{enviando ? 'Enviando…' : 'Enviar anamnese'}</button>
              </div>
            </div>
          </>
        )}

        {etapa === 'concluido' && resultado && (
          <div className="card intake-ok">
            <div className="ok-mark">✓</div>
            <h2>Recebemos seus dados!</h2>
            <p className="muted" style={{ maxWidth: 420, margin: '8px auto 0' }}>{resultado.mensagem}</p>
            {resultado.imc != null && (
              <p style={{ marginTop: 16 }}>Seu IMC calculado: <strong style={{ color: 'var(--rose-dark)' }}>{resultado.imc}</strong></p>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
