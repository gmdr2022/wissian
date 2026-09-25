# ADR-REEST-005 — Manter continuidade existente e estados de aprovação separados

Data: 22/09/2026. Estado: direção técnica do pacote; aplicação e testes pendentes.

## Contexto

C7 e ACTIVE_SLICE distinguem progresso local, gates humanos/institucionais e ativação externa.

## Decisão

Usar continuidade existente; pacote é diretriz, não novo cofre. Reportar testes reais e nunca promover automaticamente gates ou executar deploy/custo/dados reais.

## Alternativas consideradas

Novo diário de status paralelo: rejeitado. Marcar aprovações por inferência de testes: rejeitado. Interromper toda tarefa por C7 indisponível: evitar quando há fallback seguro.

## Consequências e trade-offs

Menos duplicidade; limitação de acesso deve ficar explícita. Homologação continua separada da implementação.

## Evidência de aceite

Relatório verificável, próximos passos recuperáveis, nenhuma aprovação fabricada ou nova despesa.

## Revisão futura

Revisar por evidência de nova necessidade ou inviabilidade encontrada, registrando impacto, compatibilidade e alternativa mais simples. Não usar o ADR como autorização de ação externa ou destrutiva.
