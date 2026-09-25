# ADR-M02 — Core mínimo, módulos opcionais e verticais

**Status:** DECIDIDO_PELO_PEDIDO · **Data:** 24/09/2026 · **Escopo:** documentação/fundação Wissian.

## Contexto
Plataforma para operações diferentes não pode pressupor alojamentos, telas, financeiro, RH ou fiscal em todo tenant.

## Decisão e limite
Core contém apenas mecanismos transversais indispensáveis. Operação administrativa usa o mesmo contrato modular das demais capacidades. Verticais possuem regras/dados próprios. Monólito modular é topologia inicial proposta; seleção por tenant de capacidades instaladas ocorre sem fork/rebuild.

## Alternativa considerada
Um ERP integrado obrigatório ou plugin loader dinâmico ampliariam dependências e superfície. Serviços separados só com necessidade demonstrada.

## Consequências e evidência
Dois testes distintos são exigidos: ausência física em composição neutra e desativação por tenant num artefato comum. Duplicação de infraestrutura para provar isolamento seria falso reuso.

Validar pelo marco e pelos testes da [matriz mestre](../matrizes/requisitos-mestre.json). Status documental da decisão não é implementação nem autorização externa.

## Origem e supersessão
Pedido atual; guia fundador §§8/9/22/23; v1.1 ADR-REEST-002; capítulos 03/07.

## Reavaliação
Reavaliar promoção de uma capacidade ao Core apenas com obrigação transversal e fronteira sem domínio; não por número de consumidores isolado.
