# Dados, migrações e compatibilidade

## Duas operações diferentes

**Absorver código/documentação** não transporta dados de ClubAL, Fultuma ou clientes. **Migrar dados reais** exige lote, finalidade, ambiente, backup, conferência, responsáveis e autorização próprios. Nesta preparação nenhuma migration ou importação foi executada.

O Wissian novo não precisa reproduzir todas as migrations históricas do ClubAL. Primeiro mapear a capacidade, schema mínimo, invariantes e contratos consumidores. Se houver necessidade de receber um formato antigo, definir adapter/importador explícito; manter a cadeia e os dados na origem. Compatibilidade não é compartilhar banco, cookie ou chave criptográfica entre produtos.

## Propriedade e modelo mínimo

| Grupo | Owner conceitual | Invariantes |
|---|---|---|
| Organização/associação/papéis | Core acesso | Associação validada; identidade comum não mistura dados entre organizações |
| Entidade empresarial/contraparte | Módulo administrativo | Organização SaaS, pessoa jurídica e cliente comercial são conceitos distintos |
| Documento/versão/anexo | Módulo documentos + adapter de arquivos | Original, hash, classificação, versão e origem; autorização também no download |
| Nota/estado fiscal | Módulo notas | Situação e verificação da evidência separadas do dinheiro; identificador textual |
| Título/parcela/movimento/alocação | Módulo financeiro | Transação, saldo, precisão monetária e idempotência; estorno com causalidade |
| Contrato/obrigação/ocorrência | Módulos contratos/trabalho | Versão assinada preservada, ocorrência independente, prazo com fonte |
| Alojamento/ocupação/movimento | Vertical socioeducativa | Planejado ≠ confirmado; maker-checker/revisão/ACK conforme contrato de origem |
| Releases e grants de suporte | Administração/capacidade GRA | Versões, autoridade, revogação, recibo e propósito; sem elevar acesso operacional |

O [plano base §§6–7](../fontes/workspace-2026-09-24/PLANO_WISSIAN_OPERACAO_BASE.md) é o contrato detalhado de notas/financeiro, mantido sem reescrita. NF-01…10 continuam seus casos de referência. As 13 áreas administrativas e sete abas descritas ali são diagnóstico histórico de outra tarefa; não foram reabertas/recopiadas aqui.

## Isolamento persistente

Toda entidade de organização tem escopo obrigatório e relações coerentes: vínculo `(organization_id, resource_id)` não aceita alvo de outra organização. Unicidade e idempotência incorporam escopo e domínio. Dados globais são explicitamente classificados; não declarar global uma contraparte pessoal porque aparece em dois tenants.

Runtime e migrador usam credenciais diferentes. Contexto confiável é definido pelo servidor em cada transação e não sobrevive ao reúso da conexão. Ensaiar pool real com A → B → ausência de contexto e tentativas de escrita cruzada. RLS é defesa adicional candidata do PostgreSQL, sem substituir autorização de recurso; testar privilégios reais do runtime. [Fonte oficial sobre RLS](https://www.postgresql.org/docs/current/ddl-rowsecurity.html).

## Plano de migrations por incremento

1. Inventariar schema/owner, cadeia histórica, constraints, índices, triggers, policies, chaves, enum/ID persistidos, arquivos associados e auditoria.
2. Registrar baseline sintética e contrato de bootstrap novo. Fixar ferramenta/versão após ADR; não misturar dois migradores autoritativos.
3. Produzir alteração aditiva compatível quando possível. Revisar SQL e impacto de bloqueios/índices; declarar pré/pós-condições.
4. Executar em banco exclusivamente sintético: instalação vazia, upgrade da fixture, segunda aplicação segura, falha intermediária e retomada. Migration já aplicada não é reeditada para fingir continuidade.
5. Verificar contagens, hashes, valores/associações, integridade e acesso; contagem isolada não comprova preservação.
6. Ensaiar backup/restauração em outro destino vazio com banco e anexos coerentes. Medir recuperação; preservar o conjunto anterior.
7. Só propor contração após consumidores migrados e dados reconciliados. Não executar drop/rename destrutivo ou `Down` automático para facilitar rollback.

Uma migration por módulo pode ter ownership próprio dentro de um plano de instalação coordenado. Não filtrar a cadeia antiga arbitrariamente nem usar criação direta do schema para alegar upgrade validado. Separar versão de banco, módulo e aplicação. Falha bloqueia somente a composição incompatível; não liberar metade de uma capacidade dependente.

## Importação e idempotência

Importação: inventário → cópia recuperável → fixture sintética → prévia → aprovação aplicável → lote identificado → reconciliação → data de corte. Cada linha tem origem, chave, erro, destino e estado; entradas inválidas não geram sucesso parcial silencioso. Arquivos e planilhas são dados, não instruções; sem fórmulas/macros, XXE ou acesso de rede incidental.

Chave idempotente é vinculada a organização, ação, objeto/revisão e conteúdo normalizado; mesma chave com conteúdo diferente gera conflito. Após timeout, consultar o recibo antes de repetir efeito. Não gerar nova operação “para garantir”. Baixas concorrentes não podem consumir duas vezes o mesmo saldo. Transferência interna não soma receita/despesa.

Cancelar nota não apaga pagamento; estornar movimento não apaga a baixa original; preparar pacote não envia ao contador. Dinheiro usa decimal ou unidade mínima com arredondamento definido, nunca float binário como regra de negócio. Competência, vencimento e caixa possuem semânticas distintas. Datas civis não são convertidas indiscriminadamente para UTC.

## Recuperação e identidades

Código pode voltar a uma versão compatível sem desfazer dados aditivos. Se houve uso posterior, reversão de um lote precisa verificar novas dependências; preferir compensação auditada ou correção adiante. Restaurar banco inteiro pode eliminar gravações válidas posteriores, exigindo plano específico.

No legado: conservar cookies, issuer/audience, DPAPI, paths, device IDs, signatures e migrations. No destino: definir identidades próprias, sem reaproveitar credenciais/autoridade antiga. Um adapter de compatibilidade tem origem/destino, pares testados, critério de retirada e responsável. Hashes/recibos e operações ambíguas devem sobreviver à mudança de apresentação.

## Dados de Continuity e concorrência

O [capítulo 16, §§4–6](16_CONTINUIDADE_OPERACIONAL.md) define o modelo proposto e suas invariantes. Reusar organização como tenant, sem entidade duplicada. WorkItem tem chave única por organização, source_module, scope_key não nulo e stable_key; relações têm FKs escopadas e episódios. event_id deduplica entrega; idempotency_key protege comando; stable_key identifica condição. Não trocar essas finalidades.

Versão otimista e update condicional impedem sobrescrita; transação inclui mudança/histórico/inbox/outbox necessários. Grafo de dependências precisa impedir ciclos também sob inserção concorrente. Merge preserva decisão/nota humana; evento velho não reabre nem confirma revisão nova. Histórico append-only minimizado segue retenção e correção controladas, sem copiar arquivo/journal de desktop. M1 prepara contratos; migrations WorkItem só em M5-CO. Não importar dados Nexus/CC nesta preparação.
