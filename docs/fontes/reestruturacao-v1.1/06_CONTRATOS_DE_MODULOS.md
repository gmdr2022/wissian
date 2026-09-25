# Contratos de composição, módulos e acesso

## Princípio

A composição é uma decisão confiável do produto; não um payload enviado pelo navegador que concede autoridade. Os exemplos de `contratos/` são referências de formato a adaptar ao que já existe, não configuração pronta para colar na produção.

## Contratos mínimos

| Contrato | Conteúdo necessário | Não pode conter |
|---|---|---|
| ProductDescriptor | ID estável, nome exibido configurável, versão de contrato, módulos selecionados, marca padrão | Dados de clientes, secrets, permissões concedidas automaticamente |
| ModuleDescriptor | ID, versão, dependências, capacidades expostas, registro de rotas/jobs/UI, migrações e permissões declaradas | Código arbitrário remoto ou acesso automático a qualquer tenant |
| ExecutionScope | Usuário/sessão ou dispositivo, produto, organização, unidade, autoridade e correlação validadas | IDs confiados cegamente de headers, query string ou UI |
| CapabilityContract | Operação explícita, DTO minimizado, erro controlado, compatibilidade | Entidades EF/DbContext de outra camada ou dados brutos do nicho |
| AuditEnvelope | Ação/recurso, escopo, ator, instante, correlação e resultado | Transcrição de dados pessoais ou payloads integrais desnecessários |
| BrandPresentation | Nome exibido, asset revisado, tokens visuais por contexto | Alteração de issuer/audience/cookie/storage ou autoridade institucional |

Reaproveitar estruturas equivalentes existentes; não manter `ProductDescriptor2`, outro identity service ou um segundo catálogo de permissions. Versões de contrato e IDs não devem acompanhar mudanças cosméticas de marca.

## Estados distintos de habilitação

A disponibilidade efetiva exige todas as condições aplicáveis: módulo incluído no artefato/composição; recurso liberado no ambiente; direito comercial existente quando exigido; autorização da identidade; escopo de cliente/unidade/produto; política de operação e gates humanos/institucionais satisfeitos.

Desabilitação não apaga dados nem equivale a revogar usuário. Inadimplência não é uma revogação de segurança. Reativar módulo não inventa consentimento/aprovação nem repete migrações de forma destrutiva.

A Microsoft distingue feature flags de direitos de licença [S01]. Neste trabalho, manter/adaptar entitlements existentes; não instalar sistema financeiro nem usar billing como autorização operacional.

## Registro e falhas

Registrar módulos permitidos por código/composição tipada. Validar IDs únicos, versão suportada, dependências presentes, ciclos, incompatibilidades e capacidade requerida. Módulo obrigatório ausente bloqueia a composição com erro sanitizado; módulo opcional indisponível desabilita apenas sua superfície de forma explícita, sem estado parcialmente permitido.

Não fazer fallback silencioso para “todos os módulos” ou “primeiro cliente”. O default atual dos clientes existentes deve ser preservado por um mapeamento de compatibilidade explícito. Instalação nova sem produto válido falha fechada; não adivinhar socioeducativo por uma string legada.

Rotas e jobs ficam sob composição server-side. UI filtra navegação com o descriptor efetivo recebido da autoridade autenticada e ainda trata negação do servidor. Não confundir tree-shaking de frontend com segurança de API.

## Contratos de integração

Módulos trocam IDs estáveis e DTOs delimitados, nunca DbContexts/tabelas privadas. Publicação e auditoria preservam transações e idempotência atuais. Não adicionar um barramento/outbox sem necessidade identificada; quando eventos já existem, conservar formato, order/replay e vínculo de tenant/produto.

REST: manter rotas e semânticas existentes com adapters se necessário. Qualquer nova versão precisa de transição explícita, testes de clientes antigos e término de suporte definido. Regerar OpenAPI usando a API/build real, nunca editar o arquivo gerado manualmente para esconder incompatibilidade.

Caches, jobs e mensagens incluem produto quando for necessário para evitar colisão entre composições no mesmo ambiente. Isso não autoriza alterar chaves persistidas em massa: usar namespaces/versionamento e migração controlada.

## Ciclo de vida

Instalar ≠ configurar ≠ habilitar ≠ conceder acesso. Deixar esses passos separados e auditáveis. Não oferecer remoção física de módulo com dados por um clique nesta fase. Desativação deve bloquear nova atividade quando apropriado, finalizar operações em curso com segurança e preservar histórico. Reativação valida dependências e versão.

## Teste de contrato obrigatório

Construir três composições somente de desenvolvimento: fundação neutra; socioeducativo sintético; e uma extensão técnica mínima fictícia usada exclusivamente para provar reutilização, sem virar outro negócio. O terceiro cenário consome o mesmo núcleo e não copia autenticação, tenancy ou auditoria.

Quando uma capability partilhada muda, executar sua suíte e a dos consumidores atuais. O núcleo não deve conhecer o nome ou schema da extensão de prova.

## Versionamento compatível

Adotar convenção de versão já existente ou registrar uma política simples em ADR. Distinguir versão da release, do módulo, do API/schema de publicação, do banco e do manifesto. Não prometer suporte N/N-1 universal: comprovar pares concretos nos testes, inclusive backend/browser/player e releases assinadas.
