# Mandato, precedência e decisões de escopo

## Current routing and historical scope

The dated preparation mandates and consolidation tables below preserve their original scope; references to a future implementation, an open stack decision or no developed SaaS describe those historical stages. For the active application and its accepted partial stack supersession, follow the [README routing](../../README.md), the application's ADR-M09 and current roadmap/evidence. This clarification changes neither architecture contracts nor the authority order below. Engineering continuity follows [chapter 12](12_CONTINUIDADE_E_HANDOFF.md) without a C7/Obsidian prerequisite.

## Resultado e autoridade

O pedido de 24/09/2026 estabelece uma plataforma independente: Core pequeno, operação administrativa por módulos, verticais separadas, reuso seletivo Codex/Base44/TEM/OWM/GRA e preservação do ClubAL. A tarefa presente prepara documentação, ZIP e organização recuperável da pasta local; não implementa o SaaS. Os prompts incorporados são fontes, não autorizações autônomas.

O pacote é suficiente para compreender a missão, recuperar as fontes e escolher a próxima etapa. Não contém o código-fonte inteiro dos produtos, dados reais, acesso remoto garantido ou provas atuais de execução das branches. Essas dependências aparecem como lacunas delimitadas, não como fatos inventados.

## Regra explícita de precedência

1. Pedido atual e autorizações expressas de Gui, respeitadas as políticas superiores de segurança e ferramentas.
2. Instruções vigentes do workspace/escopo; resolver conflito de direção pelo pedido atual, sem estender autorização a outro projeto.
3. Comportamento executável atual para fatos de implementação; novas decisões de produto regem a intenção futura. Divergência entre intenção e código vira requisito, não afirmação de implementação.
4. Esta camada mestre e ADRs para reconciliar as fontes desta entrega. Decisões apenas propostas continuam propostas.
5. Guia fundador para identidade, produto e marca; plano de operação base para seus fluxos, dados e critérios; contratos de origem para invariantes já existentes, sempre na área que lhes cabe.
6. Pacote v1.1, snapshots de branches, notas de continuidade e conversa como evidência histórica. Data mais recente não vence escopo mais específico ou um controle de segurança.

Não escolher por maioria de documentos. Registrar origem, tópico, regra substituída, alcance da substituição, consequência, teste e gatilho de revisão. Uma nova tarefa válida pode superseder esta versão; atualizar o contrato ativo sem adulterar fontes preservadas.

## Conflitos resolvidos nesta consolidação

| ID | Fontes em tensão | Regra aplicada e consequência |
|---|---|---|
| CF-01 | v1.1 mantém um repositório; pedido atual exige Wissian próprio | Repositório Wissian separado do ClubAL. Monorepo interno do Wissian continua opção; não fundir históricos. GitHub privado é destino futuro, sem criar remote agora. |
| CF-02 | v1.1 prioriza socioeducativo; plano recente prioriza administração | Operação base E0–E3 é primeiro fluxo útil; socioeducativo conserva roadmap e contratos como vertical futura. Nenhuma funcionalidade é descartada por sair do primeiro recorte. |
| CF-03 | v1.1 diz empresa não é tenant; plano usa Fultuma como cliente | Empresa jurídica, operadora da plataforma e organização usuária são papéis distintos. Fultuma pode ter tenant operacional comum sem privilégios universais. |
| CF-04 | v1.1 orienta preservar .NET/Vite; ADR local propõe Next/Nest | Não há stack Wissian implementada. M0 caracteriza reuso; ADR decide por custo total/evidência. Proibir rewrite automático e transplante integral. A decisão vale para o destino, sem mudar stack ClubAL. |
| CF-05 | operação base chama documentos/busca de fundação | Oferta base não é Core técnico. Registro de documentos, busca de domínio e financeiro são módulos; Core oferece apenas contratos mínimos exigidos. |
| CF-06 | pacote antigo chama a alterar AGENTS/ProductBoundary no ClubAL | Nesta missão o ClubAL é fonte preservada. Adaptações ocorrem no destino; eventual correção da origem requer tarefa própria. |
| CF-07 | instruções históricas mandam prosseguir/commitar conforme fluxo | Nenhum prompt arquivado amplia o pedido atual. Sem implementação, commit/push, publicação ou dados reais nesta preparação. |
| CF-08 | snapshot antigo 75f1fc3/8ac5f09 e compare +27 | Leitura local atual: integração 3380fa9, main local 9fb158d, diferença 0/2. São referências distintas; não substituir uma pela outra nem afirmar frescor remoto sem fetch. |
| CF-09 | roadmap antigo não desenvolver ERP; pedido amplia capacidades | Catálogo amplo é direção de produto; cada capacidade precisa de fluxo e prioridade. Não construir ERP completo de uma vez nem soldar módulos ao núcleo. |
| CF-10 | ocultar módulo vs ausência física de código | Mesmo artefato confiável pode servir tenants com módulos diferentes, autorizados no servidor. Uma composição neutra separada deve provar independência de build e execução; desativação por tenant não exige recompilar. |
| CF-11 | Core limpo vs cookies/IDs/migrations ClubAL | Identidades novas do Wissian são próprias; IDs antigos só atravessam adapters explícitos quando houver consumidor compatível. Nunca compartilhar cookies/chaves para simplificar SSO nem renomear em massa a origem. |
| CF-12 | E0–E3 como único prompt vs missão de plataforma | E0–E3 vira primeira trilha administrativa M2–M4, precedida de M0/M1. Modularidade, herança, catálogo, verticais, operação e expansão continuam objetivos mestre. |

CF-08 não certifica o conteúdo das branches. CF-04 permanece decisão técnica aberta com procedimento fechado; não há razão para bloquear a entrega documental esperando uma escolha de implementação prematura.

## Direção adotada e propostas

Adotado pelo pedido: independência do repositório; Core mínimo; módulos selecionáveis; verticais separadas; preservação e absorção por capacidade; backup antes da organização; proibições explícitas. Consolidação recomendada: monólito modular, primeiro fluxo E0–E3, mesma versão por organização, versões de contrato e Git de branches curtas. Pendentes: stack/ORM/provedor de identidade, arquitetura física de isolamento, primeiro piloto, volumes, orçamento, hospedagem, integrações e empacotamento comercial.

Conhecimento de domínio e qualidade de execução sustentam diferenciação; complexidade, siglas ou promessa de produto impossível de copiar não são critérios técnicos. O resultado deve ser demonstrado em tarefas reais, confiabilidade, segurança e evolução; não há garantia de exclusividade ou sucesso comercial.

## Adendo 2.1.0 — conflitos resolvidos para Nexus/CC

| ID | Tensão | Regra de precedência e alcance |
|---|---|---|
| CF-13 | Arquitetura ilustrativa põe Events/Notifications no Core | Reutilizar contratos mínimos de evento/auditoria; notificações continuam capacidade/adapter opcional conforme capítulo 03. Nenhum broker obrigatório em M1. |
| CF-14 | Nova capability vs tarefas/aprovações/agenda já previstas | Evoluir coordenação e atenção; owners originais mantêm estado oficial e aprovação. Sem segundo calendário, financeiro ou motor de autorização. |
| CF-15 | Reaproveitar investimento vs portar desktop | Preservar princípios/casos/decisões; aposentar WPF, shell arbitrário e integrações Codex como destino SaaS. Repositórios não são alterados. |
| CF-16 | Trio prioritário vs primeiro ciclo administrativo | M2–M4/E0–E3 continuam primeiro valor; fundações mínimas em M0/M1; MVP em M5-CO, sem renumerar M5a/b/c ou verticais. |
| CF-17 | `tenant_id` sugerido vs organização vigente | Uma autoridade: organização SaaS; mapear alias de envelope, sem duplicar modelo. Workspace/unidade só se já fizerem sentido no domínio. |
| CF-18 | Continuidade/handoff C7 e uso legado do CC vs produto do cliente | Capítulo 12 e processo de aceite de engenharia não são runtime de Continuity nem exigem continuar CC como produto. |

O pedido atual autoriza aprimorar o pacote; não inicia desenvolvimento, fechamento técnico dos projetos ancestrais ou ação externa. [ADR-M08](adrs/ADR-M08_CONTINUIDADE_OPERACIONAL.md) complementa decisões existentes; desenho novo permanece proposta até teste no destino.

## CF-19 — reforço do pedido durante a revisão

Gui tornou imprescindível não recriar do zero: fragmentar/dividir e reutilizar de forma aprimorada as bases existentes para a versão administrativa limpa e addons especializados. Esta orientação restringe alternativas anteriores de reimplementação: somente lacuna/incompatibilidade demonstrada pode justificar código de adaptação; substituição material precisa de revisão com Gui. M0 verifica procedência e custo de separação, sem presumir que o legado está vazio nem que tudo citado já foi auditado. A arquitetura/stack vigente compatível é o ponto de partida, e especialidades não migram para Core.

## CF-20 — base administrativa e retirada do controle preparatório

O pedido posterior de Gui em 24/09/2026 confirma o uso da Fultuma como organização comum para financeiro, controle fiscal administrativo e apoio contábil, com prioridade ao benefício verificável sobre controles locais. [Capítulo 17](17_BASE_ADMINISTRATIVA_E_TRANSICAO.md) delimita a oferta base, os testes de utilidade/segurança e a transição. “Core básico” na experiência do cliente não move regras financeiras ao núcleo técnico. M5/M8 permanecem expansões condicionais, sem obrigação de implementar marca, contratos ou todas as pastas da empresa. A autorização atual abrange revisão documental e retirada recuperável do kit Fultuma; o desenvolvimento será solicitado depois.
