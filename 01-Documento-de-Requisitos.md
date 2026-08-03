# Documento de Requisitos — CRM para Clínica de Estética

**Projeto:** CRM para clínica de estética (single-tenant, com preparo para multi-clínica)
**Versão:** 1.1 (fluxo de entrada revisado: intake pela paciente + triagem)
**Data:** 03/08/2026
**Autor:** Nathan Rodrigues
**Status:** Aprovado para desenvolvimento (MVP / Fase 1)

---

## 1. Visão geral e objetivo

Sistema de gestão (CRM) para uma clínica de estética real (~10 pessoas: 1 recepcionista, ~5 profissionais, 3–4 sócios). O sistema digitaliza a operação da clínica: captação da paciente por link de anamnese, cadastro de pacientes, anamnese online, triagem, agenda dos profissionais, catálogo de procedimentos e registro de fotos de antes/depois.

**Fluxo de entrada da paciente (visão macro):** a cliente procura a clínica demonstrando interesse → a clínica envia um **link de anamnese sem login** → a própria paciente **informa o procedimento de interesse** e **preenche seus dados + a anamnese** → os dados chegam prontos ao sistema e abrem uma **solicitação para triagem** → a recepção/profissional **avalia e decide**: agenda (com mensagem direta à paciente) ou recusa o procedimento informando o motivo. Detalhado na seção 4.10.

**Objetivos do projeto:**
- Resolver a operação diária da clínica (substituir papel + planilha + WhatsApp).
- Servir de projeto de aprofundamento em .NET e portfólio.
- Gerar receita com uma clínica pagante.

**Estratégia:** *pensar grande, começar pequeno*. Nasce **single-tenant** (uma clínica), mas com o modelo de dados preparado (`ClinicaId`) para evoluir para produto multi-clínica sem reescrita.

**Natureza dos dados:** o sistema trata **dados sensíveis de saúde** (anamnese, condições médicas, fotos). Conformidade com a **LGPD** é requisito de primeira classe.

---

## 2. Escopo por fase

| Fase | Conteúdo |
|------|----------|
| **Fase 1 (MVP)** | Pacientes · Anamnese online · Catálogo de procedimentos · Agenda por profissional · Mídia (fotos antes/depois) · Autenticação/papéis · LGPD base |
| **Fase 2** | Controle de frequência · Lembrete de aniversário · Campanhas com pacotes · Formas de desconto · Indicações de paciente · Eventos do mês · Agendamento online pelo paciente · 2FA |
| **Fase 3 (opcional)** | Marketing/Prova Social (depoimentos, fotos públicas, link de avaliação do Google) · Form builder de anamnese (se virar produto) |

---

## 3. Atores / Papéis

| Ator | Descrição | Acesso |
|------|-----------|--------|
| **Admin** | Sócios (3–4). | Acesso total ao sistema. |
| **Profissional** | Esteticistas/biomédicos (~5). | Ficha clínica, anamnese, procedimentos, mídia, sua agenda. |
| **Recepção** | Recepcionista (1). | Envio do link de anamnese, **triagem** das solicitações (agendar/recusar), cadastro, agenda, catálogo, campanhas (Fase 2). **Não** vê detalhe clínico sensível. |
| **Paciente** | Cliente da clínica. | **Sem login.** Via link tokenizado: informa o procedimento de interesse, preenche os próprios dados e a anamnese. |
| **Sistema** (Fase 2) | Rotinas automáticas. | Dispara lembretes de aniversário e afins. |

---

## 4. Requisitos Funcionais (RF)

> Etiqueta de fase: **[F1]** Fase 1 · **[F2]** Fase 2 · **[F3]** Fase 3

### 4.1 Autenticação e Autorização
- **RF01 [F1]** — O sistema deve permitir login da equipe por e-mail e senha.
- **RF02 [F1]** — O sistema deve controlar o acesso por papel (Admin, Profissional, Recepção).
- **RF03 [F1]** — A Recepção não deve ter acesso aos detalhes clínicos sensíveis (anamnese detalhada, fotos clínicas).
- **RF04 [F2]** — O sistema deve oferecer autenticação em dois fatores (2FA).

### 4.2 Gestão de Pacientes
- **RF05 [F1]** — Cadastrar paciente: Nome, CPF (opcional), Telefone/WhatsApp, Data de nascimento, Sexo, Endereço, Rede social. O cadastro pode ser criado de duas formas: (a) **pela própria paciente**, ao preencher o link de anamnese (auto-cadastro), ou (b) **manualmente pela recepção**.
- **RF06 [F1]** — O CPF é o identificador único do paciente; o sistema deve garantir unicidade quando informado.
- **RF07 [F1]** — Permitir cadastro sem CPF, marcando o registro como "CPF pendente" e cobrando o preenchimento depois. Para menor, aceitar CPF do responsável.
- **RF08 [F1]** — Avisar sobre possível duplicidade (mesmo CPF ou telefone) antes de criar um novo paciente.
- **RF09 [F1]** — Editar, consultar e buscar pacientes (por nome, CPF ou telefone).
- **RF10 [F1]** — Exibir uma **aba "Mídia"** por paciente (galeria de fotos/vídeos).
- **RF11 [F2]** — Registrar indicações: qual paciente indicou qual novo paciente.

### 4.3 Anamnese
- **RF12 [F1]** — Disponibilizar um **modelo fixo** de anamnese (campos na seção 8).
- **RF13 [F1]** — Gerar um **link de anamnese tokenizado** para preenchimento remoto **sem login**, emitido/enviado pela clínica quando a cliente demonstra interesse (não há endpoint público de auto-geração — ver RNF09).
- **RF14 [F1]** — Pelo link, a paciente **informa o procedimento de interesse** (primeiro passo), **preenche os próprios dados pessoais** e a **anamnese**, com **aceite de declaração de veracidade** datado. O envio cria/atualiza o cadastro da paciente e a anamnese e abre uma **solicitação de atendimento** para triagem (ver 4.10).
- **RF15 [F1]** — Coletar **Peso** e **Altura** e calcular o **IMC** automaticamente.
- **RF16 [F1]** — Registrar a **Pressão arterial** aferida na clínica (não no preenchimento remoto).
- **RF17 [F1]** — Manter **histórico datado** de anamneses (cada preenchimento é um novo registro; nada é sobrescrito).
- **RF18 [F2]** — Suportar a segunda anamnese (modelo da outra profissional).

### 4.4 Mídia / Fotos
- **RF19 [F1]** — Fazer upload de fotos/vídeos do paciente para armazenamento em nuvem (S3).
- **RF20 [F1]** — Etiquetar cada mídia (antes / depois / durante / documento) e vincular **opcionalmente** a um procedimento.
- **RF21 [F1]** — Registrar **dois consentimentos** por mídia: uso clínico e uso público/marketing.
- **RF22 [F1]** — Servir a mídia apenas via **URL assinada temporária** (acesso restrito).

### 4.5 Catálogo de Procedimentos
- **RF23 [F1]** — CRUD de procedimentos: Nome, Descrição, Categoria, **Duração padrão**, **Valor de referência**.

### 4.6 Agenda
- **RF24 [F1]** — Agendar um procedimento para um paciente com um profissional (marcação **interna** por recepção/profissional), tipicamente **a partir de uma solicitação de triagem aprovada** (RF41), mas também de forma avulsa.
- **RF25 [F1]** — Dimensionar o horário do agendamento pela **duração** do procedimento.
- **RF26 [F1]** — Impedir **colisão de horário** para o mesmo profissional.
- **RF27 [F1]** — Permitir editar o **valor cobrado** no atendimento (pode diferir do valor de referência).
- **RF28 [F2]** — Controle de **frequência** (presença/faltas do paciente).
- **RF29 [F2]** — Agendamento **online** pelo próprio paciente.

### 4.7 Comercial e Engajamento (Fase 2)
- **RF30 [F2]** — Cadastrar **campanhas** com pacotes e valores.
- **RF31 [F2]** — Aplicar **formas de desconto** no atendimento/pacote.
- **RF32 [F2]** — **Lembrete de aniversário** de cada paciente (rotina automática).
- **RF33 [F2]** — Cadastrar **eventos do mês**.

### 4.8 Marketing / Prova Social (Fase 3)
- **RF34 [F3]** — Registrar depoimentos e selecionar fotos públicas (apenas as com consentimento público).
- **RF35 [F3]** — Enviar o link de avaliação do Google e exibir avaliações recebidas.

### 4.9 LGPD / Auditoria
- **RF36 [F1]** — Manter **trilha de auditoria** de acesso e alteração de prontuários.
- **RF37 [F1]** — Permitir **exclusão/anonimização** do paciente sob solicitação (direito ao esquecimento).

### 4.10 Captação e Triagem (novo fluxo de entrada)
- **RF38 [F1]** — Registrar o **procedimento de interesse** informado pela paciente no início do preenchimento do link (a partir do catálogo, com opção de texto livre "outro").
- **RF39 [F1]** — Ao receber o preenchimento, o sistema cria uma **Solicitação de atendimento** vinculada ao paciente, à anamnese e ao procedimento de interesse, com status inicial **"Recebida"**.
- **RF40 [F1]** — Exibir para a recepção/profissional uma **fila de triagem** com as solicitações recebidas (dados da paciente + anamnese pronta + procedimento desejado).
- **RF41 [F1]** — Na triagem, permitir **decidir** a solicitação em um de dois desfechos: **Agendar** (cria o agendamento e envia mensagem direta à paciente com a data) ou **Recusar** (registra o **motivo** — ex.: contraindicação clínica — e comunica a paciente).
- **RF42 [F1]** — Ao agendar a partir da triagem, **pré-preencher** o agendamento com a paciente e o procedimento de interesse, permitindo ajuste de profissional, data/hora e valor.
- **RF43 [F1]** — Gerar a **mensagem direta** à paciente (WhatsApp/e-mail) tanto na aprovação (data marcada) quanto na recusa (motivo). Envio manual no MVP; automação em fase futura.

---

## 5. Requisitos Não-Funcionais (RNF)

- **RNF01** — Aplicação **web responsiva** (uso em computador e celular).
- **RNF02** — Backend em **ASP.NET Core (.NET 8)**; frontend em **React**.
- **RNF03** — Banco de dados **PostgreSQL** (via EF Core).
- **RNF04** — Imagens no **AWS S3**, região **`sa-east-1` (São Paulo)**, com URLs assinadas.
- **RNF05** — **HTTPS** em trânsito e **criptografia em repouso** (banco e storage).
- **RNF06** — Senhas com **hash forte** (Argon2/bcrypt).
- **RNF07** — Dados hospedados em **região Brasil** (LGPD).
- **RNF08** — Token de anamnese: **128 bits**, não sequencial, com **expiração** e **uso único**; **rate limiting** no endpoint.
- **RNF09** — **Não** deve existir endpoint público de auto-geração de link/paciente: o auto-cadastro só ocorre **dentro de um link tokenizado emitido pela clínica** (proteção contra spam/abuso); o endpoint de preenchimento tem rate limiting e o token é de uso único e expira.
- **RNF10** — Modelo de dados **multi-tenant ready** (`ClinicaId` nas entidades principais).
- **RNF11** — Hospedagem em **PaaS gerenciado** com **backup automático**.
- **RNF12** — Arquitetura em **Clean Architecture** (projetos `Crm.Domain` / `Crm.Application` / `Crm.Infrastructure` / `Crm.Api`; Application organizada por features / vertical slices).

---

## 6. Regras de Negócio (RN)

- **RN01** — CPF é único no sistema quando informado.
- **RN02** — IMC = Peso(kg) / (Altura(m))².
- **RN03** — Não é permitido dois agendamentos sobrepostos para o mesmo profissional.
- **RN04** — O valor cobrado no atendimento pode diferir do valor de referência do catálogo.
- **RN05** — Uma foto só pode ser usada em marketing se `ConsentimentoPublico = verdadeiro`.
- **RN06** — A Recepção não acessa dados clínicos sensíveis.
- **RN07** — Nenhuma anamnese antiga é apagada ou sobrescrita (histórico datado).
- **RN08** — Toda solicitação de atendimento nasce em "Recebida" e só sai da fila de triagem em um desfecho: **Agendada** ou **Recusada**.
- **RN09** — Uma recusa exige **motivo** registrado; um agendamento a partir da triagem gera um **Agendamento** vinculado à solicitação.
- **RN10** — O procedimento de interesse informado pela paciente é uma **intenção**, não uma marcação; só vira compromisso quando a triagem agenda.

---

## 7. Premissas e Restrições

- Single-tenant no MVP; uma clínica.
- A entrada da paciente começa pelo **contato dela** com a clínica; a clínica então envia o link. Não há captação totalmente aberta ao público (link sempre tokenizado).
- A pressão arterial é aferida na clínica (após a paciente comparecer), não no preenchimento remoto.
- A comunicação com a paciente (envio do link, mensagem de agendamento ou de recusa) ocorre por WhatsApp/e-mail (envio manual no MVP; automação futura).
- Existe uma segunda anamnese (de outra profissional) a ser incorporada pós-MVP.

---

## 8. Anexo — Campos da Anamnese (modelo fixo, Fase 1)

**Vitais/antropométricos (adicionados ao modelo de papel):** Peso · Altura · IMC (calculado) · Pressão arterial (na clínica).

**Perguntas (Sim/Não, com detalhe onde indicado):** utiliza lentes de contato · epilepsia/convulsões · funcionamento intestinal regular · tratamento facial anterior · ingere água com frequência · ingere bebida alcoólica · exposição ao sol (+ frequência) · usa filtro solar · está no período menstrual · qualidade do sono (+ horas/noite) · possui prótese corporal/facial · tabagismo · alterações cardíacas · portador de marcapasso · gestante (+ semanas) · usa cremes/loções faciais (+ especificar) · pratica atividade física (+ especificar) · usa anticoncepcional (+ qual) · possui alergia (+ especificar) · possui boa alimentação · problemas de pele · está em tratamento médico · tumor/lesão pré-cancerosa.

**Livre:** Motivo Estético.
**Encerramento:** Declaração de veracidade (aceite datado + assinatura digital simples).

---

## 9. Pontos em aberto

- ~~Confirmar a arquitetura do backend~~ → **Definido: Clean Architecture** (Domain/Application/Infrastructure/Api), Application por features/vertical slices.
- Definir os campos da 2ª anamnese (outra profissional).
- Detalhar regras de campanhas, descontos e indicações (Fase 2).
