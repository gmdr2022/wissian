# Wissian

> **Current development mandate — Gui, 2026-10-08:** this repository holds the canonical specifications and preserved source evidence. The active application is [gmdr2022/pixel-perfect-replica](https://github.com/gmdr2022/pixel-perfect-replica), using the approved native TypeScript/TanStack Start route. Follow the permanent [product-first delivery and consolidated migration decision](docs/mestre/03_ARQUITETURA_E_MODULARIDADE.md) and [roadmap](docs/mestre/09_ROADMAP_INTEGRADO.md): build/validate M2–M4 with active Lovable construction and Codex integration, retain portability and migrate independently before real customers. Earlier .NET/M0 preparation descriptions below are historical evidence, not the active application/runtime or a reason to restart development.

Plataforma B2B da Fultuma Tech Ltda, com Core mínimo, administração por módulos e especialidades separadas.

**Runtime and current evidence:** the application's accepted [ADR-M09](https://github.com/gmdr2022/pixel-perfect-replica/blob/main/docs/adr/ADR-M09-runtime-lovable.md) partially supersedes [ADR-M03](docs/mestre/adrs/ADR-M03_STACK_E_REUSO.md) for Rota B. Use its [README](https://github.com/gmdr2022/pixel-perfect-replica/blob/main/README.md), [roadmap](https://github.com/gmdr2022/pixel-perfect-replica/blob/main/roadmap.md) and linked evidence for current progress and open gates. Earlier .NET work remains preserved and paused; this routing does not rename repositories, restart M0, authorize a rewrite or imply milestone completion, human acceptance or production readiness.

Historical M0 baseline: the experiment in `experiments/m0-reuso/README.md` passed 16 synthetic cases. Its
[evidence](docs/mestre/evidencias/m0-primeira-fatia.json) bounds that earlier validation;
the [M1 contract](docs/mestre/contratos/M1_FUNDACAO.md) remains a reference, not a claim that the active application is still at M0.
Módulos/addons continuam [planejados](docs/mestre/03_ARQUITETURA_E_MODULARIDADE.md),
sem disponibilidade comercial presumida.

## Começar ou retomar

1. Identificar o pedido vigente, ler [AGENTS.md](AGENTS.md) e conferir Git/alterações existentes. Ler esta entrada não autoriza desenvolvimento, commit ou publicação.
2. Recover current state from the relevant repository's existing roadmap, ADRs and slice evidence, and compare it with Git/workspace state. Follow [chapter 12](docs/mestre/12_CONTINUIDADE_E_HANDOFF.md); C7/Obsidian is not required.
3. Selecionar **somente a rota pertinente**. Ler os arquivos da rota e a seção/caso necessário; consultar as fontes históricas apenas quando houver lacuna concreta.
4. Gui autorizou M0 em 25/09/2026 e o início foi caracterizado. Reutilizar a evidência existente; avançar por marco/fatia conforme o pedido vigente. Não reiniciar auditoria global nem criar scaffold por preferência.

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
| Existing roadmap, ADRs and slice evidence in the relevant repository | Canonical engineering continuity, current slice, verified results, blockers and next step; no duplicate session diary |

Backups, ZIPs e relatórios de entregas anteriores foram arquivados fora do checkout. Não são dependências do desenvolvimento. As configurações remanescentes na raiz protegem Git, encoding e fontes; não escolhem stack. Os exemplos antigos de ambiente/Docker/formatação continuam no acervo, sem ativação automática.

As buscas comuns com `rg` excluem o acervo histórico por `.ignore`. Para uma consulta histórica deliberada, usar `rg --no-ignore` no **arquivo ou coleção específica** apontada pela matriz. Não repetir inventário global a cada tarefa. O [índice](docs/mestre/00_INDICE.md) localiza assuntos e IDs diretamente.

## Verificar alterações documentais

```powershell
python -B docs/mestre/ferramentas/validar_pacote.py --root . --workspace --json
python -B docs/mestre/ferramentas/testar_validacao.py
```

O primeiro comando verifica somente o escopo documental governado, permitindo evolução do código fora dele. Atualização do manifesto e recuperação estão no [procedimento de manutenção](docs/mestre/14_INSTALACAO_E_RESTAURACAO.md). Executar a validação após mudanças documentais; repetir a suíte quando o verificador ou seus contratos forem afetados. Não rodar tudo em cada consulta.

Python é ferramenta documental. Os comandos do ensaio de domínio estão em seu README;
current application build/test commands belong to the active application's README. Testes de pacote e caracterização
isolada não comprovam funcionamento ou segurança do SaaS.
