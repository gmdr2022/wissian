# Pré-mortem e recuperação

## Suponha que a reestruturação falhou

| Falha plausível | Sinal antecipado | Mitigação neste plano | Resposta |
|---|---|---|---|
| Perda da evolução Base44/Codex | main usada sem conferir integração/local | R00/R01, comparação por capacidade e preservation ledger | Interromper edição concorrente, recuperar apenas mudanças próprias |
| Meses construindo plataforma abstrata | novas camadas sem consumidor/teste | core v1 delimitado, primeiro produto e prova fictícia mínima | Remover escopo não necessário do plano, não código útil sem análise |
| Core só existe no nome | importa Domain inteiro ou UI reexporta nichos | teste de dependências + build sem vertical | Manter status extração parcial e corrigir a fronteira |
| Quebra de login/pareamento | renomeação de cookies/DPAPI/IDs | inventário de compatibilidade e alias | Reverter apresentação/código compatível; não resetar credenciais |
| Dados se misturam | cache/query baseado apenas em ID ou usuário | escopo tenant/unidade/produto e testes negativos | Bloquear liberação; corrigir antes de integrar |
| Perda de histórico | reset/drop de migrations ou rename incorreto | expand/contract, SQL review e restore sintético | Restaurar apenas em procedimento autorizado e considerar gravações posteriores |
| Funcionalidade desaparece | capacidade movida para ClubAL futuro | matriz de preservação, produto socio recomposto | Repor composição/capacidade sem duplicar core |
| Custos imprevistos | novo domínio/serviço/SDK obrigatório | zero nova contratação, adaptadores atuais | Adiar contratação; preservar caminho local |
| Suporte vira backdoor | role plataforma acessa tudo | manter GRA/JIT e grants segregados | Desabilitar nova superfície e testar negações |
| “Tudo passou” sem execução real | mocks tratados como E2E integral | estados explícitos e commands ledger | Reclassificar evidência e executar o que falta |
| Documentação contraditória | AGENTS antigo x prompt novo x C7 | supersessão limitada e uma continuidade | Corrigir ponteiros/contratos sem apagar história |
| Rebranding aumenta risco jurídico | nome tratado como marca liberada | provisório/configurável, sem registro/compra | Trocar display name sem alterar IDs |
| Falta de validação comercial | outras verticais iniciadas antes do piloto | ClubAL reservado; foco no primeiro fluxo | Voltar à validação do produto, não expandir core |

## Unidade de reversão

Cada incremento registra arquivos/commits próprios, alterações de configuração, migrações, processos de teste e a baseline compatível anterior. Quando houver branch/worktree de implementação, usar controle de versão sem modificar branches de origem. Não usar `reset --hard`, `clean -fd` ou checkout de diretório inteiro para reverter um arquivo com trabalho de outra pessoa/agente.

Patches locais e commits contêm somente mudanças inspecionadas. Não utilizar `git add -A` indiscriminadamente. Arquivos untracked, segredos e recursos fora do Git exigem cuidado próprio; nunca armazenar tokens/dados operacionais num “backup” versionado.

## Três planos de recuperação

**Código/UI/composição:** voltar à versão anterior compatível ou desativar a nova composição em laboratório; preservar preferências, IDs e dados. **Schema/dados:** preferir compatibilidade aditiva; um downgrade só é possível se testado e sem perda. **Dispositivo/cache/release:** reaplicar artefato confiável compatível pelo mecanismo existente, respeitando assinatura, schema e estado ambíguo de operação.

Feature flag não resolve automaticamente transação parcialmente persistida, publicação divergente ou cache incompatível. A recuperação deve verificar estado e causalidade.

## Teste de recuperação obrigatório

No laboratório, iniciar com baseline sintética; executar alteração; provocar uma falha controlada; verificar o estado parcial; executar procedimento de recuperação; comprovar identidade, escopo, histórico, publicação e disponibilidade. Guardar resultados sanitizados. Não provocar falhas em equipamento de cliente ou base real.

## Quando pedir decisão humana

Perda funcional deliberada; mudança de regra institucional; reconfiguração de autoridade; uso real; contratação; ação externa/publicação; transferência/titularidade; exclusão irreversível. Antes de pedir, inspecionar o que já está disponível para evitar repetir perguntas respondidas. Apresentar decisão objetiva e continuar partes independentes do trabalho.
