# Absorção seletiva de Codex, Base44 e frentes maduras

## Unidade de decisão

**Primeira caracterização M0, 25/09/2026:** a
[ficha estruturada da fatia](evidencias/m0-primeira-fatia.json) reúne origem/ref/hash,
símbolos, dependências, comparações, decisões ADAPT/DEFER, limites e rollback. O
ensaio executável de seis fragmentos passou 16 casos de domínio. Isso não conclui
absorção de capacidades nem testes de banco/API/UI. A matriz de preparação conserva
PENDING_AUDIT/NOT_RUN e aponta para a evidência específica, sem promoção em bloco.
A stack foi decidida na [ADR-M03](adrs/ADR-M03_STACK_E_REUSO.md); próxima integração
segue o [contrato M1](contratos/M1_FUNDACAO.md).

Reuso no Core cumpre a [regra de identidade própria](03_ARQUITETURA_E_MODULARIDADE.md)
fixada por Gui: adaptar nomes e fronteiras no destino, manter procedência fora do
Core e não confundir renomeação com desacoplamento. Classificação de addon opcional
não elimina suas dependências nem os testes de autorização por organização.

Migrar capacidade e contrato observável, não branch inteira. Data, número de commits e origem da ferramenta não medem qualidade. Cada capacidade recebe uma [ficha](modelos/FICHA_ABSORCAO.md) e uma linha na [matriz de migração](matrizes/migracao.json), com evidência suficiente antes da edição.

1. Descobrir refs e estado local efetivos; registrar commit/base de comparação, arquivos locais não commitados e fontes de UX. Não modificar origem.
2. Caracterizar antes/depois das versões: entrada, permissão, dados, ação, sucesso, falha, persistência e recuperação. Capturar apenas dados sintéticos.
3. Mapear código, schema, migrations, permissões, APIs, assets, preferências, testes, dependências e consumidores. Uma mudança de interface pode conter mudança de domínio.
4. Avaliar reuso literal, extração delimitada, adapter ou reimplementação do comportamento quando a dependência proprietária inviabilizar o reuso. Justificar com custo/risco e teste de equivalência.
5. Implementar no destino a menor fatia que preserve o comportamento desejado. Nenhum SDK/conta/serviço Base44 é requisito implícito.
6. Provar resultado no fluxo integrado, negativos e regressões adjacentes. Comparar UX com a referência preservada.
7. Registrar conclusão e lacunas por item. Só avançar após o gate da fatia; não fechar “Base44 absorvido” com uma única tela.

## Decisões e significado

| Decisão | Uso | Evidência mínima |
|---|---|---|
| ADOPT | Código/contrato saudável e compatível, incorporado com pouca adaptação | Origem/ref/hash, dependências/licença, caracterização e testes no destino |
| ADAPT | Preservar experiência/regra mudando fronteira ou tecnologia necessária | Equivalência funcional antes/depois, invariantes e justificativa técnica |
| KEEP | Destino já implementa resultado igual ou melhor; evitar reaplicação | Mapa da integração anterior e teste comparativo |
| DEFER | Capacidade útil depende de prioridade, fonte, gate ou consumidor | Condição objetiva de retomada e preservação da origem |
| REJECT | Alternativa conflita com contrato, duplica ou não é adequada | Motivo específico, alternativa e efeito de produto; origem continua preservada |

`EXCLUIR_DO_CORE` é uma classificação arquitetural, não uma exclusão funcional. Pode combinar-se com ADAPT para uma vertical. `PENDING_AUDIT` significa sem decisão final; não preencher ADOPT por conveniência. Rejeitar uma implementação não rejeita automaticamente a intenção de UX que ela atende.

## Comparação Base44 × Codex

### Referências preservadas e cobertura de interface

Gui confirmou em 25/09/2026 o foco exclusivo no Wissian. As melhorias já produzidas
na Base44 continuam sendo patrimônio a examinar; isso não mantém uma frente de
desenvolvimento no ClubAL. O encerramento dos repositórios segue o
[capítulo 05](05_GIT_E_REPOSITORIOS.md).

| Referência de origem | Commit verificado no GitHub em 25/09/2026 | Papel na absorção |
|---|---|---|
| `base44/setup-32dcb42e` | [`46370e599c021c52ee9734249b438f4a664ab33b`](https://github.com/gmdr2022/clubal/tree/46370e599c021c52ee9734249b438f4a664ab33b) | Fonte Base44 a comparar por tela, componente e fluxo |
| `integration/base44-main` | [`3380fa923bd6e3b05610642da0d283fb81514c7b`](https://github.com/gmdr2022/clubal/tree/3380fa923bd6e3b05610642da0d283fb81514c7b) | Evolução integrada a comparar; não presume que toda melhoria Base44 foi incorporada |

Ambas devem continuar recuperáveis com seu histórico. A `main` isolada não é
inventário suficiente do patrimônio. Diferença de commits/arquivos não comprova
qualidade, autoria de cada melhoria ou equivalência de UX. A caracterização M0
anterior não validou visualmente estas interfaces.

Antes de implementar uma fatia de interface no Wissian, a ficha correspondente deve
identificar os arquivos e comportamentos pertinentes nas duas referências, a
evidência visual/sintética disponível e o destino de cada melhoria relevante:
ADAPT/ADOPT/KEEP/DEFER/REJECT, com motivo. Cobrir navegação, hierarquia, densidade,
tokens/temas, responsividade, teclado e estados vazio/erro/espera/permissão quando
tocados. Preservar interação e resultado útil, adaptando identidade e autorização
ao Wissian. Ausência de prova fica explícita; não tratar captura visual, build ou
arquivamento como aceite ou absorção concluída.

Essa conferência acompanha apenas a fatia atual, sem reabrir auditoria integral ou
manutenção dos produtos encerrados. Aperfeiçoamentos e correções passam a ocorrer
no Wissian, com procedência e testes próprios, sem sincronização bidirecional.

| Dimensão | Pergunta de comparação | Tratamento |
|---|---|---|
| Mapa de alojamentos | Alterou geometria, relações, confirmação, estados, legenda, navegação ou apenas aparência? | Preservar intenção completa na vertical; testar revisão, concorrência, leitura em lista e planejamento separado da ocupação confirmada |
| Logos/marca | Qual camada representa: Fultuma, produto, organização ou preferência pessoal? | Adotar assets com origem/licença e sanitização; não substituir IDs/cookies/chaves |
| Shell/navegação | Melhorou localização, hierarquia, densidade ou fluxo? | Integrar com módulos efetivos e permissões; sem menus especializados no Core |
| Dados/formulários | Há campos, validações, transições ou entidades novos? | Mapear API/schema e semântica; interface sem backend equivalente é lacuna |
| Temas | Preserva catálogo, tokens, contraste e preferências em todos os estados? | Reusar TEM e avaliar o resultado Base44; não impor tema institucional sobre preferência pessoal |
| Segurança | Existe proteção real no servidor, por tenant e recurso? | Reaproveitar implementação robusta sem perder a experiência; rejeitar confiança na UI |
| Dependência Base44 | Requer SDK, conta, backend gerenciado, automação ou configuração não exportada? | Inventariar disponibilidade real; adapter/reimplementação justificados, sem prometer exportação completa |

A inspeção desta entrega confirmou refs e um relato de absorção do cabeçalho institucional; **não verificou integralmente o export Base44, todas as telas nem o mapa alterado**. M0 deve obter evidência do código/UX no escopo real. Ausência de fonte deve bloquear somente a decisão dependente, sem inventar o resultado antigo.

## Preservação de TEM/OWM/GRA

TEM é candidato forte ao shell: manter semântica de tokens e preferências isoladas; aceites humanos pendentes continuam pendentes. OWM fornece candidatos de tenancy, acesso, configuração, branding, agenda, publicação e dados; distinguir módulos opcionais de domínio socioeducativo. GRA fornece contratos de releases, concessões temporárias e staging; preservar default-off, propósito, escopo, MFA, revisão/hash exatos e auditoria. Assistência de implantação não é login administrativo universal.

Usar os [114 requisitos extraídos](matrizes/requisitos-frentes-origem.json) e [35 pacotes OWM de origem](matrizes/roadmap-owm-origem.json) para conferência de cobertura. IDs originais não serão renumerados como se tivessem surgido no Wissian. Conteúdo marcado adiado não vira dependência de toda fundação.

## Definição de absorvido

Origem/ref preservadas; decisão e equivalência registradas; owner e fronteira claros; testes reais no destino; dependências/licenças revisadas; identidade técnica tratada; nenhuma capacidade relevante desaparecida sem disposição; rollback possível; UX demonstrada e aceite humano separado. Build sozinho não prova absorção.

Uma melhoria da origem encontrada depois da extração pode gerar uma nova entrada com diff do contrato. Não executar sincronização bidirecional automática de código ou dados. Manter proveniência e revisitar somente mudanças relevantes à capacidade já consumida.

## Absorção conceitual Nexus/CC

As fichas ABS-041…048 na matriz acrescentam catálogo/contexto, saúde/resolução, recuperação, fila/stable keys, dependências, passagens, decisão humana e integração defensiva. A decisão de **aproveitar princípios** está aprovada na [ADR-M08](adrs/ADR-M08_CONTINUIDADE_OPERACIONAL.md); o estado técnico PENDING_AUDIT continua correto, pois nenhum código foi incorporado ou testado no destino.

O [mapa do capítulo 16](16_CONTINUIDADE_OPERACIONAL.md) distingue preservar/generalizar/aposentar. Referências privadas: `gmdr2022/NexusDevHub` e `gmdr2022/codex-checkpoint`. Quando necessário, ler somente trecho/caso pertinente e registrar SHA/licença/procedência. Não portar WPF, launcher, shell, JSON/mutex/journal, associação `.ccard` ou integrações específicas do Codex. A absorção não altera nem exclui origem; o encerramento documental e arquivamento autorizados seguem exclusivamente o [capítulo 05](05_GIT_E_REPOSITORIOS.md). Dados reais e fixtures sensíveis não atravessam. A absorção será concluída por comportamento de negócio e provas no SaaS, não pela quantidade de código recuperado.

## Diretriz reforçada por Gui — fragmentar e reutilizar

O destino administrativo limpo e os addons especializados serão obtidos por **fragmentação, desacoplamento e aprimoramento das implementações existentes**. Não é projeto greenfield nem autorização para recriar o que já foi construído. Gui informa que quase tudo já está pronto nas bases de origem; a extensão exata desse reuso será comprovada por capacidade, sem confundir ausência de aplicação nesta pasta documental com ausência de software reutilizável.

Antes de criar componente, schema, fluxo, UI ou infraestrutura, localizar seu equivalente em Codex/Base44/TEM/OWM/GRA e demais fontes pertinentes; preencher a ficha com origem/ref/hash/símbolo, testes/consumidores, fronteira, dependência a cortar, destino e melhoria preservada. Preferir manter/extrair/adaptar código, dados sintéticos, assets e testes compatíveis. Novo código se limita à ligação, adapter ou lacuna demonstrada; troca ampla de stack/reimplementação não é caminho padrão nem pode decorrer de preferência. Qualquer exceção material ao reuso exige evidência de impossibilidade/incompatibilidade e revisão da decisão com Gui, preservando o original.

Core básico contém mecanismos mínimos; capacidades administrativas e Continuity são opcionais. Regras de alojamentos, pedagogia, turnos específicos, dispositivos/telas e demais especialidades pertencem a seus addons. Mesmo conceito comum não justifica extrair para Core sem consumidor e equivalência semântica. Os princípios Nexus/CC atravessam; seus executores desktop proibidos não. A direção de reuso não autoriza portar mecanismos inseguros ou copiar identidades/credenciais legadas.

Para Nexus/CC, consultar a [matriz de patrimônio e aplicações](matrizes/reaproveitamento-nexus-cc.json): 28 decisões, 18 usos e vínculo ABS-041…048. Separar preservar/generalizar/princípio/futuro/aposentar no destino. Ideia futura não é descartada; código novo exige lacuna provada como em qualquer absorção.
