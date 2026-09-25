# Ambiente e configuração

## Disponível nesta fase

Há Git local e arquivos de organização. Nenhuma instalação de Node/pnpm, framework, banco ou Docker foi feita por esta preparação. Não executar geradores de projeto antes de reconciliar o plano existente.

| Arquivo criado na raiz | Função e limite |
| --- | --- |
| `.editorconfig` | UTF-8, espaços, LF e preservação de quebras Markdown |
| `.gitattributes` | Convenção de fim de linha no Git, incluindo CRLF para batch |
| `.gitignore` | Ignorar segredos, dependências, caches, dados locais e estado de infraestrutura |
| `.dockerignore` | Excluir material sensível/gerado de um futuro contexto de build na raiz |
| `.env.example` | Catálogo proposto, com valores vazios; não é configuração executável |
| `.prettierrc.json` | Convenções de formatação; ferramenta ainda não instalada |
| `.prettierignore` | Preservar fonte fundadora e evitar formatação de gerados/dados |
| `README.md` / `CONTRIBUTING.md` | Entrada e contrato de colaboração |
| `AGENTS.md` | Limites e fontes locais para agentes |
| `SECURITY.md` / `NOTICE.md` | Requisitos de segurança e titularidade informada |

Arquivos de ignore não detectam todo segredo. Revisão e futura verificação automatizada continuam necessárias. O `.dockerignore` só vale para o contexto em que for aplicado; builds com outro contexto exigem configuração equivalente.

## Criar no bootstrap, depois do plano

| Arquivo/local | Conteúdo esperado |
| --- | --- |
| `package.json` na raiz | `private: true`, `packageManager` com versão exata, `engines` e scripts realmente existentes |
| `pnpm-workspace.yaml` | Seleção de `apps/*` e `packages/*`; configurações compatíveis com a versão escolhida |
| `pnpm-lock.yaml` | Gerado pelo gerenciador, revisado e versionado; não escrever à mão |
| `.node-version` | Versão Node exata, igual no desenvolvimento e CI; compatível com a LTS escolhida |
| `tsconfig.base.json` | TypeScript estrito e regras comuns; configurações próprias de Next/Nest estendem quando compatíveis |
| `eslint.config.mjs` | Flat config, TypeScript/frameworks e restrições de importação; dependências declaradas |
| `apps/*/package.json` | Dependências e comandos de cada executável; pacote privado por padrão |
| `apps/*/.env.example` | Variáveis mínimas de cada processo; somente nomes consumidos pela implementação |
| `apps/web/Dockerfile`, `apps/api/Dockerfile` | Builds em etapas, usuário sem privilégios, imagem mínima e versões controladas |
| `compose.yaml` | Dependências locais necessárias, healthchecks e portas vinculadas ao localhost |
| Configurações de testes | Runner compatível com a stack e Playwright para fluxos web, quando esses fluxos existirem |
| CI no provedor escolhido | Checks executáveis, permissões mínimas e dependências fixadas; nenhum deploy automático presumido |

O pnpm requer [arquivo de workspace na raiz](https://pnpm.io/workspaces). O lint seguirá o formato de [configuração atual do ESLint](https://eslint.org/docs/latest/use/configure/configuration-files), separado da [formatação Prettier](https://prettier.io/docs/configuration). Os Dockerfiles futuros seguirão [builds em etapas](https://docs.docker.com/build/building/multi-stage/); esses mecanismos ainda não foram exercitados neste projeto.

Não criar Dockerfile que aponta para build inexistente, CI que apenas imprime sucesso, hooks obrigatórios sem ferramenta instalada, `CODEOWNERS` com usuários fictícios nem lockfile vazio. Nx/Turborepo entram somente se a execução de tarefas justificar uma camada adicional.

## Variáveis e segredos

O catálogo da raiz documenta os conceitos, mas não deve ser injetado integralmente em todos os processos. A API recebe sua conexão de runtime; somente o migrador recebe `MIGRATION_DATABASE_URL`. A web não recebe nenhuma delas. O provedor de identidade e a divisão exata da sessão ainda serão definidos.

Na implementação, validar configuração no início de cada processo e recusar campos obrigatórios ausentes, sem imprimir valores. Segredos ficam em arquivos locais ignorados ou no mecanismo de segredos do ambiente. Exemplos contêm somente valores sintéticos. Variáveis `NEXT_PUBLIC_*` devem ser consideradas públicas.

Não configurar credenciais de produção na máquina de teste nem persistir tokens em argumentos/scripts. Fixar a política de sessão, origens permitidas, TLS e autorização junto do primeiro fluxo de identidade.

## Ambientes e DevOps

Desenvolvimento usa dados sintéticos; homologação tem credenciais próprias; produção exige acesso e autorização próprios. Ambientes de PR, se adotados, terão expiração e limites de custo. Não haverá cópia automática de dados de produção.

Antes de build, download grande ou execução em lote, verificar CPU, memória e armazenamento e limitar concorrência. Em Windows, os comandos devem funcionar em PowerShell e respeitar caminhos com espaços. Parametrizar diretórios; não versionar caminhos pessoais da máquina. Compose/WSL devem ser escolhidos conforme capacidade real, não instalados por conveniência do agente.

Quando houver CI, separar: validação de PR; geração de artefato identificado pelo commit; promoção autorizada desse mesmo artefato. O pipeline deverá verificar formatação, tipos, testes afetados, build e segredos/dependências conforme risco. Jobs de PR não recebem segredos de deploy. Produção tem aprovação própria, credenciais de curta duração quando suportadas e recuperação documentada.

Não existe automação remota configurada hoje. Escolha de provedor, região, custos, identidade, RPO/RTO e procedimentos operacionais acompanha o plano e a primeira entrega executável.
