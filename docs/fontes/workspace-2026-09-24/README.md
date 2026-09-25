# Wissian Platform

Plataforma-base da **Fultuma Tech Ltda.** para produtos digitais operacionais.

**Fase atual: organização técnica anterior ao desenvolvimento.** Há documentação, convenções e diretórios reservados. Ainda não há aplicação, API, banco configurado, dependências instaladas, CI ou ambiente publicado.

## Referência e escopo

O [guia fundador](Wissian-Guia-Fundador-da-Plataforma.md) define a identidade e os princípios do produto. A plataforma compartilha capacidades justificadas; produtos especializados preservam seus próprios domínios. Wissian não é sinônimo de ClubAL nem do futuro produto socioeducativo.

Este repositório é dedicado à engenharia de Wissian. Administração da Fultuma, contratos comerciais e arquivos pessoais pertencem aos respectivos espaços da empresa.

Está disponível o [plano da operação base](PLANO_WISSIAN_OPERACAO_BASE.md), que propõe a Fultuma como primeira organização usuária dos mesmos recursos oferecidos aos clientes, começando por documentos, notas fiscais e financeiro. O [pedido preparado para o Codex](INICIAR_WISSIAN_NO_CODEX.md) delimita uma futura implementação local das etapas E0–E3; sua criação não autoriza nem inicia desenvolvimento.

As recomendações técnicas abaixo devem ser confrontadas com esse plano e com eventual plano anterior disponibilizado **antes do bootstrap das aplicações**. A lista de capacidades do guia não constitui backlog obrigatório.

## Direção técnica proposta

| Camada | Recomendação | Motivo para Wissian |
| --- | --- | --- |
| Organização | Monorepo com pnpm workspaces | Coordenar aplicações e contratos com um lockfile |
| Interface web | TypeScript, React e Next.js | Compor experiências web por funcionalidade |
| API | TypeScript, Node.js LTS e NestJS | Isolar capacidades em módulos e atender diferentes consumidores |
| Persistência | PostgreSQL | Transações, integridade relacional e isolamento em camadas |
| Contratos | HTTP/REST e OpenAPI | Integrações independentes da linguagem do consumidor |
| Operação | Contêineres; Compose para desenvolvimento | Ambiente reproduzível sem escolher nuvem antecipadamente |

Esta é uma recomendação de arquitetura, não uma stack instalada ou uma promessa de escala. Versões, ORM, provedor de identidade e hospedagem ficam para a reconciliação com o plano. A justificativa e as alternativas estão na [ADR 0001](docs/architecture/decisions/0001-base-tecnica.md).

## Organização

```text
.
├── Wissian-Guia-Fundador-da-Plataforma.md  # fonte original preservada
├── README.md                             # entrada de novos colaboradores
├── CONTRIBUTING.md                       # contribuição e revisão
├── AGENTS.md                             # instruções locais para agentes
├── SECURITY.md                           # requisitos e tratamento privado de falhas
├── NOTICE.md                             # titularidade informada
├── apps/                                 # futuras aplicações executáveis
├── packages/                             # futuros contratos e bibliotecas reutilizadas
├── database/                             # futuras migrações e fixtures sintéticas
├── infra/                                # futura configuração de infraestrutura
├── scripts/                              # futura automação de engenharia
├── tests/                                # futuros testes entre componentes
└── docs/
    ├── architecture/                     # estrutura e decisões técnicas
    ├── engineering/                      # ambiente e workflow Git
    └── templates/                        # modelo de pull request
```

Os seis diretórios reservados contêm somente `.gitkeep`. A [arquitetura de pastas](docs/architecture/README.md) distingue esse estado da estrutura futura de código.

## Começar a colaborar

1. Leia o guia fundador e a arquitetura.
2. Consulte [CONTRIBUTING.md](CONTRIBUTING.md), [ambiente](docs/engineering/ambiente.md) e [workflow Git](docs/engineering/git.md).
3. Reconcile o plano de desenvolvimento com a ADR antes de criar código ou dependências.

Por enquanto, basta Git e um editor com suporte a Markdown/EditorConfig. Não há comando de instalação, execução, lint, teste ou build de produto. Esses comandos só serão publicados depois de implementados e verificados.

O repositório local foi inicializado em `main`. Remote, primeiro commit e proteções de servidor dependem de etapas futuras autorizadas. Confira sempre o estado real com `git status`.

## Documentação e operação

- [Arquitetura e fronteiras](docs/architecture/README.md): responsabilidades, dados e estrutura futura.
- [ADR 0001](docs/architecture/decisions/0001-base-tecnica.md): premissas, alternativas e critérios de revisão.
- [Ambiente](docs/engineering/ambiente.md): arquivos criados agora e configurações do futuro bootstrap.
- [Git](docs/engineering/git.md): branches, commits, revisão e releases.
- [Segurança](SECURITY.md): requisitos de implementação e comunicação de vulnerabilidades.
- [Plano da operação base](PLANO_WISSIAN_OPERACAO_BASE.md): fluxos administrativos, cobertura das áreas da empresa, etapas e critérios de aceite.
- [Iniciar no Codex](INICIAR_WISSIAN_NO_CODEX.md): pedido pronto para a primeira implementação local, quando autorizado por Gui.

Quando existirem, acrescentar ao README os comandos reais, variáveis necessárias, URLs de desenvolvimento, testes, suporte e links para procedimentos de operação/recuperação. Contratos compartilhados ficam neste repositório; memória interna de trabalho não substitui documentação para colaboradores.

## Titularidade

A titularidade indicada pelo responsável do projeto é da **Fultuma Tech Ltda.** Consulte [NOTICE.md](NOTICE.md). Não foi adotada licença de código aberto nem realizada publicação.
