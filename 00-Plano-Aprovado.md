# CRM para Clínica de Estética — Plano (MVP / Fase 1)

## Contexto

O objetivo é construir, do zero (greenfield), um CRM para uma **clínica de estética real** (clínica parceira, ~10 pessoas: 1 recepcionista, ~5 profissionais, 3–4 sócios). As motivações do desenvolvedor são combinadas e legítimas: **resolver a operação da clínica**, **aprofundar em .NET**, **montar portfólio** e **ganhar dinheiro** com uma clínica pagante.

Estratégia acordada: **pensar grande, começar pequeno**. Nasce single-tenant (uma clínica), mas com o modelo de dados preparado (`ClinicaId`) para virar produto multi-clínica no futuro sem reescrita dolorosa. Como se lida com **dado sensível de saúde** (anamnese, condições médicas, fotos de antes/depois), LGPD é requisito de primeira classe, não enfeite.

Este plano cobre **apenas a Fase 1 (MVP)**. Fases 2 e 3 estão listadas como escopo futuro.

---

## Decisões travadas (grelha)

| # | Decisão | Escolha |
|---|---------|---------|
| 1 | Natureza | Clínica real, single-tenant, começar pequeno |
| 2 | Multi-tenant readiness | `ClinicaId` nas entidades principais desde o dia 1 |
| 3 | Plataforma | App **web responsivo** (equipe no PC/celular; paciente via link) |
| 4 | Stack | **API .NET (ASP.NET Core) + React** |
| 5 | Arquitetura backend | **Clean Architecture** — 4 projetos: `Crm.Domain` · `Crm.Application` · `Crm.Infrastructure` · `Crm.Api` (Application organizada por features / vertical slices) |
| 6 | Banco | **PostgreSQL** (EF Core) |
| 7 | Papéis | Admin (sócios) · Profissional (~5) · Recepção (1) + paciente sem login |
| 8 | Identificação do paciente | **CPF = chave única canônica**; telefone/WhatsApp como contato; CPF "pendente" permitido temporariamente |
| 9 | Anamnese: formato | **Modelo fixo** (não configurável no MVP) |
| 10 | Anamnese: conteúdo | Ficha real da clínica **+ Peso/Altura/Pressão + IMC automático** |
| 11 | Anamnese: histórico | **Histórico datado** (guarda cada preenchimento) |
| 12 | Anamnese: entrega | **Link tokenizado seguro** por WhatsApp (sem login), emitido pela clínica; a própria paciente informa o **procedimento de interesse**, preenche seus **dados** e a **anamnese** (auto-cadastro dentro do link) |
| 12b | Entrada + triagem | Cliente demonstra interesse → clínica envia link → paciente preenche → sistema abre **Solicitação de atendimento** → recepção/profissional faz **triagem** e decide **Agendar** (mensagem direta com a data) ou **Recusar** (motivo) |
| 13 | Pressão arterial | Aferida **na clínica** (não no preenchimento remoto) |
| 14 | Mídia/Fotos | **Aba "Mídia" por paciente**: galeria com data, etiqueta (antes/depois/durante/documento), vínculo opcional a procedimento, **2 flags de consentimento** (clínico / público) |
| 15 | Storage de imagens | **AWS S3**, região **`sa-east-1`** (São Paulo), com URLs assinadas temporárias |
| 16 | Agenda: quem marca | **Só interno** (recepção/profissional); agendamento online do paciente = Fase 2 |
| 17 | Agenda: disponibilidade | **Por profissional** (sem colisão de horário); sala/equipamento = futuro |
| 18 | Catálogo: preço | **Valor de referência editável** no atendimento |
| 19 | Catálogo: execução | Qualquer profissional faz qualquer procedimento (no MVP) |
| 20 | Duração | Cada procedimento tem **duração padrão** que dimensiona o slot da agenda |
| 21 | Autenticação | E-mail + senha (**hash forte**, ex. Argon2/bcrypt); **2FA na Fase 2** |
| 22 | Hospedagem | **PaaS gerenciado, região Brasil** (ex. Azure App Service / AWS) |

---

## Fluxo de entrada da paciente (intake + triagem)

Este é o coração do novo fluxo. Ele reduz o atrito para a paciente (ela não perde tempo com login nem com cadastro presencial) e concentra a decisão na clínica.

1. **Interesse** — a cliente procura a clínica (WhatsApp, Instagram, etc.) demonstrando interesse.
2. **Envio do link** — a clínica gera e envia um **link de anamnese tokenizado, sem login** (não há auto-geração pública; o link é sempre emitido pela clínica → preserva o antispam).
3. **Preenchimento (paciente)** — no link, a paciente **primeiro informa o procedimento de interesse**, depois **preenche os próprios dados pessoais** e a **anamnese**, e aceita a declaração de veracidade.
4. **Recebimento (sistema)** — ao enviar, o sistema **cria/atualiza o cadastro** da paciente, **grava a anamnese datada + IMC** e abre uma **Solicitação de atendimento** (status `Recebida`) com o procedimento de interesse.
5. **Triagem (recepção/profissional)** — a solicitação entra numa **fila de triagem**; quem estiver no comando avalia a anamnese e o procedimento desejado.
6. **Decisão** — dois desfechos:
   - **Agendar** → cria um `Agendamento` (pré-preenchido com paciente + procedimento; ajusta profissional, data/hora, valor) e **envia mensagem direta** à paciente com a data.
   - **Recusar** → registra o **motivo** (ex.: contraindicação clínica) e **comunica a paciente** por mensagem direta.

> A pressão arterial continua sendo **aferida na clínica** quando a paciente comparece — não no preenchimento remoto.

---

## Escopo por fase

**Fase 1 (MVP) — a clínica opera o dia a dia:**
1. Cadastro de pacientes
2. Doutores + Agenda (interna, por profissional)
3. Catálogo de procedimentos e valores
4. Anamnese online por link tokenizado (paciente informa procedimento de interesse + auto-cadastro)
5. Triagem das solicitações (fila → agendar ou recusar com motivo + mensagem direta)
6. Mídia (fotos antes/depois) por paciente

**Fase 2 — engajamento e comercial:** controle de frequência · lembrete de aniversário · campanhas com pacotes · formas de desconto · indicações de paciente · eventos do mês · agendamento online do paciente · 2FA.

**Fase 3 (opcional) — Marketing / Prova Social:** depoimentos, seleção de fotos públicas (usando os flags de consentimento já existentes), envio do link de avaliação do Google, exibição de avaliações · possível form builder de anamnese (se virar produto multi-clínica).

---

## Modelo de dados (rascunho)

Todas as entidades principais carregam `ClinicaId`.

- **Clinica** — `Id`, `Nome`, ... (raiz do tenant)
- **Usuario** (equipe) — `Id`, `Nome`, `Email`, `SenhaHash`, `Papel` (Admin/Profissional/Recepcao), `ClinicaId`
- **Paciente** — `Id`, `Nome`, `Cpf` (único quando presente), `CpfPendente` (bool), `Telefone`, `DataNascimento`, `Sexo`, `Endereco`, `RedeSocial`, `Origem` (`AutoCadastroLink` / `Recepcao`), `ClinicaId`
- **Anamnese** — `Id`, `PacienteId`, `DataPreenchimento`, respostas (campos fixos, ver abaixo), `Peso`, `Altura`, `Imc` (calculado), `PressaoArterial` (preenchida na clínica), `MotivoEstetico`, `AceiteDeclaracao` (bool + data/hora)
- **AnamneseLink** (token de acesso) — `Token` (aleatório 128-bit), `PacienteId?` (nulo até o auto-cadastro), `TelefoneDestino` (para quem a clínica enviou), `Expiracao`, `Usado` (bool)
- **SolicitacaoAtendimento** (triagem) — `Id`, `PacienteId`, `AnamneseId`, `ProcedimentoInteresseId?` (nulo se "outro"), `ProcedimentoInteresseTexto?`, `DataEnvio`, `Status` (`Recebida`/`EmTriagem`/`Agendada`/`Recusada`), `MotivoRecusa?`, `UsuarioTriagemId?`, `DataDecisao?`, `AgendamentoId?`, `ClinicaId`
- **Procedimento** (catálogo) — `Id`, `Nome`, `Descricao`, `Categoria`, `DuracaoMinutos`, `ValorReferencia`, `ClinicaId`
- **Agendamento** — `Id`, `PacienteId`, `UsuarioProfissionalId`, `ProcedimentoId`, `SolicitacaoAtendimentoId?` (origem na triagem, quando houver), `Inicio`, `Fim` (derivado da duração), `Status`, `ValorCobrado` (editável), `ClinicaId`
- **Midia** — `Id`, `PacienteId`, `ProcedimentoId?` (opcional), `UsuarioId` (quem registrou), `ChaveS3`, `Data`, `Etiqueta` (antes/depois/durante/documento), `ConsentimentoClinico` (bool), `ConsentimentoPublico` (bool), `ClinicaId`
- **LogAuditoria** — `Id`, `UsuarioId`, `Acao`, `EntidadeTipo`, `EntidadeId`, `DataHora` (trilha de acesso/alteração a prontuário)

### Anamnese — campos fixos (do formulário real + adições)

Vitais/antropométricos (adicionados): **Peso**, **Altura**, **IMC** (auto), **Pressão** (na clínica).
Sim/Não (com detalhe onde indicado): lentes de contato · epilepsia/convulsões · funcionamento intestinal · tratamento facial anterior · ingere água · bebida alcoólica · exposição ao sol (+freq.) · filtro solar · período menstrual · qualidade do sono (+horas) · prótese corporal/facial · tabagismo · alterações cardíacas · marcapasso · gestante (+semanas) · cremes/loções (+especificar) · atividade física (+especificar) · anticoncepcional (+qual) · alergia (+especificar) · alimentação · problemas de pele · tratamento médico · tumor/lesão pré-cancerosa. **Motivo Estético** (texto livre) · **Declaração de veracidade** (aceite datado).

> Nota: existe uma anamnese diferente para outra profissional — a ser incorporada depois (fora do MVP).

---

## LGPD e requisitos não-funcionais

- **Trilha de auditoria** de acesso/alteração de prontuário (`LogAuditoria`).
- **Direito ao esquecimento**: exclusão/anonimização real do paciente sob solicitação (não soft-delete "de mentira" para dados que exigem remoção).
- **Criptografia**: HTTPS em trânsito; criptografia em repouso no banco e no S3.
- **Consentimento**: aceite datado da anamnese + 2 flags por mídia.
- **Residência de dados**: banco e storage em **região Brasil** (`sa-east-1` / PaaS BR).
- **Link de anamnese seguro**: token aleatório de 128 bits (não sequencial), **emitido pela clínica** (nunca auto-gerado pelo público), com expiração e uso único; rate limiting no endpoint de preenchimento. O auto-cadastro da paciente só acontece **dentro** desse link tokenizado — **não há endpoint público aberto de criação de paciente** (elimina spam/abuso).
- **Autorização por papel**: Recepção não vê detalhes clínicos sensíveis; Profissional vê o clínico; Admin vê tudo.

---

## Arquitetura & entregáveis técnicos

- **Backend:** ASP.NET Core Web API (.NET 8), **Clean Architecture** (`Crm.Domain` / `Crm.Application` / `Crm.Infrastructure` / `Crm.Api`), EF Core + migrations, autenticação JWT por papel.
- **Frontend:** React (responsivo), consumindo a API; rota pública isolada só para a anamnese tokenizada.
- **Storage:** integração AWS S3 (SDK) com URLs assinadas.
- **Deploy:** PaaS gerenciado (região Brasil) + PostgreSQL gerenciado + backup automático.

---

## Verificação (como validar de ponta a ponta)

1. **Auth/papéis:** logar como cada papel e confirmar que a autorização restringe telas/endpoints corretamente.
2. **Fluxo do paciente:** cadastrar paciente (com e sem CPF) → confirmar bloqueio de duplicidade por CPF.
3. **Anamnese + triagem:** enviar link tokenizado → abrir sem login → informar procedimento de interesse + preencher dados + anamnese → confirmar que cria o paciente e a anamnese, que gera IMC, que o token expira/queima após uso, e que **abre uma solicitação `Recebida`**. Na triagem: **agendar** (confirma que cria o `Agendamento` vinculado e marca a solicitação como `Agendada`) e **recusar** (confirma que exige motivo e marca como `Recusada`).
4. **Agenda:** marcar dois agendamentos no mesmo horário do mesmo profissional → confirmar que o sistema bloqueia a colisão; conferir que a duração do procedimento dimensiona o slot.
5. **Mídia:** subir foto antes/depois → confirmar que sobe pro S3, que só abre via URL assinada, e que os flags de consentimento são gravados.
6. **LGPD:** exercer exclusão/anonimização de um paciente e conferir a trilha de auditoria.

---

## Pontos a confirmar / definir depois

- ~~Confirmar arquitetura backend~~ → **Definido: Clean Architecture** (Domain/Application/Infrastructure/Api), Application por features/vertical slices.
- Incorporar a 2ª anamnese (da outra profissional) — pós-MVP.
- Regras finais de campanhas/descontos/indicações — Fase 2.
