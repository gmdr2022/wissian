# Operação, observabilidade, entrega e recuperação

Direção proposta, dependente da stack e do ambiente decididos em M0. Reusar o conhecimento de [operação v1.1](../fontes/reestruturacao-v1.1/14_OPERACAO_IMPLANTACAO_CUSTOS.md), [rollback v1.1](../fontes/reestruturacao-v1.1/15_RISCOS_E_ROLLBACK.md) e GRA; não transplantar agentes, permissões elevadas ou automações remotas do ClubAL.

## Ambientes e distribuição

Separar desenvolvimento sintético, teste/integração e piloto/produção autorizados por configuração e credenciais. Nunca apontar testes a banco real por fallback. Confirmar ambiente ao iniciar jobs/migrations. Segredos não entram em build, imagem, seed, log, template ou ZIP documental. Configuração obrigatória ausente interrompe com diagnóstico seguro.

Um release deve identificar origem, versão de app, schema, contratos, módulos e dependências; lockfiles e licença/SBOM proporcional quando houver software distribuído. CI futura executa os gates pertinentes e registra artefatos sanitizados. Não criar deploy automático, ambientes públicos ou contas nesta preparação. Proteção de branches, identidade de publicação e política de releases exigem configuração real autorizada.

## Sinais e limites

| Sinal | Finalidade | Limite de privacidade/custo |
|---|---|---|
| Logs estruturados | Falhas, causa, correlação, módulo e operação | Sem corpo de documento, token, senha ou conteúdo institucional; retenção controlada |
| Métricas | Latência/erros, fila, retries, jobs travados, armazenamento, importação e fechamento | Evitar cardinalidade explosiva; IDs de usuário/tenant não viram labels indiscriminados |
| Traces | Relação entre solicitação/job/banco e integração | Contexto mínimo e sanitizado; amostragem e custo conhecidos |
| Auditoria | Quem fez o quê e resultado em operações relevantes | Acesso e retenção próprios; não confundir telemetria operacional com prova de auditoria |
| Saúde | Liveness do processo, readiness de dependências e versão compatível | Resposta pública não revela topologia/segredos; degradação não aceita operações sem autoridade |

Correlacionar erro de usuário com referência segura de atendimento. Alertas devem indicar ação, responsável e limiar baseado em baseline; não criar monitor barulhento sem processo de resposta. Limitar upload, busca, exportação e importação por tenant e observar abuso/noisy neighbor sem prometer isolamento de recursos inexistente.

## Runbook de incidente

1. Identificar release/ambiente/tenants afetados, sem coletar dados desnecessários.
2. Interromper a capacidade afetada por controle autorizado, preservando evidência e acesso mínimo para recuperação.
3. Classificar falha de aplicação, dependência, schema, autoridade ou dados; verificar último estado consistente.
4. Escolher rollback de aplicativo compatível, correção adiante ou restauração em ambiente separado; não executar down migration destrutiva por padrão.
5. Revalidar isolamento, integridade, jobs pendentes e pares de versão; reconciliar operações com resposta ambígua por idempotência, não repeti-las cegamente.
6. Comunicar externamente apenas com autorização e processo vigente; registrar impacto observado, sem declarar perda inexistente ou inexistência de perda sem prova.

## Backup e restore de produto futuro

Definir escopo de banco, blobs, configuração não secreta, referências a segredos, auditoria e versões. Criptografia, acesso, retenção e localização são decisões do ambiente. Backup consistente precisa de ligação verificável entre registro e arquivo. Medir RPO/RTO com exercício sintético e definir objetivos compatíveis com uso/custo; nenhum SLA é aprovado nesta entrega.

Ensaiar restore em destino isolado, verificar checksums, contagens, vínculos, totais monetários, permissões e funcionamento. Um restore de todos os tenants não deve vazar dados por filtros de suporte. A recuperação de tenant individual depende do modelo escolhido e deve ser desenhada, não presumida a partir de um dump geral.

## Mudança de módulo e migração

Desativar módulo interrompe suas novas operações/jobs conforme política e mantém acesso autorizado à retenção/exportação necessária. Reativar exige schema compatível e não recria dados. Não remover tabelas ao desativar. Upgrade usa expansão/compatibilidade e evidência; contração é ação posterior com gate próprio. Cada release declara quais pares app/schema funcionam e por quanto tempo; testes provam os pares suportados.

GRA contribui com disciplina de release, suporte, diagnóstico, idempotência e recuperação. O roadmap da origem permanece default-off onde registrado; esta entrega não ativa atualização remota nem WhatsApp. Suporte temporário futuro precisa de tempo, escopo, revogação e auditoria, sem acesso permanente por conveniência.

O backup documental realizado nesta tarefa tem procedimento e prova específicos em [14](14_INSTALACAO_E_RESTAURACAO.md). Ele não substitui backup de aplicação ou de dados que ainda não existem no Wissian.

## Operar a capability de continuidade

Medir atraso de projeção, fila por prioridade/idade, itens sem responsável, passagens sem reconhecimento, verificações bloqueadas, conflitos, retries/DLQ e deduplicações. Limiares e orçamento são definidos com baseline em M5-CO.0, não inventados neste pacote. Repetição de sinal não gera aviso; notificação tem chave por destinatário/item/episódio/transição/canal. Sem IDs pessoais como labels indiscriminados.

Rollback/suspensão preservam histórico e gates da origem; fluxo nativo permanece utilizável. Antes de reativar, reconciliar snapshot/replay com dedupe e autoridade atuais; projeção incompleta não pode parecer fila vazia. Timeout de ação com possível efeito exige consulta a recibo e reconciliação, não retry cego. Restaurar banco/eventos/anexos de forma coerente no laboratório e validar revisão humana, ordem e idempotência. [Capítulo 16](16_CONTINUIDADE_OPERACIONAL.md) detalha fronteiras; TC-05/10/11/14/15/18 cobrem operação.
