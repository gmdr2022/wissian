# Iniciar a operação base do Wissian no Codex

Este texto está preparado para Gui usar em uma nova tarefa aberta na pasta da plataforma. Sua criação não iniciou o desenvolvimento.

O [plano completo](PLANO_WISSIAN_OPERACAO_BASE.md) cobre a base administrativa e sua expansão. O pedido abaixo delimita a primeira execução ao MVP local de documentos, notas e financeiro.

## Pedido para copiar

Leia `PLANO_WISSIAN_OPERACAO_BASE.md` e implemente as etapas E0–E3 na pasta atual do Wissian. Estou autorizando o desenvolvimento local desse primeiro conjunto, com dados exclusivamente sintéticos e sem ativação de serviços externos.

O objetivo é permitir que a Fultuma use a mesma operação base que será oferecida às empresas clientes. A base deve funcionar sem ClubAL, produto socioeducativo ou qualquer especialidade.

O resultado principal é entrar na plataforma, localizar uma nota fiscal emitida, abrir o documento, identificar o cliente, acompanhar parcelas e recebimentos, controlar despesas e preparar o fechamento mensal. Quero fluxos funcionais com persistência e recuperação, não apenas telas demonstrativas.

1. Confira as instruções locais, o Git e o estado real. Reconcile o plano com o guia fundador, a ADR 0001 e qualquer plano anterior acessível. Preserve o guia original e todo trabalho preexistente. A plataforma estava em preparação quando este pedido foi escrito; não assuma que continua vazia.
2. Resolva decisões técnicas rotineiras com a documentação atual, atualize a fase e as decisões necessárias e prossiga sem pedir confirmação a cada etapa. Pergunte apenas por conflito material de produto, autorização externa ou informação indispensável não disponível.
3. Entregue E1: identidade, organizações, permissões, clientes/fornecedores, documentos privados e busca. Depois E2: notas emitidas/recebidas por cadastro/importação, parcelas e baixas. Depois E3: contas a pagar/receber, conciliação, caixa e pacote do mês, conforme o plano.
4. Use Fultuma Demonstração e Empresa Exemplo como organizações sintéticas, ambas na mesma implementação. Não importe documentos reais, minutas privadas, planilhas, credenciais ou dados pessoais para o Git nem para serviços externos.
5. Separe situação fiscal, situação financeira, conciliação e evidência. Cadastro de nota emitida fora do sistema não significa que o Wissian a emitiu oficialmente. Nenhum botão deve aparentar pagamento, assinatura ou emissão que não ocorreu.
6. Implemente e execute os testes pertinentes, incluindo NF-01–NF-10, isolamento em dois contextos, acesso direto a anexos, baixa parcial/estorno, duplicidade, erro de importação, competência versus caixa e exportação após revogação de permissão.
7. Valide as jornadas no navegador, persistência após reinício e recuperação do ambiente de teste. Documente comandos reais e deixe o ambiente local reproduzível. Antes de builds pesados, confira capacidade do computador e limite concorrência.
8. Prepare a interface para meu aceite no Codex Checkpoint, sem aprovar por mim. Informe exatamente o que está pronto, testes executados, falhas e pendências. Se o trabalho continuar em outra tarefa, salve continuidade no cofre existente e indique o próximo passo seguro, sem tratar etapa incompleta como concluída.

Não execute commit/push, publicação, migração de dados reais, emissão fiscal, movimentação bancária, cobrança, assinatura, envio de mensagens, contratação de fornecedor ou alteração de permissões externas. Prepare qualquer etapa futura desse tipo de forma concreta e revisável antes de solicitar a autorização correspondente.

Ao concluir E0–E3, entregue o MVP local e o roteiro de demonstração. E4–E7 permanecem no plano de expansão e não devem atrasar esse primeiro resultado.
