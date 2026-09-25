# Inspeção remota — evidências e lacunas

**Data:** 22/09/2026. **Método:** conector GitHub, somente leitura, trechos e árvores selecionados. **Repositório:** `gmdr2022/clubal`.
Não foi clonado nem executado o projeto nesta entrega. Não houve alteração no repositório, commit, merge, push ou deploy. Não foi consultado o cofre C7 local. Não há conclusão de segurança integral, cobertura de testes atual ou equivalência funcional entre branches.

## Referências observadas

| Ref remota | SHA observado | Interpretação permitida |
|---|---|---|
| main | `8ac5f093092ba4afef4bea813562a9c985dd96b3` | Estado da branch remota na consulta, não do disco do Codex |
| integration/base44-main | `75f1fc34624dfb39beef57475d4e619303dcfcee` | Contém evolução posterior ao snapshot de main |
| base44/setup-32dcb42e | `46370e599c021c52ee9734249b438f4a664ab33b` | Origem a preservar e reconciliar |

A API de comparação main → integration informou `ahead_by=27`, `behind_by=0`, `status=ahead`; o merge-base foi o SHA da main acima. Isso prova ancestralidade nesses snapshots, não aprovação de integração, qualidade dos commits ou cobertura completa do Base44. A consulta de comparação foi paginada para metadados; não houve revisão integral de todos os diffs.

## Inventário de evidências

| ID | Fonte inspecionada | O que sustenta | Limite |
|---|---|---|---|
| E01 | branches e compare de SHAs | Refs e relação de ancestralidade acima | Snapshot temporal |
| E02 | AGENTS.md da integração | Stack, contratos, C7, gates e antigas diretrizes de produto único | Política documental, não prova de execução |
| E03 | ops/runtime/ACTIVE_SLICE.md, linhas 1–140 da integração | Continuidade C7; evolução GRA/temas; integração Base44 seletiva relatada; gates pendentes | Histórico relatado não reexecutado |
| E04 | docs/01_ARCHITECTURE_CURRENT.md, linhas 1–200 | Legado local-first, camadas Python, paths, compatibilidade e estado local | Documento não descreve sozinho toda a evolução web |
| E05 | src/backend/ClubAL.Domain/ProductBoundary.cs completo | Lista global fixa com três nomes PEMSE | Acoplamento concreto, não todos os seus consumidores |
| E06 | src/backend/ClubAL.Api/Program.cs, linhas 1–170 | DI, autenticação de dispositivos/edge, autorização, antiforgery, nomes de cookies, limites | Trecho inicial; não é inventário completo de endpoints |
| E07 | src/web/operador/package.json completo | React/TypeScript/Vite; scripts de lint, typecheck, Vitest, Playwright e Cloudflare | Scripts declarados, não executados |
| E08 | scripts/owm/Invoke-OwmFoundationValidation.ps1, linhas 1–175 | Suite integrada, envs requeridos, restore locked, migrations, testes e self-test | Pode alterar banco/instalar dependências; exige sandbox |
| E09 | Árvore src/backend | Api/Application/Contracts/Domain/Infrastructure/Migrator/Edge/DeviceSimulator | Existência de projetos, não independência |
| E10 | Árvore ClubAL.Domain | Audit, Configuration, Devices, IdentityAccess, Knowledge, Operations, Publication, Releases, ReportsAudit, Scheduling, Socioeducational, Tenancy | Candidatos a classificação, não classificação automática |
| E11 | Árvore ClubAL.Infrastructure | DI central, Persistence, socioeducativo, identidade, branding, releases, Player, Consulta e outros | Conteúdo interno não integralmente lido |

## Achados que mudam a execução

**Não partir cegamente da main.** O trabalho remoto mais recente identificado está na integração. O Codex deve verificar se a sua cópia local contém mudanças adicionais, commits ainda não enviados e tarefas inacabadas.

**A documentação evoluiu para C7.** A integração estabelece o cofre configurado como continuidade interna e `ACTIVE_SLICE.md` como ponteiro portátil. Não reinstalar a memória, copiar transcrições ou gerar um novo diário paralelo.

**Há acoplamento real de nomenclatura no domínio.** `ProductBoundary.FormalModules` contém `Operacao PEMSE`, `Rotinas PEMSE`, `Consulta PEMSE`. Não basta editar README ou trocar o logotipo. Investigar consumidores de serialização, UI, testes, documentação e launchers antes de alterar.

**Alguns nomes antigos são segurança, não cosmética.** Program.cs declara cookies `ClubAL.Operador.Antiforgery.Local` e `__Host-ClubAL.Operador.Antiforgery`. Trocar marca visível não é autorização para mudar cookies, DPAPI ou chaves protegidas.

**As ferramentas de validação já existem.** O script OWM exige caminhos locais de toolchain/PostgreSQL/objetos de marca e variáveis para banco e credenciais sintéticas. Ele também chama validação/migração do banco. Não executar apontando para ambientes reais.

**Não interpretar documentação antiga como limite de tooling web.** O AGENTS menciona ausência de linter canônico no contexto legado; o package.json da SPA declara `lint`, `typecheck` e testes. Usar os comandos por escopo e conferir a implementação atual.

## Lacunas a fechar na R00

Grafo real de imports e assemblies; consumidores de ProductBoundary; organização de migrations/DbContext; todos os endpoints, jobs, hubs e caches; composição frontend e clientes OpenAPI; arquivos alterados localmente; semântica já existente para produtos/módulos/entitlements; cobertura atual de Base44; licenças de dependências e assets; estado recente da CI; implantação efetiva; restrições de dados; estado de aprovações humanas e institucionais.

Nenhuma dessas lacunas autoriza inventar caminhos, números de testes, migrações ou estados. A matriz inicial identifica propostas e observações separadamente.

## Links auditáveis

As URLs completas e os intervalos consultados constam de `evidencias/repositorio_observado.json` e de [fontes](18_FONTES_E_REFERENCIAS.md). Revalidar no checkout antes de usar como instrução de edição. Não usar os SHAs como ordens de checkout/reset.
