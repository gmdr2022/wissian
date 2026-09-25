# Operação, implantação e custo nesta etapa

## Orçamento e autorização

Objetivo financeiro da reestruturação: **nenhuma nova contratação obrigatória**. Usar ambientes e ferramentas já disponíveis. Não comprar domínio, marca, certificado pago, servidor, banco gerenciado, plano de observabilidade, e-mail ou biblioteca. Se um recurso pago for realmente necessário, documentar justificativa, alternativa e impacto; não provisionar.

Isso não é afirmar que operar o SaaS comercialmente custará zero. Este pacote não contém cotação atual de infraestrutura, previsão de receita ou dimensionamento produtivo.

## Local primeiro para validar a mudança

Manter a toolchain existente e laboratórios sintéticos separados. Não depender da conta pessoal/PC de Gui como requisito de produção futuro, mas não transformar essa preocupação em migração de nuvem agora. Não criar containers, orquestradores ou serviços de sistema se o fluxo local existente atender.

Registrar somente processos/arquivos criados pelo incremento. Encerrar pelo PID/handle conhecido, nunca matar todos os processos Node/.NET/PostgreSQL. Não abrir túnel ou escutar interfaces externas sem necessidade/autorização.

## Cloudflare e endereço

Preservar a integração Cloudflare que já existe no checkout e revisar os scripts impactados. Pages pode servir uma demonstração estática e tem regras próprias para Functions [S06]; isso não comprova hospedagem automática do backend .NET/PostgreSQL. Não portar o backend para Workers/D1 nem substituir o banco para alegar custo zero.

A documentação atual também orienta Workers para novos projetos em vários casos [S07]. Essa orientação geral não é motivo para migrar este sistema durante uma separação modular. Antes de publicar algo novo, comparar com a arquitetura efetiva e exigir autorização.

Wissian não precisa de site/domínio próprio enquanto for fundação privada. Endereços `*.pages.dev` só podem ser declarados criados/disponíveis após confirmação no serviço; nenhum foi reservado por este pacote. Identidade visual não pode depender de um domínio provisório hardcoded.

## Artefatos e ambientes

Separar artefato de engenharia, demo sintética, build de teste e release candidata. Não incluir `.codex`, C7, instruções internas, segredos, fixtures restritas, dumps, screenshots operacionais ou mapas de clientes em bundles públicos. Repositório privado não torna privado o site publicado [S08].

Cada build declara versão, composição e contratos suportados. Preferir mecanismo de release/rollout/assinatura atual, sem criar outro registry paralelo. Não alterar políticas de assinatura/atualização para facilitar renomeação.

## Observabilidade mínima

Preservar health checks e logs já existentes; acrescentar somente identificação técnica do módulo/produto quando necessária, sem dados pessoais. Medir custo/recursos no laboratório quando possível. Não adicionar serviços externos de analytics ou crash reporting automaticamente.

## Futuro piloto real

Fora da execução atual: definir hospedagem, backups, monitoramento, retenção, TLS, recuperação, suporte, termos/contratos, titularidade e autorização institucional. Preparar uma lista de pendências, não uma compra nem deploy. A autorização atual termina antes da ativação externa.
