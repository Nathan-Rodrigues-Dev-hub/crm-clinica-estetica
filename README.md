# CRM para Clínica de Estética

CRM para clínicas de estética. Digitaliza a operação: captação da paciente por link de anamnese, **triagem**, agenda, catálogo de procedimentos e fotos de antes/depois — tratando **dados sensíveis de saúde** com LGPD desde o dia 1. Cobrança por vaga de profissional (R$120/profissional/mês).

> **Estratégia:** pensar grande, começar pequeno. Hoje roda em produção **single-tenant** (uma clínica por VPS/banco), mas com `ClinicaId` em toda entidade desde o início — o modelo de dados já é multi-tenant por baixo dos panos. A expansão para várias clínicas numa infraestrutura compartilhada (marca **Essenza**) está especificada e aguardando implementação: ver [issue #30](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/30) (PRD) e [issue #31](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/31) (identidade visual).

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
| `deploy/` | Deploy de produção (VPS único, Docker Compose: Postgres + API + Caddy/HTTPS). Ver `deploy/README.md`. |
| `.github/workflows/ci.yml` | CI (GitHub Actions): build + testes do backend (com Postgres) e build do frontend. |

## Status

- ✅ **Backend (Fase 1 / MVP)** — Auth JWT por papel, Pacientes, Catálogo, Anamnese (link interno + intake público sem login + registro manual pela recepção), Triagem (agendar/recusar), Agenda (bloqueio de colisão), Mídia (upload, renomear, download), LGPD (anonimização). Validação de CPF com dígito verificador (FluentValidation), middleware global de erros (ProblemDetails) e **testes automatizados** (xUnit + WebApplicationFactory contra Postgres).
- ✅ **Frontend (React)** — login/sessão JWT com guarda por papel, anamnese pública e manual, histórico de anamnese, pacientes, catálogo, triagem, agenda e galeria de mídia (com visualização em tela cheia). Responsivo (iPhone).
- ✅ **Em produção** — piloto rodando num VPS (Docker Compose: Postgres + API + Caddy/HTTPS). Ver `deploy/README.md`.
- ✅ **Licenciamento por vagas de profissional** — `Clinica.LimiteProfissionais` bloqueia criar/reativar profissional além do plano contratado; painel de Equipe mostra vagas em uso. Camada 1 de um plano maior anti-fraude ([issue #25](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/25)).
- ✅ **Hierarquia de permissões** — além de Admin/Profissional/Recepção, existe o papel **SuperAdmin** (suporte/dono da plataforma): só ele gerencia contas Profissional e redefine senha de terceiros; todo usuário troca a própria senha por autoatendimento.
- ⏳ **Expansão multi-tenant (Essenza)** — especificada, não implementada: infraestrutura compartilhada servindo várias clínicas por subdomínio, conta de SuperAdmin fora de qualquer clínica, identidade visual própria. Ver [issue #30](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/30) (PRD) e [issue #31](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/31) (spec visual).
- ⏳ **Fase 2/3** — engajamento, comercial e marketing. Itens com custo (S3, WhatsApp/e-mail automático) no backlog ([issue #14](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/14)).

## Stack

- **Backend:** ASP.NET Core (.NET 8), Clean Architecture, EF Core + PostgreSQL, JWT, BCrypt, FluentValidation.
- **Frontend:** React 18 + Vite + TypeScript, react-router, axios (responsivo).
- **Storage de mídia:** local em disco (dev e produção — volume Docker persistente); interface `IFileStorage` já isola isso, S3 fica para quando a escala multi-clínica pedir.

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

Usuários de teste (seed): `admin@clinica.local` · `profissional@clinica.local` · `recepcao@clinica.local` — senha `Admin@123`. Uma conta **SuperAdmin** é criada à parte só se `Seed:SuperAdminSenha` estiver configurada (não entra no seed padrão). Detalhes em [`backend/README-backend.md`](backend/README-backend.md) e [`frontend/README.md`](frontend/README.md); deploy de produção em [`deploy/README.md`](deploy/README.md).
