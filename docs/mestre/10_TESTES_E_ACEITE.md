# Testes, QA e critérios de aceite

Há três conjuntos distintos: integridade documental (verificada pelas [ferramentas](ferramentas/README.md), com resultado próprio por execução); especificação de testes do futuro Wissian (NOT_RUN); evidências históricas do ClubAL (relatadas por suas fontes, sem repetição nesta tarefa). Nunca somar essas categorias como cobertura de produto.

## Rastreabilidade utilizável

[requisitos-mestre.json](matrizes/requisitos-mestre.json) liga obrigação, marco e casos de teste. [testes-planejados.json](matrizes/testes-planejados.json) descreve fixture/ação/oráculo/negativo/tipo. [cobertura-origem.json](matrizes/cobertura-origem.json) mantém todos os REQ01–32, WB-01–16 e NF-01–10 sem apagar critérios. Os 60 testes originais permanecem intactos; o mapa diz quando são reaproveitados, adaptados ou adiados, sem executá-los automaticamente.

## Gates por evidência

| Gate mestre | Prova mínima | Não basta |
|---|---|---|
| GM0 — procedência | Fonte/ref/hash, estado de leitura, ficha, autorização e decisão de stack/primeiro slice | Lista de nomes de branches |
| GM1 — fundação | Grafo real de build, inicialização neutra, extensão sintética, composição inválida rejeitada | JSON de exemplo válido ou menus escondidos |
| GM2 — identidade/isolamento | Mesmo release, dois tenants, papéis distintos; API/DB/anexos/cache/jobs/busca/exportação com negativos | Mock de repositório ou filtro de frontend |
| GM3 — domínio | NF-01–10 e invariantes de dinheiro/estados/replay/concorrência; importação por item | Tela mostrando números sem persistência |
| GM4 — experiência | Fluxo principal, vazio/erro/espera/recuperação, teclado, leitor de tela, zoom, tema | Screenshot estático ou ferramenta automática isolada |
| GM5 — recuperação | Bootstrap/upgrade, falha controlada e restore sintético medido; pares de versão definidos | Existir arquivo de backup |
| GM6 — piloto/externo | Autorização aplicável, operação e dados sob controle, responsáveis e contingência | PASS de build, documentação ou aceite de outra aplicação |

Os gates G0–G7 originais seguem no acervo com suas definições; GM0–GM6 não os renomeiam retroativamente. Cada ficha traz a equivalência necessária para a capacidade.

## Casos indispensáveis

1. Organização A cria registro e arquivo; B tenta leitura/escrita/exportação/download por ID conhecido, pesquisa e job: nenhuma informação cruzada.
2. Usuário autorizado perde associação ou permissão com aba aberta e job pendente: próxima ação falha segura; dados em cache respeitam escopo/revogação.
3. Módulo contratado porém desativado, ou ativado sem permissão, ou dependência ausente: operação negada sem efeitos parciais. Reativação não amplia acesso nem duplica seeds/jobs.
4. Composição neutra construída sem vertical: não registra endpoint, hub, job, asset, seed ou menu do domínio ausente. Mesmo artefato multitenant não exige rebuild para toggle seguro entre organizações.
5. Nota sintética de 1.000 com duas parcelas de 500; baixa de 300 deixa saldos 200 e 700; estorno restaura 500/1.000 sem apagar baixa. Cancelamento fiscal não apaga caixa. Fonte: NF-01–04.
6. Repetição de importação, timeout de resposta e duas baixas concorrentes: uma operação lógica, conflito explícito, valor exato e evidência. NF-05/09/10 e casos mestre.
7. Importação com linha inválida, fórmula ou XML malicioso: rejeição segura por item, diagnóstico, nenhuma rede inesperada ou execução. Importação válida não prova emissão oficial.
8. Pacote mensal com pendência e geração interrompida: versão anterior preservada, itens faltantes visíveis, retry idempotente e nenhum “enviado” automático.
9. Restore sintético e reversão de versão: dados, identidade, isolamento e compatibilidade mantidos; downgrade incompatível bloqueado com caminho de recuperação.
10. UI de Base44 e Codex comparadas no mesmo roteiro: mudanças de alojamentos, marca, navegação e dados têm decisão explícita e não desaparecem por conveniência de merge.

## Execução proporcional

Para a substituição de controles locais, aplicar o [recorte e ensaio comparativo do capítulo 17](17_BASE_ADMINISTRATIVA_E_TRANSICAO.md). TM-11/12/13 incluem as duas organizações sintéticas, portabilidade, comparação de tarefas e falha de conexão. Medir o resultado; não declarar superioridade de uso ou segurança apenas pela aparência ou pelo acesso online. Esta revisão de critérios mantém todos os casos de aplicação em NOT_RUN.

Após escolher stack, materializar comandos reais no repositório: teste unitário de regra; integração com persistência/roles; contrato/API; composição/build; E2E dos fluxos; negativos de acesso/importação; acessibilidade manual e automática; smoke da distribuição relevante. Não criar scripts fictícios nem supor que nomes de comandos do ClubAL valem para Wissian. Mocks servem a regras locais; não satisfazem isolamento, transação, autenticação ou dispositivo.

Registrar versão/commit, ambiente, comando, exit code, data, fixture sintética, requisito, resultado, evidência sanitizada e limitação. Estados de execução: NOT_RUN, PASS, FAIL, BLOCKED, SKIPPED. `PASS` exige execução e oráculo satisfeitos. Aceite humano é campo separado e não é inferido de testes.

Desempenho deve ter baseline e orçamento definidos por fluxo/volume antes do gate pertinente. Medir percentis, carga, tamanho de arquivo, concorrência, uso de banco e custo com amostra documentada; números inventados não constituem SLA. Em host limitado, preflight, execução serial e checkpoints. Não repetir testes válidos sem alteração relevante.

## Definição de pronto por fatia

Requisitos vinculados; comportamento demonstrável; positivos/negativos pertinentes aprovados; risco residual explícito; observabilidade/recuperação proporcionais; diff revisado; fontes licenciadas/procedência; instruções reais de uso; documentação de contrato atualizada; pendências humanas preservadas. Produto comercial/piloto só se torna pronto após seus gates próprios. Para E0–E3, M4 concluído demonstra primeiro ciclo administrativo, não cobertura de WB-01–16 inteira.

## Aceite adicional de continuidade operacional

WM-25…WM-40 e TC-01…TC-22 estendem, sem substituir, requisitos/casos existentes. GM1/GM2/GM4/GM5 aplicam-se também a M5-CO. Produto só passa com trio integrado, consumidor real, persistência, duas organizações, decisão humana sobre revisão atual, deterministicidade e recuperação. Não exigir ResourceHealth de dispositivo antes de existir o módulo consumidor.

Casos essenciais: duplicação/reordenação/recorrência; mesma chave com conteúdo conflitante; reabertura e confirmação obsoleta; merge preservando notas humanas; disputa de versão; ciclo A→B/B→A concorrente; reconhecimentos repetidos; papel revogado; escopos cruzados em todos os canais; handler/parametro hostil; timeout após efeito; replay/DLQ/restore; módulo ausente/suspenso; ausência de aprovador; queue sem responsável; ruído e acessibilidade. Todos estão NOT_RUN até execução de aplicação. Validar JSON de exemplo não prova idempotência/transação SaaS.

Teste de pacote adicional verifica vínculos e invariantes documentais, incluindo ausência de Continuity nas composições neutra/primeiro ciclo e nas dependências do Core. Os resultados documentais ficam na evidência da execução, separados dos testes de aplicação.

O reforço de reuso e preservação de todas as ideias acrescenta WM-39/40 e TC-21/22: totais vigentes de 40 requisitos mestre e 42 testes planejados. Todos os casos de aplicação continuam NOT_RUN.

WM-40/TC-22 acrescentam a cobertura integral de decisões/usos Nexus/CC: adiado não vira descartado, mecanismo aposentado não retorna e regra de especialidade não migra ao Core. O validador checa rastreabilidade documental; os testes do produto permanecem NOT_RUN.
