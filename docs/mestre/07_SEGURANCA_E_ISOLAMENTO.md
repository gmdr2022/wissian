# Segurança, privacidade e isolamento

**Contrato alvo; implementação e evidências por caso estão na [fundação M1](contratos/M1_FUNDACAO.md).** O estado documental não promove controles ainda pendentes. Deriva do guia fundador §§22–25, plano da operação §§7/10/11 e [segurança v1.1](../fontes/reestruturacao-v1.1/08_SEGURANCA_PRIVACIDADE.md). Não certifica conformidade regulatória.

## Fronteiras e ameaças

| Fronteira | Falha a impedir | Controle e evidência exigidos |
|---|---|---|
| Navegador → API | IDOR, troca de tenant/organização/unidade, papel forjado | Identidade autenticada; associação vigente; tenant derivado de contexto validado; recurso e ação verificados no servidor; teste com dois tenants e URLs diretas |
| Plano de controle → operação | Administrador da plataforma lê dados empresariais por padrão | Escopos separados; suporte excepcional temporário, justificável, revogável e auditado; fluxo sem acesso permanece funcional |
| API → banco | Filtro esquecido, consulta agregada ou associação cruzada | Repositórios com escopo, constraints compostas, revisão de SQL e RLS quando escolhido; testar com a credencial real de execução da aplicação em laboratório |
| API → arquivos/busca/cache | URL compartilhada, índice global, chave sem tenant | Namespaces por organização e classificação; checar autorização antes de fornecer arquivo ou resultado; não revelar existência por contagem, autocomplete ou erro |
| Jobs/eventos → dados | Autoridade de usuário revogada mas job ainda executa | Tenant/ator/correlação explícitos; revalidar ação ao executar; idempotência; expiração; dead-letter controlada e retomada autorizada |
| Módulo → outro módulo | Acesso direto às tabelas ou ampliação transitiva | Contrato público mínimo; dependências declaradas; nenhum trust implícito por estar no mesmo processo |
| Upload/importação → parser | XXE, zip bomb, fórmula, path traversal, MIME falso | Limites, extensão e conteúdo, parser sem entidades/rede, nomes internos gerados, quarentena e diagnóstico por item; nunca executar macro/fórmula |
| Integração externa → aplicação | Webhook falso/repetido, dados exfiltrados, segredo em log | Assinatura/autenticidade quando aplicável, replay window, idempotência, egress limitado, segredo segregado e rotação; integração só após autorização específica |

## Decisão de acesso

Para cada operação, exigir conjuntamente: sessão válida; associação ativa à organização; módulo instalado na composição; disponibilidade/contratação pertinente; ativação operacional; permissão de ação; acesso ao recurso/unidade/classificação; estado do fluxo permitindo a transição. Ausência ou erro em qualquer parcela nega acesso. Tela escondida, compra, nome de marca ou feature flag nunca substituem essa decisão.

Negar por padrão e verificar cada solicitação é coerente com o [OWASP Authorization Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html). Registrar causa interna sem expor dados de outro tenant. Cachear decisões só com escopo, versão de autoridade e invalidação comprovados; a primeira implementação pode evitar esse cache.

Login, troca de contexto, convite, expiração, revogação, logout e recuperação entram nos testes de M1. Contratar módulo não recria sessão privilegiada. Não importar cookies, chaves DataProtection/DPAPI ou credenciais do ClubAL. Se compatibilidade futura exigir bridge, definir ameaça, duração e revogação antes de construir.

## Banco, arquivos e exportação

RLS é defesa adicional, não prova isolada. O PostgreSQL documenta exceções para proprietário/superusuário/BYPASSRLS e operações que não se submetem às políticas de linha. Evitar executar aplicação como dono privilegiado e testar o desenho escolhido com roles efetivas. [PostgreSQL — Row Security](https://www.postgresql.org/docs/current/ddl-rowsecurity.html).

Uploads devem ter política por capacidade e classificação, limites de tamanho/quantidade/descompactação, validação de conteúdo e armazenamento protegido. Download passa pela autorização; nome original é metadado não confiável, não caminho físico. Links temporários, quando usados, têm escopo, duração e risco de revogação documentados. Essas diretrizes seguem o [OWASP File Upload Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html).

Exportações são operações privilegiadas: escopo explícito, minimização, trilha, validade e destino autorizado. Operador de suporte não recebe backup de todos os tenants para investigar um. Definir política de retenção por classe e finalidade antes de dados reais; não fixar prazo legal sem fonte aplicável e revisão responsável. Exclusão lógica não equivale a eliminação física de backups. Solicitações de dados, incidentes e retenção têm workflow próprio, sem selo automático de conformidade.

## Auditoria e segredos

Evento mínimo: identificador estável, instante UTC, tenant, ator pseudonimizado quando possível, ação, recurso por referência, resultado, motivo técnico controlado, versão de autoridade e correlação. Dados alterados sensíveis não entram integralmente no evento. Restrição de leitura/escrita, retenção e prova contra alteração precisam de desenho proporcional; escrever em uma tabela chamada audit não a torna imutável.

Nunca copiar `.env`, tokens, arquivos pessoais, dados institucionais, credenciais Base44, cofre ou sessões de navegador para o destino. Exemplos só com valores vazios ou sintéticos. Bibliotecas/SDKs extraídos do Base44 passam por licença, manutenção, dependência de backend e telemetria antes de adoção. IA, se houver valor comprovado, não recebe dados reais por conveniência nem pode autorizar ações; isolar conteúdo externo/instructions, medir erro e permitir revisão/interrupção.

## Portões

M1 exige testes negativos entre dois tenants em API e persistência, além de revogação e módulo desativado. M2–M4 ampliam para anexos, buscas, exportações, filas e concorrência. Antes de piloto real, acrescentar restauração medida, gestão de segredos, canal de incidente, controle de suporte, retenção e autorização explícita. Antes de clientes externos, medir capacidade/custo e validar termos/responsabilidades aplicáveis. Os riscos e donos estão em [13](13_RISCOS_E_DECISOES_ABERTAS.md).

## Continuidade operacional: ameaças adicionais

Aplicar todos os controles anteriores ao [contrato do capítulo 16](16_CONTINUIDADE_OPERACIONAL.md). Filas, contagens, comentários, dependências, passagens, decisões, ResourceHealth, recibos e replay exigem tenant e acesso atual ao recurso. Roteamento por papel não concede permissão. Revalidar produtor, usuário e módulo ao receber, executar, confirmar e retornar recibo; revogação entre enqueue/execução impede efeito.

ActionCatalog só resolve handlers registrados no servidor com parâmetros tipados/limitados e permissões enumeradas. Entrada nunca contém comando PowerShell/BAT/EXE, código, caminho executável, handler livre ou URL de destino escolhida pelo navegador. Tenant/escopo do payload não substitui ExecutionScope. Preservar segregação de solicitante/aprovador quando crítica; serviço ou IA não cria aprovação humana. Confirmação sobre revisão antiga falha segura. Receber passagem não conclui item. Suspender Continuity não remove gate do owner.

TC-03/04/06/08/09/10/11/15/16/19/20 exercitam essas fronteiras futuramente, incluindo acesso cruzado, ciclos concorrentes, campo inesperado, importação excessiva e replay após revogação. Resultado documental não prova segurança implementada.
