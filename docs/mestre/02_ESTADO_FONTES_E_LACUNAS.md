# Estado inspecionado, fontes e limites

> Este capítulo registra a inspeção de preparação em 24/09/2026. Quantidades, branches e caminhos de instalação citados são snapshots históricos; conferir apenas o que a fatia atual usa. A organização vigente começa no [README](../../README.md).

## Cobertura efetiva de 24/09/2026

Os quatro anexos da conversa foram disponibilizados localmente pelas ferramentas. Os caminhos `/mnt/data/...` do pedido identificam os anexos, não uma pasta existente neste Windows. Foram lidos integralmente os 49 arquivos internos do RAR e os três Markdown; também foram lidos os documentos/configurações locais do Wissian. Os seis `.gitkeep` foram verificados como reservas: cada um contém apenas uma quebra de linha (1 byte).

| Entrada | Evidência e tratamento |
|---|---|
| RAR v1.1 | 49 arquivos; 226.686 bytes descompactados; estrutura somente de arquivos/diretórios regulares; manifesto original validado |
| Guia fundador | 37.548 bytes; SHA-256 `5e402556313afa90a188aaf425543fc8d5e1c5e03aca2fd747083f8c674b10c7` |
| Iniciar operação base | 3.841 bytes; SHA-256 `a3190731da5978cc8f5a4fe29351fee224646c6c2066a55f1cc49dc90b5f8259` |
| Plano operação base | 54.936 bytes; SHA-256 `b38b0ecc222ed544a14b122441a2106e683a1a385e437ec43e9f59b2db41a96c` |
| Pasta Wissian | 26 arquivos fora de `.git`; 18 arquivos Git; `main` inicial sem commits/remotes; ausência de produto executável |
| ClubAL | Inspeção focada de Git, metadados e trechos/contratos indicados; não auditoria integral do código nem das branches |

O hash do RAR é `641b3b48b16d2fcf7c60e6c0869f00836d5594b67bc081f9726d56dd2d9d64fd`. Seus 49 arquivos coincidem byte a byte com o ZIP anterior de hash `6d0ec332c8f0a7e39a42d8cd4fdba27a1142f54857537610b572a69e229a8e31`. Os três Markdown coincidem com as versões locais; por isso o pacote guarda uma única cópia de cada, entre os 26 arquivos originais locais.

O [inventário de fontes](matrizes/fontes.json) registra cada arquivo, tamanho, hash, nível de inspeção e destino. [Fontes do legado](matrizes/fontes-legado.json) registra separadamente 55 ponteiros versionados; itens apenas catalogados não são declarados lidos integralmente. A extração estrutural do programa OWM e de 114 requisitos TEM/QR/GRA/PCE preserva células e procedência; não promove seu estado funcional.

## Referências Git observadas

ClubAL local: `integration/base44-main`, HEAD `3380fa923bd6e3b05610642da0d283fb81514c7b`; main local `9fb158ddfd942f6fa15abeb370713351f130f965`; ref local de acompanhamento Base44 `46370e599c021c52ee9734249b438f4a664ab33b`. A comparação main local → integração indica dois commits adicionais, sem commits exclusivos da main local. O snapshot remoto antigo do RAR é outra comparação, datada de 22/09. Não houve fetch, pull ou nova consulta ao servidor GitHub nesta tarefa.

O checkout ClubAL possui três alterações preexistentes: `.codex/config.toml` modificado e dois fundos de clima excluídos no working tree. Foram preservadas; o conteúdo da configuração privada não foi copiado. As refs e a lista de caminhos de diferenças estão em [evidências](evidencias/repositorios-inspecionados.json), com `code_diffs_semantically_reviewed=false`.

## Roadmaps recuperáveis

- TEM: fonte atual relata TEM-0…3 verificados localmente; TEM-4 mantém aceite humano/de parque; TEM-5 depende de PCE-1 e está adiado. Preservar os 15 temas, preferências e separação de identidade institucional.
- GRA: fonte atual relata GRA-1…6 e ensaios locais de GRA-7; funcionalidades default-off e gates externos permanecem. Avisos internos; WhatsApp futuro. Usar contratos de grant, revisão/hash, recibo, compensação e recuperação, sem inferir suporte irrestrito.
- OWM: ponteiro indica OWM-12 em preparação sem ativação; último pacote OWM-11E; OWM-13 não aberto. OWM-6H não ativado. Preservar fallback e contratos do legado.
- QR-C1/C2: provas locais relatadas; dispositivos físicos e decisão de fornecedor permanecem gates próprios. QR de estoque, matrícula e check-in não são a mesma autoridade.
- PCE e TEM-5: referências preservadas, adiadas na fonte. Não retomar automaticamente dentro do novo pacote.

Esses são **estados relatados em documentos consultados**, não testes reexecutados nesta entrega. [Roadmap integrado](09_ROADMAP_INTEGRADO.md) define seu uso no destino.

## Conversa e fontes externas

A conversa “Aprimorar prompt do Wissian”, ID `6ab554df-5510-83e9-84ce-da14ee66568c`, foi recuperada. A ferramenta limita cada mensagem a 20.000 caracteres; o prompt de fundação veio truncado no início da seção 25. As seções disponíveis e o pedido atual sustentam o mandato; o trecho ausente não foi inventado nem apresentado como transcrição integral. Os quatro arquivos anexos foram recuperados integralmente, independentemente dessa limitação.

As páginas públicas pontualmente reconsultadas estão no [registro técnico](15_REFERENCIAS_TECNICAS.md). Fontes fiscais, jurídicas, etimológicas e preços mencionados nos originais permanecem referências históricas; esta consolidação não valida situação empresarial, legislação aplicável ou custo atual.

## Lacunas que M0 deve fechar

1. Comparação funcional completa das versões Base44/Codex por capacidade, incluindo mapa de alojamentos, navegação, logos, estados e estruturas alteradas. Cabeçalho já absorvido não prova cobertura de todas as mudanças.
2. Grafo e custo de extração de código, licenças/ativos, versões/lockfiles e testes realmente disponíveis.
3. Schema e maturidade de tenancy/roles/entitlements, incluindo jobs, anexos, cache, exportações e revogação.
4. Seleção da stack e ferramenta de dados por experimento; nenhuma tecnologia nova instalada aqui.
5. Volumes, orçamento, primeiro piloto, dados autorizados, responsáveis e serviços externos.

As lacunas não autorizam completar fatos por suposição. Isolar uma capacidade sem fonte e continuar apenas a parte independente cuja evidência seja suficiente.

## Nova consulta — adendo Continuity 2.1.0

A conversa [Consolidar pacote mestre Wissian](https://chatgpt.com/s/cx_6ab565f2fcd88191a95079873e68d650) foi aberta e lida antes de editar, e sua tarefa original foi recuperada. Confirmou instalação na raiz dedicada e preservação dos 141 arquivos. A conversa “Análise dos projetos Nexus e CC”, ID `6ab55c55-c548-83e9-8a18-dce16aa5a720`, foi consultada como contexto; sua resposta longa veio limitada pela ferramenta. O pedido atual contém explicitamente as decisões necessárias; não foi reconstruído o trecho truncado.

O caminho solicitado com `WISSIAN/_PACOTE...` não existe neste host. A cópia real é `WISSIAN_PACOTE_MESTRE_v2.0.0_2026-09-24/`, abaixo da raiz instalada Wissian. Seus 141 arquivos estavam idênticos aos correspondentes ativos. Inventário recursivo, UTF-8, parsing de todos os JSON/CSV e sintaxe dos scripts cobrem todos os arquivos; leitura semântica da camada vigente e consulta temática das fontes históricas orientam o adendo. Nenhuma fonte histórica é reescrita.

Não foi necessária consulta nova ao código privado de Nexus/CC para adotar a direção já aprovada. Versões/CI/contagens da conversa são relatos de origem, não verificações atuais. Fichas ABS-041…048 explicitam auditoria técnica pendente quando a implementação precisar de caso/algoritmo específico. Fonte ausente não autoriza inventar código; não bloqueia fundação independente.

As fontes técnicas deste adendo são contratos locais e requisito de Gui; não foram escolhidas bibliotecas, APIs externas ou versões de framework. M5-CO.0 fechará limites de fila, retenção, políticas humanas e contrato do consumidor conforme stack real.

## Fonte complementar recuperada integralmente

Após o limite inicial do conector, o link compartilhado [Análise dos projetos Nexus e CC](https://chatgpt.com/share/6ab56aa2-7a48-83e9-aa78-9d85f0317d41), fornecido por Gui durante esta tarefa, permitiu ler no navegador toda a proposta, incluindo arquitetura, dados e pré-mortem. Confirmada infraestrutura interna transversal para módulos, opcional por composição, sem decisão de produto/contratação separada. Isso resolve a lacuna da resposta de análise inicialmente truncada; não resolve retroativamente o trecho ausente do prompt antigo de fundação de outra conversa nem equivale a auditar os repositórios.
