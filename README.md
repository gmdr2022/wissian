# Wissian

Plataforma B2B da Fultuma Tech Ltda, com Core mínimo, administração por módulos e especialidades separadas. **Estado: desenvolvimento M1 iniciado; o piloto ainda não está disponível.** Stack decidida por evidência na [ADR-M03](docs/mestre/adrs/ADR-M03_STACK_E_REUSO.md): .NET/ASP.NET Core, EF/PostgreSQL e React/TypeScript/Vite. O código existente será fragmentado e adaptado conforme o [contrato de absorção](docs/mestre/04_ABSORCAO_CODEX_BASE44.md), preservando a identidade própria do Wissian.

O ensaio em `experiments/m0-reuso/README.md` passou 16 casos sintéticos. A
[evidência da fatia](docs/mestre/evidencias/m0-primeira-fatia.json) delimita o que foi
inspecionado e executado. Frente ativa: [fundação M1](docs/mestre/contratos/M1_FUNDACAO.md).
Módulos/addons continuam [planejados](docs/mestre/03_ARQUITETURA_E_MODULARIDADE.md),
sem disponibilidade comercial presumida.

## Começar ou retomar

1. Identificar o pedido vigente, ler [AGENTS.md](AGENTS.md) e conferir Git/alterações existentes. Ler esta entrada não autoriza desenvolvimento, commit ou publicação.
2. Recuperar a nota `continuity` do projeto C7 `wissian` e confrontá-la com o workspace. Sem C7, esta entrada e os contratos abaixo continuam suficientes para orientar o trabalho seguro.
3. Selecionar **somente a rota pertinente**. Ler os arquivos da rota e a seção/caso necessário; consultar as fontes históricas apenas quando houver lacuna concreta.
4. Seguir o [avanço autônomo autorizado](docs/mestre/09_ROADMAP_INTEGRADO.md#avanço-autônomo-autorizado): concluir, verificar e registrar a fatia ativa, então iniciar a próxima. Reutilizar a evidência M0; não reiniciar auditoria global nem criar scaffold por preferência.

| Trabalho autorizado | Leitura dirigida |
|---|---|
| Descoberta e primeira fatia M0 | [Absorção](docs/mestre/04_ABSORCAO_CODEX_BASE44.md), [estado e lacunas](docs/mestre/02_ESTADO_FONTES_E_LACUNAS.md), [ADR da stack](docs/mestre/adrs/ADR-M03_STACK_E_REUSO.md) e [prompt M0](docs/mestre/prompts/01_AUDITAR_FONTES.md) |
| Fundação M1 | [Arquitetura](docs/mestre/03_ARQUITETURA_E_MODULARIDADE.md), [segurança](docs/mestre/07_SEGURANCA_E_ISOLAMENTO.md), [dados](docs/mestre/06_DADOS_MIGRACOES_COMPATIBILIDADE.md) e [prompt da fundação](docs/mestre/prompts/02_IMPLEMENTAR_FUNDACAO.md) |
| Primeiro ciclo administrativo M2–M4 | [Recorte da base](docs/mestre/17_BASE_ADMINISTRATIVA_E_TRANSICAO.md), [UX](docs/mestre/08_UX_MARCA_E_QUALIDADE.md), [aceite](docs/mestre/10_TESTES_E_ACEITE.md) e os casos do fluxo |
| Absorver outra capacidade | [Ficha](docs/mestre/modelos/FICHA_ABSORCAO.md), [matriz de migração](docs/mestre/matrizes/migracao.json) e [prompt de absorção](docs/mestre/prompts/03_ABSORVER_CAPACIDADE.md) |
| Continuity M5-CO, quando pertinente | [Contrato específico](docs/mestre/16_CONTINUIDADE_OPERACIONAL.md) e [prompt M5-CO](docs/mestre/prompts/06_IMPLEMENTAR_CONTINUITY.md) |
| Retomar ou revisar uma fatia | [Retomada](docs/mestre/prompts/04_RETOMAR.md), [revisão](docs/mestre/prompts/05_REVISAR.md) e evidências atuais da fatia |

O [roadmap](docs/mestre/09_ROADMAP_INTEGRADO.md) define os marcos; o [mandato](docs/mestre/01_MANDATO_E_PRECEDENCIA.md) resolve conflitos de fontes. A primeira oferta administrativa atende Fultuma e demais organizações com o mesmo produto: financeiro, notas registradas e apoio ao fechamento contábil. Não exige concluir o catálogo inteiro, marca, contratos ou ERP completo.

## Organização e fontes de verdade

| Local | Finalidade |
|---|---|
| `README.md` / `AGENTS.md` | Entrada de trabalho / regras locais |
| [docs/mestre](docs/mestre/00_INDICE.md) | Contratos vigentes, ADRs, matrizes, exemplos, prompts e ferramentas documentais |
| [docs/fontes](docs/fontes/README.md) | 75 fontes originais, preservadas por hash; consulta sob demanda, sem instruções executáveis vigentes |
| C7, projeto `wissian` | Continuidade, decisões de sessão, evidências e próximo passo; sem diários duplicados aqui |

Backups, ZIPs e relatórios de entregas anteriores foram arquivados fora do checkout. Não são dependências do desenvolvimento. As configurações remanescentes na raiz protegem Git, encoding e fontes; não escolhem stack. Os exemplos antigos de ambiente/Docker/formatação continuam no acervo, sem ativação automática.

As buscas comuns com `rg` excluem o acervo histórico por `.ignore`. Para uma consulta histórica deliberada, usar `rg --no-ignore` no **arquivo ou coleção específica** apontada pela matriz. Não repetir inventário global a cada tarefa. O [índice](docs/mestre/00_INDICE.md) localiza assuntos e IDs diretamente.

## Verificar alterações documentais

```powershell
python -B docs/mestre/ferramentas/validar_pacote.py --root . --workspace --json
python -B docs/mestre/ferramentas/testar_validacao.py
```

O primeiro comando verifica somente o escopo documental governado, permitindo evolução do código fora dele. Atualização do manifesto e recuperação estão no [procedimento de manutenção](docs/mestre/14_INSTALACAO_E_RESTAURACAO.md). Executar a validação após mudanças documentais; repetir a suíte quando o verificador ou seus contratos forem afetados. Não rodar tudo em cada consulta.

## Executar a fundação M1

Pré-requisito: SDK .NET **10.0.401** com runtime **10.0.12**, fixados em
`global.json` e `Directory.Build.props`. O executável `dotnet` deve resolver esse SDK;
uma instalação isolada pode ser usada sem substituir SDKs dos projetos de origem.
O Core não tem pacote NuGet externo. A persistência usa EF/Identity 10.0.12 e
Npgsql EF 10.0.3, com lockfiles. A solução não inclui o ensaio M0.

```powershell
dotnet restore Wissian.slnx --locked-mode --disable-parallel
dotnet run --project tests/Wissian.Foundation.Tests --no-restore --disable-build-servers -p:BuildInParallel=false -p:UseSharedCompilation=false
```

Os 25 testes da fundação incluem a composição neutra em HTTP/loopback. A extensão
técnica existe somente nos testes; não faz parte do host executável. O host real
oferece identidade, sessão e o shell React próprio descrito abaixo.

Para os 16 testes de persistência, fornecer os binários PostgreSQL **18.6**:

```powershell
./scripts/Test-Postgres.ps1 -PostgresBin /caminho/para/pgsql/bin
./scripts/Test-Postgres.ps1 -PostgresBin /caminho/para/pgsql/bin -Suite Api
```

O launcher Windows cria um cluster novo em `.local/postgres-tests`, escuta somente
loopback, gera credenciais sintéticas distintas e encerra sua instância no final.
Conservar a pasta local para diagnóstico; ela é ignorada pelo Git. Não fornecer
conexão de banco existente. Não há serviço instalado nem banco de origem reutilizado.
SDK isolado pode ser indicado por `-Dotnet /caminho/para/dotnet.exe`.

A [evidência S2](docs/mestre/evidencias/m1-fundacao-s2.json) delimita isolamento/RLS,
concorrência e migração comprovados em PostgreSQL real. Runtime e migrador têm
senhas/privilégios distintos. A [evidência S3A](docs/mestre/evidencias/m1-fundacao-s3a.json)
registra a base de identidade/autorização HTTP. A [evidência S3B](docs/mestre/evidencias/m1-fundacao-s3b.json)
amplia a suíte para **42 testes de API** com HTTPS local, recuperação/ativação
assistida e idempotência. Esses testes não são a matriz histórica de 42 casos do
produto, que continua como plano. Metadados globais de autenticação não são módulos
de organização. RLS reforça o escopo; não autentica o usuário nem substitui as
permissões e regras do módulo.

Gerar migrations com a ferramenta local (`dotnet tool restore`,
`dotnet ef migrations add <Nome> --project src/Wissian.Persistence`). Não modificar
migration já aplicada; `Down` destrutivo é recusado. Instalação, repetição, falha
transacional e retomada em outro banco sintético são exercitadas pela suíte.
O runtime não executa migrations nem recebe a credencial do migrador.

### Interface e jornadas em navegador

A interface usa Node **24 LTS** (CI fixado em `.node-version`; mínimo 24.16.0),
React 19.2.7, TypeScript 6.0.2 e Vite 8.1.1,
com versões/lockfile próprios. O build de produção é servido pelo mesmo host HTTPS
da API; não há CORS amplo, serviço externo, token de sessão em JavaScript nem SDK
de origem. Instalar dependências sem scripts de terceiros:

```powershell
npm --prefix src/Wissian.Web ci --ignore-scripts --no-fund
npm --prefix src/Wissian.Web run build
dotnet build Wissian.slnx --no-restore --disable-build-servers -m:1
```

O build .NET copia a interface para `wwwroot` junto do executável/publicação.
`Wissian__WebRoot` pode apontar para um build em caminho absoluto; configuração
explícita inválida impede a inicialização. Sem build/configuração, a composição
somente API continua disponível. `/`, `/access` e `/m/...` servem o shell; rotas
desconhecidas de `/api` continuam `404`. CSP, `no-store`, `no-referrer` e HTTPS
protegem o fluxo. Chaves, banco e certificado seguem as exigências de operação.

A entrada permite login, MFA, cancelamento do desafio, escolha de organização,
saída confirmada, orientação de acesso assistido e consumo de link de ativação ou
recuperação. Trocar organização desmonta o conteúdo anterior; outras abas recebem
apenas um aviso para revalidar no servidor. Ao perder sessão/permissão ou conexão
durante a revalidação, o conteúdo é fechado. Nenhum módulo de negócio é anunciado
antes de existir: views entram por composição compilada e capacidades devolvidas
pelo servidor, que continua autorizando cada chamada.

O laboratório de navegador exige Edge instalado e usa apenas dados sintéticos:

```powershell
npm --prefix src/Wissian.Web run build:fixture
./scripts/Test-Postgres.ps1 -PostgresBin /caminho/para/pgsql/bin -Suite Browser
```

A fixture de UI fica em `.local/web-fixture`, separada do produto. O runner usa a
API/Kestrel, PostgreSQL real e Playwright/Edge, sem instalar confiança no certificado
ou criar endpoint administrativo de teste. Credenciais sintéticas passam somente
ao processo filho controlado. Screenshots ficam em `.local/browser-tests`; bancos,
chaves, links e screenshots não são publicados pelo CI. Ver
[evidência S4](docs/mestre/evidencias/m1-fundacao-s4.json) para cobertura e limites.
Aceite de Gui e uso com tecnologia assistiva permanecem separados dos testes.

### Configuração e contrato HTTP atual

O host exige `Wissian__Database` com o papel `wissian_runtime` e
`Wissian__KeyDirectory` com caminho absoluto exclusivo, mantidos fora do Git.
Também exige listener HTTPS/certificado configurado no Kestrel. Não há fallback
para autenticação sem banco ou tráfego de identidade em HTTP. O bootstrap real
de operação será entregue separadamente; os testes montam tudo com dados sintéticos.
O host recusa schema incompatível ou credencial privilegiada antes de atender.

Chaves DataProtection pertencem ao Wissian e são persistidas com proteção DPAPI no
Windows. Outro sistema operacional requer configuração explícita de proteção;
chaves de origem e certificados de teste não servem para operação. A suíte prova
reinício com as mesmas chaves e rejeição de cookie emitido por outro conjunto.

| Endpoint | Contrato |
|---|---|
| `GET /api/identity/csrf` | Emite token para `X-CSRF-TOKEN`; renovar depois do login |
| `POST /api/identity/login` | Identificador/senha; `202 mfa_required` quando houver autenticador habilitado |
| `POST /api/identity/mfa` | Código do autenticador; desafio válido por cinco minutos e vinculado à versão da credencial |
| `GET /api/identity/session` | Contexto e organizações atuais; capacidades calculadas no servidor |
| `POST /api/identity/context` | Organização, vínculo, versão esperada e `commandId` (UUID); recibo idempotente ou `409` em conflito |
| `POST /api/identity/cancel-mfa` | Encerra apenas o desafio MFA pendente; exige CSRF |
| `POST /api/identity/logout` | Revoga a sessão persistida e elimina o cookie |
| `GET /api/identity/access-help` | Explica o fluxo assistido, sem afirmar envio de e-mail |
| `POST /api/identity/activate` | `userId`, `token`, `newPassword`; ativa apenas identidade previamente provisionada, sem conceder vínculo |
| `POST /api/identity/reset-password` | Mesmo corpo, token com finalidade própria; troca senha e revoga sessões atomicamente |

Todos os POST exigem CSRF. Login/MFA têm limite por IP e lockout por identidade;
campos JSON desconhecidos são recusados, corpo limitado a 32 KiB. Cookies próprios
têm `Secure`, `HttpOnly`, `SameSite=Strict` e sessão absoluta de oito horas, sem
renovação automática. Permissões e organização não são confiadas ao navegador.
Alterações rastreadas de ativação do usuário via `WissianDbContext` invalidam tickets e desafios;
operações futuras de gestão de identidade devem usar esse caminho e a API Identity,
carregando o usuário atual antes de mudar seu estado, sem update destacado, bulk ou
SQL avulso para alterar credenciais/estado.

Login, seleção de contexto, logout e invalidação identificada geram auditoria
persistida; falhas anônimas geram log estruturado com correlação, sem identificador
submetido nem senha. Autorização para comandos de negócio deve ocorrer na mesma
transação do efeito; a checagem `CanReadAsync` é exclusiva de endpoints de leitura.
Repetir o mesmo comando de seleção devolve o mesmo recibo somente com autoridade
atual e enquanto ele ainda representa o contexto vigente. Reusar a chave com outro
corpo, ou tentar restaurar contexto superado, retorna conflito sem efeito. Recibo,
alteração e auditoria pertencem à mesma transação; runtime não reescreve recibos.

### Ativação e recuperação assistidas no laboratório

Links Identity têm finalidade, usuário e versão de credencial vinculados, com
validade de uma hora. Consumo válido altera a credencial, inutiliza os links
anteriores e exige novo login. MFA permanece habilitado; recuperação de senha não
remove o autenticador nem reativa conta suspensa. Falha antes do commit desfaz
senha, versão da credencial, revogações e auditoria em conjunto.

O operador local atende **somente banco sintético novo em loopback**, com nome
`wissian_*_synthetic`, papel `wissian_runtime` e as mesmas chaves DPAPI do host.
`Wissian__Database` e `Wissian__KeyDirectory` devem estar configurados fora do Git.
O comando abaixo é executado a partir da raiz do workspace; o usuário já deve estar
provisionado no laboratório. Para ativação, ele deve estar inativo, sem senha e
sem e-mail confirmado. O operador não cria organizações, usuários ou vínculos.

```powershell
dotnet run --project src/Wissian.Operator --no-restore -- activation <user-id> --synthetic
dotnet run --project src/Wissian.Operator --no-restore -- recovery <user-id> --synthetic
```

O resultado é o caminho de um JSON em `.local/account-actions/<id>/`, com acesso
restrito ao usuário atual do Windows. O token não aparece na linha de comando nem
no stdout. O arquivo é uma credencial temporária: somente o destinatário deve
recebê-lo por canal aprovado, após conferência de identidade. Não há endpoint
público de emissão, envio de e-mail ou emissão para contas reais neste estágio.
Provisionamento operacional, canal real de entrega e recuperação de MFA perdido
permanecem gates de operação. O JSON inclui `relativeUrl`, que deve ser resolvido
somente contra a origem HTTPS confiável do Wissian. O token segue no fragmento de
`/access`, é consumido pela UI e removido da URL, sem query, armazenamento local ou
envio automático a terceiros. Fechar/recarregar a tela exige reabrir o link.

Python é ferramenta documental. Os comandos do ensaio M0 estão em seu README.
Testes de pacote e caracterização isolada não comprovam funcionamento do SaaS.
