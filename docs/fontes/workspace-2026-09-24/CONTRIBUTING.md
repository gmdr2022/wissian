# Contribuindo com Wissian

## Antes de alterar

Leia o [README](README.md), o [guia fundador](Wissian-Guia-Fundador-da-Plataforma.md), a [arquitetura](docs/architecture/README.md) e o [workflow Git](docs/engineering/git.md). A colaboração é restrita às pessoas autorizadas pela Fultuma.

O projeto está na fase de organização. O próximo trabalho de produto depende da apresentação e reconciliação do plano existente; os diretórios vazios não são autorização para implementar todas as capacidades listadas no guia.

## Preparar o ambiente

Siga [ambiente de desenvolvimento](docs/engineering/ambiente.md). Não há aplicações ou comandos de produto disponíveis ainda. Quando o bootstrap existir, use as versões fixadas e o único gerenciador/lockfile adotado; não instale dependências globais para contornar uma configuração inconsistente.

Use somente dados sintéticos. Nunca copie credenciais, bancos ou arquivos reais de clientes para fixtures, issues, PRs ou exemplos. Valores locais ficam fora do Git; exemplos versionados permanecem sanitizados.

## Definir a mudança

Cada tarefa deve indicar problema, usuário/consumidor afetado, escopo, critérios de aceite e evidência esperada. Classifique a demanda como capacidade de plataforma ou domínio de produto. Uma vertical não deve impor regras ao núcleo compartilhado.

Mantenha um único resultado revisável por PR. Mudanças de arquitetura, contrato público, segurança, dados ou operação atualizam a documentação correspondente. Para decisões duradouras, crie uma ADR numerada com contexto, proposta, alternativas, consequências, status e gatilho de revisão.

## Convenções

- Documentação e comunicação do projeto em português do Brasil; identificadores técnicos em inglês e nomes de diretórios em ASCII/kebab-case, respeitando convenções dos frameworks.
- UTF-8, indentação de dois espaços e LF; scripts `.bat`/`.cmd` usam CRLF. Não reformatar o guia fundador como efeito colateral.
- TypeScript estrito na implementação proposta. Evitar `any` e supressões sem justificativa localizada; validar dados externos em runtime.
- Organizar por capacidade/funcionalidade. Evitar pastas genéricas `common`/`utils` como destino automático de regras de negócio.
- Testes unitários ficam próximos ao código; integração, contratos, isolamento e fluxos completos usam as áreas documentadas na arquitetura.
- Commits seguem Conventional Commits. Exemplos e escopos estão no workflow Git.

## Revisar e verificar

Use o [modelo de PR](docs/templates/pull-request.md). Informe comportamento anterior/novo, validações realmente executadas e limitações. Screenshot, trace e log devem estar livres de dados pessoais e segredos.

Escolha a menor bateria confiável: documentação/configuração exige links e sintaxe; código exige testes relevantes e análise estática; mudança de dados exige migração e recuperação; autorização exige casos negativos entre organizações. A ausência atual de CI não equivale a validação automática.

Mudanças de interface devem avaliar teclado, foco, estados de erro/vazio/espera, contraste, zoom, movimento e internacionalização. Mudanças de API devem avaliar compatibilidade dos consumidores. Documente o que não pôde ser validado.

O PR só fica pronto quando resolve seu objetivo, preserva o comportamento fora do escopo, contém documentação necessária e tem evidência proporcional ao risco. Após existir CI, todos os checks obrigatórios devem passar; aceite de produto e aprovação de produção continuam sendo decisões distintas.

## Dependências e propriedade intelectual

Adicione uma dependência somente com finalidade, compatibilidade, manutenção e licença verificadas. Preserve procedência de componentes de terceiros. Novos colaboradores devem receber as condições de acesso e contribuição definidas pela Fultuma antes de contribuir; este guia técnico não substitui esses instrumentos.

## Publicação e suporte

Commit, push, criação de remote, publicação e deploy por agentes exigem a autorização aplicável; uma proposta de PR não autoriza produção. Relatos sensíveis seguem [SECURITY.md](SECURITY.md).
