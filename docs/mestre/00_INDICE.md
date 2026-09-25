# Índice de consulta do Wissian

A entrada principal é [README.md](../../README.md). Este índice é um mapa de assuntos, não uma lista de leitura obrigatória. Escolher a pergunta/fatia e abrir só seu contrato; expandir para a fonte indicada quando necessário.

| Pergunta | Fonte canônica |
|---|---|
| Qual pedido prevalece e o que está autorizado? | [01 — Mandato](01_MANDATO_E_PRECEDENCIA.md) |
| Que fontes foram inspecionadas e o que falta provar em M0? | [02 — Estado e lacunas](02_ESTADO_FONTES_E_LACUNAS.md) |
| O que pertence ao Core, módulos ou especialidades? | [03 — Arquitetura](03_ARQUITETURA_E_MODULARIDADE.md) |
| Onde reutilizar e como comprovar equivalência? | [04 — Absorção](04_ABSORCAO_CODEX_BASE44.md) e [ficha](modelos/FICHA_ABSORCAO.md) |
| Como preservar Git e as origens? | [05 — Git](05_GIT_E_REPOSITORIOS.md) |
| Quem é dono dos dados e como migrar? | [06 — Dados](06_DADOS_MIGRACOES_COMPATIBILIDADE.md) |
| Como negar acesso indevido e isolar organizações? | [07 — Segurança](07_SEGURANCA_E_ISOLAMENTO.md) |
| Qual experiência e identidade preservar? | [08 — UX](08_UX_MARCA_E_QUALIDADE.md) e [guia fundador original](../fontes/workspace-2026-09-24/Wissian-Guia-Fundador-da-Plataforma.md) |
| Qual é o próximo marco e sua dependência? | [09 — Roadmap](09_ROADMAP_INTEGRADO.md) |
| Que teste e aceite encerram a fatia? | [10 — Testes e aceite](10_TESTES_E_ACEITE.md) |
| Como operar, observar, recuperar e reverter? | [11 — Operação](11_OPERACAO_OBSERVABILIDADE_ROLLBACK.md) |
| Onde registrar progresso e como retomar? | [12 — Continuidade de engenharia](12_CONTINUIDADE_E_HANDOFF.md) |
| Quais decisões e riscos continuam abertos? | [13 — Riscos](13_RISCOS_E_DECISOES_ABERTAS.md) e [ADRs](adrs/README.md) |
| Como validar/manter os documentos e recuperar arquivos? | [14 — Manutenção documental](14_INSTALACAO_E_RESTAURACAO.md) |
| Quais referências técnicas sustentaram a preparação? | [15 — Referências](15_REFERENCIAS_TECNICAS.md) |
| Como funciona a coordenação interna dos módulos? | [16 — Continuity](16_CONTINUIDADE_OPERACIONAL.md), [ADR-M08](adrs/ADR-M08_CONTINUIDADE_OPERACIONAL.md) e [matriz Nexus/CC](matrizes/reaproveitamento-nexus-cc.json) |
| O que a base administrativa deve resolver para Fultuma/clientes? | [17 — Base administrativa](17_BASE_ADMINISTRATIVA_E_TRANSICAO.md) |

## Localização por identificador

| Identificador | Abrir diretamente |
|---|---|
| WM-xx / TM-xx / TC-xx | [Requisitos](matrizes/requisitos-mestre.json) / [testes planejados](matrizes/testes-planejados.json) |
| ABS-xxx | [Candidatos de migração](matrizes/migracao.json) |
| NC-xx / UC-NC-xx | [Patrimônio Nexus/CC](matrizes/reaproveitamento-nexus-cc.json) |
| WB-xx / NF-xx / REQxx / testes v1.1 | [Cobertura de origem](matrizes/cobertura-origem.json), com caminho e trecho de fonte |
| TEM / QR / GRA / PCE | [Requisitos das frentes](matrizes/requisitos-frentes-origem.json) e [ponteiros do legado](matrizes/fontes-legado.json) |
| OWM | [Roadmap extraído](matrizes/roadmap-owm-origem.json) |
| Arquivo ou anexo original | [Matriz de fontes](matrizes/fontes.json), com caminho/hash; [acervo](../fontes/README.md) |

JSON é o formato estruturado; CSV serve à consulta tabular e deve permanecer equivalente. Consultar [matrizes](matrizes/README.md) e [exemplos de contratos](contratos/README.md) somente para o assunto necessário.

## Prompts por ação

[M0](prompts/01_AUDITAR_FONTES.md) · [Fundação/M2–M4](prompts/02_IMPLEMENTAR_FUNDACAO.md) · [Absorver capacidade](prompts/03_ABSORVER_CAPACIDADE.md) · [Retomar](prompts/04_RETOMAR.md) · [Revisar](prompts/05_REVISAR.md) · [M5-CO](prompts/06_IMPLEMENTAR_CONTINUITY.md).

O pedido vigente escolhe um escopo; não executar essa lista em lote. Uma fonte histórica mantém valor de evidência mesmo quando sua prioridade foi superada.
