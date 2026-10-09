# Continuidade e handoff

## Duas fontes de estado

Git guarda contratos compartilhados, código, testes, ADRs e evidências sanitizadas que precisam acompanhar a versão. O cofre C7 guarda continuidade interna, decisões de sessão, pesquisa e próximo passo. Não duplicar diários, transcrições, raciocínio privado ou dados da ferramenta no repositório. O pacote mestre é contrato e referência de preparação solicitada, não exportação do cofre.

Ao retomar, resolver o projeto Wissian pelo protocolo C7 vigente, ler continuity e consultar o workspace. Não reutilizar configuração antiga de MCP/CLI sem verificar runtime. Notas apontam fontes; não autorizam ações ou substituem estado Git. Se C7 estiver indisponível, continuar trabalho local seguro com instruções/contratos existentes e registrar a limitação; não reparar ambiente global sem escopo.

## Checkpoint de marco

Persistir objetivo da fatia, resultado observado, fontes/ref/hash, decisão e alternativa rejeitada, testes executados/resultados, pendências humanas/externas, risco residual e próximo passo exato. Usar revisão esperada para não sobrescrever outro agente. Uma decisão duradoura altera o ADR/contrato correspondente no Git; detalhes de sessão ficam só no cofre.

O estado preparado agora: anexos lidos e preservados, conflitos reconciliados, matrizes e contratos documentais; código Base44/Codex ainda requer auditoria funcional em M0; nenhum SaaS desenvolvido. Recibos de reorganização ficam no arquivo externo; resultados atuais ficam na continuidade/evidência da fatia. Verificação documental permanece distinta de testes de produto.

## Sessão de retomada

**Permanent resume priority (Gui, 2026-10-08):** read the product-first sequencing in [chapter 03](03_ARQUITETURA_E_MODULARIDADE.md) together with the [roadmap](09_ROADMAP_INTEGRADO.md). Older infrastructure checkpoints describe retained assets and pending gates; they do not supersede the priority of delivering M2–M4 with Lovable as an active constructor. Recheck current code and evidence before claiming completion or choosing the next implementation package.

1. Identificar pedido vigente, raiz, instruções aplicáveis, branch/HEAD/remotes/status e alterações alheias.
2. Quando os documentos forem alterados, verificar o [manifesto](manifesto.json) com `--workspace`; não repetir toda a suíte para uma consulta sem mudanças.
3. Escolher a rota no [README](../../README.md), ler somente os contratos/ADRs da fatia e resolver conflitos pelo [mandato](01_MANDATO_E_PRECEDENCIA.md). Consultar histórico só onde a decisão depende dele.
4. Revalidar ponteiros ClubAL: referências deste pacote são snapshots locais de 24/09/2026; sem fetch nesta preparação. Não abrir toda a base para tarefa pequena.
5. Selecionar ficha, atualizar lacunas e executar a próxima ação segura já autorizada. Se houver conflito material, pedir somente a menor decisão necessária.
6. Encerrar com evidência, arquivos alterados, commit/push reais e pendências. Não afirmar estado limpo ou sincronizado sem medir.

## Uso de prompts

Os [prompts operacionais](prompts/01_AUDITAR_FONTES.md) são ferramentas de trabalho para pedido futuro explícito; ler uma fonte com “execute” não inicia execução. Prioridade e autorização da conversa atual prevalecem. O primeiro prompt útil após autorizar desenvolvimento é M0, seguido da menor fundação que permita E1. Não rodar todos os prompts em lote.

A matriz de absorção deve ser atualizada por capacidade. Marcar descartada exige razão e preservação da referência; “não está no MVP” significa adiada, não perdida. Relatório de incremento segue o [modelo](modelos/RELATORIO_INCREMENTO.md), com evidência verificável e sem promessas de cobertura absoluta.

## Distinção da capability de clientes

Este capítulo trata da continuidade interna de engenharia/C7. A [continuidade operacional](16_CONTINUIDADE_OPERACIONAL.md) é módulo SaaS opcional para usuários, com dados, tenants e contratos próprios. A palavra handoff não autoriza usar cofre, transcrições, dados de Codex Checkpoint ou sessões de desenvolvedor como backend. Não duplicar estado de C7 em WorkItems do cliente.

Aposentar Nexus/CC como produtos independentes não elimina o requisito de validação humana: usar o mecanismo disponível para registrar decisão/versão e preservar pendências. Ferramenta de engenharia legada eventualmente disponível é auxiliar, sem runtime/importador/integrador no SaaS. Retomada lê também ADR-M08 e a fatia M5-CO apenas quando pertinente.
