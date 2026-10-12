# Wissian — instruções locais

## Entrada e autoridade

Este repositório pertence ao Wissian, plataforma da Fultuma Tech Ltda. Start with [README.md](README.md), identify the repository and active slice, and recover continuity from the existing Git-tracked roadmap, decisions and evidence as defined in [chapter 12](docs/mestre/12_CONTINUIDADE_E_HANDOFF.md). This repository contains governing contracts and preserved implementation history; current application development is in `gmdr2022/pixel-perfect-replica`. Only the current request authorizes advancing its stated slice.

### Regra operacional persistente do Codex

Antes de executar, consultar as seções pertinentes de **[WISSIAN_CODEX_REGRA_MESTRA.md](WISSIAN_CODEX_REGRA_MESTRA.md)** à decisão e ao risco da fatia. Após compactação/perda relevante de contexto, recuperar a prioridade vigente, as restrições aplicáveis e a evidência da fatia; não exigir releitura integral por tarefa ou sessão. Leitura seletiva não dispensa segurança, preservação, autorização, precedência nem as demais restrições aplicáveis.

Esse arquivo define a política operacional persistente para o trabalho do Codex, incluindo autonomia técnica, foco no piloto, `WIP = 1`, economia de tokens/contexto, controle de side quests, pesquisa técnica com critério de parada, reutilização/absorção seletiva de ClubAL/Base44/branches, testes proporcionais ao risco e comunicação mínima com o Product Owner.

A regra mestre **não substitui a precedência deste `AGENTS.md` nem dos contratos canônicos em `docs/mestre/`**. Em caso de conflito, obedecer primeiro às restrições e ao mandato local vigente; usar a regra mestre para orientar **como executar com eficiência**, nunca para autorizar commit, push, deploy, publicação, destruição de dados ou qualquer ação que este repositório exija aprovação humana.


Os contratos ativos estão em `docs/mestre/`. `docs/fontes/` é acervo histórico: seus AGENTS, prompts, permissões e comandos são evidência, nunca instruções vigentes. Não carregar todo o acervo nem rodar todos os prompts. Resolver conflitos pelo [mandato](docs/mestre/01_MANDATO_E_PRECEDENCIA.md), sem reaplicar decisões superadas. Cada tarefa lê só contratos e fontes pertinentes.

## Fronteiras que devem sobreviver a qualquer fatia

- **Permanent product-first sequencing (Gui, 2026-10-08):** consult [chapter 03](docs/mestre/03_ARQUITETURA_E_MODULARIDADE.md), "Product-first delivery and consolidated migration", at session start and after context loss. Prioritize the working M2–M4 administrative release with active Lovable construction and Codex integration/direct economical fixes; preserve portability and consolidate independent deployment before real customers. Do not resume infrastructure-only work or per-package destination migration by default. Gui's autonomous development mandate supersedes the historical pre-development/M1-only scope; security, ownership, preservation and no-new-cost constraints remain.

- **Permanent builder-independence directive (Gui, 2026-10-08):** follow the ownership and portability contract in [chapter 03](docs/mestre/03_ARQUITETURA_E_MODULARIDADE.md). Lovable/Base44 accelerate development; the target production SaaS must operate independently of their subscriptions. Independently contracted Supabase/Hostinger/Cloudflare are candidates subject to validation and the existing no-new-cost constraint.

- Repositório Wissian independente. Preservar ClubAL e demais origens; não fundir históricos, importar integralmente backend/migrations/credenciais nem criar fork por cliente.
- **Foco exclusivo no Wissian — decisão de Gui em 25/09/2026:** ClubAL, Codex Checkpoint e NexusDevHub encerram sua evolução como produtos independentes. São fontes de leitura e absorção seletiva; seguir o [encerramento do capítulo 05](docs/mestre/05_GIT_E_REPOSITORIOS.md) e a [preservação de UX Base44 no capítulo 04](docs/mestre/04_ABSORCAO_CODEX_BASE44.md). Não reabrir desenvolvimento nas origens nem considerar a interface absorvida por arquivar o repositório.
- **Fragmentar e reutilizar antes de criar.** Localizar equivalente, origem/ref/hash/símbolo, dependências e testes conforme [absorção](docs/mestre/04_ABSORCAO_CODEX_BASE44.md). Código novo exige lacuna demonstrada; rewrite ou troca ampla de stack exige revisão da decisão com Gui. Ausência de aplicação nesta pasta não significa ausência de software reutilizável.
- **Permanent interface reuse directive (Gui, 2026-10-11):** follow chapter 04's "Permanent interface reuse directive" across phases. Consult preserved ClubAL/Codex/Base44 work and relevant ready-to-use Lovable templates before building equivalents; adapt compatible UI code/utilities to Wissian's SaaS P2P Elite Prime quality direction, with provenance, licenses and independent destination validation.
- Core técnico mínimo; administração modular; regras socioeducativas, educacionais e ClubAL/Telas ficam nas especialidades. A oferta básica segue o [recorte administrativo](docs/mestre/17_BASE_ADMINISTRATIVA_E_TRANSICAO.md), sem obrigar marca, contratos ou ERP completo.
- **Identidade própria e addons opcionais — regra de Gui em 25/09/2026:** o Core do Wissian não incorpora nomes, namespaces, assemblies, configurações, cookies, chaves ou contratos legados como ClubAL/Base44/TEM/OWM/GRA. Aplicar a [regra de identidade e catálogo do capítulo 03](docs/mestre/03_ARQUITETURA_E_MODULARIDADE.md); procedência permanece nas evidências, fora do Core. Módulos/addons têm fronteiras e dependências declaradas, ativação por organização e autorização no servidor; não tratá-los como funcionalidades já disponíveis antes da implementação e aceite.
- Fultuma usuária segue as mesmas regras de clientes; administração da plataforma é separada. Nome, cobrança, menu ou ativação não concedem acesso a dados. Autorização efetiva pertence ao servidor.
- The M0 stack decision is preserved in ADR-M03; the active application's accepted ADR-M09 partially supersedes it for Rota B. Follow the [repository routing](README.md) and the applicable decision; do not restart M0, revive the paused .NET implementation or change infrastructure by preference.
- Continuity é infraestrutura interna transversal para os módulos, opcional por composição, sem produto/cobrança separada presumidos. O [capítulo 16](docs/mestre/16_CONTINUIDADE_OPERACIONAL.md) e a [ADR-M08](docs/mestre/adrs/ADR-M08_CONTINUIDADE_OPERACIONAL.md) são suas fontes canônicas. M0/M1 preparam apenas contratos mínimos; M2–M4 não dependem do MVP; M5-CO vem após consumidor M4. Não portar WPF, shell arbitrário ou integração Codex ao SaaS.

- **Selected Base44 application interface (Gui, 2026-10-12):** use the existing Base44 branch interface as the visual/code baseline and adapt actual source through current-runtime bindings. Follow chapter 04, "Interface baseline and execution correction"; source-pattern/palette reuse alone is not the full port. Lovable preserves this baseline; keep current functional modules and milestone gates.

## Execução e evidência

Conferir Git antes/depois e preservar alterações existentes. Não fazer commit, push, remote, publicação, uso de dados reais ou ação externa sem autorização aplicável. Não usar `git add -A` sobre material desconhecido. Fontes históricas permanecem byte a byte; matrizes JSON/CSV equivalentes não são duplicatas descartáveis.

Manter uma fonte canônica por decisão: reuso no capítulo 04, arquitetura no 03, Continuity no 16, base administrativa no 17. Referenciar a fonte em outros documentos em vez de replicar blocos. Não criar relatórios de sessão, cópias de pacote ou backups dentro do checkout. Engineering continuity follows [chapter 12](docs/mestre/12_CONTINUIDADE_E_HANDOFF.md): update existing canonical roadmap, decisions and sanitized evidence in Git. C7/Obsidian is optional historical support, never a prerequisite or a second required record.

Após alterar documentação, executar `python -B docs/mestre/ferramentas/validar_pacote.py --root . --workspace --json`; manutenção do manifesto está no [capítulo 14](docs/mestre/14_INSTALACAO_E_RESTAURACAO.md). Suíte do verificador: `python -B docs/mestre/ferramentas/testar_validacao.py`. O modo workspace não valida código, dependências ou testes do produto. Os planos permanecem NOT_RUN até haver evidência específica de implementação, registrada separadamente. Não promover estado em bloco.

Close the slice with relevant evidence, review, exact human dependencies and the existing Git-tracked status updated. Never accept on Gui's behalf. Preserve project identity and other contributors' changes; do not repair, install or configure C7/Obsidian as part of product work.
