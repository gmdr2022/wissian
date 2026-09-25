# Fontes e referências

Consulta registrada na montagem original em 22/09/2026; fontes remotas não reconsultadas nesta revisão de identidade. Referências externas são primárias (documentação oficial, OWASP e órgãos públicos). As decisões de implementação deste pacote são recomendações próprias, não garantias emitidas pelas fontes. Não foram copiados documentos inteiros ou código privado para o pacote.

## Evidências do repositório

### E01 — Refs e comparação

Fonte: <https://api.github.com/repos/gmdr2022/clubal/compare/8ac5f093092ba4afef4bea813562a9c985dd96b3...75f1fc34624dfb39beef57475d4e619303dcfcee?per_page=1&page=2>

Escopo: Metadados da comparação; ahead=27; behind=0; não foi revisão de todos os diffs.

### E02 — Instruções locais

Fonte: <https://github.com/gmdr2022/clubal/blob/75f1fc34624dfb39beef57475d4e619303dcfcee/AGENTS.md>

Escopo: Linhas 1–160 solicitadas; arquivo retornado integralmente.

### E03 — Continuidade portátil

Fonte: <https://github.com/gmdr2022/clubal/blob/75f1fc34624dfb39beef57475d4e619303dcfcee/ops/runtime/ACTIVE_SLICE.md>

Escopo: Linhas 1–140; relatos históricos não reexecutados.

### E04 — Arquitetura legado

Fonte: <https://github.com/gmdr2022/clubal/blob/75f1fc34624dfb39beef57475d4e619303dcfcee/docs/01_ARCHITECTURE_CURRENT.md>

Escopo: Linhas 1–200; descrição parcial frente ao OWM atual.

### E05 — Fronteira de produto

Fonte: <https://github.com/gmdr2022/clubal/blob/75f1fc34624dfb39beef57475d4e619303dcfcee/src/backend/ClubAL.Domain/ProductBoundary.cs>

Escopo: Arquivo completo: lista global de três nomes PEMSE.

### E06 — Bootstrap da API

Fonte: <https://github.com/gmdr2022/clubal/blob/75f1fc34624dfb39beef57475d4e619303dcfcee/src/backend/ClubAL.Api/Program.cs>

Escopo: Linhas 1–170; não cobre todos os endpoints.

### E07 — Scripts SPA

Fonte: <https://github.com/gmdr2022/clubal/blob/75f1fc34624dfb39beef57475d4e619303dcfcee/src/web/operador/package.json>

Escopo: Arquivo completo; comandos não executados.

### E08 — Validador integrado OWM

Fonte: <https://github.com/gmdr2022/clubal/blob/75f1fc34624dfb39beef57475d4e619303dcfcee/scripts/owm/Invoke-OwmFoundationValidation.ps1>

Escopo: Linhas 1–175 solicitadas; script retornado integralmente.

### E09 — Projetos backend

Fonte: <https://api.github.com/repos/gmdr2022/clubal/contents/src/backend?ref=75f1fc34624dfb39beef57475d4e619303dcfcee>

Escopo: Listagem de projetos; não análise de dependências.

### E10 — Domínio

Fonte: <https://api.github.com/repos/gmdr2022/clubal/git/trees/812c6e90872011037a273a4411ee93d91d93bd22>

Escopo: Árvore não recursiva; truncated=false.

### E11 — Infraestrutura

Fonte: <https://api.github.com/repos/gmdr2022/clubal/git/trees/7b30b6aeff82fd06ec1e3ed95c7c08a3cd16ba74>

Escopo: Árvore não recursiva; truncated=false; conteúdos não integralmente lidos.

## Referências públicas

### S01 — Microsoft — multitenant deployment/configuration

Fonte: <https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/approaches/deployment-configuration>

Aplicação e limite: Isolamento, implantação consistente e distinção entre flags e entitlements. Não exige Azure.

### S02 — Microsoft — aplicar migrations EF Core

Fonte: <https://learn.microsoft.com/pt-br/ef/core/managing-schemas/migrations/applying>

Aplicação e limite: Inspeção/teste de SQL e riscos de estratégias de implantação. Não autoriza produção.

### S03 — OWASP — Authorization Cheat Sheet

Fonte: <https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html>

Aplicação e limite: Autorização por requisição, privilégio mínimo e testes negativos.

### S04 — GitHub — criar repositório a partir de template

Fonte: <https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-repository-from-a-template>

Aplicação e limite: Template inicia novo repositório; não é mecanismo de atualização compartilhada.

### S05 — ANPD — dados de crianças e adolescentes

Fonte: <https://www.gov.br/anpd/pt-br/assuntos/noticias/anpd-divulga-enunciado-sobre-o-tratamento-de-dados-pessoais-de-criancas-e-adolescentes>

Aplicação e limite: Notícia de 24/05/2023 sobre melhor interesse e hipóteses legais; não é análise jurídica do SaaS.

### S06 — Cloudflare — pricing de Pages Functions

Fonte: <https://developers.cloudflare.com/pages/functions/pricing/>

Aplicação e limite: Diferencia entrega estática e execução Functions; não dimensiona o backend deste projeto.

### S07 — Cloudflare — framework guides

Fonte: <https://developers.cloudflare.com/pages/framework-guides/>

Aplicação e limite: Orientação atual para novos projetos; não motivação para migrar o ClubAL existente.

### S08 — Cloudflare — Git integration

Fonte: <https://developers.cloudflare.com/pages/get-started/git-integration/>

Aplicação e limite: Integração/publicação a partir de Git; visibilidade da origem não é política de acesso do site.

### S09 — INPI — guia básico de marcas

Fonte: <https://www.gov.br/inpi/pt-br/servicos/marcas/guia-basico>

Aplicação e limite: Busca de semelhantes e registro; não houve liberação da marca Wissian.

### S10 — INPI — perguntas sobre programas de computador

Fonte: <https://www.gov.br/inpi/pt-br/acesso-a-informacao/perguntas-frequentes/programas-de-computador>

Aplicação e limite: Orientação de registro/titularidade; não determina titularidade deste projeto.

## Como usar

IDs E01–E11 referem-se ao que foi observado no repositório. IDs S01–S10 são referências públicas. Um inventário de pastas não prova que a aplicação funciona; um teste descrito em documento não foi reexecutado nesta entrega. Revalidar fontes relevantes quando os contratos, ambiente ou legislação forem novamente avaliados.

Não há pesquisa de mercado nova, domínio reservado, marca liberada ou cotação de hospedagem neste pacote. Não usar arquivos de referência como autorização para ações externas.
