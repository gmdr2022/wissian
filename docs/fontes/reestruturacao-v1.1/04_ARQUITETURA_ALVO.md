# Arquitetura alvo — compartilhar tecnologia, separar produtos

**Estado:** desenho orientador a materializar incrementalmente, não descrição de código pronto.

## Modelo em quatro camadas

```text
Fultuma Tech Ltda (marca corporativa: Fultuma; formalização não comprovada neste pacote)
  ├─ Fundação privada — núcleo da plataforma Wissian
  │    identidade e escopos / contratos de auditoria / configuração comum
  │    primitives de módulo / componentes visuais básicos / testes de base
  ├─ Capacidades reutilizáveis opcionais
  │    agenda / publicação minimizada / dispositivos / telas / consulta
  │    conhecimento / relatórios / releases / edge, conforme consumo real
  ├─ Produto socioeducativo (nome comercial pendente)
  │    suas regras / seus fluxos / suas permissões e projeções
  └─ ClubAL (produto para clubes, reservado)
       composição futura, não desenvolvimento paralelo agora

Clientes do produto socioeducativo
  ├─ organização A → unidades, usuários, configuração e dados isolados
  └─ organização B → mesmas regras de produto, configuração e dados isolados
```

Empresa não é tenant. Produto não é organização. Unidade não é produto. Módulo não é papel de acesso. Assinatura não é autorização. Uma mesma pessoa pode participar de mais de uma organização: a sessão deve manter seleção e escopo explícitos, sem compartilhar dados por coincidência de usuário ou e-mail.

## Dependências permitidas

- Núcleo: depende apenas de contratos e componentes realmente transversais; não conhece cliente, vertical, SDK Base44, instalação PEMSE ou regras de alojamento.
- Capacidade compartilhada: depende do núcleo e de contratos explícitos de outras capacidades. Não consulta diretamente tabelas privadas de outra capacidade ou vertical.
- Módulo socioeducativo: depende de núcleo e capacidades selecionadas. Seus DTOs e regras não retornam como dependência reversa do core.
- Host/composição de produto: conecta implementações, endpoints, migrações e UI. Pode conhecer os módulos que compõe; esse host NÃO é o núcleo limpo.
- Adaptadores: unem tecnologia externa/legado aos contratos. Não duplicam a autoridade de domínio ou escrevem simultaneamente duas fontes de verdade.

Construir um grafo acíclico verificável. Um projeto `Core` que referencia o atual `ClubAL.Domain` inteiro, incluindo Socioeducational, não é independente só porque o nome mudou. Um barrel TypeScript que reexporta todos os módulos também pode contaminar o bundle neutro.

## Organização física: preservar caminhos antes de mover

Começar mapeando os projetos atuais, isolando namespaces/funções e composição; extrair projetos/assemblies só onde necessários para provar fronteiras. Manter `ClubAL.sln`, entrypoints e namespaces estáveis enquanto isso reduz risco. Propor nomes novos neutros como `Foundation.*` somente após verificar convenções; não tornar a marca Wissian um requisito de renomeação ampla.

A árvore abaixo é **conceitual**, não uma ordem de criar toda a estrutura:

```text
src/backend/<projetos existentes ou extraídos>
    foundation/             contratos e implementação neutra
    capabilities/           recursos opcionais compartilhados
    verticals/socioeducational/
    composition/            hosts e conectores de produto
src/web/operador/<estrutura atual adaptada>
    foundation/             shell e UI primitives
    capabilities/
    products/socioeducational/
    composition/
<legado existente>           permanece operável via adaptadores
```

Não espelhar automaticamente todos esses diretórios em cada camada. Registrar o mapa real origem→destino, referências afetadas e teste que demonstra cada extração. Evitar dezenas de projetos vazios ou um pacote “Common” que recoloque todo o acoplamento no mesmo lugar.

## Prova de fundação limpa

O resultado deve oferecer um host/smoke de desenvolvimento privado que inicializa a fundação, apresenta identidade neutra e executa um fluxo mínimo com organizações/unidades sintéticas sem registrar ou referenciar o domínio socioeducativo. Esse host não é outro produto comercial nem novo backend produtivo paralelo.

Provar quatro dimensões:

1. **Código:** grafo de dependências da fundação não alcança implementações socioeducativas ou de clubes.
2. **Execução:** sem o módulo, não se registram seus endpoints, jobs, hubs, seeds ou migrações de instalação nova do módulo; acesso direto permanece negado.
3. **Artefato:** distribuição limpa não embute assets, fixtures, código de negócios ou configurações de clientes excluídos. Exceções históricas de compatibilidade ficam em adaptador/artefato legado explicitamente separado.
4. **Experiência:** nenhum rótulo, menu, tela vazia, ajuda ou pesquisa do nicho aparece na composição neutra.

Banco antigo pode conter tabelas históricas inertes: não as apagar por “pureza”. Independência da fundação não exige destruição da história. O plano de migration e o bootstrap do cenário neutro precisam ser definidos em ADR antes de separar persistência. Não fingir independência usando storage falso quando se afirma ter validado persistência real.

## Fundação mínima versus plataforma infinita

Extrair primeiro capacidades já comprovadamente transversais: resolução de escopo, identidade/acesso, contratos de configuração e auditoria, branding de apresentação e primitives de composição. Não reimplementar provedores de autenticação existentes.

O catálogo de módulos deve ser simples, tipado e fechado em build/configuração confiável. Não implementar carregamento de DLL/JavaScript de terceiros, DSL geral, marketplace, container orchestration ou serviço universal de provisioning.

## Um repositório, possíveis artefatos diferentes

Código comum não exige um banco de produção compartilhado entre produtos nem releases sincronizadas para sempre. Inicialmente, manter o fluxo operacional existente, com versões e composição explícitas. Uma futura divisão de repositórios só é aceitável se consumir pacotes/versionamento comuns, com estratégia de atualização e suporte.

Uma cópia via template do GitHub não é um canal automático de atualização do núcleo [S04]. O mecanismo de reutilização deste plano é dependência de componentes versionados e testes de integração.

## Decisão de arquitetura verificável

Escolher o menor desenho que permita extrair a fundação e preservar o produto atual. Para cada camada nova, responder: quem consome, qual contrato isola, que teste falharia se a fronteira fosse violada e que complexidade elimina. Sem resposta concreta, não adicionar a camada.

Referências: [ADRs](adrs/README.md), [contratos](06_CONTRATOS_DE_MODULOS.md), [migração](07_DADOS_MIGRACOES_COMPATIBILIDADE.md).
