# ADR-M04 — Organização usuária e autoridade separada

**Status:** DIRETRIZ_CONSOLIDADA · **Data:** 24/09/2026 · **Escopo:** documentação/fundação Wissian.

## Contexto
Fultuma é fornecedora e pode operar como organização usuária. Relação comercial, empresa jurídica, tenant e administrador de plataforma não são sinônimos.

## Decisão e limite
Fultuma de demonstração e Empresa Exemplo usam as mesmas regras. Tenant e autorização são validados no servidor para todos os canais. Plano de controle não recebe leitura universal de dados empresariais. Direito contratado, feature flag e permissão são estados distintos.

## Alternativa considerada
Tenant Fultuma privilegiado simplificaria testes internos, mas ocultaria vazamentos e impediria prova de generalidade. Um único administrador global sobre operações viola a fronteira desejada.

## Consequências e evidência
Suporte excepcional precisa de concessão escopada/expirável/revogável/auditada. Layout e cobrança não determinam autoridade.

Validar pelo marco e pelos testes da [matriz mestre](../matrizes/requisitos-mestre.json). Status documental da decisão não é implementação nem autorização externa.

## Origem e supersessão
Plano base §§3/7 e WB-16; guia §§22/24; v1.1 REQ07/08/09/18; capítulo 07.

## Reavaliação
Reavaliar unidade/modelo físico por necessidade de cliente e threat model; não enfraquecer separação lógica.
