# Workflow Git

## Modelo recomendado

Usar **trunk-based development com branches curtas e pull requests**, semelhante ao [GitHub flow](https://docs.github.com/en/get-started/using-github/github-flow). A integração acontece em `main`; não criar `develop` ou branches permanentes por ambiente. O conceito também se aplica a outro provedor Git.

Cada branch resolve um resultado pequeno e deve durar preferencialmente até dois dias úteis. Se a tarefa for maior, dividir a entrega com compatibilidade preservada; flags só quando houver caso concreto e plano de remoção. Branch de release prolongada exige necessidade de manter versões simultâneas.

```text
main ────●────────────────●────────
          ╲              ╱
           feat/access-policy
                  PR + checks + squash
```

Nomes sugeridos: `feat/access-policy`, `fix/tenant-isolation`, `docs/setup-guide`, `chore/tooling`. Incluir identificador de tarefa quando existir, sem inventar tickets.

## Estado local e primeiro commit

O repositório foi inicializado localmente em `main`, ainda sem primeiro commit ou remote. Em Git, essa branch está em estado inicial, sem histórico. A organização atual não foi enviada a nenhum serviço.

Antes de criar a primeira branch de trabalho, revisar os arquivos e realizar o primeiro commit quando autorizado, por exemplo `chore(repo): organiza a base de engenharia`. Selecionar explicitamente o conteúdo; não usar `git add -A` sobre material desconhecido. Depois desse marco, aplicar o fluxo normal de branches curtas.

Agentes não fazem commit, push, alteração de remote ou publicação por inferência desta página; dependem da autorização da tarefa.

## Commits

Adotar [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/):

```text
tipo(escopo opcional): descrição objetiva
```

Tipos: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `build`, `ci`, `perf` e `revert`. Preferir escopos funcionais (`access`, `organizations`, `api`, `web`, `database`, `infra`, `repo`). Descrição em português; identificadores e nomes técnicos mantêm a grafia original.

```text
docs(architecture): define limites entre plataforma e produtos
fix(access): impede leitura de outra organização
feat(api)!: altera contrato de associação a organizações

BREAKING CHANGE: descreve o contrato removido e a migração do consumidor.
```

O exemplo de quebra ilustra a convenção, não uma alteração existente. O título do PR deve seguir o mesmo formato quando virar um squash commit. Evitar `wip`, `ajustes` e mensagens que não expliquem o resultado.

## Pull request e proteções

O [modelo de PR](../templates/pull-request.md) inclui objetivo, impacto, evidência e recuperação. Recomenda-se squash merge para manter um commit coerente por mudança; não reescrever o histórico compartilhado. Branches concluídas podem ser removidas pelo fluxo autorizado do provedor.

Ao criar o repositório remoto, configurar e verificar suporte efetivo a: PR obrigatório, checks requeridos, bloqueio de force push/exclusão de `main` e revisão por responsável. Proteger alterações nos workflows e configurações de segurança. Permissão de publicação de pacote/deploy deve ser separada de contribuição comum.

Quando houver dois ou mais mantenedores, exigir ao menos uma aprovação de outra pessoa e revisar novamente após mudanças materiais. Enquanto houver apenas um, não alegar revisão independente nem criar uma regra impossível de cumprir; registrar autorrevisão e checks, mantendo revisão humana adicional para mudanças de alto risco.

`CODEOWNERS` só entra após identificar contas/equipes reais e habilitar a exigência no provedor. Um arquivo por si só não impõe proteção. **Nenhuma dessas proteções remotas está configurada nesta fase.**

## Versionamento e entrega

Quando existir artefato publicável, usar tags SemVer, começando em `0.x.y` durante validação inicial. API pública precisa de política de compatibilidade própria; mudar a tag não migra consumidores. Pacotes com consumidores independentes poderão ter versões próprias quando isso trouxer benefício.

Não criar tags/releases vazias agora. Changelog será derivado das mudanças realmente entregues, com instruções de migração quando necessárias. Um commit em `main`, um build validado e um deploy em produção são fatos diferentes.

Correção urgente segue branch curta a partir de `main`, com os checks relevantes e recuperação definida. Urgência não autoriza force push, vazamento de segredo ou redução silenciosa dos controles.
