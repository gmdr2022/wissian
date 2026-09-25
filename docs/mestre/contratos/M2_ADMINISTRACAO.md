# M2 — entrar, organizar e localizar

**Estado:** S1 de contrapartes e S2A da API de documentos verificadas localmente.
Interface documental e vínculos da S2B ainda não implementados. M2 não está concluído. Aceite humano e
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

## S2A — API de documentos privados

`Wissian.Documents` depende apenas do Core e da persistência EF/Npgsql adotada.
É dono de `wissian_documents`, sem referência obrigatória a contrapartes. A
composição independente é testada. A API preserva título, origem textual opcional,
classificação, versão do cadastro e revisões do original. Edição de metadados não
reescreve os arquivos. Arquivamento preserva os IDs e impede edição ou novos
originais; restauração devolve a edição. Reinspeção também funciona em arquivados,
sem restaurá-los. Versões antigas autorizadas continuam consultáveis.

`documents.read` permite consultar; `documents.write`, junto da leitura, permite
comandos. Classificação `restricted` exige também `documents.restricted`, inclusive
para criar/reclassificar. Sem essa permissão, metadados, contagem, histórico, bytes
e recibos restritos não ficam visíveis. Classificação atual do documento protege
todas as revisões. `internal` significa acesso interno autorizado da organização,
nunca publicação. Ativação, associação, sessão e permissões são revalidadas no
servidor em cada operação; RLS cobre também tabelas de originais e revisões.

### Originais, limites e inspeção

- Aceitar somente PDF, PNG, JPEG e texto UTF-8; extensão, tipo declarado, assinatura
  ou codificação devem concordar. Não extrair arquivos, executar conteúdo nem
  renderizar anexos no servidor. Assinatura de formato não substitui antivírus.
- Limites técnicos iniciais do piloto: 10 MiB por original, 100 revisões por
  documento, 1.000 documentos e 100 MiB de originais por organização. Não são planos
  comerciais. Arquivados e quarentena entram nas cotas. Trava transacional no banco
  impede consumo concorrente do mesmo saldo, inclusive entre classificações.
- Guardar originais imutáveis como `bytea` em tabela privada separada dos metadados.
  Tamanho e SHA-256 são conferidos no banco e antes do download. Relações compostas
  incluem organização. Gravação de metadados, original, recibo e auditoria usa a
  mesma transação. Essa opção mantém coerência no backup PostgreSQL e evita um
  segundo serviço neste recorte; **restauração completa ainda precisa ser exercitada
  antes do piloto**, no trabalho de recuperação M7.
- Inspeção ocorre antes da transação de gravação. Ao terminar, autorização e
  versões são novamente conferidas sob trava. Duas transferências e duas chamadas
  nativas simultâneas por processo limitam memória; excesso retorna `429`. Timeout
  de 20 segundos não libera a vaga de uma chamada nativa que ainda esteja presa.
- O adaptador atual usa AMSI do Windows, sem shell, elevação ou alteração do
  antivírus. Somente HRESULT de sucesso e resultado `CLEAN`/`NOT_DETECTED` aceitam
  o conteúdo. Erro, risco indeterminado, timeout ou plataforma não suportada deixam
  o original em `quarantined`; detecção ou bloqueio administrativo rejeitam novo
  conteúdo (`422`). Reinspeção pode bloquear uma revisão antes aceita, preservando
  o original sem liberá-lo. [Contrato de resultados AMSI](https://learn.microsoft.com/en-us/windows/win32/api/amsi/ne-amsi-amsi_result).
- `accepted` significa que o provedor não detectou ameaça naquela inspeção, não
  garantia universal de inocuidade. Antes do ambiente real, verificar provedor
  instalado/ativo, atualização de assinaturas, política de dados do antivírus e
  conta de serviço, além de executar o teste nativo nesse ambiente. Uma API AMSI
  presente não é prova suficiente da saúde operacional do antivírus.
  [Pré-requisitos de inspeção documental AMSI](https://learn.microsoft.com/en-us/sharepoint/security-for-sharepoint-server/document-virus-check-with-amsi-option).
- Download exige inspeção aceita há no máximo 24 horas e autorização atual. É
  resposta autenticada `attachment`, sem cache, sem ranges e com `nosniff`/CSP
  restritiva. Nome de download é gerado; nome fornecido não vira caminho. Não há
  URL pública, listagem de disco, extração, prévia de PDF ou envio para serviço externo.

### Contrato HTTP e falhas

Prefixo: `/api/organizations/{organizationId}/documents`.
`GET /` aceita `q`, `status=active|archived|all` e `page`, 25 itens por página.
`GET /{id}` devolve metadados e revisões; `GET /{id}/revisions/{number}/download`
entrega o original autorizado. Downloads também geram auditoria mínima.

`POST /uploads` recebe multipart com exatamente `command` (JSON, até 8 KiB) e
`file`. Comando: `commandId`, `expectedSessionVersion`, `documentId` e
`expectedVersion` (ambos nulos na criação), `action=create|revision`, `value`
(`title`, `classification`, `source`, somente na criação), `revision=null`.
`POST /commands` recebe JSON com a mesma estrutura, ações `edit|archive|restore|scan`;
`value` só na edição, `revision` somente em `scan` para identificar o original.
CSRF é obrigatório em ambos. O limite padrão de 32 KiB das demais rotas permanece.

Quarentena pode ser um envio persistido com resposta `200` e
`scanState=quarantined`; a interface deve informar que o download está bloqueado.
Repetição do mesmo UUID/pedido devolve o recibo original, após reautorizar. Recibo
não substitui leitura do estado atual. Payload diferente ou versão concorrente
gera `409`; falta de visibilidade retorna `404`. Corpo interrompido/inválido não
persiste parcialmente. Limite por arquivo retorna `413`; cota total retorna `409`.
Falha de auditoria desfaz a gravação inteira. Nunca reportar sucesso só porque os
bytes chegaram ao servidor.

### Evidência e próximo consumidor

[Evidência S2A](../evidencias/m2-documentos-s2a.json) separa a suíte HTTPS/PostgreSQL
com inspetor sintético controlado do teste real do adaptador Windows: texto benigno
aceito e padrão inofensivo EICAR bloqueado, somente em memória. Não usar malware
real nem gravar a amostra de teste em banco/arquivo. O CI verifica as fronteiras com
o inspetor sintético; não certifica antivírus no ambiente de implantação.

S2B entrega interface documental e ligação opcional com contraparte por contratos
públicos e referência da mesma organização. Não tornar contrapartes uma dependência
obrigatória de documentos. Os padrões de Knowledge já caracterizados informam
versionamento/filtro; não transferem o domínio normativo por unidade. M2 sai
somente após E1 completo em navegador e negativos de upload/download/vínculos.
