# Segurança, privacidade e fronteiras de confiança

## Invariantes que a reestruturação não pode reduzir

Negar por padrão, validar acesso no servidor e manter privilégio mínimo. A orientação OWASP distingue autenticação de autorização e recomenda verificação de permissões por requisição [S03]. Não substituir a política existente por um simples `isAdmin`, nem confiar em ocultação de menu.

Organização, unidade, produto, recurso, usuário/sessão/dispositivo e ação precisam ser coerentes. ID fornecido pelo cliente é entrada não confiável; resolver autoridade a partir da identidade e das relações autorizadas. Não fazer lookup global por ID e só depois “lembrar” de filtrar quando houver alternativa scoped.

## Superfícies de ataque da separação

| Superfície | Falha a prevenir | Prova exigida |
|---|---|---|
| API e rotas profundas | Acessar módulo ausente por URL direta | Resposta negada/rota ausente conforme contrato, sem conteúdo |
| Jobs, hubs e eventos | Processar trabalho de módulo desabilitado ou outro cliente | Registro condicionado e mensagens escopadas |
| Pesquisa e listas | Vazamento por filtros, contagens ou autocomplete | Dados de outra organização/unidade/produto não aparecem |
| Cache/PWA | Recuperar conteúdo restrito após troca de contexto/logout | Chaves e invalidação testadas; apenas conteúdo autorizado offline |
| Assets e relatórios | URL previsível expõe arquivo privado | Autorização real e minimização, não somente nome aleatório |
| Plataforma/administração | Dono do software obtém acesso operacional universal | Plano de controle separado do plano de dados |
| Suporte JIT | Reutilizar concessão antiga para outro propósito/recurso | Finalidade, escopo, MFA, expiração e revogação preservados |
| Configuração de marca | Upload/tema altera autoridade ou executa script | Sanitização, tipos/limites e CSP/políticas existentes |
| Manifesto de módulo | Payload do browser ativa funcionalidade restrita | Configuração confiável; frontend não concede autoridade |
| Integração Base44 | Trazer SDK, conta ou endpoint externo não aprovado | Inspeção de dependências e chamadas; reaproveitamento seletivo |

## Preservar a evolução já construída

O `ACTIVE_SLICE.md` relata GRA-1–GRA-4 e suporte GRA-5 com controles específicos [E03]. Não recriar suporte genérico nem substituir a delegação por um acesso permanente do fabricante. Manter os flags/gates default-off que ainda estiverem pendentes. A documentação relata provas locais anteriores; reexecutar os casos impactados para a nova baseline.

Aprovação do cliente, membership institucional, administração da plataforma, diagnóstico técnico e edição de informação operacional continuam sendo autoridades diferentes. Nenhum cadastro de produto/módulo pode promovê-las automaticamente.

## Dados socioeducativos

O melhor interesse de crianças e adolescentes é uma exigência destacada pela ANPD [S05]. Neste plano, implementar minimização e controles técnicos; não declarar que eles bastam para conformidade jurídica.

Somente fixtures explicitamente sintéticas em desenvolvimento, screenshots, testes, registros do Codex e demonstrações. Não importar prontuários, imagens, escalas com pessoas reais, relatórios ou bases do PEMSE/Elo para o modelo-base. Dados reais não vão para prompts, ferramentas externas, logs, issues, cofre de engenharia ou Git.

A classificação operacional `restrito` é uma política técnica conservadora; não substitui a classificação jurídica de dado pessoal sensível. Finalidade, base legal, retenção, direitos dos titulares, contratos de tratamento, transferências internacionais e necessidade de RIPD devem ser definidos por responsáveis competentes antes de uso real. Não impor consentimento como única base legal por padrão.

## Auditoria e privacidade

Preservar causalidade e histórico das operações. Logs de engenharia devem registrar códigos, IDs sintéticos, tempo e correlação, não payloads integrais. Auditar acesso a operações restritas conforme política existente; falha da auditoria não pode ser convertida silenciosamente em sucesso quando ela for um pré-requisito.

Não chamar toda trilha de “imutável” se um administrador do banco puder alterá-la. Descrever a garantia real (por exemplo, append-only na aplicação, constraint/trigger existente) e os limites. Não introduzir dados pessoais em métricas para provar adoção.

## Credenciais e toolchain

Nenhum segredo deve entrar no ZIP ou nos commits. Ler apenas as variáveis necessárias sem imprimir seus valores; não fazer dumps de `.env`, credenciais C7, tokens ou configurações privadas. Gerar segredos sintéticos locais somente em laboratório, com armazenamento gitignored e limpeza controlada.

Não mudar mecanismos criptográficos, peppers, chaves de assinatura ou trust anchors durante extração. Não desabilitar TLS, CSRF, CSP, checagem de assinatura, audit ou isolamento para destravar testes. Não rodar instaladores/scripts remotos sem revisão e autorização aplicável.

## Comportamento em falhas

Módulo ausente, escopo inválido, publicação corrupta, versão incompatível, sessão expirada e fonte de autorização indisponível têm resultados explícitos. Preservar apresentação offline apenas do conteúdo já autorizado pela política; operações privilegiadas não recebem bypass para manter disponibilidade.

Cobrir enumeração de IDs, troca de tenant, segundo produto na mesma origem, reaproveitamento de token, cache antigo, command replay, expiração durante operação e tentativas de subir privilégio alterando manifesto/feature flag.

## Condição de bloqueio

Vazamento de dados, ampliação de autoridade, perda de revogação, quebra de assinatura, importação de segredo ou perda de auditoria bloqueiam o incremento afetado. Corrigir antes de integrar. Aprovação humana de produto não autoriza aceitar regressão crítica de segurança.
