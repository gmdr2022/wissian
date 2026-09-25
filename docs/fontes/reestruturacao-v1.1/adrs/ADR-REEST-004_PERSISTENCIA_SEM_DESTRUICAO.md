# ADR-REEST-004 — Extrair persistência preservando histórico e autoridade

Data: 22/09/2026. Estado: direção técnica do pacote; aplicação e testes pendentes.

## Contexto

Os projetos atuais incluem Infrastructure e Migrator, com provável agregação de domínios cuja estrutura integral será inspecionada.

## Decisão

Inventariar owners/migrations; manter histórico e uma autoridade por dado. Usar alterações aditivas e testes de upgrade. Definir a inicialização neutra em ADR local detalhado após inspeção, sem filtrar migrações arbitrariamente.

## Alternativas consideradas

Reset/drop para limpar: rejeitado. Dois backends autoritativos em dual-write: rejeitado. Não separar nada por dificuldade: estado intermediário permitido, não conclusão final.

## Consequências e trade-offs

A pureza do core é medida no código/artefato e composição, sem exigir apagar tabelas históricas de bancos migrados. Pode haver extração em várias fatias.

## Evidência de aceite

SQL revisado, upgrade/bootstrap/idempotência/restore em PostgreSQL sintético; core não importa o módulo para funcionar.

## Revisão futura

Revisar por evidência de nova necessidade ou inviabilidade encontrada, registrando impacto, compatibilidade e alternativa mais simples. Não usar o ADR como autorização de ação externa ou destrutiva.
