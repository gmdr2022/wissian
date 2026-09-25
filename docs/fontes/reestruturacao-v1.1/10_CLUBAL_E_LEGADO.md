# ClubAL futuro e continuidade do legado

## Dois significados a distinguir

**ClubAL histórico:** nome atual do repositório, aplicações, formatos, paths, scripts, cookies, instaladores e diversos contratos. **ClubAL futuro:** marca reservada para o produto direcionado a clubes. A separação comercial não remove a necessidade de compatibilidade técnica com o primeiro significado.

Não renomear o repositório, remoto, C7 project ID, solution, executável ou IDs de instalador nesta fase só para alinhar nomes. Registrar aliases necessários e manter operação existente até migração testada.

## O que reservar

Documentar a marca e uma hipótese de composição futura: programação de aulas/atividades, comunicação, uso de espaços e exibição em telas para clubes. Não implementar agora cadastro financeiro de associados, venda de ingresso, CRM, e-commerce, academia completa ou gestão de shows.

Um arquivo de escopo reservado e um descriptor marcado como não implementado bastam; não criar aplicação vazia com dez menus “em breve”, novo deploy ou novo banco. ClubAL não aparece como opção contratável/ativa para clientes do produto socioeducativo.

## O que continua atendendo o socioeducativo

Agendas, rotinas, telas, consulta, publicação, dispositivos, atualização e fallback offline podem continuar necessários ao primeiro produto. Não movê-los para uma pasta “ClubAL futuro” inacessível ao socioeducativo. Separar capacidade tecnológica de especialização comercial.

## Legado Python e Windows

Preservar as camadas `app/`, `core/`, `infra/`, `ui/`, `renderers/`, `weather/`, o entrypoint `clubal.py`, os launchers e o empacotamento enquanto houver consumidores. O fato de a fundação SaaS ser extraída do OWM não autoriza descontinuar o funcionamento local ou portar tudo para web.

Antes de mover qualquer arquivo, rastrear imports, resources do PyInstaller, script Inno Setup, atalhos, manifest, paths de dados, consultas por nome de arquivo e validação de distribuição. O executável e o runtime da fonte precisam continuar de acordo com o contrato existente.

## NEXUS e ferramentas auxiliares

NEXUS é relatado como entrada do Product Owner para testar a fonte mais recente [E02]. Preservar separação entre execução de desenvolvimento e aplicativo instalado. Não editar outro repositório NEXUS/C7 por efeito colateral deste pacote. Caso um launcher necessite ajuste fora do checkout, registrar dependência e procedimento, sem editar projeto alheio sem autorização.

Não empacotar `.codex`, AGENTS, cofre, relatórios internos ou ferramentas de pesquisa no produto final. Separação da fundação não é distribuição pública do código.

## Compatibilidade comprovada

Validar launchers, self-test, leitura de dados locais, ausência de rede/escrita e artefatos Windows nos ambientes apropriados. Uma execução em Linux ou browser automatizado não comprova instalador Windows nem dispositivo físico.

Se faltar ambiente, marcar `BLOCKED_ENV` ou `NOT_RUN`, entregar as evidências disponíveis e manter o gate. Não alegar que o pacote Windows foi testado porque `compileall` passou.

## Encerramento

ClubAL futuro fica definido como escopo e marca reservada. Legado necessário fica preservado e coberto pelos testes impactados. Nenhuma perda funcional pode ser escondida sob o rótulo “limpeza”.
