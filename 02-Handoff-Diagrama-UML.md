# Handoff — Diagramas UML do CRM da Clínica de Estética

> **Para quem lê isto (ex.: Claude no app / renderizador UML):**
> Este documento descreve, por escrito, como o sistema funciona e quem são os atores, **e já traz o código-fonte dos diagramas** em PlantUML e Mermaid. Sua tarefa é **produzir/desenhar os diagramas UML** a partir daqui: (1) Diagrama de Casos de Uso por ator, (2) Diagrama de Classes (modelo de dados), (3) Diagrama de Sequência do fluxo de entrada da paciente (intake + triagem), e (4) Diagramas de Estados da solicitação/triagem e do agendamento. Pode renderizar o código abaixo como está ou redesenhá-lo com um visual mais bonito, mantendo a semântica.

---

## 1. Resumo do sistema

CRM para uma **clínica de estética** (single-tenant, ~10 pessoas). Gerencia a captação da paciente por link, anamnese online, **triagem**, agenda dos profissionais, catálogo de procedimentos e fotos de antes/depois. Trata **dados sensíveis de saúde** (LGPD). O MVP é a **Fase 1**; há funcionalidades de Fase 2 e 3 marcadas.

**Fluxo de entrada (resumo):** a cliente demonstra interesse → a clínica envia um link de anamnese sem login → a paciente **informa o procedimento de interesse** e **preenche seus dados + anamnese** → o sistema abre uma **Solicitação de atendimento** → a recepção/profissional faz a **triagem** e decide **agendar** (mensagem direta com a data) ou **recusar** (motivo).

## 2. Atores / Papéis

- **Admin** (sócios): acesso total.
- **Profissional** (esteticistas): ficha clínica, anamnese, triagem, procedimentos, mídia, própria agenda.
- **Recepção**: envio da anamnese, **triagem** (agendar/recusar), cadastro, agenda, catálogo — **sem** acesso a dados clínicos sensíveis.
- **Paciente**: **sem login**; via link tokenizado informa o procedimento de interesse, preenche os próprios dados e a anamnese.
- **Sistema** (Fase 2): rotinas automáticas (ex.: lembrete de aniversário).

---

## 3. Diagrama de Casos de Uso (PlantUML)

```plantuml
@startuml Casos de Uso - CRM Clinica de Estetica
left to right direction
skinparam actorStyle awesome

actor "Recepção" as Rec
actor "Profissional" as Prof
actor "Admin" as Adm
actor "Paciente" as Pac
actor "Sistema" as Sis

rectangle "CRM Clínica de Estética" {
  ' --- Fase 1 (MVP) ---
  usecase "Fazer login" as UC_Login
  usecase "Cadastrar/editar paciente" as UC_Pac
  usecase "Enviar link de anamnese" as UC_EnviarLink
  usecase "Informar procedimento de interesse" as UC_Interesse
  usecase "Preencher dados + anamnese (sem login)" as UC_Preencher
  usecase "Triar solicitação" as UC_Triar
  usecase "Agendar (mensagem direta)" as UC_Agendar
  usecase "Recusar procedimento (motivo)" as UC_Recusar
  usecase "Aferir pressão / registrar na clínica" as UC_Pressao
  usecase "Gerenciar catálogo de procedimentos" as UC_Cat
  usecase "Registrar fotos antes/depois (Mídia)" as UC_Midia
  usecase "Consultar ficha clínica do paciente" as UC_Ficha
  usecase "Anonimizar/excluir paciente (LGPD)" as UC_LGPD

  ' --- Fase 2 ---
  usecase "Controlar frequência" as UC_Freq
  usecase "Gerenciar campanhas e descontos" as UC_Camp
  usecase "Registrar indicações" as UC_Ind
  usecase "Enviar lembrete de aniversário" as UC_Aniv
  usecase "Agendar online" as UC_AgOnline
}

UC_Triar ..> UC_Agendar : <<extend>>
UC_Triar ..> UC_Recusar : <<extend>>

Rec --> UC_Login
Rec --> UC_Pac
Rec --> UC_EnviarLink
Rec --> UC_Triar
Rec --> UC_Pressao
Rec --> UC_Cat
Rec --> UC_Freq
Rec --> UC_Camp
Rec --> UC_Ind

Prof --> UC_Login
Prof --> UC_Ficha
Prof --> UC_Triar
Prof --> UC_Midia
Prof --> UC_Pressao

Adm --> UC_Login
Adm --> UC_Cat
Adm --> UC_LGPD
Adm --> UC_Camp

Pac --> UC_Interesse
Pac --> UC_Preencher
Pac --> UC_AgOnline

Sis --> UC_Aniv

note bottom of UC_Freq : Fase 2
note bottom of UC_Camp : Fase 2
note bottom of UC_Ind : Fase 2
note bottom of UC_Aniv : Fase 2
note bottom of UC_AgOnline : Fase 2
@enduml
```

---

## 4. Diagrama de Classes / Modelo de Dados (PlantUML)

> Todas as entidades principais carregam `ClinicaId` (preparo multi-tenant).

```plantuml
@startuml Modelo de Dados - CRM Clinica de Estetica
skinparam linetype ortho

class Clinica {
  +Id
  +Nome
}

class Usuario {
  +Id
  +Nome
  +Email
  +SenhaHash
  +Papel : {Admin, Profissional, Recepcao}
}

class Paciente {
  +Id
  +Nome
  +Cpf  ' único quando presente
  +CpfPendente : bool
  +Telefone
  +DataNascimento
  +Sexo
  +Endereco
  +RedeSocial
}

class Anamnese {
  +Id
  +DataPreenchimento
  +Peso
  +Altura
  +Imc  ' calculado
  +PressaoArterial ' aferida na clínica
  +MotivoEstetico
  +AceiteDeclaracao : bool
  +DataHoraAceite
  +Respostas ' campos fixos Sim/Não
}

class AnamneseLink {
  +Token ' 128 bits, não sequencial
  +TelefoneDestino
  +Expiracao
  +Usado : bool
}

class SolicitacaoAtendimento {
  +Id
  +ProcedimentoInteresseTexto ' quando "outro"
  +DataEnvio
  +Status : {Recebida, EmTriagem, Agendada, Recusada}
  +MotivoRecusa
  +DataDecisao
}

class Procedimento {
  +Id
  +Nome
  +Descricao
  +Categoria
  +DuracaoMinutos
  +ValorReferencia
}

class Agendamento {
  +Id
  +Inicio
  +Fim ' derivado da duração
  +Status : {Agendado, Realizado, Faltou, Cancelado}
  +ValorCobrado ' editável
}

class Midia {
  +Id
  +ChaveS3
  +Data
  +Etiqueta : {Antes, Depois, Durante, Documento}
  +ConsentimentoClinico : bool
  +ConsentimentoPublico : bool
}

class LogAuditoria {
  +Id
  +Acao
  +EntidadeTipo
  +EntidadeId
  +DataHora
}

Clinica "1" o-- "*" Usuario
Clinica "1" o-- "*" Paciente
Clinica "1" o-- "*" Procedimento

Paciente "1" -- "*" Anamnese
Paciente "0..1" -- "*" AnamneseLink
Paciente "1" -- "*" SolicitacaoAtendimento
Paciente "1" -- "*" Midia
Paciente "1" -- "*" Agendamento

Anamnese "1" -- "0..1" SolicitacaoAtendimento
SolicitacaoAtendimento "0..1" -- "0..1" Agendamento : origem
Procedimento "1" -- "*" SolicitacaoAtendimento : interesse

Usuario "1" -- "*" Agendamento : profissional
Usuario "1" -- "*" SolicitacaoAtendimento : triagem
Usuario "1" -- "*" Midia : registrou
Usuario "1" -- "*" LogAuditoria

Procedimento "1" -- "*" Agendamento
Procedimento "1" -- "*" Midia : vínculo opcional
@enduml
```

### 4.1 Mesmo modelo em Mermaid (alternativa)

```mermaid
classDiagram
  class Clinica { +Id +Nome }
  class Usuario { +Id +Nome +Email +SenhaHash +Papel }
  class Paciente { +Id +Nome +Cpf +CpfPendente +Telefone +DataNascimento +Sexo +Endereco +RedeSocial }
  class Anamnese { +Id +DataPreenchimento +Peso +Altura +Imc +PressaoArterial +MotivoEstetico +AceiteDeclaracao }
  class AnamneseLink { +Token +TelefoneDestino +Expiracao +Usado }
  class SolicitacaoAtendimento { +Id +ProcedimentoInteresseTexto +DataEnvio +Status +MotivoRecusa +DataDecisao }
  class Procedimento { +Id +Nome +Descricao +Categoria +DuracaoMinutos +ValorReferencia }
  class Agendamento { +Id +Inicio +Fim +Status +ValorCobrado }
  class Midia { +Id +ChaveS3 +Data +Etiqueta +ConsentimentoClinico +ConsentimentoPublico }
  class LogAuditoria { +Id +Acao +EntidadeTipo +EntidadeId +DataHora }

  Clinica "1" o-- "*" Usuario
  Clinica "1" o-- "*" Paciente
  Clinica "1" o-- "*" Procedimento
  Paciente "1" --> "*" Anamnese
  Paciente "0..1" --> "*" AnamneseLink
  Paciente "1" --> "*" SolicitacaoAtendimento
  Anamnese "1" --> "0..1" SolicitacaoAtendimento
  SolicitacaoAtendimento "0..1" --> "0..1" Agendamento : origem
  Procedimento "1" --> "*" SolicitacaoAtendimento : interesse
  Paciente "1" --> "*" Midia
  Paciente "1" --> "*" Agendamento
  Usuario "1" --> "*" Agendamento : profissional
  Usuario "1" --> "*" SolicitacaoAtendimento : triagem
  Usuario "1" --> "*" Midia : registrou
  Procedimento "1" --> "*" Agendamento
  Procedimento "1" --> "*" Midia : opcional
```

---

## 5. Diagrama de Sequência — Entrada da Paciente (intake + triagem) (Mermaid)

```mermaid
sequenceDiagram
  actor Pac as Paciente
  actor Rec as Recepção
  participant Sys as Sistema (API)
  actor Tri as Recepção/Profissional (triagem)

  Pac-->>Rec: Procura a clínica com interesse
  Rec->>Sys: Gera link de anamnese (informa telefone destino)
  Sys->>Sys: Gera token 128 bits (expira, uso único)
  Sys-->>Rec: Retorna link tokenizado
  Rec-->>Pac: Envia link por WhatsApp
  Pac->>Sys: Abre link (sem login)
  Sys-->>Pac: Passo 1 — escolher procedimento de interesse
  Pac->>Sys: Informa procedimento de interesse
  Sys-->>Pac: Passo 2 — dados pessoais + anamnese
  Pac->>Sys: Preenche dados + anamnese (peso, altura, respostas) + aceite
  Sys->>Sys: Cria/atualiza paciente, calcula IMC, grava anamnese datada, queima token
  Sys->>Sys: Abre Solicitação de atendimento (status Recebida)
  Sys-->>Tri: Solicitação entra na fila de triagem
  Tri->>Sys: Avalia anamnese + procedimento desejado
  alt Pode fazer o procedimento
    Tri->>Sys: Agenda (profissional, data/hora, valor)
    Sys->>Sys: Cria Agendamento, marca solicitação como Agendada
    Sys-->>Pac: Mensagem direta com a data marcada
    Note over Pac,Tri: Pressão é aferida quando a paciente comparece
  else Não pode (contraindicação/motivo)
    Tri->>Sys: Recusa + registra motivo
    Sys->>Sys: Marca solicitação como Recusada
    Sys-->>Pac: Mensagem direta com o motivo
  end
```

---

## 6. Diagramas de Estados (Mermaid)

### 6.1 Solicitação de atendimento (triagem)

```mermaid
stateDiagram-v2
  [*] --> Recebida : paciente envia o link preenchido
  Recebida --> EmTriagem : recepção/profissional abre
  EmTriagem --> Agendada : pode fazer → agenda + mensagem direta
  EmTriagem --> Recusada : não pode → motivo + mensagem direta
  Agendada --> [*]
  Recusada --> [*]
```

### 6.2 Agendamento

> Um agendamento nasce quando uma solicitação é **Agendada** na triagem (ou de forma avulsa).

```mermaid
stateDiagram-v2
  [*] --> Agendado
  Agendado --> Realizado : paciente compareceu
  Agendado --> Faltou : não compareceu
  Agendado --> Cancelado : cancelado
  Realizado --> [*]
  Faltou --> [*]
  Cancelado --> [*]
```

---

## 7. O que pedir ao renderizador

1. Desenhar o **Diagrama de Casos de Uso** (seção 3), destacando visualmente o que é Fase 2.
2. Desenhar o **Diagrama de Classes** (seção 4), mostrando cardinalidades.
3. Desenhar o **Diagrama de Sequência** da entrada da paciente — intake + triagem (seção 5).
4. Desenhar os **Diagramas de Estados** — solicitação/triagem (6.1) e agendamento (6.2).

Ferramentas sugeridas para renderizar: PlantUML (plantuml.com), Mermaid Live Editor (mermaid.live), ou o próprio Claude no app.
