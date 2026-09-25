# Riscos, decisões abertas e reavaliação

O pacote define direção e processo; não presume arquitetura implementada, Base44 integralmente auditado ou requisitos comerciais aprovados. Confiança alta na integridade dos anexos e na separação solicitada; confiança limitada sobre reutilização de código até M0. Responsável técnico significa quem assumir a fatia; Gui decide prioridade, experiência e ações externas pertinentes.

| ID | Risco / como se manifesta | Prevenção e gatilho de parada | Responsável / marco |
|---|---|---|---|
| RSK-01 | Perder melhorias Base44 num merge/rewrite | Comparar cenários, dados/estrutura/assets e refs, ficha por capacidade; bloquear adoção sem procedência e caracterização | Técnico + Gui para equivalência de UX / M0 e absorções |
| RSK-02 | Core inflado ou contaminado por vertical | Teste de grafo e composição neutra; bloquear dependência Core→operacional/vertical | Técnico / M1 em diante |
| RSK-03 | Vazamento entre organizações | Threat model, autorização servidor, teste integração negativo em todos os canais; bloquear piloto com falha de isolamento | Técnico / M1–M7 |
| RSK-04 | Plano de controle com acesso irrestrito | Autoridade separada, suporte excepcional expirável; bloquear acesso implícito | Técnico + operação / M1–M7 |
| RSK-05 | Falso saldo/duplicidade/estado fiscal enganoso | Dinheiro exato, idempotência, constraints, NF-01–10; bloquear release com diferença não explicada | Técnico + validação do fluxo / M3–M4 |
| RSK-06 | Stack escolhida por preferência, perdendo reuso | ADR com experimento mínimo e custo de absorção; reavaliar se primeira capacidade exige reescrita ampla | Técnico / M0 |
| RSK-07 | Documentação histórica vira autorização | Fontes arquivadas e precedência; impedir scripts de origem disparados por leitura | Todo executor / todas as fases |
| RSK-08 | Backup existente mas restauração inviável | Ensaio e hashes; para produto medir restore e pares de versão; bloquear dados reais sem recovery | Técnico/operação / entrega e M7 |
| RSK-09 | Escopo de ERP inteiro impede primeiro valor | M4 como primeiro ciclo; catálogo posterior por fluxo e custo; reavaliar se M1 acumular domínios | Gui + técnico / planejamento |
| RSK-10 | Marca/asset/código com direito não demonstrado | Proveniência/licença por origem; registrar dúvida, substituir apenas após decisão; não declarar registro de marca | Gui + responsável contratual / M0 e lançamento |
| RSK-11 | API/SDK Base44 torna destino dependente de credenciais/serviço | Inventário de chamadas e modelo de dados, adapter explícito, fixture sintética; bloquear cópia com segredo | Técnico / M0 |
| RSK-12 | Teste documental vendido como produto pronto | Matrizes NOT_RUN, gates separados, evidência de ambiente/commit; não promover por número de arquivos | Técnico + revisão / toda entrega |
| RSK-13 | Alteração simultânea local sobrescrita | Baseline antes de mover/copiar, backup validado e interrupção ao detectar divergência | Executor / instalação |
| RSK-14 | Importações maliciosas ou lote parcial silencioso | Limites, parser restrito, staging de dados, resultado por item, rollback da transação | Técnico / M2–M6 |
| RSK-15 | Reuso de mecanismo remoto amplia privilégio/custo | Não transplantar agente GRA/integração automaticamente; mínimo experimento local | Técnico + Gui para externo / M6–M7 |
| RSK-16 | Host limitado inviabiliza testes e builds | Preflight, serializar peso, checkpoint, não repetir casos válidos; registrar limitação sem aprovar teste | Executor / cada lote |

## Decisões abertas com saída definida

| Decisão | Recomendação provisória | Alternativa forte e trade-off | Evidência que fecha / reavalia |
|---|---|---|---|
| Stack/backend/frontend | Escolher após auditoria; monólito modular pequeno como hipótese | Reaproveitar .NET/React ou adotar Next/Nest: comparar custo real de reuso, manutenção, segurança e distribuição | Caracterizar 1 fluxo, 1 autorização, 1 persistência e 1 módulo; ADR em M0; não criar duas aplicações completas |
| Isolamento de dados | Tenant explícito e banco relacional com controles verificáveis | Banco/schema por tenant vs compartilhado com políticas: custo, restore individual e requisitos do cliente | Threat model, teste com runtime role, restore e custo; antes de dados reais |
| Origem funcional Base44 | Procedência por comportamento e commit | Escolha única da branch mais recente é insuficiente; adaptar pode custar mais porém preserva semântica | Inventário de arquivos/calls/modelos/assets e comparação por caso |
| Primeiro contexto Fultuma | Tenant de demonstração comum, sem exceções de plataforma | Customização exclusiva reduz generalidade | Dois tenants sintéticos cumprem mesmo fluxo; exceções exigem decisão explícita |
| Emissão fiscal/contabilidade | Registro e apoio documental/gerencial primeiro | Emissão/integradores acrescentam obrigações, credenciais e custos | Caso de uso, regras verificadas por fonte competente, ambiente de teste e autorização / E7 |
| Identidade visual final | Preservar guia e inventariar alterações Base44 | Redesenhar agora aumenta retrabalho sem evidência | Comparação de assets, procedência e aceite visual de Gui |
| Hospedagem/custos/SLO | Sem fornecedor ou número contratado neste pacote | Separação em muitos serviços eleva operação | Medidas de uso, requisitos e orçamento / M7 |
| Verticais prioritárias | Administração primeiro; restante preservado | Socioeducativo primeiro somente com mudança explícita de prioridade | Demanda/fluxo/risco e capacidade; não apagar roadmap anterior |

Não é necessário fechar decisões de emissão, fornecedor cloud ou todas as verticais para começar M0/M1. É necessário fechar stack e primeiro contrato antes de construir a aplicação, e fechar isolamento/recuperação antes de dados reais. Reavaliar também se novas fontes contradisserem hashes/refs, se houver mudança de autorização, ou se requisitos de um cliente exigirem boundary diferente.

## Pré-mortem Continuity e reuso — adendo 2.1.0

| ID / risco | Sinal de alerta | Mitigação e parada | Teste preventivo futuro / responsável |
|---|---|---|---|
| RSK-17 — task manager genérico | Cards sem fonte, rótulos livres/boards substituem fluxo real | Exigir entidade/ocorrência e ação útil; bloquear MVP sem consumidor mensal | TC-02/13/17; produto + técnico M5-CO |
| RSK-18 — fadiga de alertas | Mesma condição gera cards/avisos e usuários ignoram fila | Stable key, episódio, relevância, severidade e dedupe por destinatário/transição; medir ruído | TC-02/13/14; produto/operação M5-CO |
| RSK-19 — dívida desktop transplantada | Imports WPF, paths/EXE, mutex ou journal viram dependência | Absorção conceitual e código compatível caracterizado; bloquear executor arbitrário | TC-09/16/20; técnico M0/M5-CO |
| RSK-20 — gargalo de aprovação | Sem revisor elegível, fila cresce, SLA autoaprova | Papel/owner e reatribuição autorizada; separar ACK/decisão; nunca contornar gate crítico | TC-07/08/19; produto + dono do domínio M5-CO |
| RSK-21 — vazamento multi-tenant | Chave/cache/consulta sem escopo, recibo após revogação | Constraints/autoridade atuais em todos os canais; bloquear promoção por um vazamento | TC-03/06/07/08/09/11/16; segurança/técnico |
| RSK-22 — duplicação e replay | Nova chave a cada retry ou evento velho reabre conclusão | Unicidades distintas, fingerprint, inbox/outbox, sequência/episódio e reconciliação | TC-02/03/04/05/10/18; dados/operação |
| RSK-23 — ciclo/fila sem saída | Bloqueios sem dono, ciclo concorrente ou cancelamento libera sucessor | Grafo transacional, escopo/episódio, motivo e rota de resolução; cancelado não satisfaz dependência | TC-06/19; técnico + operação |
| RSK-24 — ação ampla/perigosa | Handler/callback/shell do cliente, resultado incerto marcado sucesso | Registro tipado, schema/limites, permissão/revisão, reautorização e recibo; impedir retry cego | TC-09/10/11/15/16; segurança/técnico |
| RSK-25 — reconstruir o que existe | Nova stack/scaffold/camada sem mapear equivalente; especialidade migra ao Core | Matriz por símbolo/ref/hash, manter/extrair/adaptar; lacuna comprovada e decisão antes de código novo | TC-01/20/21; arquitetura + Gui para exceção material |

Confiança alta na separação documental; reuso literal, volume, custo e utilidade de UX só serão confirmados no destino. M5-CO.0 fecha orçamento/limiares e responsabilidades; falhas de isolamento, aprovação indevida, perda de atualização e duplicação de efeito bloqueiam promoção. Alternativa forte é manter temporariamente coordenação nativa do módulo; aceita-se adiar M5-CO para preservar primeiro ciclo e reuso, sem apagar a direção. Reavaliar se consumidor M4 não existir ou se integração exigir reconstrução ampla.

RSK-26 — perder ideias ao reduzir a fusão ao trio. Sinal: offline/envelope, saúde de processos, demais passagens/verificações e contexto desaparecem ou são declarados descartados. Mitigação: matriz NC/UC-NC com motivo/destino/fase, WM-40/TC-22; revisor confirma cobertura em cada extração, sem inflar MVP ou Core. Responsável: arquitetura/produto.
