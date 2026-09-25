# Repositórios, branches e preservação

## Topologia decidida

**Wissian:** repositório próprio, independente, com `main` própria e único foco de desenvolvimento. **ClubAL, Codex Checkpoint e NexusDevHub:** produtos independentes descontinuados, preservados como fontes de absorção seletiva, com branches históricas e integrações conservadas. **Base44:** ref/export de origem a comparar, não ramo a sobrescrever. Produtos futuros podem consumir contratos/pacotes do Wissian; repositório separado por vertical só se houver benefício concreto. Nenhuma branch permanente por cliente.

Em 25/09/2026, Gui autorizou criar e sincronizar o repositório privado
[gmdr2022/wissian](https://github.com/gmdr2022/wissian). Sua existência, privacidade e
remote `origin` foram verificados. O Git local preexistente é preservado; o baseline
inicial inclui contratos, fontes documentais preservadas e ensaio M0 revisados, sem
histórico ou remote do ClubAL. Consultar Git para hash/estado atual, não snapshots.
Branches curtas com prefixo `codex/` passam a ser possíveis após esse baseline.

## Encerramento autorizado das origens — 25/09/2026

Gui concordou com a descontinuação dos três repositórios e esclareceu que não
trabalhará mais no ClubAL: o foco é apenas o novo SaaS Wissian. O cuidado solicitado
com a Base44 significa preservar e considerar os aprimoramentos de interface já
produzidos, sem manter o ClubAL como frente ativa.

O encerramento é limitado a aviso no README/instruções, descrição do GitHub,
fechamento dos PRs automáticos de atualização e arquivamento privado para leitura.
Preservar código, histórico, branches, tags e alterações locais existentes. Não
mesclar branches nem atualizações de dependências para concluir o encerramento;
não excluir arquivos, reescrever história, mudar visibilidade ou promover release.
Usar checkout isolado para os avisos e verificar as refs no servidor antes/depois.

As duas referências Base44 obrigatórias e o critério de cobertura visual pertencem
ao [capítulo 04](04_ABSORCAO_CODEX_BASE44.md). Arquivar não comprova absorção,
equivalência funcional, aceite visual nem substituição de instalações existentes.
Instalações, dados operacionais e serviços externos não são alterados por esta
decisão. O estado remoto efetivo é conferido no GitHub, não inferido deste texto.

Evolução, correção e novas funcionalidades passam a ocorrer no Wissian. Reabrir
desenvolvimento ou escrita nos repositórios encerrados exige novo pedido explícito
de Gui; uma lacuna de reuso é tratada no destino, preservando a origem consultada.

### Retenção temporária e descarte futuro de Nexus/CC

Gui esclareceu que Codex Checkpoint e NexusDevHub permanecem disponíveis somente
para auxiliar a implementação das ideias já alinhadas e registradas no plano do
Wissian. O [capítulo 16](16_CONTINUIDADE_OPERACIONAL.md) e a
[matriz de patrimônio](matrizes/reaproveitamento-nexus-cc.json) são o contrato dessas
ideias; a origem não se torna produto, serviço ou dependência permanente do SaaS.

A intenção de descartar esses dois repositórios fica condicionada cumulativamente a:

1. Wissian em operação efetiva, com evidência do fluxo em uso; documentação, build
   ou ensaio isolado não atendem a essa condição.
2. Reaproveitamentos previstos implementados e validados no Wissian, com cobertura
   rastreável dos itens do plano e aceite aplicável. Itens adiados ou NOT_RUN não
   contam como concluídos; não antecipar funcionalidades só para apagar as fontes.
3. Código, contratos, testes, procedência e materiais necessários à manutenção e
   recuperação preservados no destino, sem dependência de consulta às origens para
   operar ou sustentar o que foi incorporado.

Até essas condições serem comprovadas, manter as fontes acessíveis. A exclusão
futura será uma operação separada, com alvos exatos, preservação recuperável e
autorização destrutiva aplicável; este encerramento não executa essa exclusão.
A condição específica trata de Nexus/CC e não cria descarte automático do acervo
ClubAL/Base44, de instalações ou de dados operacionais.

## Sequência futura

1. Revalidar raiz, status, remotes, branches e worktrees dos dois repositórios; registrar alcance temporal. Não usar pull/stash/reset como limpeza.
2. Revisar pacote preparado e arquivos não rastreados. Primeiro commit somente mediante autorização; selecionar caminhos próprios explicitamente, excluindo backups, ZIP, recibos locais e segredos.
3. Criar branch curta por fatia com prefixo `codex/`, após existência de baseline. Nomes de fatias são definidos na execução, não refs criadas por este documento.
4. Extrair capacidades aprovadas para o Wissian com registro de origem: repositório, SHA, caminhos/símbolos, autores/licenças, alterações e testes. Cherry-pick só se o commit for coeso e compatível; não importar merge geral, histórico inteiro ou credenciais.
5. Preparar descrição de PR e evidências. Commit/push/PR externo/merge/release seguem autorização da tarefa; nunca inferir de um modelo documental.
6. Manter o remoto privado autorizado e confirmar sincronização comparando HEAD com a ref obtida do servidor após push. URL existente ou ref local antiga não comprovam upload íntegro; verificar clone independente quando mudar regras de empacotamento.

Não adicionar remote ClubAL ao Wissian nesta preparação. Futuramente pode-se consultar clone auxiliar ou ref de leitura, com necessidade e autorização suficientes. Um fetch atualiza refs locais e deve ser registrado; não prova revisão dos diffs. Um hash antigo é identificador de evidência, nunca ordem de reset.

## Política de integração proposta

Branches curtas sobre `main`; checks pertinentes, revisão proporcional ao risco e rollback da fatia. Sem `develop` permanente por rotina, sem cópias por cliente, sem force push no histórico compartilhado. Squash pode ser usado no Wissian desde que a procedência dos componentes absorvidos permaneça no registro de migração e os avisos de licença sejam preservados.

Proteção de `main`, revisão e checks dependem da capacidade real do provedor.
Proibição de force push/exclusão de histórico continua regra local; não equivale a
proteção configurada no GitHub. Estado dessa proteção deve ser consultado, nunca
presumido pela privacidade do repositório. Com mantenedor único, autorrevisão não se
chama revisão independente. Não contratar plano pago implicitamente.

## Validação contínua no GitHub

O workflow `.github/workflows/dotnet.yml` valida os contratos e a integridade das
fontes em pushes para `main`/`codex/**`, PRs para `main` e execuções manuais. Uma ref
somente documental registra explicitamente testes de produto `NOT_RUN`; a presença
parcial da aplicação falha em vez de simular sucesso.

Quando `global.json` e `Wissian.slnx` estão presentes, a segunda etapa usa o SDK
fixado pelo projeto, restore com lockfiles, build, suíte executável do Core,
verificação de migrations e suítes de persistência/API em PostgreSQL 18.6 sintético.
O CI também verifica tipos/build React, dependências do lockfile e jornadas em
Edge sobre a API real. O consumidor sintético de UI tem build separado e não entra
na interface de produção. O runner Windows respeita DPAPI e ACLs. O download do banco
tem SHA-256 fixado; cada suíte cria e encerra seu cluster isolado. `dotnet test` não
substitui esses executáveis. Actions têm commits fixados, permissão de leitura,
limite de duração e cancelamento de execuções superadas na mesma branch.

O CI não publica o produto nem utiliza segredos/dados reais. Logs de testes não
devem conter credenciais; bancos, keyrings e arquivos de acesso não são enviados
como artefatos. Resultado verde significa somente que as verificações daquela ref
passaram, sem equivaler a aceite humano, merge ou prontidão de produção.

## Versionamento e fronteira de dados

Separar versão documental (este pacote), versão da aplicação, contrato de API/eventos, módulo, banco e artefatos de dispositivos. Sem tags/releases vazias. Contratos suportados são pares explícitos testados; “N/N-1” universal exige prova que ainda não existe.

O Git deve conter código, contratos, ADRs, testes sintéticos e documentação compartilhada. Dados empresariais, certificados, tokens, cofre C7, anexos operacionais, logs indiscriminados, backups locais e dumps permanecem fora. O [NOTICE original](../fontes/workspace-2026-09-24/NOTICE.md) informa a direção de titularidade sem substituir instrumentos jurídicos.

O manifesto valida bytes, por isso `.gitattributes` impede normalização automática
de `docs/` e dos fragmentos de procedência do ensaio. Não converter fontes históricas
para fazer um clone validar. Binários de build, `bin/obj`, caches e backups não são
versionados. Sincronização Git remota protege o código/contratos; não é backup dos
futuros bancos e anexos operacionais.

## Conferência final da fatia

Status antes/depois, diff e não rastreados inspecionados; origem intacta; mudanças próprias delimitadas; contrato atualizado; teste real e limitações; recuperação registrada; somente ações autorizadas. Não declarar sincronização remota por comparar apenas refs de acompanhamento já existentes.

## Referências Nexus/CC

NexusDevHub e codex-checkpoint são ancestrais conceituais preservados. Referenciar commit/caminho/hash ao caracterizar futuramente um caso específico. O adendo original não autorizava alterações remotas; a decisão específica de encerramento de 25/09/2026, registrada acima, autoriza os avisos e o arquivamento, sem autorizar remoção. Cópia documental e instalação ativa permanecem separadas; backups e ZIPs originais não são substituídos.
