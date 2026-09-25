# Registro de decisões mestre

As cinco ADRs originais e a ADR de stack local permanecem no acervo. Esta camada explicita adoção, adaptação e decisão aberta; não falsifica aprovação histórica.

- [ADR-M01 — Repositório independente e procedência](ADR-M01_REPOSITORIO_E_PROCEDENCIA.md): DECIDIDO_PELO_PEDIDO.
- [ADR-M02 — Core mínimo, módulos opcionais e verticais](ADR-M02_CORE_MODULOS_E_VERTICAIS.md): DECIDIDO_PELO_PEDIDO.
- [ADR-M03 — Escolha de stack por caracterização](ADR-M03_STACK_E_REUSO.md): PROPOSTA_COM_GATE_M0.
- [ADR-M04 — Organização usuária e autoridade separada](ADR-M04_ORGANIZACOES_E_AUTORIDADE.md): DIRETRIZ_CONSOLIDADA.
- [ADR-M05 — Schema novo, histórico preservado e migração separada](ADR-M05_DADOS_E_COMPATIBILIDADE.md): DIRETRIZ_CONSOLIDADA.
- [ADR-M06 — Primeiro ciclo administrativo e catálogo futuro](ADR-M06_PRIMEIRO_FLUXO_E_CATALOGO.md): DIRETRIZ_CONSOLIDADA.
- [ADR-M07 — Evidência, acervo imutável e recuperação](ADR-M07_EVIDENCIA_E_PRESERVACAO.md): DIRETRIZ_CONSOLIDADA.

`DECIDIDO_PELO_PEDIDO` deriva da solicitação atual; `DIRETRIZ_CONSOLIDADA` traduz fontes sem comprovar implementação; `PROPOSTA_COM_GATE_M0` precisa de evidência antes de execução dependente. Criar ADR nova quando decisão duradoura mudar; manter ligação à anterior.

## Adendo 2.1.0

- [ADR-M08 — Continuidade operacional](ADR-M08_CONTINUIDADE_OPERACIONAL.md): direção DECIDIDO_PELO_PEDIDO; implementação CONTRATOS_PROPOSTOS. Complementa M02/M05/M06/M07, preservando primeiro ciclo e Core mínimo.
