// Utilitários de formatação (pt-BR). Datas da API vêm em UTC (ISO 8601).

export function formatarMoeda(valor: number | null | undefined): string {
  if (valor === null || valor === undefined) return '—'
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

/** Converte um ISO UTC para data/hora local legível. */
export function formatarDataHora(iso: string | null | undefined): string {
  if (!iso) return '—'
  const d = new Date(iso)
  return d.toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })
}

export function formatarData(iso: string | null | undefined): string {
  if (!iso) return '—'
  // Datas "date-only" (ex.: nascimento, YYYY-MM-DD) devem ser formatadas sem passar
  // pelo fuso: new Date('1990-03-15') vira meia-noite UTC e "volta" um dia no Brasil.
  const soData = /^(\d{4})-(\d{2})-(\d{2})$/.exec(iso)
  if (soData) return `${soData[3]}/${soData[2]}/${soData[1]}`
  const d = new Date(iso)
  return d.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

export function formatarHora(iso: string | null | undefined): string {
  if (!iso) return '—'
  const d = new Date(iso)
  return d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}

export function formatarCpf(cpf: string | null | undefined): string {
  if (!cpf) return '—'
  const d = cpf.replace(/\D/g, '')
  if (d.length !== 11) return cpf
  return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9)}`
}

export function formatarTelefone(tel: string | null | undefined): string {
  if (!tel) return '—'
  const d = tel.replace(/\D/g, '')
  if (d.length === 11) return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`
  if (d.length === 10) return `(${d.slice(0, 2)}) ${d.slice(2, 6)}-${d.slice(6)}`
  return tel
}

/** Converte um valor de <input type="datetime-local"> (horário local) para ISO UTC. */
export function localParaUtcIso(valorLocal: string): string {
  // valorLocal ex.: "2026-08-10T14:30" (interpretado como horário local do navegador)
  return new Date(valorLocal).toISOString()
}

/** Converte um ISO UTC para o formato aceito por <input type="datetime-local"> (horário local). */
export function utcIsoParaLocalInput(iso: string): string {
  const d = new Date(iso)
  const off = d.getTimezoneOffset()
  const local = new Date(d.getTime() - off * 60000)
  return local.toISOString().slice(0, 16)
}

/** Monta um link wa.me com a mensagem pré-preenchida. */
export function linkWhatsApp(telefone: string | null | undefined, mensagem: string): string {
  const num = (telefone ?? '').replace(/\D/g, '')
  const comDdi = num.length <= 11 ? `55${num}` : num
  return `https://wa.me/${comDdi}?text=${encodeURIComponent(mensagem)}`
}

export function iniciais(nome: string): string {
  const partes = nome.trim().split(/\s+/)
  const primeira = partes[0]?.[0] ?? ''
  const ultima = partes.length > 1 ? partes[partes.length - 1][0] : ''
  return (primeira + ultima).toUpperCase()
}
