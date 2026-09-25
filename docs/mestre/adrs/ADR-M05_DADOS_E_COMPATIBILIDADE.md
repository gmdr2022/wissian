# ADR-M05 — Schema novo, histórico preservado e migração separada

**Status:** DIRETRIZ_CONSOLIDADA · **Data:** 24/09/2026 · **Escopo:** documentação/fundação Wissian.

## Contexto
Preservar ClubAL não obriga copiar seu schema completo para uma plataforma nova. Compatibilidade só existe se houver consumidor real declarado.

## Decisão e limite
Criar migrations próprias no destino quando houver implementação. Cada módulo possui seus dados e evolução. Código e dados têm planos separados; nenhuma migration real nesta entrega. Manter identidades e histórico da origem intactos; adapters só para pares e usos necessários. Desativar módulo não elimina dados.

## Alternativa considerada
Clonar todos os schemas carregaria dados/vertical; renomear toda a origem destruiria contratos. Dual-write automático criaria divergência e recuperação ambígua.

## Consequências e evidência
Requer mapeamento de IDs/versões quando absorção precisar de compatibilidade e ensaio sintético de bootstrap/upgrade/rollback. Expand-contract é direção, não autorização para contração destrutiva.

Validar pelo marco e pelos testes da [matriz mestre](../matrizes/requisitos-mestre.json). Status documental da decisão não é implementação nem autorização externa.

## Origem e supersessão
v1.1 ADR-REEST-003/004 (preservados com adaptação da identidade do destino); plano base §§6/7/10; capítulo 06.

## Reavaliação
Reavaliar ao identificar cliente/consumidor antigo que precise interoperar; exigir matriz de pares antes de prometer compatibilidade.
