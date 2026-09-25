# Roadmap executável — R00 a R09

## Modo de trabalho

Uma mudança coesa por incremento; uma fonte autoritativa de escrita por conjunto de arquivos; testes focados após cada mudança; regressão ampla nos marcos. Subagentes podem explorar/revisar em paralelo, mas não editar simultaneamente arquivos compartilhados sem isolamento e ownership. Não criar uma estrutura nova de agentes se a existente atender.

Cada incremento exige: hipótese, caminhos reais, contrato preservado/alterado, teste antes/depois, evidência, rollback e próximo passo. Não transformar cada fase em um grande PR. Não abrir vários produtos ou refatorações independentes para ocupar tempo enquanto um gate está bloqueado.

O plano orienta a ordem; dependências concretas do checkout podem exigir divisões menores. Registrar a decisão e continuar. Complexidade técnica reversível é decisão do Codex; produção, custos, dados reais e perda funcional dependem de Gui.

## R00 — reconhecer checkout, instruções e trabalho ativo

**Entrada:** pacote e acesso ao checkout. **Objetivo:** identificar a base efetiva sem tocar no trabalho alheio.

Inspecionar raiz/remoto, instruções globais/locais/aninhadas, C7 e ACTIVE_SLICE; HEAD, branch, upstream, worktrees e alterações locais. Conferir existência e ancestralidade atuais de main, integração e Base44. Leitura remota autenticada pode ser feita pelo conector ou Git já configurado; não imprimir credenciais. Atualização de remote refs por fetch, quando permitida, não muda o working tree, mas deve ser registrada; não usar pull automático.

Localizar solution/SDK/lockfiles, package.json, suites, .NET migration scripts e ferramentas de laboratório. Descobrir escopos do ProductBoundary e o grafo de domínio/DI/UI. Não varrer segredos/arquivos operacionais para produzir um inventário indiscriminado.

**Saída:** baseline registrada, tarefa ativa reconciliada, inventário inicial de impactos, riscos e comandos existentes. **Aceite:** todos os caminhos a editar confirmados; estado local preservado; destino de testes identificado como sintético. **Rollback:** não aplicável a leitura; não houve alteração funcional.

## R01 — consolidar a base e caracterizar comportamento

**Depende:** R00. **Objetivo:** não perder evolução Base44/Codex.

Se a consolidação já estiver concluída, verificar evidências e não refazê-la. Caso contrário, preservar o incremento em andamento, montar integração em branch/worktree segura e analisar alterações por capacidade, sem sobrescrever a origem Base44. Identificar mudanças apenas locais que uma worktree nova não incluiria.

Executar testes de baseline no laboratório; separar falhas anteriores, bloqueios de ambiente e regressões. Capturar cenários sintéticos representativos de Operação→publicação→Rotinas/Consulta, escopo, marca/temas, dispositivo/atualização e fluxos restritos já implementados. Não impor quantidade de testes histórica como meta falsa.

**Saída:** baseline consolidada local candidata e matriz de decisões de integração. **Aceite:** nenhuma mudança existente descartada silenciosamente; comportamento-chave caracterizado; inconsistências conhecidas registradas; nenhuma promoção para produção. **Rollback:** retornar somente alterações próprias em branch isolada, preservando dados e branches de origem.

## R02 — direção explícita e fronteira de produto

**Depende:** R01. **Objetivo:** primeira mudança funcional pequena e reversível.

Atualizar cirurgicamente AGENTS/contratos atuais que impõem produto único e módulos exclusivos PEMSE, conservando os demais invariantes. Acrescentar ADR de supersessão e testes da nova fronteira.

Caracterizar consumidores de `ProductBoundary.FormalModules`; mover a definição de composição do produto para contrato apropriado ou adaptar a estrutura existente, mantendo aliases e defaults dos clientes/instalações atuais. Introduzir apresentação de marca desacoplada somente se não houver equivalente. Evitar DB migration nessa primeira fatia salvo necessidade demonstrada.

**Saída:** produto descrito explicitamente; comportamento padrão antigo preservado; modo neutro de laboratório iniciado sem gerar novo negócio. **Aceite:** teste de caracterização e novos negativos passam; nenhuma busca-substituição geral; testes de segurança não removidos. **Rollback:** desligar composição nova em laboratório ou reverter a mudança própria mantendo compatibilidade anterior.

## R03 — extrair o núcleo transversal

**Depende:** R02. **Objetivo:** criar fronteiras de dependência verdadeiras.

Revisar IdentityAccess, Tenancy, contratos de Audit/Configuration e composição. Extrair o menor conjunto necessário; separar permissions específicas de mecanismos genéricos. Reusar autenticação e políticas existentes. Remover dependência reversa do core para enums/DTOs socioeducativos.

Criar testes de arquitetura/dependências na ferramenta adequada existente. Evidenciar imports transitivos, project references e reexports de frontend. Se o core ainda referencia o domínio inteiro, registrar como extração parcial e continuar até eliminar a dependência.

**Saída:** núcleo neutro e testes de fronteira. **Aceite:** grafo declarado corresponde ao compilado; suite core roda sem carregar o módulo socioeducativo; mesma autorização/escopo continua nos consumidores. **Rollback:** adapters compatíveis e alteração incremental dos consumidores, sem duplicar o serviço de identidade.

## R04 — separar capacidades comuns opcionais

**Depende:** R03; pode ser fracionada por capacidade. **Objetivo:** retirar do core o que não é universal.

Classificar Scheduling, publicação, Devices/Player/Consulta/Edge, conhecimento/relatórios/release conforme o consumo real. Não extrair tudo indiscriminadamente: iniciar pelas dependências exigidas pelo produto socioeducativo e prova neutra. Preservar uma autoridade de agenda/publicação e os esquemas/versionamento existentes.

Compor DI, endpoints, jobs, subscriptions, assets e navegação por módulo. Módulo ausente não registra sua superfície. Não introduzir carga dinâmica de código. Revisar dependências opcionais em bootstrap, service worker e geração do OpenAPI.

**Saída:** capacidades opcionais com owners e contratos. **Aceite:** sem módulo, não há handler/job/vazamento; com módulo, o comportamento anterior é preservado; ciclo de dependência é rejeitado. **Rollback:** retornar a composição anterior no ambiente sintético sem apagar histórico/configuração.

## R05 — recompor o produto socioeducativo

**Depende:** R03/R04 suficientes. **Objetivo:** produto especializado, não fork por instituição.

Mover regras/DTOs/políticas socioeducativas para owner específico; manter occupation/planning/movement/restricted/spatial semantics conforme implementadas. Compor Operação, Rotinas e Consulta com capacidades compartilhadas. Separar textos de cliente e configuração da semântica real do domínio.

Executar o fluxo mínimo e casos restritos impactados para duas organizações fictícias. Preservar diferenças entre plano/estado confirmado e aprovações/execução; gates pendentes permanecem. Reservar ClubAL como futura composição, sem ativar uma segunda oferta.

**Saída:** produto recomposto e rastreabilidade de capacidades preservadas. **Aceite:** organização nova por configuração suportada, sem branch; nenhuma dependência core→socio; regressão do fluxo e negativos de acesso passam. **Rollback:** adapters legados e revisão anterior do host, sem regressão dos dados.

## R06 — persistência e compatibilidade final da separação

**Depende:** inventário R00; começa antes se R03 exigir, fecha após R05. **Objetivo:** provar upgrade/recuperação.

Inspecionar migrator/DbContexts/histórico; executar estratégia definida em ADR. Preservar IDs, cookies, paths, tokens, caches e contratos externos. Gerar SQL revisável para qualquer alteração. Não fazer drop/recreate, reset de migrations, dual-write ou reassignment universal de produto.

Validar bootstrap novo, upgrade sintético, repetição idempotente, falha controlada e restauração. Exercitar clientes/versionamentos legados reais do laboratório. Classificar cobertura de N/N-1 por pares concretos, não por slogan.

**Saída:** plano de compatibilidade, scripts/artefatos locais e evidências. **Aceite:** perda/colisão de dados ausente nos testes; identidade e auditoria preservadas; recuperação verificada. **Rollback:** retorno de código compatível e/ou procedimento de recuperação testado; nunca Down automático destrutivo.

## R07 — experiência neutra e linguagem de produto

**Depende:** R02–R06 no escopo afetado. **Objetivo:** separar identidade sem perder usabilidade.

Centralizar marca/nome exibido e preferências; remover menus do nicho da fundação; preservar fluxo útil neutro e UX especializada do socioeducativo. Reaproveitar temas, componentes e melhorias Base44. Corrigir strings de cliente hardcoded sem alterar IDs ou mensagens institucionais legítimas.

Testar teclado, foco, rascunhos, erros, conteúdo longo e reflow, com capturas sintéticas. Revisar o que ainda depende de validação humana ou dispositivo físico. Não publicar sites/domínios novos nem aprovar naming.

**Saída:** duas composições visualmente coerentes. **Aceite:** marca não altera autoridade; perfil de cliente/pessoa preservado; sem termos do nicho na UI neutra; sem elementos “em breve” de ClubAL futuro. **Rollback:** provider/configuração de apresentação anterior sem perder preferências.

## R08 — prova de reutilização e artefato limpo

**Depende:** R03–R07. **Objetivo:** provar que existe fundação, não somente organização de pastas.

Compilar e executar a composição neutra sem referências transitivas ao módulo socioeducativo. Gerar inventário do artefato e lista estrita de exceções de compatibilidade. Provar que módulo ausente não aparece em endpoints/jobs/seed/assets/bundle.

Criar uma extensão fictícia mínima exclusiva de teste que consuma os mesmos contratos sem copiar core. Atualizar um componente neutro e comprovar integração com os consumidores. Essa extensão não entra em produção nem vira o próximo SaaS.

Executar preparação do artefato reutilizável com versão, dependências, bootstrap, configuração sintética, testes e documentação. Sem publicação em registry, GitHub template ou repositório remoto novo.

**Saída:** fundação v1 candidata e receita de consumo local. **Aceite:** construir/rodar sem vertical é evidência real; adicionar/remover extensão sintética não exige editar lógica interna de core; sem arquivos privados/operacionais no artefato. **Rollback:** preservar versão anterior dos componentes e mapa de dependências.

## R09 — regressão integrada e handoff

**Depende:** R00–R08 concluídas ou exceções legítimas explicitamente abertas. **Objetivo:** entregar sem certeza inventada.

Rodar suites pertinentes com banco/browser/API reais do laboratório, testes negativos, distribuição/legado e validações documentais. Revisar diffs, segredos, dependências, documentação e compatibilidade. Diferenciar mocks, navegador com fixture, API real e hardware físico.

Preencher matriz de requisitos, release readiness e pendências. Preparar proposta de integração/PR sem fazer push, merge ou deploy sem autorização. Atualizar continuidade C7 e ponteiro portátil quando material. Relatar limitações que impedem chamar o core de limpo/pronto.

**Saída:** relatório final, componentes versionados localmente, procedimento de reprodução e lista curta de validação humana. **Aceite:** nenhum requisito crítico sem evidência; nenhum gate externo aprovado por inferência; trabalho e estado preservados. **Próxima prioridade de negócio:** validar o produto socioeducativo, não iniciar outro nicho automaticamente.

## Pausas e limites de autonomia

Pausar somente o ramo dependente quando houver: risco de perda de dados, segredo identificado, conflito de autoria, custo externo, falta de aprovação, ambiente real, workspace errado ou decisão de negócio não resolvível. Progredir em inspeção/testes/documentação independentes. Um bloqueio não deve virar desculpa para reescrever a arquitetura sem prova.

A falta de tempo/contexto exige checkpoint recuperável com progresso real, não claim de conclusão. Retomar pelo [prompt de continuidade](prompts/RETOMAR.md).
