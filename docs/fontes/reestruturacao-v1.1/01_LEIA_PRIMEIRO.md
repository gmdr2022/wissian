# Guia do pacote Wissian

**Revisão 1.1.0 — 22/09/2026.** Atualização da identidade da plataforma-base para **Wissian**, alinhada ao guia fundador: marca corporativa **Fultuma**, empresa proprietária definida por Gui **Fultuma Tech Ltda**, produto socioeducativo com nome próprio pendente e ClubAL reservado para clubes. A adoção interna do nome não comprova registro ou titularidade formalizada.

Esta revisão preserva a arquitetura, o roadmap, os contratos de compatibilidade e as evidências históricas do plano. As recomendações iniciais da pasta dedicada devem ser reconciliadas com a tecnologia e o estado real do legado antes da implementação; a mudança de nome não autoriza uma troca de stack.

## O que este material entrega

Um direcionamento de produto, arquitetura, migração, segurança e execução para o Codex reorganizar o ClubAL existente. O pacote contém especificações, decisões propostas/adotadas para a implementação, matrizes rastreáveis, exemplos não produtivos e um verificador da integridade do próprio pacote.

**Não contém uma versão reestruturada do SaaS, um backup do repositório, credenciais ou resultados de testes do ClubAL executados nesta conversa.** A inspeção foi remota, parcial e somente leitura. O estado local do Codex pode estar mais avançado.

## Como Gui deve usar

Leve o ZIP à pasta dedicada do Wissian e extraia-o em uma subpasta de apoio, sem substituir os arquivos de governança existentes. O [Prompt.txt](Prompt.txt) acompanha o pacote para uso quando Gui decidir aplicar o plano. Entregue ao Codex o arquivo `00_COMECE_AQUI_CODEX.md` junto com acesso ao restante da pasta, ao guia fundador disponível no destino e ao checkout legado correto. O Codex deverá distinguir destino e origem antes de integrar qualquer conteúdo. Não é preciso copiar o pacote inteiro sobre a raiz do repositório nem executar comandos técnicos manualmente.

Ao solicitar a aplicação do plano, preserve tarefas em andamento até um checkpoint seguro. Anexar o pacote para leitura, organização ou revisão não autoriza iniciar sua implementação. O plano preserva o trabalho existente e exige reconciliar a convergência com Base44.

## Leitura por finalidade

| Finalidade | Arquivos |
|---|---|
| Início autônomo | [00 — prompt principal](00_COMECE_AQUI_CODEX.md) |
| Direção aprovada e exclusões | [02 — decisões](02_DECISOES_E_LIMITES.md) |
| O que foi realmente observado | [03 — evidências](03_ESTADO_VERIFICADO_REPOSITORIO.md) |
| Separação de responsabilidades | [04 — arquitetura](04_ARQUITETURA_ALVO.md), [05 — extração](05_MAPA_DE_EXTRACAO.md), [06 — contratos](06_CONTRATOS_DE_MODULOS.md) |
| Preservação de estado e acesso | [07 — dados](07_DADOS_MIGRACOES_COMPATIBILIDADE.md), [08 — segurança](08_SEGURANCA_PRIVACIDADE.md) |
| Produtos e experiência | [09 — socioeducativo](09_PRODUTO_SOCIOEDUCATIVO.md), [10 — ClubAL e legado](10_CLUBAL_E_LEGADO.md), [11 — UX e marca](11_UX_MARCA_I18N.md) |
| Execução e prova | [12 — roadmap](12_ROADMAP_EXECUTAVEL.md), [13 — aceite](13_TESTES_E_ACEITE.md) |
| Operação e riscos | [14 — custos](14_OPERACAO_IMPLANTACAO_CUSTOS.md), [15 — rollback](15_RISCOS_E_ROLLBACK.md) |
| Reutilização futura | [16 — novo produto](16_PLAYBOOK_NOVO_PRODUTO.md) |
| Retomada e manutenção | [17 — continuidade](17_CONTINUIDADE_E_RELATORIOS.md), [prompts de retomada](prompts/RETOMAR.md), [revisão independente](prompts/REVISAO_INDEPENDENTE.md) |
| Fontes e titularidade | [18 — fontes](18_FONTES_E_REFERENCIAS.md), [19 — propriedade](19_PROPRIEDADE_E_MARCA.md) |
| Decisões técnicas | [índice de ADRs](adrs/README.md) |
| Requisitos, casos e inventário | [matrizes](matrizes/README.md) |
| Exemplos tipados, sem aplicação automática | [contratos](contratos/README.md) |
| Verificação do ZIP | [ferramentas](ferramentas/README.md), [registro da entrega](99_VALIDACAO_DO_PACOTE.md) |

## A entrega esperada do Codex

Ao final técnico: uma fundação v1 delimitada e testada; produto socioeducativo recomposto sobre ela; capacidades comuns opcionais; ClubAL futuro reservado sem excluir o legado necessário; migrações e identificadores preservados; interfaces sem vazamento indevido de nicho; documentação e procedimentos reproduzíveis.

Os rótulos `foundation`, `socioeducativo` e `clubal` utilizados nos exemplos são propostas de identificadores de composição, não ordens para substituir IDs existentes. A marca socioeducativa continua indefinida. A existência de uma configuração de exemplo não comprova implementação.

Prontidão técnica local não é autorização de produção, aceite institucional, conformidade jurídica global ou certeza de receita. Esses estados são registrados separadamente.
