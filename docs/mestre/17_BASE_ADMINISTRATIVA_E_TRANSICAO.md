# Base administrativa: utilidade, limites e transição

**Diretriz de Gui em 24/09/2026, antes do desenvolvimento.** A Fultuma pretende operar no Wissian os mesmos controles administrativos úteis oferecidos às demais organizações. Esta revisão confirma cobertura no planejamento; não comprova aplicação pronta, segurança implementada ou vantagem de uso já medida. O desenvolvimento depende de pedido posterior.

## Decisão e fronteira

O primeiro ciclo M2–M4/E1–E3 já é adequado ao objetivo de substituir o controle manual de notas, financeiro e preparação contábil mensal. A implementação permanece condicionada à caracterização de reuso em M0, à fundação M1 e aos testes de cada fluxo. Não construir um ERP completo nem reproduzir uma pasta da empresa como um módulo por mera correspondência de nomes.

“Core básico”, no sentido de experiência oferecida ao cliente, compreende a base administrativa pretendida. Na arquitetura, o **Core técnico continua mínimo**: identidade, organizações, autorização, composição, configuração e auditoria. A composição existente `admin-first-cycle` reúne `core`, `documents`, `counterparties`, `fiscal-records`, `finance` e `monthly-close`. Modularidade técnica não decide preço ou contratação separada. Fultuma usuária não recebe acesso especial por ser a fornecedora.

Esta diretriz delimita a primeira entrega. Marca/comunicação, gestão de contratos, societário, licenças, PI, pessoas e demais expansões M5/M8 não são requisitos para substituir os controles administrativos deste recorte. Suas referências permanecem históricas ou condicionais; só entram com demanda e autorização específicas. Documentos anexos necessários ao financeiro não exigem construir esses módulos.

## Cobertura confirmada no plano

| Necessidade útil | Comportamento já previsto | Marco e rastreabilidade |
|---|---|---|
| Receitas e despesas | Contas a pagar/receber, vencimentos, responsáveis, categorias, centros de custo e recorrência sem duplicidade | M3/M4; WM-09/10/12; TM-08/09/11; plano base §6.3/E3 |
| Pagamentos e recebimentos | Parcelas, baixas parciais, comprovantes, saldo correto e estorno com histórico; registrar não movimenta dinheiro no banco | M3; WM-08/09/10; NF-01–05/09 |
| Caixa e conciliação | Competência, vencimento e caixa distintos; conciliação com diferenças explícitas; transferência interna sem receita artificial | M4; WM-12; TM-11; NF-06 |
| Controle fiscal administrativo | Registrar notas emitidas fora do Wissian e recebidas, relacionar contraparte/arquivo, conferir situação e evitar importação duplicada | M2/M3; WM-06/07/08/11; TM-06/07/08/10; NF-07/10 |
| Apoio à contabilidade | Organizar documentos por competência, guias/comprovantes/recibos e preparar pacote mensal versionado, com faltas visíveis e revisão | M4; WM-12; TM-11; WB-04/14; NF-08 |
| Localização e uso cotidiano | Busca, filtros preservados, documentos relacionados, pendências acionáveis, interface responsiva e acessível | M2–M4; WM-06/13; TM-06/12; capítulo 08 |
| Segurança e continuidade dos dados | Permissões por ação/recurso, isolamento, revogação, auditoria, exportação autorizada, backup e restauração verificada | M1–M4 e gate M7; WM-04/05/16/22; TM-04/05/06/13/17/18 |

Fontes vigentes: [arquitetura](03_ARQUITETURA_E_MODULARIDADE.md), [composições](contratos/composicoes.proposta.json), [roadmap](09_ROADMAP_INTEGRADO.md), [segurança](07_SEGURANCA_E_ISOLAMENTO.md), [UX](08_UX_MARCA_E_QUALIDADE.md), [operação](11_OPERACAO_OBSERVABILIDADE_ROLLBACK.md), [requisitos](matrizes/requisitos-mestre.json) e [testes planejados](matrizes/testes-planejados.json). O [plano base preservado](../fontes/workspace-2026-09-24/PLANO_WISSIAN_OPERACAO_BASE.md), §§5–7/9/11, detalha esses fluxos; seu catálogo amplo não amplia esta entrega.

**Limite fiscal/contábil:** a base inicial organiza registros e evidências. Não executa emissão oficial, apuração de tributos, transmissão de declarações, escrituração completa, folha ou operações bancárias. Também não substitui o trabalho e a validação do contador. Eventuais integrações oficiais permanecem específicas, futuras e autorizadas.

## Como demonstrar benefício sobre pastas e planilhas

Online e visualmente bonito são intenções de produto, não provas de eficiência ou segurança. Antes de declarar substituição operacional, cumprir os critérios existentes com o seguinte recorte de aceite:

1. **Utilidade com comparação:** usar dados sintéticos equivalentes em um controle manual mínimo e no Wissian. Medir tempo, passos, redigitação, erros e recuperação para localizar nota/comprovante, conferir saldo/vencimento, lançar baixa parcial, desfazer erro e preparar o mês. Definir baseline e metas por tarefa antes do ensaio; registrar ganho observado e eventuais regressões. As metas históricas do plano base continuam propostas, não resultados. Não usar a planilha antiga sem contas a receber como se ela já executasse esse fluxo.
2. **Clareza e apresentação:** mostrar o que vence, o que está pendente e qual ação é permitida, com acesso ao detalhe; totais indicam período, critério e cobertura. Fluxos funcionam por teclado, em tela estreita e com zoom; erro, espera, vazio e conflito preservam contexto. Aceite visual de Gui permanece separado da aprovação dos testes.
3. **Integridade:** executar NF-01–10, incluindo a nota sintética de 1.000, duas parcelas de 500, baixa de 300 e estorno, além de duplicação, concorrência e falha de geração do pacote. Diferenças e documentos ausentes não viram sucesso silencioso.
4. **Mesmo produto para clientes distintos:** Fultuma Exemplo e Empresa Exemplo concluem o mesmo ciclo na mesma versão, com configurações/permissões próprias e sem código exclusivo. Testar tentativas de acesso cruzado, revogação com aba/job aberto, downloads e exportações; a administração da plataforma não ganha leitura universal.
5. **Recuperação e saída:** exportar registros e documentos autorizados com vínculos conferíveis; abrir o pacote exportado fora da sessão e verificar conteúdo/totais sem acesso adicional ao SaaS. Testar perda de conexão e timeout sem duplicação ou falso salvamento. Restaurar banco e anexos sintéticos em ambiente isolado, verificar saldos, vínculos e permissões e medir recuperação. Não prometer operação offline, disponibilidade ou segurança superiores sem evidência correspondente.

TM-11/12/13 detalham essa comparação e recuperação; TM-04/05/06/09 e o gate GM6 continuam obrigatórios. Nenhum caso de aplicação foi executado nesta revisão: todos permanecem `NOT_RUN`. M4 demonstra o ciclo sintético; entrada de dados reais depende também do gate de piloto M7, autorização de ambiente/dados e responsáveis definidos.

## Retirada do kit local da Fultuma

Gui autorizou retirar as pastas preparatórias locais após esta conferência de cobertura. A decisão reduz duplicação de planejamento; **não é migração para um SaaS já disponível**. A verificação local encontrou 21 arquivos/26 diretórios e nenhum valor previsto/realizado, data de pagamento ou comprovante preenchido nos campos financeiros da planilha. Não houve auditoria contábil.

Preservar o kit em cópia recuperável com inventário, hashes e ensaio de restauração antes de remover as 13 pastas internas. Manter o guia verbal e uma entrada curta na raiz da empresa. Localização e recibo da preservação pertencem à pasta da Fultuma e à continuidade C7; não incorporar seu arquivo privado ao repositório do produto. Documentos societários e minutas arquivados não foram substituídos por capacidades fiscais ou financeiras.

Até o piloto ser aceito, documentos reais novos precisam de armazenamento autorizado e recuperável; não há controle online disponível para recebê-los nesta fase. A restauração do kit continua possível. Uma adoção futura por clientes não autoriza apagar seus arquivos locais automaticamente.

## Encaminhamento

**Recomendação:** manter o primeiro ciclo administrativo existente, concluir M0 por reuso/fragmentação e provar M1–M4 quando Gui solicitar desenvolvimento. A alternativa de converter todas as pastas corporativas em módulos foi rejeitada por ampliar custo e complexidade sem benefício demonstrado.

Confiança alta na cobertura documental do recorte; implementação, vantagem de uso e segurança efetiva ainda não verificadas. O trade-off aceito é retirar o kit preparatório da pasta ativa mantendo recuperação, durante um intervalo sem SaaS operacional. Reavaliar se surgir necessidade imediata de controle real, emissão oficial desde o primeiro uso, lacuna de reuso em M0 ou falha nos critérios de utilidade, isolamento, exportação ou restauração.
