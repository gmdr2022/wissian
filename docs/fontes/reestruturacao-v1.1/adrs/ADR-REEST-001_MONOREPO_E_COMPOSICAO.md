# ADR-REEST-001 — Manter monorepo e composição modular

Data: 22/09/2026. Estado: direção técnica do pacote; aplicação e testes pendentes.

## Contexto

O projeto possui evolução OWM, legado e convergência Base44; criar clones por cliente multiplicaria pontos de correção.

## Decisão

Manter inicialmente o repositório privado e separar componentes por contratos/assemblies quando necessário. Produtos consomem código comum; clientes usam configuração. Não adicionar microserviços ou repositórios remotos.

## Alternativas consideradas

Cópias independentes: rejeitadas pela divergência de manutenção. Serviços separados: adiados sem requisito de escala/organização demonstrado. Uma aplicação genérica sem fronteiras: insuficiente para o objetivo.

## Consequências e trade-offs

Menor custo operacional, mas exige disciplina de dependências e testes de consumidores. Compartilhar código não obriga a compartilhar dados/release para sempre.

## Evidência de aceite

Grafo acíclico, isolamento do core, composição neutra e socioeducativa; nenhuma branch permanente por cliente.

## Revisão futura

Revisar por evidência de nova necessidade ou inviabilidade encontrada, registrando impacto, compatibilidade e alternativa mais simples. Não usar o ADR como autorização de ação externa ou destrutiva.
