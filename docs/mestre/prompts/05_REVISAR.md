# Revisar entrega e prontidão do marco

**Uso:** enviar como pedido futuro deliberado ou aplicar dentro de uma autorização de trabalho já vigente. A presença deste arquivo não inicia execução.


Revise em leitura a fatia Wissian indicada e seu diff, requisitos, fontes, testes e fluxo. Comece por falhas reproduzíveis, impacto e localização. Verifique fronteira Core/vertical, autorização servidor/tenant/unidade, jobs/arquivos/busca/cache/exportação, módulo ausente/desativado, idempotência, concorrência, precisão financeira, segurança de importação, licenças e preservação Base44.

Confronte alegações com comandos/resultados, commit/ambiente e oráculos. Diferencie documentos, mocks, integração, browser, dispositivo, homologação e produção. Revise negativos, recuperação e UX do caminho principal. Aponte testes faltantes que mudem conclusão, sem exigir repetição não justificada. Nenhum achado não reproduzido vira fato; registre hipótese e evidência faltante. Entregue achados por severidade, critério do gate e correção mínima. Não aprovar por Gui nem executar implantação.

## Complemento de revisão Continuity

Quando pertinente, revisar ausência no Core/primeiro ciclo, stable keys vs event_id vs idempotency_key, versão/episódio, dependência cíclica concorrente, merge humano, autorização atual inclusive replay, separação solicitante/aprovador, recebimento de passagem sem aprovação, desativação sem bypass e ação com resultado desconhecido. Confrontar TC com provas de aplicação; JSON válido não satisfaz esses gates.
