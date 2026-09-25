# ADR-REEST-002 — Substituir o produto único por fundação e produtos

Data: 22/09/2026. Estado: direção técnica do pacote; aplicação e testes pendentes.

## Contexto

AGENTS e ProductBoundary fixam ClubAL como único produto com três módulos PEMSE; a nova decisão de Gui separa fundação e produtos.

## Decisão

Superseder somente essas fronteiras de produto. Preservar Operação/Rotinas/Consulta como experiências socioeducativas e seus contratos de edição/leitura/publicação.

## Alternativas consideradas

Apagar todos os nomes antigos: rejeitado por risco a contratos. Manter a definição global antiga: rejeitado porque não permite fundação neutra.

## Consequências e trade-offs

Documentação e testes de direção devem mudar em conjunto; identidade técnica histórica pode permanecer em adapters.

## Evidência de aceite

Testes da nova composição e negativos equivalentes aos antigos; nenhum bypass de segurança por atualização de expectativa.

## Revisão futura

Revisar por evidência de nova necessidade ou inviabilidade encontrada, registrando impacto, compatibilidade e alternativa mais simples. Não usar o ADR como autorização de ação externa ou destrutiva.
