import { api } from './client'
import type {
  AberturaLinkDto,
  AgendamentoDto,
  AtualizarProcedimentoRequest,
  CriarAgendamentoRequest,
  CriarPacienteRequest,
  CriarProcedimentoRequest,
  DecisaoResultado,
  DuplicidadeResponse,
  IntakeResultado,
  LinkGeradoDto,
  MidiaDto,
  PacienteDto,
  PacienteListItem,
  ProcedimentoDto,
  ProfissionalListItem,
  SolicitacaoDetalhe,
  SolicitacaoListItem,
  StatusAgendamento,
  StatusSolicitacao,
  UsuarioLogado,
} from './types'

// ---- Auth / Usuários ----
export const authApi = {
  login: (email: string, senha: string) =>
    api.post<{ token: string; expiraEm: string; usuario: UsuarioLogado }>('/api/auth/login', { email, senha }),
  me: () => api.get<UsuarioLogado>('/api/auth/me'),
}

export const usuariosApi = {
  profissionais: () => api.get<ProfissionalListItem[]>('/api/usuarios/profissionais'),
}

// ---- Pacientes ----
export const pacientesApi = {
  buscar: (termo: string, page = 1, pageSize = 20) =>
    api.get<PacienteListItem[]>('/api/pacientes', { params: { termo: termo || undefined, page, pageSize } }),
  obter: (id: string) => api.get<PacienteDto>(`/api/pacientes/${id}`),
  duplicidade: (cpf?: string, telefone?: string) =>
    api.get<DuplicidadeResponse>('/api/pacientes/duplicidade', {
      params: { cpf: cpf || undefined, telefone: telefone || undefined },
    }),
  criar: (req: CriarPacienteRequest) => api.post<PacienteDto>('/api/pacientes', req),
  atualizar: (id: string, req: CriarPacienteRequest) => api.put<PacienteDto>(`/api/pacientes/${id}`, req),
}

// ---- Procedimentos ----
export const procedimentosApi = {
  listar: (incluirInativos = false) =>
    api.get<ProcedimentoDto[]>('/api/procedimentos', { params: { incluirInativos } }),
  criar: (req: CriarProcedimentoRequest) => api.post<ProcedimentoDto>('/api/procedimentos', req),
  atualizar: (id: string, req: AtualizarProcedimentoRequest) =>
    api.put<ProcedimentoDto>(`/api/procedimentos/${id}`, req),
}

// ---- Anamnese (link interno) ----
export const anamneseApi = {
  gerarLink: (telefoneDestino?: string, validadeHoras = 48) =>
    api.post<LinkGeradoDto>('/api/anamnese/links', { telefoneDestino: telefoneDestino || undefined, validadeHoras }),
}

// ---- Intake público (sem login) ----
export const publicAnamneseApi = {
  abrir: (token: string) => api.get<AberturaLinkDto>(`/api/public/anamnese/${token}`),
  submeter: (token: string, payload: unknown) =>
    api.post<IntakeResultado>(`/api/public/anamnese/${token}`, payload),
}

// ---- Triagem ----
export const triagemApi = {
  fila: (status?: StatusSolicitacao) =>
    api.get<SolicitacaoListItem[]>('/api/triagem', { params: { status } }),
  detalhe: (id: string) => api.get<SolicitacaoDetalhe>(`/api/triagem/${id}`),
  assumir: (id: string) => api.post(`/api/triagem/${id}/assumir`),
  agendar: (id: string, req: {
    usuarioProfissionalId: string
    procedimentoId?: string | null
    inicio: string
    valorCobrado?: number | null
    observacao?: string | null
  }) => api.post<DecisaoResultado>(`/api/triagem/${id}/agendar`, req),
  recusar: (id: string, motivo: string) =>
    api.post<DecisaoResultado>(`/api/triagem/${id}/recusar`, { motivo }),
}

// ---- Agenda ----
export const agendaApi = {
  listar: (profissionalId?: string, de?: string, ate?: string) =>
    api.get<AgendamentoDto[]>('/api/agenda', {
      params: { profissionalId: profissionalId || undefined, de, ate },
    }),
  criar: (req: CriarAgendamentoRequest) => api.post<AgendamentoDto>('/api/agenda', req),
  alterarStatus: (id: string, status: StatusAgendamento, motivoCancelamento?: string) =>
    api.patch(`/api/agenda/${id}/status`, { status, motivoCancelamento: motivoCancelamento || undefined }),
  atualizarValor: (id: string, valorCobrado: number) =>
    api.patch(`/api/agenda/${id}/valor`, { valorCobrado }),
}

// ---- Mídia ----
export const midiasApi = {
  listar: (pacienteId: string) => api.get<MidiaDto[]>('/api/midias', { params: { pacienteId } }),
  upload: (form: FormData) =>
    api.post<MidiaDto>('/api/midias', form, { headers: { 'Content-Type': 'multipart/form-data' } }),
  conteudo: (id: string) => api.get<Blob>(`/api/midias/${id}/conteudo`, { responseType: 'blob' }),
  remover: (id: string) => api.delete(`/api/midias/${id}`),
}
