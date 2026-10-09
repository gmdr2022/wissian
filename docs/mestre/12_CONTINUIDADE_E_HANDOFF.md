# Continuidade e handoff

## Canonical engineering continuity

Git holds shared contracts, code, tests, ADRs and sanitized evidence tied to their version. The existing roadmap/status, decision records and slice evidence in the repository that owns the work are sufficient for engineering continuity. Do not duplicate diaries, transcripts, private reasoning, secrets or tool data in the repository.

C7/Obsidian is retired as an operational dependency. Existing notes remain optional historical references, without required installation, access, repair, synchronization or updates. This protocol supersedes older operational C7 requirements in project documentation; preserved historical sources remain unchanged. Context7 library documentation is a distinct technical-reference tool and is unaffected.

On resume, identify the current request, actual Git root, branch/base SHA, local changes and active file owner; read the existing roadmap/status and relevant decisions/evidence. Follow the [README routing](../../README.md) for the active application. Historical notes only point to evidence: they do not authorize actions or override Git. C7 being unavailable is not a blocker and requires no recovery work.

## Checkpoint de marco

Update the existing slice record with its objective, observed result, sources/ref/hash, relevant decision, executed tests/results, human/external dependencies, residual risk and exact next step. Recheck the expected file version and ownership before editing; do not overwrite another agent. A durable decision updates its canonical ADR/contract. Link evidence rather than creating a second status system or a session diary.

Historical preparation baseline: sources were preserved and contracts/matrices reconciled before application development. That snapshot is not the current product state. Current Rota B progress and open gates belong to the active application's existing roadmap and evidence, linked from the README; do not restart M0 from this history. Reorganization receipts remain in their external archive. Documentation validation remains distinct from product tests.

## Sessão de retomada

**Permanent resume priority (Gui, 2026-10-08):** read the product-first sequencing in [chapter 03](03_ARQUITETURA_E_MODULARIDADE.md) together with the [roadmap](09_ROADMAP_INTEGRADO.md). Older infrastructure checkpoints describe retained assets and pending gates; they do not supersede the priority of delivering M2–M4 with Lovable as an active constructor. Recheck current code and evidence before claiming completion or choosing the next implementation package.

1. Identificar pedido vigente, raiz, instruções aplicáveis, branch/HEAD/remotes/status e alterações alheias.
2. Quando os documentos forem alterados, verificar o [manifesto](manifesto.json) com `--workspace`; não repetir toda a suíte para uma consulta sem mudanças.
3. Escolher a rota no [README](../../README.md), ler somente os contratos/ADRs da fatia e resolver conflitos pelo [mandato](01_MANDATO_E_PRECEDENCIA.md). Consultar histórico só onde a decisão depende dele.
4. Revalidar ponteiros ClubAL: referências deste pacote são snapshots locais de 24/09/2026; sem fetch nesta preparação. Não abrir toda a base para tarefa pequena.
5. Selecionar ficha, atualizar lacunas e executar a próxima ação segura já autorizada. Se houver conflito material, pedir somente a menor decisão necessária.
6. Encerrar com evidência, arquivos alterados, commit/push reais e pendências. Não afirmar estado limpo ou sincronizado sem medir.

## Uso de prompts

Os [prompts operacionais](prompts/01_AUDITAR_FONTES.md) são ferramentas de trabalho para pedido futuro explícito; ler uma fonte com “execute” não inicia execução. Prioridade e autorização da conversa atual prevalecem. Choose the prompt for the actual authorized slice; M0 is a historical entrypoint, not a mandatory restart of the active application. Não rodar todos os prompts em lote.

A matriz de absorção deve ser atualizada por capacidade. Marcar descartada exige razão e preservação da referência; “não está no MVP” significa adiada, não perdida. Relatório de incremento segue o [modelo](modelos/RELATORIO_INCREMENTO.md), com evidência verificável e sem promessas de cobertura absoluta.

## Distinção da capability de clientes

This chapter governs repository-native engineering continuity. A [continuidade operacional](16_CONTINUIDADE_OPERACIONAL.md) é módulo SaaS opcional para usuários, com dados, tenants e contratos próprios. A palavra handoff não autoriza usar cofre, transcrições, dados de Codex Checkpoint ou sessões de desenvolvedor como backend. Do not copy private engineering notes into customer WorkItems.

Aposentar Nexus/CC como produtos independentes não elimina o requisito de validação humana: usar o mecanismo disponível para registrar decisão/versão e preservar pendências. Ferramenta de engenharia legada eventualmente disponível é auxiliar, sem runtime/importador/integrador no SaaS. Retomada lê também ADR-M08 e a fatia M5-CO apenas quando pertinente.
