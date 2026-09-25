# Contratos de exemplo — propostas

[Catálogo](catalogo_modulos.proposta.json), [composições](composicoes.proposta.json) e [evento de auditoria](evento_auditoria.exemplo.json) são referências verificáveis de planejamento. `reference_only=true`, `runtime_activation=false` e `grants_authority=false` são obrigatórios. Nenhum arquivo é lido por runtime de produto nesta entrega.

IDs exemplificam namespace, não renomeiam IDs ClubAL nem obrigam criar cada módulo agora. Rotas/jobs/migrations vazios significam contratos concretos ainda por desenhar; não prova de ausência de código no legado. Antes de implementar um módulo, preencher ficha e registro tipado real com versão, owner, dependências, interface, permissões enumeradas, dados e ciclo de vida. Não usar wildcard do exemplo como concessão.

O catálogo aceita dependências opcionais para demonstrar ausência semântica: financeiro pode funcionar sem notas fiscais; contratos sem agenda integrada; socioeducativo sem player no experimento de fronteira. Quando ativada uma integração, validar dependências e comportamento, não habilitar tudo. Exemplos de composições verticais não constituem especificação funcional completa.

O verificador valida forma, referências, ausência de ciclos e Core sem dependência. Só teste de build/runtime pode provar a arquitetura implementada. Uma composição com módulo instalado não ativa esse módulo em todos os tenants nem concede permissão.

## Continuity (2.1.0)

[continuity.proposta.json](continuity.proposta.json) registra envelopes, chaves, políticas, ações mínimas e exemplo sintético posterior de dispositivo. É proposta documental, não schema de banco, API implementada ou configuração de ativação. A interpretação normativa e os estados estão no [capítulo 16](../16_CONTINUIDADE_OPERACIONAL.md). O verificador rejeita propostas perigosas/incoerentes; não executa handlers nem eventos.

O catálogo adiciona um único módulo opcional `continuity`; a composição `admin-continuity-reference` o combina ao consumidor mensal sem alterar as composições existentes. Integrações opcionais são wiring do host por contratos públicos, sem imports circulares. Permissões de exemplo não concedem acesso.
