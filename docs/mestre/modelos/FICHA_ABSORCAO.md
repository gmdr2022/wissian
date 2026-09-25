# Ficha de absorção — preencher por capacidade

**ID / dono / marco / estado:** PENDING_AUDIT. Não preencher como concluído por template.

| Campo | Evidência a registrar |
|---|---|
| Problema e fluxo | Pessoa/organização, ação inicial, resultado útil, fora do escopo |
| Procedência | Repositório, ref+commit, caminho/símbolo, hash da leitura, licença/asset e grau de inspeção |
| Origem Codex | Comportamento, regra, persistência, interface, testes e limitação reais |
| Origem Base44 | Mesmo cenário; estrutura, chamadas/SDK, modelo de dados, UX, assets, permissões e dependências |
| Cobertura de UX Base44 | Comparar as duas referências preservadas no [capítulo 04](../04_ABSORCAO_CODEX_BASE44.md); registrar tela/fluxo, evidência disponível, melhorias mantidas/adaptadas/adiadas/rejeitadas e motivo; arquivamento não equivale a absorção |
| Evoluções TEM/OWM/GRA | Requisitos/ADRs pertinentes e estado local; não transferir aprovação |
| Comparação | Baseline comum, divergências, screenshot/evidência sintética quando necessário; motivo para cada escolha |
| Decisão | Adotar, adaptar, manter na origem, adiar ou rejeitar; alternativa e custo/risco |
| Destino | Core/capacidade comum/módulo operacional/vertical/adapter; prova de fronteira |
| Contrato | Entradas/saídas, owner dos dados, permissão, tenant, estado, versão, efeitos, falhas |
| Dados | Há migração? Mapas de IDs, constraints, replay, compatibilidade e recovery; dados reais nunca implícitos |
| Licenças e segredo | Dependências examinadas; nenhuma credencial/dado copiado; limitações de direito |
| Evidência de equivalência | Casos positivo/negativo, execução/commit/ambiente, UX/hardware pendentes |
| Rollback | Ação concreta, versão/estado recuperável, gatilho, teste sintético |
| Aceite | Técnico separado de humano e de produção; responsável e estado explícito |
| Continuidade | Contrato/ADR atualizado, checkpoint no cofre, próximo passo seguro |

Concluída só quando comportamento escolhido foi implementado e validado no destino, diferenças aprovadas e origem preservada. Listar arquivos copiados sem prova de uso não encerra a absorção.

## Quando a origem for Nexus/CC

Separar princípio/caso aprovado de código auditado: commit, símbolo, hash, licença e fixture; dizer explicitamente se não consultado. Registrar owner de domínio preservado, equivalência negócio, mecanismos desktop aposentados, limite de campo do produtor/humano, escopo/chaves/revisão e resultado no destino. Sem fonte de código, não declarar reutilização literal nem teste antigo aprovado.
