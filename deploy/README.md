# Deploy do piloto — VPS único (Brasil)

Sobe o stack completo (**Postgres + API .NET 8 + Caddy**) em **uma máquina só**, com **HTTPS automático** e o frontend servido na **mesma origem** da API (sem CORS). É o Épico A do [PRD do piloto](../03-PRD-Piloto-e-Deploy.md) (issue #16).

```
Internet ──HTTPS──▶ Caddy (web) ──┬─ /api/*  ──▶ API .NET (api:8080)
                    :80 :443       └─ demais  ──▶ SPA estático (/srv)
                                                     API ──▶ Postgres (db:5432)
                                                     API ──▶ /data/uploads (fotos)
```

Só o container **web** expõe portas ao host. API e banco ficam na rede interna do compose.

## Pré-requisitos

- Um **VPS em região Brasil** (LGPD + latência). Para o stack do piloto, mire **2 GB de RAM** (Postgres + .NET + Caddy juntos não cabem bem em 1 GB). Ex.: Vultr/DigitalOcean/Hetzner cobram por hora — dá para subir para a demo e destruir depois.
- **Docker** + **Docker Compose v2** instalados no VPS.
- Um **domínio (ou subdomínio)** com um registro **A** apontando para o **IP do VPS** — necessário para o Let's Encrypt emitir o certificado.

Instalar Docker num Ubuntu zerado:

```bash
curl -fsSL https://get.docker.com | sh
```

## Passo a passo

```bash
# 1. Clonar o repositório no VPS
git clone <url-do-repo> crm && cd crm/deploy

# 2. Criar o .env a partir do exemplo e editar os segredos
cp .env.example .env
nano .env          # DOMAIN, TLS_EMAIL, senhas, JWT_KEY, admin inicial

# 3. Gerar uma JWT_KEY forte de verdade (cole no .env)
openssl rand -base64 48

# 4. Apontar o DNS: registro A do DOMAIN -> IP do VPS (fazer antes de subir)

# 5. Subir o stack (build + start em background)
docker compose -f docker-compose.prod.yml up -d --build

# 6. Acompanhar os logs (migrations + seed rodam no start da API)
docker compose -f docker-compose.prod.yml logs -f api
```

Na primeira subida o Caddy emite o certificado TLS automaticamente (pode levar alguns segundos). Depois é só abrir `https://<DOMAIN>` no celular.

## Variáveis (`.env`)

| Variável | Para quê |
|----------|----------|
| `DOMAIN` | Domínio público. `localhost` p/ testar sem domínio (cert interno). |
| `TLS_EMAIL` | E-mail do Let's Encrypt. |
| `POSTGRES_USER` / `POSTGRES_PASSWORD` / `POSTGRES_DB` | Credenciais do banco. |
| `JWT_KEY` | **Trocar** o placeholder — ≥ 32 bytes, aleatória. |
| `ADMIN_EMAIL` / `ADMIN_SENHA` / `ADMIN_NOME` | Admin inicial (seed roda **uma vez**). |
| `CLINICA_NOME` | Nome da clínica no seed. |
| `SEED_PROCEDIMENTOS` | `true` para popular procedimentos de exemplo. |
| `LIMITE_PROFISSIONAIS` | Vagas de profissional do plano contratado. Só é lido no primeiro start; para mudar depois, `UPDATE "Clinicas" SET "LimiteProfissionais" = <novo_valor>;` direto no Postgres do cliente. |

Segredos ficam **só no `.env`** do VPS (não versionado). O `appsettings.json` mantém apenas placeholders de desenvolvimento.

## Persistência

Os dados sobrevivem a `docker compose down` / recriação de containers porque ficam em **volumes nomeados**:

- `crm_pgdata` → banco (Postgres).
- `crm_uploads` → **fotos/arquivos** (`/data/uploads`). Nunca no Postgres, nunca no PC do cliente.
- `caddy_data` → certificados TLS (evita re-emitir a cada restart).

> `docker compose down` **preserva** os volumes. `docker compose down -v` **apaga** os dados — não use em produção.

## Trocar o domínio

1. Ajuste o registro **A** do novo domínio para o IP do VPS.
2. Edite `DOMAIN` (e `TLS_EMAIL`) no `.env`.
3. `docker compose -f docker-compose.prod.yml up -d` (recria só o web; o Caddy reemite o certificado).

## Operação do dia a dia

```bash
# Status / logs
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs -f api

# Atualizar após um git pull
git pull && docker compose -f docker-compose.prod.yml up -d --build

# Parar (mantém os dados)
docker compose -f docker-compose.prod.yml down
```

## Aceite (checklist do PRD)

- [ ] `https://<DOMAIN>` abre no celular, com cadeado (HTTPS válido).
- [ ] Login funciona; navegação sem `localhost` em nenhuma chamada.
- [ ] Enviar uma anamnese pelo link público conclui com sucesso.
- [ ] Após `docker compose down` e `up`, pacientes/anamneses/agenda **e fotos** continuam lá.

> Os dados persistem em volumes nomeados do Docker (`crm_pgdata`, `crm_uploads`). Backup automático offsite está **fora do escopo** do piloto (decisão do dono do projeto).
