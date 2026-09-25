# M2 — entrar, organizar e localizar

**Estado:** S1 de contrapartes verificada localmente; documentos, anexos, versões e
vínculos da S2 ainda não implementados. M2 não está concluído. Aceite humano e
produção permanecem separados. Consumidores: Fultuma Exemplo e Empresa Exemplo,
com dados sintéticos, mesma versão e políticas de acesso por organização.

## S1 — contrapartes

A área **Clientes e fornecedores** permite cadastrar nome, tipo (pessoa ou
organização), relação (cliente, fornecedor, ambos ou outra) e referência interna
opcional. Referência não é identificação fiscal validada nem chave única; nomes
iguais não são automaticamente fundidos. Não coletar contatos ou dados fiscais
adicionais antes de um consumidor e contrato que os exijam.

Consulta paginada de 25 registros, filtro literal por nome/referência e situação;
detalhe, edição, arquivamento e restauração. Arquivar conserva ID, recibos e
auditoria; não há exclusão física. A UI preserva filtros ao voltar à lista,
rascunho no conflito e pedido original quando a resposta se perde. Dados não são
persistidos no navegador. Formulário/URL não concedem autoridade.

### Fronteiras e transação

- `Wissian.Counterparties` depende do Core e de EF/Npgsql já adotados, sem importar
  Host, persistência de identidade, outra especialidade ou código legado integral.
  Seu DbContext e migrations são donos apenas de `wissian_counterparties`.
- O Host compõe o módulo disponível; `counterparties.read` e
  `counterparties.write`, elegibilidade, ativação, sessão e associação atuais
  governam cada requisição. Escrita requer também leitura. Sem módulo/grant, não
  há menu e a API responde `404`; identidade ausente recebe `401`.
- O adapter do Host compartilha conexão/transação entre os contextos, mantendo
  autorização, efeito, recibo e auditoria atômicos. A API de um módulo não recebe
  o DbContext privado de outro. [EF Core — transações entre contextos](https://learn.microsoft.com/en-us/ef/core/saving/transactions#cross-context-transaction).
- A função restrita `wissian.lock_current_authority` trava somente o escopo da
  sessão atual; não concede UPDATE nas tabelas de autoridade ao runtime. Revogação
  que vence a trava é observada antes do efeito; operação autorizada já em curso
  termina antes da revogação concorrente. Espera de trava limitada a cinco
  segundos devolve erro recuperável, sem sucesso aparente. RLS é defesa adicional.
  [PostgreSQL — concorrência nas políticas](https://www.postgresql.org/docs/18/ddl-rowsecurity.html).
- Comandos usam UUID, versão da sessão e versão do cadastro. Recibo imutável por
  organização/ator/comando contém hash do pedido e resultado mínimo. Replay
  revalida acesso; conteúdo diferente conflita. Edições concorrentes não se
  sobrescrevem. Resposta confirmada e atualização visual são estados distintos.
- Nome original, tokens, payloads e dados do cadastro não entram na auditoria:
  apenas ator, organização, ação, recurso, correlação e versão de autoridade.

### Contrato HTTP e instalação

Prefixo: `/api/organizations/{organizationId}/counterparties`.
`GET /` aceita `q`, `status=active|archived|all` e `page`; `GET /{id}` lê detalhe.
`POST /` cria; `POST /{id}` edita/arquiva/restaura. Todos os POST exigem CSRF e
`commandId`, `expectedSessionVersion`, `expectedVersion` (nulo na criação),
`action=create|edit|archive|restore` e `value` (nulo para arquivar/restaurar).

Aplicar primeiro migrations da fundação e depois as do módulo, com migrador
separado. O runtime recusa schema ausente/incompatível ou propriedade privilegiada;
não executa migrations. Os laboratórios fazem essa instalação somente em bancos
sintéticos. As migrations aplicadas são imutáveis; as duas aditivas da autoridade
mantêm a correção de escopo sem reescrever a primeira. `Down` destrutivo é recusado.

### Evidência e saída

[Evidência S1](../evidencias/m2-contrapartes-s1.json): 14 casos de API/banco e oito
jornadas de navegador, além dos contratos anteriores executados no CI por commit.
Cobertura inclui dois tenants, RLS, CSRF, ausência/revogação de grants, concorrência,
replay, filtros, rollback quando a auditoria falha, nova instância do host sobre
registros persistidos, teclado, foco e largura de 320 px. Aceite de Gui e tecnologia
assistiva permanecem pendentes; screenshots sintéticos locais não foram publicados.

## Próxima fatia — documentos

M2-S2 conecta contraparte a documento privado, com classificação, metadados,
versões, busca e vínculo estável. Antes de aceitar anexos, definir tipos/tamanhos,
validação, armazenamento fora da raiz web, download autenticado, integridade,
retenção e recuperação. Os padrões de Knowledge já caracterizados informam
versionamento/filtro; não transferem o modelo normativo por unidade. M2 sai
somente após E1 completo e negativos de upload/download e referências arquivadas.
