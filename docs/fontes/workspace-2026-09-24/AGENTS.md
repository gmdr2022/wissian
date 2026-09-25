# Wissian — instruções locais

## Identidade e fontes

Este workspace pertence exclusivamente à engenharia de Wissian, plataforma-base da Fultuma Tech Ltda. Não presumir ClubAL ou importar código, políticas e dados de outras verticais.

Leia o [guia fundador](Wissian-Guia-Fundador-da-Plataforma.md), o [README](README.md) e os contratos relevantes em `docs/`. Preserve o documento fundador original. A lista de capacidades é conceitual, não um backlog a executar.

## Limite desta fase

A fase atual é organização anterior ao desenvolvimento. Não criar aplicações, dependências, schemas, migrações, autenticação, CI de produto ou infraestrutura em execução com base apenas nesta estrutura. O [plano da operação base](PLANO_WISSIAN_OPERACAO_BASE.md) propõe o uso da mesma base por Fultuma e clientes; confronte-o com a ADR proposta e com eventual plano anterior disponível antes de implementar. O [pedido preparado](INICIAR_WISSIAN_NO_CODEX.md) só autoriza execução quando Gui o enviar como solicitação. Uma solicitação posterior explícita pode mudar esta fase; atualize a documentação junto com a transição.

## Direção técnica

Monorepo, backend modular, TypeScript/Next.js/NestJS/PostgreSQL são recomendações ainda não implementadas. A [ADR 0001](docs/architecture/decisions/0001-base-tecnica.md) registra premissas e gatilhos de revisão. `apps/`, `packages/`, `database/`, `infra/`, `scripts/` e `tests/` são reservas sem código.

## Comandos e evidência

Há Git local; não há comandos de aplicação, testes, lint ou build. `git status --short --branch` e `git diff --check` auxiliam a inspeção, mas arquivos não rastreados também precisam ser revisados. Não invente scripts nem relate testes de produto como executados nesta fase.

Antes e depois de mudanças, confira o estado real e preserve trabalho alheio. Não fazer commit/push, criar remote ou publicar sem autorização. Não inserir credenciais reais, dados de clientes ou caminhos pessoais em arquivos versionáveis.

## Fronteiras

Lógica compartilhada exige benefício e contratos claros; regras de uma vertical continuam no produto consumidor. A API será responsável por autorização; o frontend não terá credenciais do banco. Isolamento, auditoria e recuperação são requisitos a demonstrar na implementação, não garantias obtidas por esta documentação.

## Continuidade

Use o projeto C7 `wissian` e o protocolo vigente quando disponível. Registre contexto interno no cofre; mantenha aqui documentação compartilhada, ADRs e contratos necessários aos colaboradores. Ausência do cofre não impede compreender ou contribuir com o repositório. Não criar diários paralelos no Git nem reparar configuração global durante tarefa de Wissian.
