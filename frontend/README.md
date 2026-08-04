# CRM Clínica de Estética — Frontend (MVP / Fase 1)

App **React + Vite + TypeScript** que consome a API .NET. Rota pública isolada para a anamnese tokenizada; área interna protegida por JWT com guarda por papel.

## Stack e estrutura

- **Vite + React 18 + TypeScript**, **react-router-dom**, **axios**.
- Sem framework de UI: design system próprio em `src/index.css` (tema rosé/mauve, responsivo).

```
frontend/
  src/
    api/          # client axios (JWT + 401), tipos dos DTOs e serviços por feature
    auth/         # AuthContext (sessão) e RequireAuth (guarda por papel)
    components/   # Layout, Modal, Toast, ui, AnamneseView, MidiaGaleria, GerarLinkModal…
    lib/          # formatação (moeda, datas UTC↔local, CPF, WhatsApp)
    pages/        # telas (login, dashboard, pacientes, catálogo, triagem, agenda…)
      public/     # AnamnesePublicaPage (sem login)
```

## Como rodar

Pré-requisito: a API no ar (ver `../backend/README-backend.md`). Por padrão em `http://localhost:5240`.

```bash
cd frontend
npm install
npm run dev          # http://localhost:5173
```

Configure a URL da API em `.env` (já incluso para dev):

```
VITE_API_URL=http://localhost:5240
```

Build de produção: `npm run build` (gera `dist/`).

## Login (usuários seed)

| Papel | E-mail | Senha |
|-------|--------|-------|
| Admin | `admin@clinica.local` | `Admin@123` |
| Profissional | `profissional@clinica.local` | `Admin@123` |
| Recepção | `recepcao@clinica.local` | `Admin@123` |

## Papéis e telas

- **Recepção**: painel, triagem (fila, sem detalhe clínico), agenda, pacientes, catálogo (leitura), gerar link de anamnese.
- **Profissional**: tudo acima + detalhe clínico da triagem (anamnese) e galeria de mídia do paciente.
- **Admin**: acesso total (inclui escrever no catálogo).

## Fluxo de ponta a ponta (demonstração)

1. Login como Admin/Recepção → **Painel** → **+ Link de anamnese** → copie/abra o link.
2. Abra `/anamnese/{token}` (sem login): escolha o procedimento, preencha dados + anamnese, aceite a declaração → confirmação com **IMC** calculado. Reabrir o link já usado mostra erro.
3. **Triagem**: a solicitação aparece na fila → **Avaliar** (Admin/Profissional vê a anamnese; recepção recebe 🔒) → **Agendar** (profissional, data/hora, valor) ou **Recusar** (motivo) → botão **Abrir no WhatsApp** com a mensagem pronta.
4. **Agenda**: filtre por profissional/período, crie avulso, mude status e valor; colisão de horário é bloqueada (RN03).
5. **Pacientes**: busca, cadastro/edição com alerta de duplicidade; detalhe com **galeria de mídia** (upload/baixar/excluir, etiquetas e consentimentos).

## Notas

- Datas de agendamento trafegam em **UTC**; o front converte para o fuso local nas telas e nos formulários.
- O envio de mensagens (WhatsApp) é **manual** no MVP (link `wa.me`) — automação depende de itens com orçamento (issue de backlog).
