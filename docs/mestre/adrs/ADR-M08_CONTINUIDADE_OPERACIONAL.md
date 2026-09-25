# ADR-M08 — Continuidade operacional opcional e herança Nexus/CC

**Status:** DECIDIDO_PELO_PEDIDO para direção de produto/fronteiras; CONTRATOS_PROPOSTOS para implementação. **Data:** 24/09/2026. Complementa ADR-M02/M05/M06/M07 sem substituí-las.

## Contexto e decisão

Gui encerra a direção de NexusDevHub e codex-checkpoint como produtos independentes e aprova aproveitar seus princípios em uma capability opcional. Implementar futuramente `continuity`, nome técnico provisório `Wissian.Continuity`, pelo mesmo catálogo do Wissian, com Agora, Passagens e Verificações. Preservar origem, stable keys, fila, dependências, feedback, histórico, autoridade humana, ações tipadas e saúde contextual. Não portar WPF/.NET Framework, shell arbitrário ou integração Codex para o SaaS. Não alterar os repositórios ancestrais nesta tarefa.

Contrato normativo detalhado: [capítulo 16](../16_CONTINUIDADE_OPERACIONAL.md). Preparar escopo, autorização, auditoria, eventos mínimos/versionamento e idempotência em M0/M1. Manter M2–M4/E0–E3. MVP em M5-CO após M4, começando pela revisão/passagem de pacote mensal. Outbox/inbox surgem com a integração durável real, não como infraestrutura especulativa no bootstrap.

## Alternativas e trade-off

| Opção | Avaliação com os mesmos critérios: valor, acoplamento, custo e recuperação |
|---|---|
| Só manter código antigo arquivado | Preserva patrimônio, mas não orienta sua utilidade no Wissian; insuficiente ao pedido |
| Portar os dois aplicativos | Carrega dependências e autoridade de desktop, inadequadas ao SaaS; viola restrições eliminatórias |
| Colocar toda coordenação no Core desde M1 | Antecipação sem consumidor e aumento de acoplamento; posterga primeiro valor administrativo |
| Coordenação separada por cada vertical | Pode servir um caso isolado; repete mecanismos de fila/decisão e fragmenta experiência quando houver equivalência |
| Capability opcional com um consumidor inicial | Recomendada: preserva princípios, prova valor e permite desligar integração mantendo o fluxo nativo |

Aceita-se custo de contrato/integração e consistência eventual observável; rejeita-se prometer entrega exactly-once ou permitir aprovação silenciosa para esconder atraso. Não escolher framework ou broker nesta ADR.

## Premissas, risco e evidência

Premissa decisiva: M4 terá revisão de pacote mensal com identidade/versionamento e uma operação de decisão pública reutilizável. Confiança alta na compatibilidade documental; utilidade/volume e reuso específico de código continuam não validados. Riscos residuais: gargalo humano, ruído, projeção atrasada e esforço de integração. Pré-mortem no [capítulo 13](../13_RISCOS_E_DECISOES_ABERTAS.md), requisitos WM-25…WM-40 e testes TC-01…TC-22. Testes de aplicação NOT_RUN.

## Recuperação e reavaliação

Suspender integração e manter revisão nativa com os mesmos gates; preservar histórico/schema e reconciliar recibos. M5-CO.0 reavalia se consumidor não existe, semântica de aprovação conflita, custo excede orçamento medido ou usuário não consegue indicar motivo/ação em Agora. Falha de isolamento, aprovação indevida ou efeito duplicado bloqueia promoção. Gatilho não autoriza incluir no Core nem reabrir os produtos antigos. Próxima ação é executar M0 quando houver pedido de desenvolvimento.

## Prioridade de reuso existente

Aplicar o [contrato canônico de fragmentação e reuso](../04_ABSORCAO_CODEX_BASE44.md): localizar equivalentes antes de criar, preservar origens e justificar lacunas. WM-39/TC-21 mantêm a rastreabilidade.

## Natureza interna transversal — esclarecimento de produto

Seguir a [definição canônica de Continuity](../16_CONTINUIDADE_OPERACIONAL.md): infraestrutura interna para módulos, opcional por composição, sem produto ou cobrança separada presumidos. Owners conservam autoridade; M5-CO não antecipa o primeiro ciclo.

