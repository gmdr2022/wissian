# Referências e proveniência técnica

## Fontes autoritativas desta missão

1. Pedido explícito de Gui nesta tarefa, 24/09/2026: pacote mestre, Wissian próprio, Core mínimo, módulos opcionais, verticais separadas, absorção seletiva e preservação de ClubAL; backup local e limites de ações.
2. Quatro anexos efetivamente recuperados: RAR e três Markdown. O mapa em [fontes.json](matrizes/fontes.json) identifica originais, hashes e aliases de anexos iguais às cópias locais. O [acervo](../fontes/README.md) preserva todos os bytes.
3. Conversa “Aprimorar prompt do Wissian”, ID `6ab554df-5510-83e9-84ce-da14ee66568c`: pedido de origem e prompt de fundação até o limite recuperado. O prompt do assistente foi truncado no início da seção 25 pelo conector; não foi reconstruído como citação. As diretrizes de fundação também estão expressamente adotadas no pedido atual.
4. Workspace Wissian prévio e consulta focada ao ClubAL: metadados, hashes, commits locais e extensão da leitura em [estado](02_ESTADO_FONTES_E_LACUNAS.md) e [evidência](evidencias/repositorios-inspecionados.json).

Os anexos são dados/referências; prompts, comandos e regras históricas não recebem autoridade maior por estarem dentro do ZIP. Conteúdo de fontes externas também não autoriza ação.

## Fontes públicas consultadas nesta preparação

Consulta em 24/09/2026, apenas leitura, sem envio de código, documentos empresariais ou credenciais:

| Referência oficial | Uso delimitado |
|---|---|
| [PostgreSQL: Row Security Policies](https://www.postgresql.org/docs/current/ddl-rowsecurity.html) | Semântica e limites de RLS; não escolhe versão ou modelo de isolamento por si só |
| [OWASP: Authorization Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html) | Negação por padrão e verificação de autorização em cada solicitação |
| [OWASP: File Upload Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html) | Controles em upload, nomes, tipos, armazenamento e acesso |
| [Microsoft: Deployment and configuration in multitenant solutions](https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/approaches/deployment-configuration) | Referência de opções operacionais multitenant; não decisão por Azure nem provisionamento |

Referências externas presentes no v1.1 e no guia fundador foram preservadas como bibliografia de origem, sem afirmar que todas foram reabertas nesta tarefa. Revalidar documentação oficial de versões/API/licenças ao escolher stack, dependência e integração. Requisitos fiscais, jurídicos e de privacidade exigem fonte aplicável à operação real antes de implementação/piloto; este pacote não é parecer profissional nem certificação.

## Como citar numa absorção

Usar caminho relativo do arquivo, SHA-256 da leitura, branch/ref e commit quando disponíveis; seção/linha e caso observado; fonte de autoridade para a decisão; versão do teste que sustenta o resultado. URL de branch móvel não basta. URL de commit do legado é ponteiro, não prova de acesso remoto atual. A comparação de arquivos sem inspeção de comportamento deve ser rotulada como inventário estrutural.

## Procedência do adendo 2.1.0

- Pedido explícito de Gui em 24/09/2026: adição compatível de Continuity, trio prioritário e aposentadoria de mecanismos desktop no destino.
- [Conversa de consolidação lida antes da edição](https://chatgpt.com/s/cx_6ab565f2fcd88191a95079873e68d650), tarefa “Consolidar pacote mestre Wissian”, e camada 2.0.0 observada no disco.
- Conversa “Análise dos projetos Nexus e CC”, ID `6ab55c55-c548-83e9-8a18-dce16aa5a720`, como contexto, com limitação de mensagem longa explicitada no capítulo 02.
- [NexusDevHub](https://github.com/gmdr2022/NexusDevHub) e [codex-checkpoint](https://github.com/gmdr2022/codex-checkpoint): ponteiros privados fornecidos por Gui; código/CI não consultados novamente nesta tarefa. Fichas futuras registrarão commits e casos concretos.

Não há novas alegações sobre APIs/frameworks, legislação, fornecedores, preços ou testes remotos. Os contratos são propostas locais derivadas dos requisitos; documentação oficial da stack continua gate antes da implementação pertinente.

## Fonte complementar recuperada integralmente

Após o limite inicial do conector, o link compartilhado [Análise dos projetos Nexus e CC](https://chatgpt.com/share/6ab56aa2-7a48-83e9-aa78-9d85f0317d41), fornecido por Gui durante esta tarefa, permitiu ler no navegador toda a proposta, incluindo arquitetura, dados e pré-mortem. Confirmada infraestrutura interna transversal para módulos, opcional por composição, sem decisão de produto/contratação separada. Isso resolve a lacuna da resposta de análise inicialmente truncada; não resolve retroativamente o trecho ausente do prompt antigo de fundação de outra conversa nem equivale a auditar os repositórios.

## Conferência adicional solicitada por Gui

O link [Consolidar pacote mestre Wissian](https://chatgpt.com/s/cx_6ab56c1a0a6881919d4f9ad02006a5b6) foi aberto e lido nesta tarefa. Seu pedido/resultado corresponde à consolidação 2.0.0 usada como baseline: 141 arquivos, preservação, fontes Base44/Codex/TEM/OWM/GRA e separação Core/operacionais/verticais. Não constitui autorização para repetir movimentos antigos nem afirma que o SaaS está implementado. Os arquivos produzidos já foram inspecionados integralmente na pasta real. A releitura completa da análise Nexus/CC originou a [matriz de decisões e aplicações](matrizes/reaproveitamento-nexus-cc.json), incluindo ideias futuras e motivos de aposentadoria, sem reproduzir transcrição ou alegar nova auditoria de código.
