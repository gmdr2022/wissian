# Wissian

Plataforma B2B da Fultuma Tech Ltda, com Core mínimo, administração por módulos e especialidades separadas. **Estado: M0 caracterizado; há um ensaio executável de reuso, ainda não uma aplicação SaaS.** Stack decidida por evidência na [ADR-M03](docs/mestre/adrs/ADR-M03_STACK_E_REUSO.md): .NET/ASP.NET Core, EF/PostgreSQL e React/TypeScript/Vite. O código existente será fragmentado e adaptado conforme o [contrato de absorção](docs/mestre/04_ABSORCAO_CODEX_BASE44.md), preservando a identidade própria do Wissian.

O ensaio em `experiments/m0-reuso/README.md` passou 16 casos sintéticos. A
[evidência da fatia](docs/mestre/evidencias/m0-primeira-fatia.json) delimita o que foi
inspecionado e executado. Próxima frente: [fundação M1](docs/mestre/contratos/M1_FUNDACAO.md).
Módulos/addons continuam [planejados](docs/mestre/03_ARQUITETURA_E_MODULARIDADE.md),
sem disponibilidade comercial presumida.

## Começar ou retomar

1. Identificar o pedido vigente, ler [AGENTS.md](AGENTS.md) e conferir Git/alterações existentes. Ler esta entrada não autoriza desenvolvimento, commit ou publicação.
2. Recuperar a nota `continuity` do projeto C7 `wissian` e confrontá-la com o workspace. Sem C7, esta entrada e os contratos abaixo continuam suficientes para orientar o trabalho seguro.
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
| C7, projeto `wissian` | Continuidade, decisões de sessão, evidências e próximo passo; sem diários duplicados aqui |

Backups, ZIPs e relatórios de entregas anteriores foram arquivados fora do checkout. Não são dependências do desenvolvimento. As configurações remanescentes na raiz protegem Git, encoding e fontes; não escolhem stack. Os exemplos antigos de ambiente/Docker/formatação continuam no acervo, sem ativação automática.

As buscas comuns com `rg` excluem o acervo histórico por `.ignore`. Para uma consulta histórica deliberada, usar `rg --no-ignore` no **arquivo ou coleção específica** apontada pela matriz. Não repetir inventário global a cada tarefa. O [índice](docs/mestre/00_INDICE.md) localiza assuntos e IDs diretamente.

## Verificar alterações documentais

```powershell
python -B docs/mestre/ferramentas/validar_pacote.py --root . --workspace --json
python -B docs/mestre/ferramentas/testar_validacao.py
```

O primeiro comando verifica somente o escopo documental governado, permitindo evolução do código fora dele. Atualização do manifesto e recuperação estão no [procedimento de manutenção](docs/mestre/14_INSTALACAO_E_RESTAURACAO.md). Executar a validação após mudanças documentais; repetir a suíte quando o verificador ou seus contratos forem afetados. Não rodar tudo em cada consulta.

Python é ferramenta documental. Os comandos do ensaio de domínio estão em seu README;
build/teste de aplicação serão definidos em M1. Testes de pacote e caracterização
isolada não comprovam funcionamento ou segurança do SaaS.
