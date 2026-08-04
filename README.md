# CRM para Clínica de Estética

CRM sob medida para uma clínica de estética real (single-tenant, preparado para multi-clínica). Digitaliza a operação: captação da paciente por link de anamnese, **triagem**, agenda, catálogo de procedimentos e fotos de antes/depois — tratando **dados sensíveis de saúde** com LGPD desde o dia 1.

> **Estratégia:** pensar grande, começar pequeno. Nasce single-tenant, mas com `ClinicaId` nas entidades para virar produto multi-clínica sem reescrita.

## Fluxo de entrada (o coração do produto)

A cliente demonstra interesse → a clínica envia um **link de anamnese sem login** → a própria paciente **informa o procedimento de interesse** e **preenche seus dados + a anamnese** → o sistema abre uma **Solicitação de atendimento** → a recepção/profissional faz a **triagem** e decide: **agendar** (mensagem direta com a data) ou **recusar** (motivo).

## Estrutura do repositório

| Caminho | O que é |
|---------|---------|
| `00-Plano-Aprovado.md` | Plano aprovado: decisões, modelo de dados, fases, verificação. |
| `01-Documento-de-Requisitos.md` | Requisitos funcionais/não-funcionais por fase, regras de negócio, campos da anamnese. |
| `02-Handoff-Diagrama-UML.md` | Diagramas UML (casos de uso, classes, sequência, estados) em PlantUML/Mermaid. |
| `CRM Clinica de Estetica - Apresentacao para Cliente.pptx/.pdf` | Apresentação do projeto para o cliente (v1.1, fluxo revisado). |
| `backend/` | **API .NET 8** (Clean Architecture) — Fase 1 implementada e testada. Ver `backend/README-backend.md`. |
| `frontend/` | **App React + Vite + TypeScript** — Fase 1 implementada. Ver `frontend/README.md`. |
| `.github/workflows/ci.yml` | CI (GitHub Actions): build + testes do backend (com Postgres) e build do frontend. |

## Status

- ✅ **Backend (Fase 1 / MVP)** — Auth JWT por papel, Pacientes, Catálogo, Anamnese (link interno + intake público sem login), Triagem (agendar/recusar), Agenda (bloqueio de colisão), Mídia, LGPD (anonimização). Validação de CPF com dígito verificador (FluentValidation), middleware global de erros (ProblemDetails) e **testes automatizados** (xUnit + WebApplicationFactory contra Postgres).
- ✅ **Frontend (React)** — login/sessão JWT com guarda por papel, anamnese pública, pacientes, catálogo, triagem, agenda e galeria de mídia. Responsivo.
- ⏳ **Fase 2/3** — engajamento, comercial e marketing. Itens com custo (S3, deploy, WhatsApp/e-mail) no backlog.

## Stack

- **Backend:** ASP.NET Core (.NET 8), Clean Architecture, EF Core + PostgreSQL, JWT, BCrypt, FluentValidation.
- **Frontend:** React 18 + Vite + TypeScript, react-router, axios (responsivo).
- **Storage de mídia:** local em dev; **AWS S3 (sa-east-1)** em produção (interface `IFileStorage` já pronta).

## Rodando (resumo)

```bash
# 1) Backend
cd backend
docker compose up -d               # PostgreSQL
cd src/Crm.Api && dotnet run        # aplica migrations + seed automaticamente (http://localhost:5240)

# 2) Frontend (em outro terminal)
cd frontend
npm install && npm run dev          # http://localhost:5173
```

Usuários de teste (seed): `admin@clinica.local` · `profissional@clinica.local` · `recepcao@clinica.local` — senha `Admin@123`. Detalhes em [`backend/README-backend.md`](backend/README-backend.md) e [`frontend/README.md`](frontend/README.md).
