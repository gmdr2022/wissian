# ADR 0001 — Base técnica inicial de Wissian

- **Status:** proposta; adoção depende da reconciliação com o plano existente.
- **Data:** 2026-09-22.
- **Escopo:** organização e direção arquitetural; nenhuma implementação autorizada por esta ADR.
- **Responsável pela decisão de produto:** Gui, pela Fultuma.

## Contexto e premissas

O [guia fundador](../../../Wissian-Guia-Fundador-da-Plataforma.md) define uma plataforma SaaS para produtos operacionais distintos, com isolamento, contratos e portabilidade. Não define framework, MVP, volume, equipe, orçamento ou nuvem. A pasta iniciou com esse único documento e sem código.

Premissa de trabalho: primeiros consumidores usarão web e API, com dados predominantemente relacionais e uma equipe inicialmente pequena. **A premissa que mais pode mudar a proposta é o primeiro produto exigir operação offline/dispositivos ou uma stack já escolhida no plano existente.**

## Opções consideradas

| Opção | Benefício | Custo/limite | Posição |
| --- | --- | --- | --- |
| Somente guia, sem convenções | Nenhum compromisso técnico | Onboarding e decisões ficam implícitos | Insuficiente para a organização pedida |
| Next.js full-stack + PostgreSQL | Menos processos e configuração | Pode acoplar o backend compartilhado ao ciclo de uma interface | Alternativa forte se houver apenas um produto web |
| Monorepo TypeScript; Next.js + NestJS modular + PostgreSQL | API independente para consumidores e fronteiras explícitas | Dois processos e disciplina de contratos | Recomendação condicionada ao plano |
| Microsserviços desde o início | Deploy e escala independentes por serviço | Rede, dados distribuídos, operação e custo antes da demanda | Sem justificativa atual |

## Recomendação

Adotar a estrutura de monorepo como organização e propor backend modular, usando TypeScript, Next.js, NestJS, PostgreSQL e pnpm. O recurso de [workspaces do pnpm](https://pnpm.io/workspaces) permite coordenar os pacotes; os [módulos do NestJS](https://docs.nestjs.com/modules) oferecem exports explícitos para compor a API. A [estrutura do Next.js](https://nextjs.org/docs/app/getting-started/project-structure) acomoda rotas em `src/app` e código separado por funcionalidade.

Essas características sustentam a organização proposta; a adequação ao negócio é uma avaliação arquitetural, não uma conclusão dos fornecedores. Java/.NET/Python não estão tecnicamente excluídos: experiência da equipe ou restrição do plano pode justificar outra stack antes de haver custo de migração.

Versões exatas serão fixadas no bootstrap após verificar compatibilidade e suporte. Selecionar uma versão [Node.js LTS suportada](https://nodejs.org/en/about/previous-releases); não escolher apenas o maior número disponível. Não foi criada dependência crítica nem contratado serviço.

## Consequências e riscos

- Aceita-se o custo de web e API separados para reduzir acoplamento com futuros consumidores. Se isso não for necessário no plano, a alternativa full-stack merece preferência pela simplicidade.
- Compartilhar TypeScript ajuda ferramentas e contratos, mas não substitui validação em runtime nem impede acoplamento entre módulos.
- Banco compartilhado exige prova de isolamento e impõe risco de impacto coletivo. Requisitos especiais podem exigir outra unidade de isolamento.
- O maior risco de escopo é construir capacidades sem consumidor. Cada módulo precisa de fluxo concreto, critério de aceite e responsável.
- Dois processos podem aumentar consumo local e operação. Começar com a menor topologia executável e medir capacidade antes de builds/lotes.

## Confiança e reavaliação

Confiança alta na necessidade de fronteiras e governança; moderada na stack, porque faltam o plano e os requisitos operacionais. Reavaliar **antes do primeiro scaffold**, se aparecerem offline obrigatório, restrição de linguagem/fornecedor, isolamento regulatório, equipe já especializada ou consumidor diferente da premissa.

Depois da implementação, extração de serviço exige evidência de gargalo, fronteira de segurança ou necessidade de deploy independente; crescimento abstrato não basta.

## Próximo passo e reversão

Ler o plano existente, mapear cada requisito à plataforma ou ao produto e confirmar a primeira entrega. Atualizar o status desta ADR quando houver decisão; então fixar versões, validar uma fatia ponta a ponta e só depois expandir.

Até lá, a reversão consiste em editar a proposta e as reservas de diretório. Não há aplicação, banco ou infraestrutura a migrar. O guia fundador permanece intacto.
