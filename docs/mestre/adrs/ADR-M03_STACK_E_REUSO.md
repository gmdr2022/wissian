# ADR-M03 — Escolha de stack por caracterização

**Status:** DECIDIDO_M0_PRIMEIRA_FATIA · **Data:** 25/09/2026 · **Escopo:** stack da fundação; piloto e segurança de runtime ainda não validados.

## Contexto
A preparação local propõe Next/Nest/TypeScript/PostgreSQL; o legado indica .NET/React/Vite/PostgreSQL. Não há aplicação Wissian implementada nesta raiz.

## Decisão e limite
Adotar **C#/.NET 10 e ASP.NET Core, EF Core com Npgsql/PostgreSQL, React/TypeScript/Vite**,
em monólito modular e repositório independente. Manter npm/lockfile no primeiro shell,
por compatibilidade com a origem, sem introduzir outro gerenciador nesta fatia.
Identidade humana parte do adapter ASP.NET Core Identity existente, com credenciais,
cookies e contexto próprios do Wissian; sem contratação de provedor nesta decisão.

A [evidência M0](../evidencias/m0-primeira-fatia.json) registra procedência, símbolos,
limites e comparação. A extração de seis arquivos de domínio compilou de forma
independente e passou 16 casos sintéticos, sem pacote externo. Código de autorização,
mapeamentos EF, fluxo modular Knowledge e login/seleção de contexto React foram
inspecionados por trechos. Banco, API e UI do Wissian não foram executados.

O ensaio preserva o namespace original apenas em `experiments/m0-reuso/origin` para
caracterização. A [regra de identidade própria](../03_ARQUITETURA_E_MODULARIDADE.md)
obriga adaptação antes de integrar ao Core. Não transplantar DI, DbContext, catálogo,
seeds ou migrations inteiros. Organização não obriga unidade/localidade legadas.

Fixar patches suportados e lockfiles na implementação M1. A origem usa SDK 10.0.302,
runtime/EF 10.0.10 e Npgsql EF 10.0.3; esses são snapshots observados, não versões
prescritas para produção. A [política oficial .NET](https://dotnet.microsoft.com/en-us/platform/support/policy)
consultada em 25/09/2026 lista .NET 10 como LTS e patch 10.0.12; o ensaio local não
instalou atualizações. O [provider Npgsql](https://www.npgsql.org/efcore/) confirma a
integração EF/PostgreSQL, não comprova isolamento do produto. Versão do servidor,
topologia de isolamento e hospedagem serão fixadas com o primeiro banco sintético.

## Alternativa considerada
| Opção | Reuso, custo e risco observados | Decisão |
|---|---|---|
| Continuar apenas no planejamento | Preserva estado, mas não reduz incerteza técnica | Superada pelo ensaio M0 |
| Copiar ClubAL completo | Menor edição inicial; traz verticais, catálogo, identidades e schema acoplados | Rejeitada pelas fronteiras do produto |
| Extrair .NET/React por capacidade | Preserva código executável e testes; exige recortar unidade/localidade, catálogo e persistência | Escolhida; menor mudança sustentada por evidência |
| Next/Nest com backend reescrito | UI React parcialmente reutilizável, mas regras e persistência C# exigem port; não foi demonstrado ganho compensatório | Alternativa forte rejeitada nesta fatia; nenhum segundo protótipo |

## Consequências e evidência
Confiança alta na escolha da família técnica para M1; média no custo de separação
completa, ainda sem medição de integração. Aceita-se esforço de extração para evitar
rewrite. Principal risco: transportar acoplamento e autoridade da origem sob nomes
novos. Mitigar por composição neutra, grafo de dependências, IDs próprios e negativos
de acesso com persistência real. Ausência de licença explícita de repositório não
autoriza distribuição pública; manter procedência e revisar dependências efetivamente
incorporadas. Não foi auditada toda a árvore de pacotes ou export Base44.

O [contrato M1](../contratos/M1_FUNDACAO.md) delimita consumidores, casos e rollback.

Validar pelo marco e pelos testes da [matriz mestre](../matrizes/requisitos-mestre.json). Status documental da decisão não é implementação nem autorização externa.

## Origem e supersessão
ADR local docs/architecture/decisions/0001-base-tecnica.md; plano base §8; v1.1 requisitos REQ29; capítulo 04.

## Reavaliação
Reabrir se a primeira extração exigir vertical no Core, impedir licença de uso,
não reproduzir build/testes ou falhar isolamento/recuperação sem solução delimitada.
Reavaliar o recorte antes da família técnica; não reabrir por novidade de framework.

## Prioridade de reuso existente

Aplicar o [contrato canônico de fragmentação e reuso](../04_ABSORCAO_CODEX_BASE44.md): localizar equivalentes antes de criar, preservar origens e justificar lacunas. WM-39/TC-21 mantêm a rastreabilidade.
