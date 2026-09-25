# Roadmap integrado e marcos verificáveis

Este documento define a ordem dos marcos do produto. **Nenhuma etapa de aplicação está concluída pela preparação documental.** Datas de entrega, equipe e orçamento dependem de medições; a decisão de stack e a caracterização inicial estão registradas no adendo M0 abaixo.

**Atualização de 25/09/2026:** M0 da primeira fatia foi caracterizado, com stack decidida
na [ADR-M03](adrs/ADR-M03_STACK_E_REUSO.md),
[procedência e 16 casos de domínio](evidencias/m0-primeira-fatia.json) e
[contrato de saída M1](contratos/M1_FUNDACAO.md). Os demais marcos e testes de produto
permanecem não executados. Esse recorte não equivale a auditoria de todo o legado.

As fontes usam nomes que colidem: R00–R09 pertencem à reestruturação v1.1; E0–E7 ao plano da operação; “Marco M1/M2” naquele plano significa primeiro ciclo/cobertura completa. Aqui, M0–M8 são marcos mestre. Referir sempre o namespace: `mestre:M1` ≠ `operacao:Marco-M1`.

## Sequência

| Marco | Resultado observável e fatias | Dependência | Evidência de saída | Fontes preservadas |
|---|---|---|---|---|
| M0 — reconhecer e decidir | Estado Git real, fichas por capacidade, comparação Base44/Codex, ADR de stack e primeiro contrato executável | Pedido de implementação vigente; código de origem acessível em leitura | Matriz de procedência completa para a primeira fatia; lacunas bloqueantes resolvidas; critérios e rollback definidos | R00–R02; E0; ADR local; TEM/OWM/GRA |
| M1 — fundação demonstrável | Bootstrap pequeno, organização/associação, autorização, composição e auditoria mínima; mesma versão serve dois tenants sintéticos | M0 | Core compila sem vertical; extensão sintética sem copiar Core; testes de isolamento, revogação e configuração inválida; inicialização documentada | R03/R04/R08; E1 parcial |
| M2 — entrar, organizar e localizar | Documentos, classificação, versões, contraparte, busca e vínculos, como módulos opcionais | M1 | Dois tenants cumprem E1; URL direta indevida negada; upload e download seguros; arquivamento preserva referências | E1; WB-12/13/15/16; R07 |
| M3 — controlar notas e parcelas | Registro fiscal, parcelas, baixas/estornos e estado de conferência independentes; sem emissão oficial | M2 | NF-01–07/NF-09 e negativos de importação/concorrência; valores exatos e idempotência | E2; WB-04/05 |
| M4 — fechar o ciclo administrativo | Conciliação, transferências internas, competência/caixa, pacote mensal versionado e recuperável | M3 | NF-01–10; diferenças explícitas; falha/retomada; UX de fechamento demonstrada; restauração sintética | E3; operacao:Marco-M1 |
| M5 — expansão da operação base | M5a contratos/obrigações/agenda; M5b governança/licenças/PI/marca; M5c pessoas/serviços/privacidade/importação revisada | M4 e prioridade por fluxo | WB-01–16 demonstrados ou exceção explicitamente aprovada; segundo tenant sem regras Fultuma; revogação e retenção verificadas | E4–E6; operacao:Marco-M2 |
| M6 — verticais e reuso comprovado | Escolher uma vertical por demanda; separar regras socioeducativas/educação/ClubAL-Telas; absorção por ficha e contratos | M1 e fronteiras estáveis; M4 é prioridade padrão, não dependência técnica de todas as verticais | Aplicação neutra permanece neutra; paridade da capacidade escolhida; compatibilidade/dispositivo apenas quando executados | R05/R06/R08; OWM/TEM/GRA/QR/PCE conforme escopo |
| M7 — operação e integrações autorizadas | Piloto controlado, operação observável, suporte, recuperação; depois integração externa necessária | Gate de dados reais e autorização específica; primeira fatia útil validada | RPO/RTO medidos; plano de incidentes e rollback ensaiado; custo/capacidade medidos; integrações testadas em ambiente autorizado | E7; R09; GRA/recovery |
| M8 — ampliação por resultado | Frota, estoque/produtos/QR, RH/ponto, jurídico/fiscal e outros módulos que tiverem problema e público definidos | Fluxo, dono, valor, dependências e custo explicitados | Contrato, teste e validação reais por módulo; seleção não infla o Core | Pedido atual e catálogo do guia |

M7 pode receber trabalhos de operação/observabilidade desde M1, sem antecipar piloto, dados reais ou serviços externos. M5/M6/M8 são trilhas priorizáveis, não obrigação de terminar todo catálogo para entregar M4. Uma vertical não depende de financeiro só por este aparecer antes no cronograma.

**Recorte confirmado em 24/09/2026:** a [base administrativa do capítulo 17](17_BASE_ADMINISTRATIVA_E_TRANSICAO.md) é a prioridade para Fultuma e demais organizações. Fechar M2–M4 com financeiro, registros fiscais e pacote contábil; comparar utilidade com um controle manual equivalente e provar isolamento, exportação e recuperação. M5a/b/c, marca, contratos e outras áreas corporativas não são condições para essa entrega nem autorização para gerar módulos por correspondência com pastas. Uso real aguarda também os controles de piloto M7.

## Etapas antigas sem perda de intenção

R00/R01 viram M0; R02 é a precedência/ADRs e futuro contrato de produto; R03/R04 viram M1 e cada extração; R05 é M6, sem socioeducativo obrigatório; R06 acompanha qualquer migration com schema novo separado da origem; R07 acompanha M1–M6; R08 é M1 e prova de cada composição; R09 acompanha cada incremento e M7. A proibição antiga de novo repositório foi superada pelo mandato atual, não o cuidado com preservação.

O estado local das frentes não representa aceite no Wissian:

| Frente | Evidência lida na origem | Tratamento no destino |
|---|---|---|
| TEM | Documentos reportam TEM-0–3 verificados localmente; TEM-4 depende de validação humana/dispositivo; TEM-5 adiado | Preservar temas/preferências e critérios, caracterizar implementação; não declarar aceite transferido |
| OWM | Programa registra pacote 12 em preparação, NOT_ACTIVATED; último 11E; 13 não aberto; 6H sem ativação | Extrair mecanismos úteis; manter portões antigos na origem; nenhum passo deste pacote ativa a migração ClubAL |
| GRA | Documento reporta evolução 0–7 local, incluindo recuperação; default-off; piloto/hardware/produção pendentes | Reaproveitar governança, rollback, evidência e suporte com novo contrato; não habilitar atualização remota automática |
| QR/Telas | Requisitos e marcos locais inventariados; capacidade física não foi testada aqui | QR administrativo de inventário e QR de pareamento são domínios distintos; evitar reuso semântico indevido |
| PCE | Roadmap pós-OWM preservado; fases citadas como adiadas na origem | Backlog condicionado, sem converter ideia em realização |

Os [114 requisitos extraídos](matrizes/requisitos-frentes-origem.json), os [35 pacotes OWM](matrizes/roadmap-owm-origem.json), os [ADRs e fontes do legado](matrizes/fontes-legado.json) e o [roadmap R00–R09 integral](../fontes/reestruturacao-v1.1/12_ROADMAP_EXECUTAVEL.md) conservam detalhes. Extração estrutural não substitui leitura completa do arquivo de origem quando a capacidade entrar no escopo.

## Cadência e encerramento

Cada fatia percorre contrato → caracterização → implementação → negativos/regressão → fluxo completo → revisão → continuidade. Ajustar tamanho ao host, custo e risco. Se houver gate humano/dispositivo, concluir as verificações independentes e registrar o ponto exato pendente; não trocar pendência por PASS. Encerrar quando critérios da fatia estão cumpridos e o próximo passo seguro autorizado terminou, sem abrir módulos adicionais por disponibilidade de tempo.

## Adendo 2.1.0 — fundações agora e MVP M5-CO depois

A sequência M0–M8 e M5a/b/c acima permanece íntegra. Acrescenta-se **M5-CO — continuidade operacional**, subtrilha opcional após o primeiro ciclo M4, podendo ser priorizada junto da expansão M5 sem exigir concluí-la toda.

| Fatia | Dependência / entrega | Gate de saída |
|---|---|---|
| M0/M1 — preparar | Contrato de escopo/autoridade, auditoria, versões/conflitos, evento mínimo e idempotência; fixture transversal | TC-01 e regras M1 existentes; Core e primeiro fluxo sem Continuity; sem schema WorkItem/UI/broker |
| M2–M4 — manter prioridade | Idempotência e revisão pertencem aos módulos reais; fechar E0–E3 | Gates originais, NF-01…10; aprovação/revisão nativa sem capability |
| M5-CO.0 — contrato do consumidor | M4 verificado; revisão do pacote mensal, política humana, retenção/limites e fonte de eventos | Fonte/owner/versões definidos; ADR de dados conforme stack; casos TC delimitados |
| M5-CO.1 — Agora + Verificações | M5-CO.0; WorkItem, stable keys, fonte versionada, inbox/outbox e ActionCatalog mínimo | Um evento repetido gera uma condição; humano decide revisão atual; origem confirma resultado |
| M5-CO.2 — Passagens integradas | M5-CO.1; categorias, destinatário, reconhecimento e feedback/reabertura | Recebimento não aprova; duas pessoas/papéis retomam contexto no mesmo fluxo persistente |
| M5-CO.3 — concluir MVP | M5-CO.1/2; isolamento, ruído, concorrência, desativação, recovery e acessibilidade | TC-02…22 executados no escopo e aceite humano distinto; nenhum gate crítico pendente |
| Extensão posterior M5a/M6/M7 | Agenda/obrigações ou devices/displays realmente demandados | Novo adapter tipado e seus testes; DeviceOfflineDetected é exemplo M6, não exigência de bootstrap |

M5-CO é identificador de trilha, não substituição de M5c (pessoas/serviços/privacidade). Sem consumidor M4 pronto, adiar M5-CO, mantendo demais frentes. Recursos deliberadamente futuros: monitor de dispositivos completo, offline/ContinuityEnvelope, importação externa, regras configuráveis, múltiplos aprovadores e canais externos. Nenhum desses itens bloqueia o trio mínimo. [Contrato completo](16_CONTINUIDADE_OPERACIONAL.md) e [prompt](prompts/06_IMPLEMENTAR_CONTINUITY.md).

## Prioridade de reuso existente

Aplicar o [contrato canônico de fragmentação e reuso](04_ABSORCAO_CODEX_BASE44.md): localizar equivalentes antes de criar, preservar origens e justificar lacunas. WM-39/TC-21 mantêm a rastreabilidade.

## Cobertura posterior que continua preservada

M5-CO é a grafia inequívoca da trilha de continuidade; M5c original permanece pessoas/serviços/privacidade. Seu primeiro consumidor administrativo não elimina os demais exemplos. [Matriz Nexus/CC](matrizes/reaproveitamento-nexus-cc.json): revisão de agenda/documentos/obrigações em M5a; passagem de plantão, comunicado, inspeção e saúde de recursos em M6/M8 conforme a especialidade; móvel/offline/edge e envelopes de transferência, integrações, migração e recuperação em M6/M7 somente com consumidor e contratos comprovados. Rules mínimas pertencem à capacidade transversal, políticas à origem; editor/motor universal continua fora do MVP. A cobertura inclui informação futura “Depois”, Pendências, Recursos e Histórico, sem impor seis telas ou produtos novos.
