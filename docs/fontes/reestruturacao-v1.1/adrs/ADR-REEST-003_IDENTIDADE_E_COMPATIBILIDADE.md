# ADR-REEST-003 — Separar apresentação de identidade técnica

Data: 22/09/2026. Estado: direção técnica do pacote; aplicação e testes pendentes.

## Contexto

Wissian é o nome escolhido para a plataforma-base da Fultuma. O código legado usa ClubAL em cookies, solution, paths, artefatos e formatos; a mudança de marca não autoriza renomear essas identidades técnicas.

## Decisão

Centralizar display name/branding; conservar IDs/aliases técnicos até migração explícita e testada. Não renomear remoto, cofre ou solução indiscriminadamente.

## Alternativas consideradas

Find/replace global: rejeitado. Aguardar liberação jurídica para todo trabalho técnico: desnecessário. Usar config do browser como autoridade: rejeitado.

## Consequências e trade-offs

Alguns nomes antigos permanecem legitimamente; uma allowlist delimitada explica compatibilidade sem tolerar acoplamento de domínio.

## Evidência de aceite

Rebranding não perde sessão, preferência, pareamento, dados ou assinatura; testes de aliases e contrato passam.

## Revisão futura

Revisar por evidência de nova necessidade ou inviabilidade encontrada, registrando impacto, compatibilidade e alternativa mais simples. Não usar o ADR como autorização de ação externa ou destrutiva.
