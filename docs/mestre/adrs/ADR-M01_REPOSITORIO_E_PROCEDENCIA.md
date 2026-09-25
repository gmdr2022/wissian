# ADR-M01 — Repositório independente e procedência

**Status:** DECIDIDO_PELO_PEDIDO · **Data:** 24/09/2026 · **Escopo:** documentação/fundação Wissian.

## Contexto
A reestruturação v1.1 propunha separação no repositório ClubAL. Gui agora pede fundação independente, preservando o legado e absorvendo melhorias seletivamente.

## Decisão e limite
Wissian usa seu próprio repositório e main. O checkout local já tem main sem commits e sem remote. Manter .git; não resetar, renomear main ClubAL, fundir históricos ou fazer merge integral. A futura criação de repositório privado/remoto e primeiro commit requerem autorização pertinente.

## Alternativa considerada
Fork/merge integral facilitaria transportar histórico, mas importaria acoplamento e aumentaria risco de apagar diferenças Base44. Referências imutáveis e fichas dão procedência sem copiar tudo.

## Consequências e evidência
Há custo explícito de caracterização e adaptação. A ausência de código herdado no pacote não significa descarte; a matriz aponta onde auditar.

Validar pelo marco e pelos testes da [matriz mestre](../matrizes/requisitos-mestre.json). Status documental da decisão não é implementação nem autorização externa.

## Origem e supersessão
Pedido atual; v1.1 ADR-REEST-001 (supersessão parcial da localização); capítulo 05 deste pacote.

## Reavaliação
Reavaliar somente por restrição comprovada de licença, distribuição ou mudança explícita de objetivo; nunca pela conveniência de um merge.
