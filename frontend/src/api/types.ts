// Tipos espelhando os DTOs da API (.NET). Enums são serializados como string.

export type Papel = 'Admin' | 'Profissional' | 'Recepcao'
export type OrigemPaciente = 'AutoCadastroLink' | 'Recepcao'
export type StatusSolicitacao = 'Recebida' | 'EmTriagem' | 'Agendada' | 'Recusada'
export type StatusAgendamento = 'Agendado' | 'Realizado' | 'Faltou' | 'Cancelado'
export type EtiquetaMidia = 'Antes' | 'Depois' | 'Durante' | 'Documento'

export interface UsuarioLogado {
  id: string
  nome: string
  email: string
  papel: Papel
  clinicaId: string
}

export interface LoginResponse {
  token: string
  expiraEm: string
  usuario: UsuarioLogado
}

export interface ProfissionalListItem {
  id: string
  nome: string
}

// ---- Equipe / usuários (gestão pelo Admin) ----
export interface EquipeUsuario {
  id: string
  nome: string
  email: string
  papel: Papel
  ativo: boolean
}

export interface CriarUsuarioRequest {
  nome: string
  email: string
  senha: string
  papel: Papel
}

export interface AtualizarUsuarioRequest {
  nome: string
  papel: Papel
  ativo: boolean
  novaSenha?: string | null
}

// ---- Pacientes ----
export interface PacienteDto {
  id: string
  nome: string
  cpf: string | null
  cpfPendente: boolean
  telefone: string | null
  dataNascimento: string | null
  sexo: string | null
  endereco: string | null
  redeSocial: string | null
  origem: OrigemPaciente
  criadoEm: string
}

export interface PacienteListItem {
  id: string
  nome: string
  cpf: string | null
  telefone: string | null
  cpfPendente: boolean
}

export interface DuplicidadeResponse {
  porCpf: PacienteListItem[]
  porTelefone: PacienteListItem[]
  haPossivelDuplicidade: boolean
}

export interface CriarPacienteRequest {
  nome: string
  cpf?: string | null
  telefone?: string | null
  dataNascimento?: string | null
  sexo?: string | null
  endereco?: string | null
  redeSocial?: string | null
}

// ---- Procedimentos ----
export interface ProcedimentoDto {
  id: string
  nome: string
  descricao: string | null
  categoria: string | null
  duracaoMinutos: number
  valorReferencia: number
  ativo: boolean
}

export interface CriarProcedimentoRequest {
  nome: string
  descricao?: string | null
  categoria?: string | null
  duracaoMinutos: number
  valorReferencia: number
}

export interface AtualizarProcedimentoRequest extends CriarProcedimentoRequest {
  ativo: boolean
}

// ---- Triagem ----
export interface SolicitacaoListItem {
  id: string
  pacienteId: string
  pacienteNome: string
  telefone: string | null
  procedimentoInteresse: string
  status: StatusSolicitacao
  dataEnvio: string
}

export interface RespostasAnamnese {
  usaLentesContato?: boolean | null
  epilepsiaConvulsoes?: boolean | null
  funcionamentoIntestinalRegular?: boolean | null
  tratamentoFacialAnterior?: boolean | null
  ingereAgua?: boolean | null
  ingereBebidaAlcoolica?: boolean | null
  exposicaoSol?: boolean | null
  exposicaoSolFrequencia?: string | null
  usaFiltroSolar?: boolean | null
  periodoMenstrual?: boolean | null
  qualidadeSono?: string | null
  horasSono?: number | null
  proteseCorporalFacial?: boolean | null
  tabagismo?: boolean | null
  alteracoesCardiacas?: boolean | null
  marcapasso?: boolean | null
  gestante?: boolean | null
  semanasGestacao?: number | null
  usaCremesLocoes?: boolean | null
  cremesLocoesDetalhe?: string | null
  praticaAtividadeFisica?: boolean | null
  atividadeFisicaDetalhe?: string | null
  usaAnticoncepcional?: boolean | null
  anticoncepcionalQual?: string | null
  possuiAlergia?: boolean | null
  alergiaDetalhe?: string | null
  boaAlimentacao?: boolean | null
  problemasDePele?: boolean | null
  emTratamentoMedico?: boolean | null
  tumorLesaoPreCancerosa?: boolean | null
}

export interface AnamneseDetalhe {
  id: string
  dataPreenchimento: string
  peso: number | null
  altura: number | null
  imc: number | null
  pressaoArterial: string | null
  motivoEstetico: string | null
  aceiteDeclaracao: boolean
  respostas: RespostasAnamnese
}

export interface SolicitacaoDetalhe {
  id: string
  pacienteId: string
  pacienteNome: string
  telefone: string | null
  cpf: string | null
  procedimentoInteresse: string
  status: StatusSolicitacao
  dataEnvio: string
  motivoRecusa: string | null
  agendamentoId: string | null
  anamnese: AnamneseDetalhe
}

export interface AgendarDecisaoRequest {
  usuarioProfissionalId: string
  procedimentoId?: string | null
  inicio: string
  valorCobrado?: number | null
  observacao?: string | null
}

export interface DecisaoResultado {
  solicitacaoId: string
  status: StatusSolicitacao
  agendamentoId: string | null
  mensagemParaPaciente: string
}

// ---- Agenda ----
export interface AgendamentoDto {
  id: string
  pacienteId: string
  pacienteNome: string
  usuarioProfissionalId: string
  profissionalNome: string
  procedimentoId: string
  procedimentoNome: string
  inicio: string
  fim: string
  status: StatusAgendamento
  valorCobrado: number | null
  observacao: string | null
  motivoCancelamento: string | null
  solicitacaoAtendimentoId: string | null
}

export interface CriarAgendamentoRequest {
  pacienteId: string
  usuarioProfissionalId: string
  procedimentoId: string
  inicio: string
  valorCobrado?: number | null
  observacao?: string | null
}

// ---- Mídia ----
export interface MidiaDto {
  id: string
  pacienteId: string
  procedimentoId: string | null
  etiqueta: EtiquetaMidia
  data: string
  nomeArquivo: string
  contentType: string
  tamanhoBytes: number
  consentimentoClinico: boolean
  consentimentoPublico: boolean
  url: string
}

// ---- Anamnese (link interno + intake público) ----
export interface LinkGeradoDto {
  token: string
  urlPreenchimento: string
  expiracao: string
}

export interface ProcedimentoResumo {
  id: string
  nome: string
  categoria: string | null
  duracaoMinutos: number
}

export interface AberturaLinkDto {
  valido: boolean
  motivo: string | null
  procedimentos: ProcedimentoResumo[]
}

export interface IntakeResultado {
  solicitacaoId: string
  pacienteId: string
  imc: number | null
  mensagem: string
}
