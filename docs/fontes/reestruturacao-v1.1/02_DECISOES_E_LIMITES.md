# Decisões de direção, escopo e limites

## Decisões trazidas por Gui

| ID | Decisão | Consequência |
|---|---|---|
| D01 | Empresa, fundação, produtos e módulos são conceitos diferentes | Não modelar a empresa como tenant nem obrigar a fundação a ter assinatura comercial |
| D02 | Wissian é o nome escolhido para a plataforma-base privada da Fultuma | Marca centralizada; Fultuma Tech Ltda é a empresa proprietária definida por Gui; sem registro ou titularidade formalizada presumidos |
| D03 | Primeiro produto: socioeducativo, independente do PEMSE | Linguagem, regras e fluxos próprios; cliente é configuração isolada |
| D04 | ClubAL fica reservado para clubes | Não desenvolver agora outro produto comercial completo |
| D05 | Reaproveitar o investimento existente | Extração incremental, sem reescrita, cópia geral ou perda da evolução Base44/Codex |
| D06 | O SaaS ainda não gera receita; evitar nova despesa | Sem compra de domínio, infraestrutura ou licenças; desenvolvimento local nos recursos existentes |
| D07 | A fundação deve ser neutra, estável e reutilizável | Critérios objetivos v1; não uma promessa de perfeição ou universalidade |
| D08 | Produtos especializados devem resolver necessidades identificáveis | Fundação não justifica retomar escopo ERP genérico ou lançar muitos nichos |

## Decisões técnicas de execução deste pacote

Estas são as escolhas-padrão para implementação, sujeitas a ajuste técnico fundamentado no checkout, sem reabrir a direção de negócio:

- **Monorepositório privado e monólito modular inicialmente.** Componentes compartilham código versionado; projetos/assemblies podem separar dependências sem introduzir novos serviços de rede. Revisar separação de repositórios só quando houver benefício operacional demonstrado.
- **Composição, não herança de classes em cascata.** Produtos selecionam módulos por contratos explícitos. Nada de branches permanentes por cliente nem cópias que se desatualizam.
- **Tecnologia existente como padrão.** Preservar .NET/PostgreSQL e React/TypeScript/Vite no OWM; manter os adaptadores Python/Tk/Flet e distribuição necessários. Nenhuma migração de stack está autorizada.
- **Neutralidade sem rebatismo destrutivo.** Nomes de apresentação podem mudar; IDs, cookies, storage keys, certificados, protocolos e migrations ficam estáveis ou recebem compatibilidade explícita.
- **Core mínimo e capacidades opcionais.** Agendas, dispositivos, telas, publicação e edge não são requisitos de qualquer SaaS concebível. Reaproveitar quando usados, não torná-los dependências do núcleo.
- **Isolamento real.** Composição de produto, licença comercial, permissão humana, escopo de cliente/unidade e liberação de experimento são verificações distintas.
- **Uma única fonte de continuidade.** Respeitar o C7 configurado e o ponteiro portátil existente. Não criar outra coleção de diários rastreados.

## Diretrizes antigas substituídas de maneira limitada

O AGENTS observado ainda diz “ClubAL remains one product” e define três módulos formais PEMSE. `ProductBoundary.cs` repete essas três strings [E02, E05]. Esses pontos devem ser substituídos por uma definição de plataforma e composição de produto, com testes atualizados para proteger a nova fronteira.

Preservar o significado operacional: Operação edita/publica; Rotinas apresenta conteúdo autorizado; Consulta é somente leitura. O que muda é o alcance: essas superfícies não definem todo o núcleo compartilhado.

Não enfraquecer testes de escopo, autenticação ou privacidade para fazê-los passar após mudar uma string. Atualizar a expectativa de identidade junto com uma prova equivalente ou mais forte da regra protegida.

## Não objetivos

Não desenvolver agora produto de eventos/comércio; motor universal de workflows; novo prontuário completo; integração automática com sistemas governamentais; IA para avaliar adolescentes; motor de cobrança/faturamento; marketplace de extensões; rebranding completo; aplicativo mobile/TV novo; migração de nuvem; microsserviços; plataforma global com implantação em vários países.

Não apagar funcionalidades atuais porque foram retiradas da promessa comercial inicial. Classificar, isolar, preservar e manter gates existentes. A redução é de acoplamento e de frentes de trabalho, não de patrimônio funcional sem análise.

## Condição de encerramento

A fundação v1 tem fronteiras demonstradas, cenário neutro executável, contratos testados, produto socioeducativo recomposto, migração segura e procedimento de atualização/recuperação. Pendências de hardware, uso real e jurídico aparecem como pendentes, nunca aprovadas por inferência. Depois disso, priorizar validação do primeiro produto, não polimento infinito do núcleo.

Fontes de evidência: [estado observado](03_ESTADO_VERIFICADO_REPOSITORIO.md). Justificativas externas: [referências](18_FONTES_E_REFERENCIAS.md). As escolhas técnicas acima são recomendações deste plano, não afirmações de que já estejam implementadas.
