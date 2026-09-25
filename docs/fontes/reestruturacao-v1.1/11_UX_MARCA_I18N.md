# Experiência, identidade e preparação internacional

## Objetivo da interface

O usuário do produto deve compreender sua operação sem conhecer a arquitetura. Não expor “tenant”, “core”, “bounded context”, “manifest”, “feature flag” ou estrutura de módulos técnicos nas telas de plantão. Nomear a ação/objeto real e preservar vocabulário institucional onde ele faz sentido.

Na fundação neutra de laboratório, usar apenas conceitos transversais: acesso, organização, unidade, perfil e preferências. No produto socioeducativo, usar rótulos próprios do domínio. Nomes de organização vêm da configuração e não de condicionais `if name == PEMSE`.

## Quatro camadas visuais

1. Fabricante: Fultuma, marca corporativa de Fultuma Tech Ltda conforme definição de Gui; assinatura discreta e sem presumir formalização jurídica comprovada.
2. Produto: nome, logo e linguagem próprios da composição.
3. Organização: marca institucional e identificação da unidade, com origem e permissão de alteração.
4. Pessoa: tema, contraste, tamanho/leitura e outras preferências existentes.

Não sobrepor a marca institucional às preferências pessoais sem regra já aprovada. O histórico observado relata convergência Base44 seletiva no cabeçalho sem substituir os temas [E03]. Preservar esse comportamento e os tokens/componentes existentes; não redesenhar tudo.

## Identidade da plataforma e dos produtos

Nome da plataforma: **Wissian**. Na documentação técnica, `Wissian Platform` é uma forma admitida; a assinatura institucional é `Wissian · A platform by Fultuma`. `Core` descreve o núcleo técnico, sem exigir outra marca ou prefixar todos os componentes. Nome socioeducativo: pendente, não inventar marca comercial definitiva. `ClubAL`: reservado para clubes, mantendo aliases técnicos históricos. Priorizar a marca do produto na experiência do respectivo cliente.

Centralizar textos de marca usando o mecanismo de configuração existente. Separar ID de produto, display name, nome empresarial e nomes legados. Não incluir a história etimológica da marca como requisito técnico ou material comercial aprovado.

## Acessibilidade e uso em plantão

Preservar teclado, ordem de foco, nomes acessíveis, estados de loading/erro, confirmação para ações sensíveis, rascunhos não salvos e ausência de dependência exclusiva de arrastar/soltar. Informação importante não deve depender apenas de cor ou animação.

Executar os testes reais de layouts existentes e casos a 320 px/desktop; medir reflow, menus, conteúdo longo e zoom quando a ferramenta permitir. Usar dados fictícios e inspecionar screenshots. Automação não equivale a homologação com leitor de tela ou aceite de usuários; esses estados ficam separados.

Uma interface neutra não é uma tela vazia após esconder módulos. O cenário base deve ter um fluxo útil mínimo de acesso/seleção autorizada/preferências e mensagens apropriadas, sem links quebrados para funcionalidades ausentes.

## Internacionalização sem projeto global prematuro

Manter português brasileiro como experiência inicial. Reutilizar infraestrutura de tradução existente; não adicionar biblioteca de i18n só por rebranding. Novos textos devem ficar centralizáveis e separáveis de IDs/enum persistido. Testar comprimento de texto e fallback quando houver chave ausente.

Não concatenar frases traduzíveis com fragmentos rígidos. Preservar Unicode, formatação localizada e acessibilidade. Datas de negócios locais e instantes globais são conceitos diferentes: mapear a semântica existente antes de converter todo campo para UTC. Rotinas recorrentes precisam considerar o fuso da unidade, não o fuso do servidor.

Usar mecanismo de fuso adequado ao runtime atual e prever adaptação Windows/IANA se isso for necessário; não prometer que uma string de fuso funcionará igualmente em qualquer sistema sem teste. Não alterar datas históricas na limpeza.

Idiomas, moedas, formatos fiscais, residência de dados e regulamentação estrangeira são decisões de expansão posteriores. “Preparada para evolução” não é “compatibilidade global comprovada”.

## Aceite de UX

Composição neutra sem rótulos/menus/assets do nicho; produto socioeducativo preserva seus fluxos e preferências; cliente A/B corretamente identificado sem vazamento; nomes legados necessários são invisíveis ou discretamente compatíveis; não há regressão de teclado/reflow/rascunhos; nenhuma oferta comercial indisponível aparece como pronta.
