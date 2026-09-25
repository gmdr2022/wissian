# M1 — fundação demonstrável do Wissian

**Estado:** contrato de implementação preparado em M0; casos abaixo NOT_RUN.
**Consumidor:** duas organizações sintéticas, Fultuma Exemplo e Empresa Exemplo,
entram na mesma aplicação, veem apenas contextos autorizados e usam uma extensão
técnica de teste. O primeiro módulo de negócio será documentos/contrapartes em M2.

A [ADR-M03](../adrs/ADR-M03_STACK_E_REUSO.md) fixa a família técnica. A
[arquitetura](../03_ARQUITETURA_E_MODULARIDADE.md) fixa identidade própria e módulos
opcionais. A [evidência M0](../evidencias/m0-primeira-fatia.json) e o
[contrato de absorção](../04_ABSORCAO_CODEX_BASE44.md) são as fontes de procedência.

## Entrega mínima e fronteiras

1. Projetos próprios `Wissian.Core`, contratos, adapters de persistência/autenticação
   e host de composição; shell React tipado. Nomes são destinos planejados, ainda
   inexistentes. Core não referencia administrativos, verticais, host ou ensaio M0.
2. Organização e associação vigentes por usuário; unidade/localidade só quando um
   domínio as exigir. `HumanSession.SelectContext` isolado não autoriza associação:
   o serviço resolve membership no servidor e revalida em cada operação.
3. Identidade, cookie e armazenamento próprios; expiração, logout, revogação,
   recuperação e proteção contra CSRF/abuso no adapter. Administrador da plataforma
   não recebe acesso a dados de organização. MFA e janelas excepcionais preservam
   seus gates; não criar suporte universal para simplificar o bootstrap.
4. Catálogo confiável mínimo, dependências/versões validadas antes de servir tráfego;
   uma extensão sintética demonstra composição por contrato sem copiar auth/Core.
   Não criar carregador remoto, marketplace, broker ou WorkItem/Continuity nesta fase.
5. PostgreSQL exclusivamente sintético, migração inicial própria, runtime sem
   privilégio de migrador; chaves compostas, unicidade por organização e concorrência
   verificadas. Nenhuma migration ou banco ClubAL será importado.
6. Auditoria mínima com ator, organização, ação, resultado, correlação e versão.
   Contratos de evento/idempotência não concedem autoridade; persistência desses
   efeitos entra onde houver consumidor real. Não generalizar documento/nota/título
   financeiro em uma entidade universal.

## Casos e oráculos de saída

| ID | Prova exigida | Resultado esperado |
|---|---|---|
| M1-01 | Build do Core e grafo direto/transitivo | Sem domínio, namespace, assembly, identificador, configuração ou asset legado no Core; sem dependência de módulo/vertical/ensaio |
| M1-02 | Host neutro e extensão de teste | Core único; dependência ausente/ciclo/versão inválida impede inicialização; extensão retirada sem quebrar host neutro |
| M1-03 | Entrar e selecionar organização autorizada | Contexto resolvido no servidor; dois tenants sintéticos independentes na mesma versão |
| M1-04 | Tenant/recurso/associação alheios e contexto ausente | Negação sem revelar existência, conteúdo ou contagens de outro tenant |
| M1-05 | Revogar vínculo/sessão com aba aberta | Próxima chamada negada; cookie antigo, configuração e cache não restauram acesso |
| M1-06 | Módulo desativado com permissão existente | Menu ausente; URL direta e operação pendente continuam negadas pelo servidor |
| M1-07 | Administrador da plataforma sem concessão operacional | Nenhum acesso implícito aos dados de A/B |
| M1-08 | Banco real sintético e conexão A → B → sem escopo | Sem vazamento por pool; FK/consulta/criação cruzada negadas usando papel real de runtime |
| M1-09 | Atualização concorrente e comando repetido | Conflito de versão explícito; repetição com mesma chave não duplica efeito; sem autorização não retorna recibo sensível |
| M1-10 | Migração nova, reaplicação, falha e recuperação | Schema recuperável, inicialização reproduzível; credencial do runtime não administra schema |
| M1-11 | Logout, expiração, recuperação e requisição sem CSRF | Sessão/ação inválida negada; fluxo de recuperação definido sem serviço externo implícito |
| M1-12 | Fluxo UI por teclado e tela estreita, vazio/erro/espera | Contexto preservado e ação compreensível; aceite visual de Gui separado dos testes |

Mapear estes casos aos TM/TC pertinentes sem alterar em bloco a matriz histórica.
Registrar comandos reais de build, testes de domínio/integração, typecheck e smoke no
README quando os projetos existirem. Banco em memória não substitui M1-08/10.
Guardar evidências por caso, versão e ambiente; teste estático não comprova isolamento.

## Ordem, critério de parada e recuperação

WIP=1: extrair fronteiras → persistir dados sintéticos → provar autorização e
composição → conectar shell → smoke/revisão. Fixar dependências suportadas antes de
adicioná-las; medir preflight de memória/disco e serializar builds no host limitado.
Não instalar infraestrutura ou serviço pago por preferência. Investigar fonte extra
somente para resolver lacuna do caso ativo; demais addons permanecem no catálogo.

Concluir quando todos os negativos críticos e o fluxo sintético passarem; aceite
humano pendente fica explícito e impede declaração de piloto aceito. Não promover
M2–M4 por existência da fundação. Se a extração violar os gates da ADR, reduzir o
recorte ou reavaliar a decisão com evidência.

Rollback do código por commit de reversão da fatia, preservando o baseline e origem;
migrações aditivas com recuperação ensaiada, sem `Down` destrutivo automático. Fixtures
só em banco identificado como sintético. Nenhum dado real ou ambiente produtivo faz
parte deste contrato.
