# Dados, migrações e compatibilidade

## Regra de partida

Reorganização de código não é autorização para reconstruir banco, apagar tabelas ou refazer histórico. Antes de qualquer comando que escreva dados, identificar destino, finalidade, owner, versão e se o ambiente é exclusivamente sintético. Não imprimir connection strings ou segredos no relatório.

O script `Invoke-OwmFoundationValidation.ps1` observado chama `Invoke-OwmDatabase.ps1 -VerifyIdempotency` [E08]. Portanto, “rodar os testes existentes” não é automaticamente uma operação somente leitura.

## Inventário de contratos persistidos

Levantar migrations, snapshots EF, tabela de histórico, DbContexts, naming conventions, schemas, índices, constraints, tenancy filters/RLS, triggers, regras de auditoria e concurrency tokens. Incluir publicação/hash/schema, fila local, cache, service worker, IndexedDB, paths Windows, DPAPI, certificados, identifiers de deploy/instalação, cookies e tokens de dispositivos.

Registrar quais identidades são estáveis e quais textos são apresentação. IDs de organização/unidade/produto já usados, chaves primárias, eventos de auditoria e origem de migração não mudam por rebranding.

## Estratégia padrão: expandir, migrar de forma controlada, só depois contrair

1. Congelar um snapshot de metadados e uma fixture sintética representativa da baseline. Guardar a versão e como reproduzi-la; não copiar dados reais para testar.
2. Extrair fronteiras de código preservando schema e IDs. Reorganização de namespace por si só não deve gerar drop/recreate.
3. Quando necessário, adicionar colunas/tabelas compatíveis com backfill explícito, idempotente e scoped. Não atribuir o mesmo produto a todos os registros sem evidência da origem.
4. Gerar e revisar SQL, planos de migração e diff de snapshot. Recusar drop/rename não intencional, perda de índices ou alteração de política de acesso. Testar em PostgreSQL do laboratório, não somente em banco em memória.
5. Exercitar instalação nova, atualização de fixture antiga, repetição da migração, falha controlada e recuperação.
6. Manter contrato antigo pelo período de transição demonstrado. Contração destrutiva não está autorizada neste pacote e depende de plano posterior específico.

A documentação EF Core recomenda inspecionar e testar migrações antes da aplicação produtiva [S02]. Isso não autoriza rodá-las em produção nesta tarefa.

## Separar core sem reescrever a história

A persistência existente pode reunir tabelas de múltiplos domínios. Separar owners no código e o contrato de migração antes de extrair fisicamente projetos de persistência.

Para banco legado, preservar a cadeia histórica e um único executor responsável pelo schema. Migrations antigas com nomes ClubAL/PEMSE não são sujeira a apagar. Mantê-las no componente de compatibilidade/implantação apropriado, fora do código de negócio do core limpo quando necessário.

Para o host neutro de desenvolvimento, definir em ADR como inicializar apenas a persistência necessária à fundação sem registrar o módulo socioeducativo. Pode haver harness de teste e composição próprios; não criar outro backend operacional nem duas fontes autoritativas para os mesmos dados. Não filtrar migrations históricas arbitrariamente nem copiar um DbContext gigante para afirmar independência.

Se a separação segura da persistência exigir mais de um incremento, entregar o estado intermediário como `EXTRACAO_PARCIAL`, não como fundação limpa final. Registrar exatamente quais dependências restam e a prova que falta. Nunca compensar com `EnsureCreated` para ignorar migrations ao afirmar compatibilidade de upgrade.

## Troca de identidade visível versus identidade técnica

| Item | Tratamento inicial |
|---|---|
| Nome/logotipo exibido | Provider/configuração de apresentação; defaults compatíveis |
| Namespace C#/nome de solution | Preservar salvo necessidade da extração demonstrada |
| Assembly e nomes de tipo serializados | Avaliar binders/contratos antes de mudar |
| Cookie, issuer/audience, DataProtection application name | Não mudar por marca; plano de transição se tecnicamente necessário |
| DPAPI, certificados, chave de assinatura, pepper | Preservar; rotação não autorizada implicitamente |
| Path de dados Windows e IDs do instalador | Manter descoberta de dados/atualização; testar upgrade |
| Manifest PWA, scope, app ID, service worker/cache | Compatibilidade explícita; não limpar indiscriminadamente caches de outras instalações |
| ID de dispositivo/deploy e claims | Não reiniciar pareamento nem ampliar permissões |
| API / eventos / hashes de publicação | Adapter/versionamento e testes de consumidores |
| Migrations e auditoria histórica | Preservar identidades e causalidade |

## Rollback de código não é rollback de dados

Preferir retornar o código à versão compatível anterior, mantendo alterações aditivas seguras. `Down` não é estratégia automática. Uma restauração completa de banco pode apagar gravações posteriores; definir recuperação, janela de dados e autorização antes de usar em ambiente real.

No laboratório, provar backup → restore em destino distinto, verificar integridade e testar a aplicação restaurada. Validar também histórico append-only, relação de ownership e isolamento; igualdade de quantidade de linhas sozinha é insuficiente.

## Sessões, dispositivos e offline

Preservar a seleção efetiva de organização/unidade, versão de sessão, JIT e revogação. Rebranding não pode reaproveitar um token fora do seu produto/audience nem derrubar sessões silenciosamente. Quando uma troca de chave de cache for necessária, migrar somente o namespace proprietário e invalidar conteúdo vencido ou indevido.

Rotinas continua com o fallback offline aprovado. Isso não autoriza login administrativo offline irrestrito nem cache de dados socioeducativos restritos. A arquitetura separa disponibilidade local de autoridade de acesso.

## Aceite mínimo

Upgrade de fixture histórica e bootstrap novo passam; IDs e eventos permanecem; duas organizações não se misturam; migração repetida é segura; SQL é revisado; falha controlada é recuperável; cookie/storage/launcher legados seguem utilizáveis ou têm adaptação testada. Aprovação para usar dados reais continua separada.
