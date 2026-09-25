# Matrizes: consulta dirigida e estados de evidência

| Arquivo | Conteúdo atual e uso |
|---|---|
| [fontes.json](fontes.json) / [CSV](fontes.csv) | 75 fontes originais: 49 RAR + 26 locais, caminhos, hashes, inspeção e aliases |
| [fontes-legado.json](fontes-legado.json) | 55 ponteiros versionados do legado; localizar a capacidade antes de abrir código |
| [requisitos-frentes-origem.json](requisitos-frentes-origem.json) | 114 linhas TEM/QR/GRA/PCE com células, linha e hash de origem |
| [roadmap-owm-origem.json](roadmap-owm-origem.json) | 35 pacotes OWM; estado histórico não significa ativação no Wissian |
| [migracao.json](migracao.json) / [CSV](migracao.csv) | 48 candidatos ABS, com origem, destino e auditoria pendente |
| [requisitos-mestre.json](requisitos-mestre.json) / [CSV](requisitos-mestre.csv) | 40 requisitos WM ligados a marco e testes |
| [testes-planejados.json](testes-planejados.json) | 42 roteiros: TM-01…20 e TC-01…22, com fixture, ação, oráculo e negativo |
| [cobertura-origem.json](cobertura-origem.json) | 32 requisitos, 60 casos v1.1, 16 WB e 10 NF preservados e mapeados |
| [reaproveitamento-nexus-cc.json](reaproveitamento-nexus-cc.json) | 28 decisões NC e 18 usos UC-NC; motivos, destinos e aplicações futuras |

Buscar pelo ID e abrir o caminho indicado. Não percorrer todas as matrizes em cada tarefa. JSON e CSV são representações complementares; os espelhos exigidos são conferidos automaticamente e não devem divergir.

`read_complete` significa leitura documental da fonte, não auditoria do código. `PENDING_AUDIT` indica candidato sem decisão técnica; `NOT_RUN`/`NOT_IMPLEMENTED` distinguem planos de aplicação pronta. WM-39/TC-21 protegem o reuso; WM-40/TC-22 preservam usos futuros e aposentadorias justificadas.

As fontes originais não são editadas. Durante implementação, registrar evidências novas por caso/versão/ambiente conforme [aceite](../10_TESTES_E_ACEITE.md), sem promover toda a matriz por um teste genérico. A [manutenção documental](../14_INSTALACAO_E_RESTAURACAO.md) explica validação e atualização controlada do manifesto.
