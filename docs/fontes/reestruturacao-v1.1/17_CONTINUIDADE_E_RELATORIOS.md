# Continuidade, relatórios e economia de contexto

## Respeitar o sistema atual

O AGENTS da integração aponta C7/Obsidian, projeto `clubal`, nota `continuity`, e o ponteiro portátil `ops/runtime/ACTIVE_SLICE.md` [E02/E03]. Ler o protocolo vigente do ambiente e usar as ferramentas já configuradas. Este pacote não instala C7, não cria um cofre novo e não autoriza editar seu repositório separado.

Não renomear o ID `clubal` do cofre por rebranding. Um alias de apresentação pode ser documentado, mas mudar a identidade da memória exige migração explícita e validação posterior. Não copiar conversas integrais ou raciocínio privado para notas.

## O que vive onde

- C7/continuidade existente: checkpoints de engenharia, escopo, evidência resumida, riscos e próximo passo.
- ACTIVE_SLICE: apenas ponteiro portátil e mudanças materiais de direção/gates; não um diário crescente.
- Repositório: contratos, ADRs, testes, instruções necessárias para construir/operar sem depender do cofre e documentação usada por CI/pacote.
- Artefatos locais gitignored: logs/screenhots sintéticos e evidências detalhadas, respeitando os caminhos convencionados pelo checkout.

Os modelos deste pacote são formatos de preenchimento, não ordens para criar todos os arquivos no repositório. Integrar com artefatos equivalentes existentes, evitando duplicação.

## Fallback se C7 estiver indisponível

Tentar o fallback CLI definido nas instruções locais, sem inventar API/comando. Se ainda indisponível, usar o ponteiro e fontes executáveis para continuar trabalho verificável. Registrar a limitação em uma nota local única gitignored no diretório aprovado; reconciliar com C7 depois, sem duas linhas de verdade. Não atribuir ao cofre informações que não foram lidas.

## Relatório de incremento

1. Objetivo e requisito(s) coberto(s).
2. Baseline, branch, HEAD e trabalho pré-existente preservado.
3. Caminhos alterados e intenção; aliases/migrations/contratos afetados.
4. Comandos realmente executados, exit codes e tipo de teste.
5. Resultados `PASS/FAIL/NOT_RUN/BLOCKED_ENV/BLOCKED_APPROVAL/NOT_APPLICABLE`, com evidências sanitizadas.
6. Riscos, limitação de cobertura e rollback.
7. Próximo incremento, pré-condições e lista de decisões humanas indispensáveis.

Não listar centenas de logs no chat; fornecer resumo e caminho da evidência. Não dizer “testado” para testes somente planejados ou “homologado” para simulação.

## Relatório final

Concluir com mapa antes/depois; estado do core; composição socioeducativa; reserva ClubAL; preservação Base44/legado; migrações e compatibilidade; testes e requisitos; desempenho observado; segurança; recuperação; licenças/naming pendentes; custo adicional autorizado; comandos de reprodução e proposta de integração.

Rotular separadamente: implementação local, validação técnica, revisão humana, homologação institucional, produção. Não somar testes sobrepostos nem afirmar ausência universal de bugs.

## Retomada eficiente

Ler prompt de retomada, continuidade, diff atual e apenas contratos do próximo incremento. Carregar módulos específicos conforme o trabalho. Não rerequisitar confirmação para decisões técnicas já autorizadas. Não continuar um roadmap antigo que conflite com esta prioridade sem reconciliar explicitamente.

Ao atingir limite de contexto ou bloqueio real, registrar último passo concluído, estado de processos, evidências, pendências e comando/teste a retomar. Nunca marcar gate como aprovado para liberar continuação automática.
