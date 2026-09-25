# Classificação e extração — inventariar antes de fragmentar

## Categorias e regra de propriedade

Classifique arquivos, símbolos, tabelas, rotas, tarefas agendadas, eventos, assets, fixtures, configurações e documentos. Cada unidade de domínio tem um owner. Uma pasta mista precisa ser decomposta por responsabilidades; não pode ser atribuída integralmente à fundação por conveniência.

| Categoria | Critério | Exemplos candidatos, a conferir |
|---|---|---|
| CORE | Necessária ao funcionamento transversal, sem semântica de nicho | Identidade, tenancy, interfaces de auditoria, composição |
| SHARED_CAPABILITY | Reutilizável, mas opcional | Scheduling, Devices, publicação, Player, Consulta, releases |
| SOCIOEDUCATIONAL | Regra ou dado específico do socioeducativo | Ocupação, cenários, movimentos, layout, operações restritas |
| TENANT_CONFIGURATION | Variação permitida por organização | Marca, unidade, nomenclatura autorizada, limites e ativação |
| LEGACY_ADAPTER | Preserva integração ou formato anterior | Entrypoints, cookies, path de dados, migrações/importação |
| DEV_ONLY | Ferramentas de engenharia, não distribuíveis | C7, .codex, NEXUS como suporte de desenvolvimento, fixtures |
| RESERVED | Ativo futuro ainda não desenvolvido | Marca/composição comercial ClubAL para clubes |
| REVIEW_REQUIRED | Responsabilidade ou titularidade indefinida | Código misto, ativos de origem/licença não confirmada |

A matriz inicial em `matrizes/inventario_inicial.json` é uma hipótese rastreável baseada na inspeção parcial. Não é um inventário completo nem autoriza mover todos os itens sem análise.

## Sequência de classificação

Mapear consumidores antes de mover produtores. Para cada item registrar: caminho/símbolo real; ref; estado observado; categoria candidata e final; consumidores; dados envolvidos; contrato externo; risco; ação; teste; rollback. Usar o modelo em `modelos/INVENTARIO.md`.

Para reutilização Base44: conferir commits/diffs e o que já foi absorvido; classificar interface, textos, interação, assets e dependência de plataforma. ADOPT quando equivalente e compatível; ADAPT quando preservar intenção exige integração; KEEP quando a implementação atual já atende melhor; DEFER quando depende de gate; REJECT quando conflita ou duplica. REJECT não é apagar a origem do Base44.

## Áreas concretas de investigação

**ProductBoundary e consumidores.** Isolar a composição formal do produto, manter IDs/aliases necessários e remover a imposição de PEMSE sobre a fundação. Procurar referências em backend, frontend, testes, launcher, documentação executável e scripts de escopo.

**Domain e Application.** Revisar IdentityAccess/Tenancy/Audit; separar modelos neutros de permissions e roles específicos. Catálogos de permissões do produto podem registrar definições em contrato comum, mas o core não importa enums do socioeducativo.

**Infrastructure e DI.** `ServiceCollectionExtensions.cs` foi localizado; seu conteúdo integral ainda deve ser lido. Separar a montagem de capacidades do registro fundamental. Hosts registram explicitamente implementações. Evitar dependências implícitas por assembly scanning irrestrito.

**Endpoints, jobs e hubs.** Inventariar além dos menus. Ausência do módulo deve excluir seus handlers, eventos, workers e exportadores e continuar protegendo URLs diretas. O gate de segurança não fica apenas no frontend.

**Scheduling/Publication/FirstVertical.** Verificar regras genéricas versus convenções PEMSE. `FirstVertical` é um nome histórico, não prova de que todo seu conteúdo deva ir ao socioeducativo. A agenda deve continuar com uma única autoridade.

**Player/Consulta/Devices/Edge/Releases.** Classificar como capacidades opcionais, preservando consumidores socioeducativos atuais. Não reservar toda a tecnologia de telas apenas ao futuro ClubAL.

**Branding/temas/textos.** Preservar evolução visual e preferências. Separar marca do fabricante, produto, organização e preferência pessoal. Não remover melhorias Base44 porque estavam em um cabeçalho identificado como ClubAL.

**Legado Python/distribuição.** Core Python de agenda não é automaticamente o novo core SaaS. Manter compatibilidade e extrair apenas adaptações necessárias; não portar Python inteiro para .NET.

**Documentação.** Contratos atuais e instruções executáveis precisam refletir a nova direção; fatos históricos conservam sua identidade. Não apagar PEMSE de fontes/referências de domínio ou migrations para satisfazer busca textual.

## Limpeza sem falsas garantias

Fazer busca de termos (`PEMSE`, `SESI`, `ClubAL`, `Socioeducational` e variantes) em escopos classificados. Para cada ocorrência decidir se é regra específica indevida, rótulo configurável, identidade legada necessária ou referência histórica legítima. Uma lista de exceções deve ter caminho, símbolo, motivo e condição de retirada.

A base limpa não pode carregar uma exceção genérica “todo o legado está permitido”. Medir o grafo e o artefato. A exceção de nome não permite dependência de negócio escondida.

## Critério de preservação

Cada capacidade existente precisa terminar em um de três estados: preservada e testada; preservada com gate anterior mantido; ou adiamento/arquivamento explicitamente justificado e aprovado quando implicar perda de uso. Não desaparecer silenciosamente no processo de extração.

O sucesso é uma composição de produto mais clara e um núcleo desacoplado, não uma contagem de arquivos movidos.
