# Repositórios, branches e preservação

## Topologia decidida

**Wissian:** repositório próprio, independente, com `main` própria. **ClubAL:** repositório de origem/produto preservado, branches históricas e integrações conservadas. **Base44:** ref/export de origem a comparar, não ramo a sobrescrever. Produtos futuros podem consumir contratos/pacotes do Wissian; repositório separado por vertical só se houver benefício concreto. Nenhuma branch permanente por cliente.

Em 25/09/2026, Gui autorizou criar e sincronizar o repositório privado
[gmdr2022/wissian](https://github.com/gmdr2022/wissian). Sua existência, privacidade e
remote `origin` foram verificados. O Git local preexistente é preservado; o baseline
inicial inclui contratos, fontes documentais preservadas e ensaio M0 revisados, sem
histórico ou remote do ClubAL. Consultar Git para hash/estado atual, não snapshots.
Branches curtas com prefixo `codex/` passam a ser possíveis após esse baseline.

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

NexusDevHub e codex-checkpoint são ancestrais conceituais preservados. Nenhum merge, remote, fetch, commit, arquivamento ou alteração nesses repositórios é necessário a este adendo. Referenciar commit/caminho/hash ao caracterizar futuramente um caso específico. A decisão de não desenvolver produtos independentes não é autorização para removê-los ou mudar seus estados remotos. Cópia documental e instalação ativa permanecem coerentes; backups e ZIPs originais não são substituídos.
