# Testes, qualidade e critérios de aceite

## Três planos de evidência separados

1. **Integridade deste pacote:** arquivos, links, JSON, referências e checksums; a ferramenta incluída verifica apenas isso.
2. **Implementação do SaaS:** compilação, testes unitários, integração com banco/API, browsers, upgrade e artefatos; precisam ser executados pelo Codex no checkout real.
3. **Uso real e autorização:** validação humana, institucional, jurídica, dispositivo físico, custo e produção; não são aprovados por testes automatizados.

O pacote entregue não executou o segundo nem o terceiro plano. Os relatos históricos do repositório são contexto, não resultados novos.

## Baseline antes de mudanças

Descobrir comandos atuais em AGENTS, CI, package.json e scripts. Verificar versões em SDK/lockfiles; não atualizar dependências para fazer uma reestruturação passar. Confirmar que scripts usam banco, diretórios, credenciais e objetos exclusivamente sintéticos.

Comandos observados, que devem ser reinspecionados antes de executar:

| Escopo | Comando ou entrada observada | Cuidado |
|---|---|---|
| OWM integrado Windows | `scripts/owm/Invoke-OwmFoundationValidation.ps1` e `-IncludeEndToEnd` | Faz bootstrap/restore/migrações; exige caminhos e variáveis locais [E08] |
| .NET | restore `ClubAL.sln --locked-mode`, build, test | Confirmar solution atual e banco dos testes |
| SPA | `npm run lint`, `typecheck`, `test`, `build`, `e2e` | Scripts reais no package.json; conferir fixtures versus API real [E07] |
| Cloudflare | `cloudflare:types:check`, `typecheck:cloudflare`, `test:cloudflare`, `cloudflare:dry-run` | Revisar scripts; dry-run não é deploy nem prova de produção |
| Legado | `py -3 clubal.py --institutional-admin --self-test` e compileall focado | Sintaxe/self-test não comprovam GUI ou instalador |
| Hygiene/segurança | validadores existentes de segurança, escopo, direção, agentes e site | Atualizar expectativas de direção sem remover garantias |
| Distribuição | builds portable/installer e `validate_distribution.ps1` | Ambiente Windows apropriado; não executar publicação |

Registrar cada resultado e distinção de baseline vs pós-mudança. Falhas existentes não podem ser ocultadas: corrigir as pertinentes; documentar as não relacionadas. Não declarar marco concluído quando o requisito crítico afetado não foi validado.

## Matriz de requisitos e testes

Os requisitos estão em `matrizes/requisitos.json`; os casos em `matrizes/casos_de_teste.json`. São especificações com estado inicial `NOT_RUN`, não suíte já implementada. Cada caso informa pré-condição, ação, expectativa, tipo de evidência e requisito coberto. O Codex deverá conectá-los a testes reais ou justificar a aplicabilidade.

## Gates de entrega

| Gate | Evidência mínima | Bloqueia o quê |
|---|---|---|
| G0 Preservação | Baseline, trabalho local/branches mapeados, decisões Base44 e nenhuma perda | Qualquer refatoração ampla |
| G1 Compilação e dependências | Builds focados e testes de grafo; core sem import transitivo de vertical | Declaração de fundação limpa |
| G2 Funcionalidade | Fluxo socioeducativo e cenário neutro no laboratório | Handoff técnico do incremento |
| G3 Acesso e privacidade | Negativos de tenant/unidade/produto/role/módulo/cache/suporte | Integração do incremento afetado |
| G4 Dados e recuperação | Upgrade sintético, idempotência, falha e restore verificáveis | Aprovação técnica de migração |
| G5 UX e legado | Browser/keyboard/reflow e contratos de fallback/distribuição relevantes | Declaração de ausência de regressões |
| G6 Reutilização | Artefato limpo + extensão técnica fictícia usando mesmo core | Fundação v1 reutilizável |
| G7 Handoff | Docs, matriz preenchida, riscos, rollback e estado de autorizações | Conclusão da reestruturação |
| G8 Uso real | Aprovações aplicáveis, ambiente/hardware e revisão profissional | Piloto real/produção; fora da autorização atual |

## Critérios transversais

- Core sem referências a negócio socioeducativo; exceções de aliases históricos delimitadas, não um wildcard.
- Ausência do módulo comprovada no grafo, build, DI, rotas, jobs, assets, seed e UI; nenhum “desligado” continua processando tarefas sensíveis.
- Dois clientes fictícios e perfis diferentes; incluir usuário com acesso a mais de uma unidade e tentar troca ilegítima de escopo.
- Fluxo ponta a ponta com API/banco reais do laboratório, além dos testes com mocks. Marcar exatamente qual técnica foi usada.
- Sem nova perda de rascunho, erro de publicação, reset de preferência, pareamento perdido ou limpeza ampla de cache.
- Integridade de publicação, versionamento, signed releases e suportes JIT preservados conforme sua implementação e gates.
- Performance comparada no mesmo ambiente/dataset. Registrar tempos, memória e tamanho de artefato quando mensuráveis; não inventar metas de milissegundos sem baseline. Regressão material exige análise, não aprovação por percentual arbitrário.
- Sem teste removido/skip para encobrir regressão. Se mudou o contrato de produto, substituir a expectativa antiga por proteção equivalente/mais forte.

## Evidência mínima por execução

Identificador, comando literal com segredos removidos, cwd/escopo, versão das ferramentas relevantes, data, duração quando obtida, exit code, resumo real, status, artefato sanitizado e distinção sintético/real do laboratório. Não somar subconjuntos sobrepostos como testes distintos.

`PASS` requer execução concluída e expectativa verificada. `NOT_RUN` não é sucesso. `BLOCKED_ENV` informa o recurso faltante. `BLOCKED_APPROVAL` não é falha do código. `NOT_APPLICABLE` exige motivo e aceite técnico coerente; não serve para pular dependência crítica.

## Encerramento sem falso “perfeito”

Declarar somente a prontidão demonstrada: por exemplo, fundação candidata validada no laboratório e produto recomposto. Sem prova independente do core, usar extração parcial. Sem browser/Windows, manter os gates correspondentes abertos. Sem avaliação humana/institucional, não anunciar produto homologado.
