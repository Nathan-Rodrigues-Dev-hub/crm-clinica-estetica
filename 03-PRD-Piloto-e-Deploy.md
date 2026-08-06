# PRD — Piloto de 1 mês, Deploy e Robustez de Dados

> **Status:** aprovado para implementação · **Origem:** conversa de negócio (05/08/2026) sobre colocar o MVP em uma clínica real por 1 mês, grátis, como demonstração antes da cobrança.
> **Fase:** ponte entre a Fase 1 (MVP já implementado) e a cobrança recorrente. Não é Fase 2.

---

## 1. Contexto

O MVP (Fase 1) está implementado e testado (ver [`00-Plano-Aprovado.md`](00-Plano-Aprovado.md) e [`backend/README-backend.md`](backend/README-backend.md)). O cliente (sócio da clínica) topou uma **reunião com protótipo** e um **piloto de 1 mês sem custo** para a equipe usar e avaliar. Ele sinalizou cautela: *"não vou me comprometer a investir nisso nesse momento, mas podemos conversar"*.

A maior dor relatada com o SaaS anterior da clínica: **o sistema caía e perdia informação do que estavam preenchendo**. Isso torna **confiabilidade e não-perda de dados** o eixo central deste PRD — é o que constrói a confiança que leva à venda.

### Decisões técnicas travadas nesta conversa (contexto para quem implementa)

- **Onde roda:** um **VPS único em região Brasil** (LGPD + latência), com API .NET + PostgreSQL + arquivos + HTTPS na mesma máquina, via Docker Compose. **Não** usar Netlify/Vercel para o conjunto — eles só hospedam frontend estático, não rodam a API .NET nem o Postgres nem Docker. O frontend pode ser servido pelo próprio proxy (Caddy) no VPS.
- **Onde ficam as fotos:** em **disco no VPS** (`uploads/`), **nunca no Postgres** (o banco só guarda metadados — ver `MidiaService.cs`/`LocalFileStorage.cs`) e **nunca no PC do cliente** (o navegador é só uma janela). Acessível de qualquer aparelho por ser web.
- **AWS S3:** **fora de escopo** neste momento. Local em disco resolve o piloto (dezenas de fotos). S3 fica para escala/produto multi-clínica (Fase 2).
- **WhatsApp/e-mail automático (issue #14, `needs-budget`):** **fora de escopo**. No piloto a mensagem de agendamento é enviada manualmente pela recepção.
- **Custo estimado do piloto:** ~R$ 40–70/mês (VPS BR + domínio). Provedores com cobrança por hora (DigitalOcean/Vultr/Hetzner) permitem subir para a demo e destruir se o cliente disser não; AWS Lightsail São Paulo mantém o dado no Brasil.

---

## 2. Objetivo

Deixar o MVP **no ar, com URL própria e HTTPS**, **confiável o suficiente para operar 1 mês sem perder dados**, e **impecável no iPhone** (aparelho provável da recepção/equipe na clínica) — para que a demo e o piloto virem a conversa de cobrança.

## 3. Critérios de sucesso do piloto (combinar com o cliente na reunião)

- A recepção consegue operar o dia a dia **de um iPhone**, sem travas de layout.
- Nenhum relato de "perdi o que estava preenchendo".
- Ao final do mês, existe um "funcionou" mensurável (ex.: recepção largou o caderno/planilha; X pacientes entraram pelo link) e uma **data de decisão** marcada.

---

## 4. Escopo — Épicos e sub-tarefas

Cada épico vira uma issue no GitHub (seção 6). As sub-tarefas abaixo são o checklist de implementação de cada uma.

### Épico A — Deploy de produção em VPS (Brasil)
Colocar o stack completo no ar em uma máquina só, com HTTPS e configuração de produção.

- [ ] `Dockerfile` de produção para a API .NET 8 (`backend/src/Crm.Api`) — build multi-stage, imagem runtime enxuta, expõe em `http://+:8080`, `ASPNETCORE_ENVIRONMENT=Production`.
- [ ] `docker-compose.prod.yml` orquestrando: `db` (Postgres 16 com volume nomeado), `api` (com volume para `uploads/` em `/data/uploads`), `web` (proxy).
- [ ] Proxy **Caddy** servindo o build estático do frontend **e** fazendo reverse proxy de `/api/*` → `api:8080`, com **HTTPS automático** (Let's Encrypt) a partir de um domínio.
- [ ] Frontend buildado apontando para a **mesma origem** (`VITE_API_URL` relativo/vazio → chamadas `/api/...`), eliminando CORS de origem cruzada.
- [ ] Segredos por variável de ambiente (`.env` não versionado): `ConnectionStrings__DefaultConnection`, `Jwt__Key` (gerar chave forte real, **trocar o placeholder** de `appsettings.json`), `Cors__Origens` = domínio de produção.
- [ ] `Storage:LocalPath` apontando para o volume persistente (`/data/uploads`), garantindo que as fotos sobrevivem a recriação de container.
- [ ] Migrations + seed aplicados em produção no start (já ocorre em `Program.cs`); validar que o seed de admin/procedimentos roda uma vez.
- [ ] `deploy/README.md`: passo a passo para subir num VPS zerado (Docker, apontar DNS, subir compose) e trocar o domínio.

**Aceite:** abrir `https://<dominio>` num celular, logar, navegar e enviar uma anamnese pelo link — tudo por HTTPS, sem localhost, dados persistindo após `docker compose down/up`.

### Épico B — Backup automático + restauração
Garantir que, mesmo se o VPS morrer, nenhum dado se perde. **É o antídoto direto à dor do SaaS anterior.**

- [ ] Serviço/rotina de backup **diário**: `pg_dump` do Postgres + snapshot (tar) da pasta `uploads/`.
- [ ] Envio **offsite** dos backups (ex.: object storage barato — Backblaze B2 / bucket S3 / outro), fora do próprio VPS.
- [ ] Política de **retenção** (ex.: 7 diários + 4 semanais) com limpeza automática dos antigos.
- [ ] **Procedimento de restauração documentado e testado** ao menos uma vez (restaurar dump + uploads em uma máquina limpa e validar login/fotos).
- [ ] Registro simples de sucesso/falha do backup (log ou alerta), para não descobrir que o backup parou só na hora do desastre.

**Aceite:** existe um backup offsite de ontem; restaurar num ambiente limpo recompõe pacientes, anamneses, agenda e fotos sem perda.

### Épico C — Autosave de rascunho na anamnese pública
Não perder o que a paciente (ou a recepção) digita, mesmo se a página fechar, cair a conexão ou o aparelho desligar antes de enviar.

Arquivo-alvo: `frontend/src/pages/public/AnamnesePublicaPage.tsx`.

- [ ] Persistir o preenchimento no `localStorage`, **chaveado pelo token** do link (`crm.rascunho.<token>`), com debounce (~600ms) enquanto a paciente digita.
- [ ] Ao reabrir o link no mesmo aparelho, **restaurar** os campos e a etapa em que parou.
- [ ] **Banner** discreto informando que o preenchimento anterior foi recuperado.
- [ ] Indicador sutil de "rascunho salvo automaticamente".
- [ ] **Limpar** o rascunho após envio bem-sucedido.
- [ ] Degradar sem erro quando `localStorage` estiver indisponível/cheio (não quebrar o fluxo).

> Nota de implementação: uma versão inicial deste autosave foi prototipada e **revertida de propósito** para implementar limpo a partir desta issue. Cobrir também o autosave nas **telas internas com formulário longo** se o esforço for baixo (ex.: cadastro/edição de paciente), mas a anamnese pública é a prioridade.

**Aceite:** preencher metade da anamnese, fechar a aba, reabrir o mesmo link no mesmo aparelho → os dados voltam e o envio segue normal; após enviar, o rascunho some.

### Épico D — Responsividade para iPhone 14+ (Safari iOS)
Garantir que todas as telas fiquem impecáveis nos iPhones que a equipe provavelmente usa.

Alvos (viewport lógico): **iPhone 14 / 14 Pro (390×844 / 393×852)** e **14 Plus / 14 Pro Max (428×926 / 430×932)**; considerar **notch e Dynamic Island** e o **home indicator**.

- [ ] `meta viewport` com `viewport-fit=cover` e uso de `env(safe-area-inset-*)` para não deixar conteúdo sob o notch/Dynamic Island nem sob o home indicator (headers, barras fixas, botões de ação).
- [ ] Corrigir altura de tela cheia com `100dvh`/`100svh` no lugar de `100vh` (evita o "pulo" da barra do Safari iOS).
- [ ] **Inputs com `font-size ≥ 16px`** para evitar o auto-zoom do Safari ao focar campo (impacta principalmente a anamnese pública).
- [ ] Alvos de toque ≥ 44×44px; ajustar `-webkit-tap-highlight-color` e estados de foco/toque.
- [ ] **Menu hamburguer / navegação lateral:** o drawer off-canvas **já existe** (`Layout.tsx` — botão `☰`, classe `sidebar-open`, `backdrop`); a tarefa é **garantir que funciona no iPhone**: hamburger como alvo de toque ≥44px, drawer com `100dvh` e respeitando safe-area, backdrop cobrindo a viewport inteira, fechar ao navegar/tocar fora, sem overflow.
- [ ] **Tela de login (`LoginPage.tsx`):** colapsar o layout de 2 colunas (`login-hero` + `login-panel`) para 1 coluna no iPhone — hero reduzido/oculto, card centralizado, inputs ≥16px, botão "Entrar" acessível com o teclado virtual aberto.
- [ ] **Cadastro de paciente (`PacienteFormModal.tsx`) e demais modais de formulário (`GerarLinkModal.tsx`):** no iPhone, virar tela cheia ou bottom-sheet em viewport estreita, `form-row` colapsando para 1 coluna, scroll interno, teclado virtual não cobrindo os botões (Cancelar/Cadastrar), inputs ≥16px. (A paciente também se auto-cadastra pelo link público — já coberto no fluxo de anamnese.)
- [ ] Auditar **todas as demais telas** no Safari iOS: anamnese pública, painel, pacientes, catálogo, triagem, agenda, galeria de mídia. Prioridade: **fluxo público de anamnese** (paciente no próprio celular) e telas de uso diário da recepção.
- [ ] Tabelas/listas largas (pacientes, agenda) com layout que não estoura a viewport estreita (scroll contido ou reflow para cartões).
- [ ] Verificar teclado virtual não cobrindo o campo ativo / botão de envio.

**Aceite:** percorrer os fluxos principais em um iPhone 14 e um 14 Pro Max (ou emulação equivalente no Safari) sem overflow horizontal, sem zoom indesejado ao focar input, sem conteúdo sob o notch/home indicator.

### Épico E — Identidade visual (Dra. Mariangel Camacho)
Aplicar a identidade da marca ao frontend, substituindo o tema atual (rosa/plum) pelo **vinho + branco**. Fonte da verdade: manual da marca em `Assinaturas [dra mariangel].pdf` (extraído nesta sessão).

**Paleta (do manual — "sofisticação, confiança, acolhimento e autoridade"):**

| Papel | Hex | Uso |
|-------|-----|-----|
| **Vinho (primária)** | `#58091F` | ações primárias, marca, nav ativo, foco, destaques |
| **Branco** | `#FFFFFF` | fundo base, superfícies |
| Vinho escuro (derivado) | `#40060F` | hover / estados pressionados |
| Vinho tint (derivado) | `#FAF3F5` | fundos suaves, seleção, linhas alternadas |
| Texto (derivado) | `#241E20` | texto principal |
| Muted (derivado) | `#6E646A` | texto secundário |
| Borda (derivado) | `#ECE3E6` | divisórias, bordas de input/card |

> Derivados são ajustáveis; o vinho `#58091F` e o branco são os fixos do manual.

**Tipografia (do manual):**

| Fonte | Papel no manual | Uso na UI | Web? |
|-------|-----------------|-----------|------|
| **Playfair Display** (serif) | conteúdos / títulos e destaques | **Títulos (h1–h4), destaques** | Google Fonts ✓ |
| **Montserrat** (sans) | subtítulos, apoio, textos curtos | **UI, corpo, labels, botões** | Google Fonts ✓ |
| Glacial Indifference | fonte da marca (logotipo) | só logo/asset; fallback geométrico | ✗ não-web |
| TAN The Whistling | fonte de apoio (assinaturas) | só peças de marca; **não usar na UI** | ✗ não-web |

**Sub-tarefas:**
- [ ] Redefinir os tokens em `frontend/src/index.css` (`:root`): remapear `--rose*` / `--plum` / `--ink` / `--line` para o vinho+branco (tabela acima). Como o CSS já usa variáveis, a troca no `:root` cascateia por todo o app.
- [ ] Importar **Playfair Display** + **Montserrat** (Google Fonts); `--font` = Montserrat; novo `--font-title` = Playfair Display aplicado em h1–h4.
- [ ] Ajustar cores hardcoded (gradiente de fundo, sombra de foco `rgba(183,110,121,…)`) para o tom vinho.
- [ ] Trocar o mark "✿" pela marca **MC** / logo da Dra. Mariangel quando o asset estiver disponível.
- [ ] Revisar contraste (WCAG AA) de texto sobre vinho e vinho sobre claro.
- [ ] Garantir a identidade também no **login** e na **anamnese pública** (as telas "de fora", que a paciente vê).

**Aceite:** app em vinho+branco, títulos em Playfair, UI em Montserrat, sem resíduo do tema rosa; login e anamnese coerentes com a marca.

### Épico F — Tela de cadastro/gestão de usuários da equipe
Hoje **não há UI** para criar usuários (recepção/profissional/admin) — são criados por *seed* no backend. Para o piloto, a clínica precisa cadastrar a própria equipe pela interface.

**Sub-tarefas:**
- [ ] Backend: endpoints CRUD de usuários da equipe (criar/editar/desativar), **restritos ao Admin**; hash de senha (BCrypt, já em uso); e-mail único por clínica.
- [ ] Frontend: tela "Equipe / Usuários" (listar, criar, editar, desativar) com papel (Admin/Profissional/Recepção), visível **só para Admin**.
- [ ] Senha inicial definida pelo Admin no cadastro (troca de senha pelo próprio usuário fica para depois, se necessário).
- [ ] Incluir a tela na auditoria de responsividade iPhone (Épico D) e na identidade visual (Épico E).
- [ ] `seed` permanece só para o Admin inicial; equipe passa a ser criada pela UI.

**Aceite:** Admin cria uma recepcionista pela interface, ela loga e opera; usuário não-Admin não enxerga a tela.

---

## 5. Fora de escopo (não fazer agora)

- Integração **AWS S3** (fica para escala/Fase 2 — interface `IFileStorage` já preparada).
- **WhatsApp/e-mail automático** (issue #14, `needs-budget`) — mensagens manuais no piloto.
- Qualquer item de **Fase 2/3** (engajamento, comercial, marketing, agendamento online do paciente, 2FA).

## 6. Issues linkadas

As sub-tarefas de cada épico estão detalhadas nas issues abaixo (criadas a partir deste PRD):

<!-- ISSUES:START -->
| Épico | Issue | Labels |
|-------|-------|--------|
| A — Deploy de produção em VPS (Brasil) | [#16](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/16) | `backend` `enhancement` |
| B — Backup automático + restauração | [#17](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/17) | `backend` `enhancement` |
| C — Autosave de rascunho na anamnese pública | [#18](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/18) | `frontend` `enhancement` |
| D — Responsividade para iPhone 14+ (Safari iOS) | [#19](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/19) | `frontend` `enhancement` |
| E — Identidade visual (vinho + branco, Playfair/Montserrat) | [#20](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/20) | `frontend` `enhancement` |
| F — Cadastro/gestão de usuários da equipe | [#21](https://github.com/Nathan-Rodrigues-Dev-hub/crm-clinica-estetica/issues/21) | `frontend` `backend` `enhancement` |

Milestone: **Piloto - deploy e robustez** (#2).

**Ordem sugerida de implementação:** E (identidade — base visual) → C, D e F (frontend, demonstráveis, já no tema novo) → A (deploy) → B (backup, depende do deploy estar de pé).
<!-- ISSUES:END -->
