# Absorver uma capacidade com equivalência demonstrável

**Uso:** enviar como pedido futuro deliberado ou aplicar dentro de uma autorização de trabalho já vigente. A presença deste arquivo não inicia execução.


Absorva somente a capacidade que Gui indicar no pedido atual. Se o nome não foi indicado, derive do próximo marco autorizado; se isso não for determinável, inventarie em leitura e peça a menor definição necessária, sem migrar código por suposição.

Confirme raiz/ref/hash atuais, abra ficha de absorção e compare Codex/Base44 no mesmo caso, incluindo modelos de dados, estrutura, UI, permissões, assets, estado e testes. Consulte TEM/OWM/GRA e ADRs aplicáveis. Use a versão integrada quando já contiver melhoria, evitando reaplicação. Escolha adotar/adaptar/manter/adiar/rejeitar com razão e fonte.

Implemente no destino respeitando Core, owner, tenant e dependências; não faça merge integral, não altere origem nem copie credenciais/dados. Separe migração de código e de dados; qualquer necessidade real de compatibilidade deve declarar pares e adapters. Prove equivalência escolhida, falhas, isolamento, desativação e rollback. Registre divergência consciente; preserve validação visual/hardware pendente. Atualize matriz, contratos e continuidade somente com resultados observados.

## Complemento para Nexus/CC

Aplicar ABS-041…048 e ADR-M08 quando a capacidade selecionada vier desses ancestrais. Preservar princípios e casos, caracterizar fonte real com commit/hash/licença, testar no destino; não portar UI WPF, executor de comandos, journal/mutex/JSON, `.ccard` ou integração Codex. Rejeição técnica do mecanismo preserva intenção útil e origem.

## Prioridade de reuso existente

Aplicar o [contrato canônico de fragmentação e reuso](../04_ABSORCAO_CODEX_BASE44.md): localizar equivalentes antes de criar, preservar origens e justificar lacunas. WM-39/TC-21 mantêm a rastreabilidade.

