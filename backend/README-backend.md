# CRM Clínica de Estética — Backend (MVP / Fase 1)

API em **ASP.NET Core (.NET 8)** com **Clean Architecture**, EF Core + PostgreSQL, autenticação JWT por papel e o novo fluxo de **intake + triagem**.

## Arquitetura

```
backend/
  Crm.sln
  docker-compose.yml            # PostgreSQL 16
  src/
    Crm.Domain/                 # Entidades + enums (sem dependências)
    Crm.Application/            # Casos de uso por feature (vertical slices) + interfaces
    Crm.Infrastructure/         # EF Core (DbContext, migrations), JWT, hash, storage
    Crm.Api/                    # Controllers, DI, Swagger, seed
```

Decisões de implementação (autônomas, dentro do escopo aprovado):
- **Sem MediatR**: cada feature é um *service* em `Application/Features/...`, registrado por convenção (`AddApplication`).
- **Hash de senha**: BCrypt (workFactor 12).
- **Storage de mídia**: `LocalFileStorage` (grava em `./uploads`) para desenvolvimento. Em produção, trocar por implementação **AWS S3 (sa-east-1)** com URLs assinadas — a interface `IFileStorage` já isola isso.
- **Datas**: agendamentos usam **UTC** (`timestamptz`). O front deve enviar/exibir convertendo do fuso local.
- Enums serializados como **string** no JSON.

## Como rodar (do zero)

Pré-requisitos: .NET 8 SDK, Docker Desktop.

```bash
# 1) Subir o PostgreSQL
cd backend
docker compose up -d

# 2) Rodar a API (aplica migrations e faz o seed automaticamente)
cd src/Crm.Api
dotnet run
```

- Swagger UI: **http://localhost:5080/swagger** (defina a porta com `ASPNETCORE_URLS=http://localhost:5080`, ou use a porta do `launchSettings`).
- Ao subir em `Development`, a API **aplica as migrations** e **cria o seed** (clínica + 3 usuários de teste).

> Se o EF reclamar de migration, gere/atualize com:
> `dotnet ef migrations add <Nome> -p src/Crm.Infrastructure -s src/Crm.Api -o Persistence/Migrations`
> `dotnet ef database update -p src/Crm.Infrastructure -s src/Crm.Api`

## Usuários de teste (seed)

| Papel | E-mail | Senha |
|-------|--------|-------|
| Admin | `admin@clinica.local` | `Admin@123` |
| Profissional | `profissional@clinica.local` | `Admin@123` |
| Recepção | `recepcao@clinica.local` | `Admin@123` |

No Swagger: `POST /api/auth/login` → copie o `token` → botão **Authorize** → cole o token.

## Endpoints principais

### Auth
- `POST /api/auth/login` — login, retorna JWT + papel.

### Pacientes (interno)
- `GET /api/pacientes?termo=&page=&pageSize=` — busca por nome/CPF/telefone.
- `GET /api/pacientes/{id}`
- `GET /api/pacientes/duplicidade?cpf=&telefone=` — alerta de duplicidade (RF08).
- `POST /api/pacientes` · `PUT /api/pacientes/{id}`

### Catálogo (Admin/Recepção para escrever)
- `GET /api/procedimentos?incluirInativos=` · `GET /api/procedimentos/{id}`
- `POST /api/procedimentos` · `PUT /api/procedimentos/{id}`

### Anamnese — link interno (Admin/Recepção)
- `POST /api/anamnese/links` — gera link tokenizado. Body: `{ telefoneDestino?, validadeHoras? }`.

### Anamnese — intake PÚBLICO (sem login, rate limited)
- `GET /api/public/anamnese/{token}` — abre o link e retorna o catálogo (escolha do procedimento de interesse).
- `POST /api/public/anamnese/{token}` — envia procedimento de interesse + dados + anamnese; cria paciente + anamnese (IMC calculado) + **solicitação (Recebida)** e queima o token.

### Triagem (interno)
- `GET /api/triagem?status=` — fila (padrão: Recebida/EmTriagem).
- `GET /api/triagem/{id}` — **detalhe com anamnese** (Admin/Profissional; recepção recebe 403).
- `POST /api/triagem/{id}/assumir`
- `POST /api/triagem/{id}/agendar` — cria o agendamento e devolve a mensagem para a paciente.
- `POST /api/triagem/{id}/recusar` — exige motivo; devolve a mensagem para a paciente.

### Agenda (interno)
- `GET /api/agenda?profissionalId=&de=&ate=`
- `POST /api/agenda` — agendamento avulso (bloqueia colisão do profissional — RN03).
- `PATCH /api/agenda/{id}/status` — Realizado/Faltou/Cancelado (cancelamento exige motivo).
- `PATCH /api/agenda/{id}/valor` — edita valor cobrado.

### Mídia (Admin/Profissional — dado sensível)
- `POST /api/midias` (multipart: `pacienteId, etiqueta, procedimentoId?, consentimentoClinico, consentimentoPublico, arquivo`)
- `GET /api/midias?pacienteId=` · `GET /api/midias/{id}/conteudo` (download autenticado) · `DELETE /api/midias/{id}`

### LGPD (Admin)
- `POST /api/lgpd/pacientes/{id}/anonimizar` — direito ao esquecimento (anonimiza PII, remove mídias, registra auditoria).

## Fluxo de ponta a ponta (o que foi testado)

1. Login (admin/profissional/recepção).
2. Cria procedimento → gera link → abre link (público) → **intake** (cria paciente + anamnese, IMC ok) → token vira 409 se reusado.
3. Fila de triagem mostra a solicitação → detalhe (admin vê anamnese; recepção 403) → **agendar** (cria agendamento, status Agendada) → **colisão** bloqueada (409).
4. **Recusar** (status Recusada + mensagem). Mídia upload/download idêntico. LGPD anonimiza (nome → `[ANONIMIZADO]`).

## Próximos passos sugeridos
- Storage S3 (produção) implementando `IFileStorage`.
- Testes automatizados (xUnit + WebApplicationFactory — `Program` já é `partial` para isso).
- Frontend React consumindo a API (rota pública isolada `/anamnese/{token}`).
- 2ª anamnese, campanhas/descontos/indicações (Fase 2).
