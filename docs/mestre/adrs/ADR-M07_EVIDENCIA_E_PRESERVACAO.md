# ADR-M07 — Evidência, acervo imutável e recuperação

**Status:** DIRETRIZ_CONSOLIDADA · **Data:** 24/09/2026 · **Escopo:** documentação/fundação Wissian.

## Contexto
Consolidação deve preservar detalhe sem manter múltiplas instruções concorrentes na raiz ou confundir intenção com execução.

## Decisão e limite
Fontes originais ficam íntegras com hashes; camada mestre resolve precedência e cria crosswalks. Backup datado e ensaio de restauração precedem organização local. Testes de produto permanecem NOT_RUN. C7 guarda continuidade interna; Git guarda contratos.

## Alternativa considerada
Reescrever tudo perderia procedência e consolidar só num resumo perderia detalhes. Preservar tudo como instrução ativa manteria conflito e risco.

## Consequências e evidência
O pacote é mais completo que um resumo, mas exige rota de leitura por fatia. Manifesto é consistência, não assinatura; arquivo final tem hash externo. Origem Git fica fora do ZIP por privacidade/portabilidade.

Validar pelo marco e pelos testes da [matriz mestre](../matrizes/requisitos-mestre.json). Status documental da decisão não é implementação nem autorização externa.

## Origem e supersessão
Pedido atual; v1.1 ADR-REEST-005; capítulos 01/10/12/14.

## Reavaliação
Reavaliar fontes se houver novos hashes/refs; produzir nova versão e revisão das decisões, nunca editar silenciosamente o acervo.
