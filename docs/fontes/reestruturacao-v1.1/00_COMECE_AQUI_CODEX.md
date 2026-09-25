# Codex — iniciar a reestruturação ClubAL → fundação modular Wissian

**Pacote:** v1.1.0, 22/09/2026. **Projeto de destino:** Wissian, em sua pasta dedicada. **Fonte legada observada:** `gmdr2022/clubal`, privado.
**Natureza:** plano de implementação incremental; executar somente quando Gui solicitar sua aplicação. A entrega ou leitura deste pacote para revisão não inicia o desenvolvimento.
**Identidade:** Wissian é o nome escolhido para a plataforma-base da Fultuma, cuja empresa proprietária definida por Gui é Fultuma Tech Ltda. `Wissian Platform` pode identificar a documentação técnica; `core` descreve o núcleo, sem criar outra marca. Este pacote não comprova registro de marca, domínio, constituição da empresa ou formalização da titularidade.

## Missão e resultado

Reestruture o software existente para extrair uma fundação tecnológica privada, neutra e reutilizável; manter capacidades comuns opcionais; compor um primeiro produto socioeducativo independente de um cliente específico; e reservar ClubAL como futuro produto para clubes. Trabalhe sobre a evolução real do projeto, preservando código, comportamento, integrações, segurança, histórico e dados. Não recomece do zero, não replique o repositório por cliente e não transforme a fundação em outro SaaS genérico que precise ser vendido.

Esta revisão prepara o plano para a identidade Wissian e para sua pasta dedicada. Quando Gui solicitar a execução da reestruturação, resolva decisões técnicas reversíveis com evidências e prossiga pelos incrementos elegíveis, dentro da autorização da tarefa atual. Nesse estágio, implemente o primeiro incremento seguro e continue enquanto não houver um bloqueio material. Pedidos limitados a leitura, organização ou atualização documental permanecem nesse escopo. Não prometa execução assíncrona ou conclusão sem evidência.

## Primeira leitura e precedência

1. Identifique separadamente a pasta dedicada do Wissian e o checkout legado do ClubAL que contém a evolução a reaproveitar. Leia o guia fundador e a governança existentes no destino, as instruções aplicáveis e os `AGENTS.md` dos escopos que tocar. Confirme as raízes Git, branches e remotos existentes; a pasta Wissian não precisa já possuir o remoto do ClubAL. A referência `gmdr2022/clubal` identifica a fonte legada, não uma ordem para renomear, substituir ou vincular o destino. Se a origem não estiver acessível, limite-se à preparação documental. Antes de editar código, confirme baseline, destino e autorização da tarefa atual, preservando o trabalho nas duas pastas.
2. Leia [decisões e limites](02_DECISOES_E_LIMITES.md), [estado verificado](03_ESTADO_VERIFICADO_REPOSITORIO.md), [arquitetura](04_ARQUITETURA_ALVO.md), [roadmap](12_ROADMAP_EXECUTAVEL.md), [testes e aceite](13_TESTES_E_ACEITE.md) e [continuidade](17_CONTINUIDADE_E_RELATORIOS.md). Consulte os demais arquivos por escopo, sem recarregar todo o pacote a cada interação.
3. Consulte C7/Obsidian pelo protocolo local já configurado e o ponteiro `ops/runtime/ACTIVE_SLICE.md`. Não crie outro cofre nem substitua a continuidade. Se indisponível, use o fallback descrito neste pacote e registre a limitação.
4. Esta direção substitui somente as antigas decisões de produto único ClubAL e de exatamente três módulos formais exclusivos PEMSE. Não substitui autorização, isolamento, privacidade, operação offline de Rotinas, read-only de Consulta, distribuição, gates humanos ou políticas superiores do ambiente. Registre a supersessão de forma localizada nos documentos e testes pertinentes.

## Preservação e consolidação obrigatórias

Na observação remota de 22/09/2026: `main` = `8ac5f093092ba4afef4bea813562a9c985dd96b3`; `integration/base44-main` = `75f1fc34624dfb39beef57475d4e619303dcfcee`; `base44/setup-32dcb42e` = `46370e599c021c52ee9734249b438f4a664ab33b`. A integração estava 27 commits à frente da main, sem commits exclusivos da main nessa comparação. Esses SHAs são referência histórica, NÃO alvos para reset nem evidência de que o checkout local esteja atualizado.

Inspecione branch, HEAD, upstream, worktrees, alterações staged/unstaged/untracked, tarefas ativas e divergências atuais. Não use `git pull`, `git stash`, reset ou troca de branch como limpeza automática. Preserve as alterações existentes; patches não incluem arquivos untracked e não constituem backup completo. Confira conteúdos sensíveis antes de qualquer cópia/commit.

Se o Codex estiver executando outra tarefa, conclua seu incremento em andamento até um checkpoint seguro antes de alterar os mesmos arquivos. Não inicie indefinidamente todo o roadmap antigo. Reconcile os próximos passos com esta nova prioridade. Se necessário, trabalhe em worktree isolada somente após identificar a baseline que contém o trabalho relevante; uma worktree criada do remoto não contém alterações locais não commitadas.

Reconcile main, trabalho local e integração Base44 por capacidade. Não faça merge cego, não sobrescreva a integração, não replique de novo mudanças já absorvidas e não altere a branch de origem do Base44. Use as decisões ADOPT / ADAPT / KEEP / DEFER / REJECT com justificativa e testes. Não escolha main apenas por ser a branch padrão.

## Autorizações deste trabalho

Permitido: inspeção; documentação contratual; ajustes localizados de AGENTS; branch/worktree local quando segura; implementação incremental; artefatos e testes em ambientes exclusivamente sintéticos; correções necessárias à reestruturação; relatório real de resultados. Commits locais somente conforme as instruções locais e sem incluir trabalho alheio. Nada neste pacote autoriza commit automático na main.

Não autorizado: push/merge na main; publicação ou deploy; alteração de visibilidade; criação de repositório remoto; contratação de serviços ou registro de domínio; ações em dados/usuários reais; migração em banco compartilhado ou produção; exclusões destrutivas; rebase/force-push; alteração de credenciais, trust anchors ou aprovações; exposição de portas à internet. Para estas ações, prepare o procedimento e registre `BLOCKED_APPROVAL`, sem executá-las.

## Execução prática

- Execute R00 e R01 do roadmap. Produza diagnóstico curto com baseline, trabalho preservado, comandos existentes e conflitos de diretriz. Falha de acesso ao remoto não autoriza reconstruir a partir de snippets: avance somente na inspeção/local que puder verificar.
- Selecione um incremento coeso da R02: teste de caracterização, descriptor de produto/marca opt-in reutilizando contratos existentes e/ou separação de `ProductBoundary`, sem renomear identificadores persistidos. A escolha exata deve seguir o grafo encontrado, não uma implementação preconcebida.
- Faça uma mudança funcional pequena, rode testes focados e adjacentes, registre comandos/exit codes/resultados e inspecione o diff. Uma correção de documentação sozinha não satisfaz o início da implementação quando há código seguro elegível.
- Siga R03–R09 em incrementos revisáveis, nunca uma grande troca de diretórios sem testes. Não introduza dependências, microserviços, um novo framework, um marketplace de plugins ou um provedor de cobrança por conveniência.
- Preserve as superfícies Operação, Rotinas e Consulta como experiências do produto socioeducativo. Elas deixam de definir a fundação inteira. Não remova capacidades existentes só por não integrarem o primeiro recorte comercial.
- A fundação limpa deve ter prova de compilação/dependências e execução sem carregar o domínio socioeducativo. Apenas esconder menus ou mover arquivos não satisfaz esse requisito.

## Evidências e relatório

Use `PASS`, `FAIL`, `NOT_RUN`, `BLOCKED_ENV`, `BLOCKED_APPROVAL` ou `NOT_APPLICABLE` com motivo. Relatos históricos de testes e testes com mocks não são provas de funcionamento integrado atual. Não instale/atualize ferramentas globalmente para contornar um bloqueio; use o bootstrap aprovado e versões do checkout.

Ao encerrar um incremento, reporte: baseline/branch/HEAD; mudanças e caminhos; comportamento preservado; comandos executados e saídas resumidas; riscos; rollback; pendências reais; próximo incremento. Atualize a continuidade existente. O formato completo está em [continuidade e relatórios](17_CONTINUIDADE_E_RELATORIOS.md).

**Comece pela inspeção e avance até uma implementação segura verificável. Só interrompa o ramo afetado quando faltar autorização, acesso, ambiente ou informação indispensável; prossiga no restante que seja independente e seguro.**
