# Roteiro de Testes em Tela — CRM GA Clinic & Academy

> **Para quem vai executar (Claude com controle de tela / navegador):** este documento é autossuficiente. Siga os casos na ordem. Em cada passo há o **resultado esperado** — compare com o que aparece na tela e registre **PASSOU / FALHOU** (com print e descrição se falhar). Não precisa de conhecimento prévio do sistema.

---

## 0. Ambiente e acesso

- **Sistema (frontend):** http://localhost:5173  ← abrir aqui
- **API (backend):** http://localhost:5240 (o frontend já fala com ela sozinho; não precisa abrir)
- Se a tela de login não carregar, o sistema não está no ar — avise o responsável (não tente subir nada).

### Usuários de teste (senha `Admin@123` para todos)

| E-mail | Papel | Enxerga |
|--------|-------|---------|
| `admin@clinica.local` | **Admin** | Tudo, incluindo **Equipe** e detalhe clínico |
| `profissional@clinica.local` | **Profissional** | Detalhe de triagem e fotos; **não** vê Equipe |
| `recepcao@clinica.local` | **Recepção** | Fila/agenda/pacientes; **não** vê detalhe clínico nem Equipe |

### Convenções
- "Toast" = mensagem flutuante no canto inferior direito (verde=sucesso, vermelho=erro).
- Sempre que criar dado de teste, use um nome reconhecível, ex.: `TESTE <feature> <hora>`.
- Alguns dados já podem existir no banco de demonstração — isso é normal.

---

## 1. Autenticação e controle por papel

**1.1 Login válido (Admin)**
1. Abrir http://localhost:5173 → deve exibir a tela de **login** em 2 colunas (painel dourado à esquerda com monograma **"GA"**, formulário à direita).
2. Campos já vêm preenchidos com `admin@clinica.local` / `Admin@123`. Clicar **Entrar**.
   - ✅ Esperado: toast "Bem-vindo(a)!" e redireciona para o **Painel** ("Olá, Administrador 👋").

**1.2 Menu por papel (Admin)**
- Na barra lateral esquerda, o Admin deve ver: **Painel, Triagem, Agenda, Pacientes, Catálogo, Equipe**.
   - ✅ O item **Equipe** aparece (é exclusivo de Admin).

**1.3 Logout**
1. No rodapé da barra lateral, clicar **Sair**.
   - ✅ Volta para o login.

**1.4 Login como Recepção e restrições**
1. Entrar com `recepcao@clinica.local` / `Admin@123`.
   - ✅ A barra lateral **NÃO** mostra **Equipe**.
2. Ir em **Triagem** → a lista aparece, mas cada linha mostra um cadeado 🔒 e há um aviso azul "Como recepção, você acompanha a fila, mas o detalhe clínico é restrito…". Clicar numa linha **não** abre o detalhe.
   - ✅ Esperado: recepção não acessa o detalhe clínico.
3. Digitar na URL `http://localhost:5173/equipe` e Enter.
   - ✅ Esperado: é redirecionada para o Painel (rota bloqueada por papel).
4. Sair.

**1.5 Login como Profissional**
1. Entrar com `profissional@clinica.local` / `Admin@123`.
   - ✅ Sem item **Equipe**; mas ao abrir **Triagem**, consegue clicar e ver o detalhe clínico (diferente da recepção).
2. Sair e voltar como **Admin** para os próximos testes.

---

## 2. Identidade visual (Épico E — dourado sobre creme)

Com qualquer tela aberta, verificar visualmente:
- ✅ Fundo geral **creme/bege** (não branco puro), cards branco quente.
- ✅ Botões primários e item de menu ativo em **gradiente dourado** (bronze → dourado), texto branco legível.
- ✅ Títulos de página (h1/h2) em fonte **serifada com versaletes** (Cormorant SC) — visivelmente diferente do texto do corpo (Inter).
- ✅ Logo/marca = monograma **"GA"** (na sidebar, no login e na anamnese pública) — **não** deve haver a florzinha "✿".
- ✅ Nenhum resíduo de rosa/vinho.

---

## 3. Painel (Dashboard)

1. Abrir **Painel**.
   - ✅ 4 cartões de métrica no topo: Fila de triagem, Agendamentos hoje, Procedimentos ativos, Atendimentos concluídos hoje.
   - ✅ Dois blocos abaixo: "Fila de triagem" (com link "Ver tudo") e "Agenda de hoje" (link "Ver agenda").
   - ✅ Botão **"+ Link de anamnese"** no topo (visível para Admin/Recepção).

---

## 4. Pacientes

**4.1 Listar e buscar**
1. Menu **Pacientes**. Digitar parte de um nome na busca.
   - ✅ A lista filtra conforme digita (busca por nome, CPF ou telefone).

**4.2 Cadastrar novo**
1. Clicar **"+ Novo paciente"** → modal abre.
2. Preencher **Nome** = `TESTE Paciente <hora>`, Telefone = `(11) 98888-0001`. Clicar **Cadastrar**.
   - ✅ Toast "Paciente cadastrado."; modal fecha; paciente aparece na lista.

**4.3 Alerta de duplicidade**
1. **+ Novo paciente** de novo. No Telefone, digitar o **mesmo** `11988880001` (ou um CPF já existente com 11 dígitos).
   - ✅ Esperado: aparece um bloco amarelo **"Possível duplicidade"** listando o paciente parecido, enquanto você digita.
2. Fechar (Cancelar).

**4.4 Detalhe e edição**
1. Clicar numa linha de paciente → abre a página de detalhe com "Dados cadastrais".
2. Clicar **Editar** → alterar o endereço → **Salvar alterações**.
   - ✅ Toast de sucesso; o dado atualiza na tela.
   - ✅ Abaixo, há a seção **Galeria de mídia** (Admin/Profissional) — ver seção 9.

---

## 5. Catálogo de procedimentos

1. Menu **Catálogo**. Clicar **"+ Novo procedimento"**.
2. Nome = `TESTE Procedimento`, Duração = `30`, Valor = `120`. **Salvar**.
   - ✅ Toast; aparece na tabela com badge **Ativo**.
3. Clicar **Editar** nele → marcar/desmarcar **"Procedimento ativo"** → Salvar.
4. Marcar o checkbox **"Mostrar inativos"** no topo.
   - ✅ Procedimentos inativos aparecem/somem conforme o filtro.

---

## 6. Link de anamnese + fluxo público + **Autosave (Épico C)**

**6.1 Gerar o link**
1. No **Painel**, clicar **"+ Link de anamnese"**.
2. (Opcional telefone.) Validade = `48`. Clicar **Gerar link**.
   - ✅ Aparece o endereço do link e botões **Copiar link / Abrir no WhatsApp / Abrir link**.
3. Clicar **Abrir link** (abre `/anamnese/<token>` em nova aba). **Copie a URL dessa aba** — vamos reusá-la no teste de autosave.

**6.2 Preencher a anamnese pública (aba nova)**
1. Na página pública: cabeçalho com marca **"GA … Ficha de anamnese"** e 2 indicadores de etapa.
2. **Etapa 1 (procedimento):** escolher um procedimento nos chips (ou "Outro…") → **Continuar**.
3. **Etapa 2 (dados + questionário):** preencher **Nome**, e alguns campos. No **Peso** e **Altura** testar validação: peso deve ficar entre 1 e 500; altura em metros (ex.: `1.70`).
   - Responder alguns Sim/Não do questionário.

**6.3 Autosave — o teste-chave**
1. **SEM enviar**, preencha metade do formulário. Observe abaixo do topo o texto **"✓ Rascunho salvo automaticamente neste aparelho"** (aparece ~1s após digitar).
2. **Feche a aba** do formulário.
3. **Abra de novo a MESMA URL** (`/anamnese/<token>` que você copiou).
   - ✅ Esperado: um **banner** "Recuperamos o preenchimento que você começou neste aparelho." e os campos (e a etapa) **voltam preenchidos**.
4. Agora conclua: marcar **"Declaro que as informações são verdadeiras"** → **Enviar anamnese**.
   - ✅ Tela "Recebemos seus dados!" com o **IMC calculado**.
5. (Confirmar limpeza) Reabrir a mesma URL novamente.
   - ✅ Esperado: como o link é de **uso único**, deve mostrar "Link indisponível" (ou, se o link ainda valesse, o rascunho **não** voltaria — foi limpo após o envio).

**6.4 A anamnese vira solicitação de triagem**
- A paciente preenchida deve aparecer na **Fila de triagem** (seção 7). Guarde o nome usado.

---

## 7. Triagem (fila e decisão) — como Admin

**7.1 Fila e filtros**
1. Menu **Triagem**. Ver a solicitação criada no passo 6.
2. Testar os chips de filtro: **Pendentes / Recebidas / Em triagem / Agendadas / Recusadas**.
   - ✅ A lista muda conforme o filtro.

**7.2 Detalhe + assumir**
1. Clicar na solicitação da paciente do teste 6 → abre o detalhe com **Dados da paciente** e a **Anamnese** preenchida (respostas Sim/Não, IMC, etc.).
2. Clicar **Assumir triagem**.
   - ✅ Toast "Solicitação assumida (em triagem)."; status muda para **Em triagem**.

**7.3 Agendar a partir da triagem**
1. Clicar **Agendar** → modal.
2. Escolher **Profissional**, **Procedimento** (pode vir pré-selecionado), **Data e hora** (escolha um horário futuro), Valor opcional. **Confirmar agendamento**.
   - ✅ Toast "Agendado com sucesso." e abre um modal **"Mensagem para a paciente"** (com botões Copiar / WhatsApp). Fechar.
   - ✅ O status da solicitação vira **Agendada**.

**7.4 Recusar (usar outra solicitação, se houver)**
1. Em outra solicitação pendente, clicar **Recusar** → escrever um motivo → **Confirmar recusa**.
   - ✅ Status vira **Recusada** e aparece o modal de mensagem à paciente.
   - Se não houver outra solicitação, gere um novo link (seção 6) e envie outra anamnese para testar a recusa.

---

## 8. Agenda

**8.1 Listar/filtrar**
1. Menu **Agenda**. Ver o agendamento criado no 7.3 (filtre a data para hoje/o dia escolhido).
2. Testar filtros: **Profissional**, **De**, **Até**.

**8.2 Novo agendamento**
1. Clicar **"+ Novo agendamento"**.
2. No campo **Paciente**, buscar por nome (mín. 2 letras) e selecionar. Escolher Profissional, Procedimento, Data/hora futura. **Agendar**.
   - ✅ Toast "Agendamento criado."; aparece na lista.

**8.3 Colisão de horário (regra de negócio)**
1. Criar **outro** agendamento para o **mesmo profissional** num **horário que se sobreponha** ao anterior.
   - ✅ Esperado: toast de **erro** informando conflito/colisão (o sistema impede sobreposição).

**8.4 Mudar status e valor**
1. Numa linha **Agendado**, clicar **Status** → escolher **Realizado** → Confirmar.
   - ✅ Badge muda para **Realizado**.
2. Testar **Cancelado**: exige **motivo** (sem motivo, dá erro).
3. Clicar **Valor** → alterar o valor cobrado → Salvar.
   - ✅ Valor atualiza na tabela.

---

## 9. Galeria de mídia do paciente (Admin/Profissional)

1. Abrir um paciente (**Pacientes → linha**). Rolar até **Galeria de mídia**.
2. Fazer **upload** de uma imagem (JPG/PNG). Escolher etiqueta (Antes/Depois/…).
   - ✅ A miniatura aparece na galeria.
3. Clicar para **baixar/abrir** a imagem.
   - ✅ A imagem abre/baixa corretamente.
4. (Permissão) Sair e entrar como **Recepção**, abrir o mesmo paciente.
   - ✅ Esperado: no lugar da galeria, aviso "A galeria de mídia é restrita a Admin e Profissional." Voltar como Admin.

---

## 10. Equipe / Usuários (Épico F — só Admin)

**10.1 Listar e criar**
1. Como **Admin**, menu **Equipe**. Ver a lista (Admin, Profissional, Recepção) com papel e status.
2. Clicar **"+ Novo usuário"**. Nome = `TESTE Recepcionista`, E-mail = `teste.recepcao@clinica.local`, Papel = **Recepção**, Senha = `Senha@123`. **Salvar**.
   - ✅ Toast "Usuário salvo."; aparece na lista como **Ativo**.

**10.2 O novo usuário loga**
1. Sair. Entrar com `teste.recepcao@clinica.local` / `Senha@123`.
   - ✅ Login funciona; papel Recepção (sem Equipe no menu). Voltar como Admin.

**10.3 Editar e desativar**
1. Em **Equipe**, **Editar** o `TESTE Recepcionista` → trocar o Papel para Profissional / ou definir **Nova senha** → Salvar.
2. Clicar **Desativar** nele.
   - ✅ Fica com badge **Inativo** (marque "Mostrar inativos" se sumir).
3. (Trava anti-lockout) Tente editar **o próprio Admin** (linha marcada "(você)"): não há botão **Desativar** para si; e se tentar mudar o próprio papel para não-Admin ou desmarcar "ativo" e Salvar → **erro** ("Você não pode alterar o próprio papel de Admin nem se desativar").
   - ✅ O sistema impede o auto-bloqueio.

**10.4 E-mail único**
1. **+ Novo usuário** com um e-mail **já existente** (ex.: `admin@clinica.local`).
   - ✅ Esperado: erro "Já existe um usuário com este e-mail."

---

## 11. Responsividade iPhone (Épico D)

Use o **modo dispositivo** do navegador (F12 → ícone de celular) e selecione **iPhone 14 Pro** (ou 390×844).

1. **Login:** deve virar **1 coluna** (o painel dourado some/reduz), card centralizado, sem rolagem horizontal.
2. **Área logada:** a barra lateral vira **menu hambúrguer** (☰) no topo.
   - Tocar no ☰ → o **drawer** desliza da esquerda com um **fundo escurecido (backdrop)** cobrindo o resto.
   - Tocar fora (no backdrop) ou navegar → o drawer fecha.
3. **Campos de formulário:** tocar num input **não deve dar zoom** automático (fonte ≥16px). Teste na anamnese pública e no cadastro de paciente.
4. **Modais** (ex.: Novo paciente): no celular aparecem como **bottom-sheet** (colados embaixo, largura total, cantos superiores arredondados, rolando por dentro).
5. **Sem overflow horizontal:** percorrer Painel, Pacientes, Agenda, Triagem — não deve haver barra de rolagem lateral nem conteúdo cortado.
6. **Notch/safe-area:** cabeçalho e conteúdo não ficam sob a "ilha"/entalhe (há respiro no topo).

---

## 12. Robustez / erros

1. **Sessão/401:** com o sistema aberto e logado, se a API cair ou o token expirar, uma ação deve redirecionar para o login automaticamente (não "tela branca").
2. **Validações:** tentar salvar formulários sem campos obrigatórios (ex.: paciente sem nome, procedimento com duração 0, recusa sem motivo) → toasts de erro claros, sem travar a tela.
3. **Link de anamnese inválido:** abrir `http://localhost:5173/anamnese/token-invalido-123`.
   - ✅ Esperado: tela "Link indisponível" (não erro genérico).

---

## Resumo do que cobrir (checklist rápido)

- [ ] 1. Login/logout + menu e rotas por papel (Admin/Profissional/Recepção)
- [ ] 2. Identidade dourada + fontes + monograma GA
- [ ] 3. Painel (métricas e blocos)
- [ ] 4. Pacientes (buscar, criar, **duplicidade**, editar, detalhe)
- [ ] 5. Catálogo (criar, editar, ativo/inativo)
- [ ] 6. Link de anamnese + fluxo público + **AUTOSAVE** (recupera rascunho; limpa após envio)
- [ ] 7. Triagem (fila, filtros, assumir, **agendar**, **recusar**, restrição da recepção)
- [ ] 8. Agenda (filtrar, criar, **colisão de horário**, status, valor)
- [ ] 9. Mídia do paciente (upload/baixar + restrição de papel)
- [ ] 10. Equipe (criar/editar/desativar, novo usuário loga, **trava anti-lockout**, e-mail único)
- [ ] 11. Responsividade iPhone (drawer, sem zoom, bottom-sheet, sem overflow)
- [ ] 12. Robustez (401, validações, link inválido)

> **Fora deste roteiro (não é testável em tela):** Épico A (deploy em VPS com Docker/HTTPS) e a decisão de **não** ter backup automático — são de infraestrutura.
