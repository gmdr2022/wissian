# Validação do pacote Wissian

**Data:** 22/09/2026. **Versão do pacote:** 1.1.0. **Ambiente desta revisão:** Python 3.14.6, Windows.

## Escopo desta revisão

Esta entrega atualiza a identidade da plataforma-base para **Wissian**, a relação com **Fultuma / Fultuma Tech Ltda** e as orientações de recebimento na pasta dedicada do projeto. O alinhamento de identidade foi conferido no `Wissian-Guia-Fundador-da-Plataforma.md` disponível localmente. O prompt de acompanhamento foi atualizado e incluído no ZIP.

Os nomes de apresentação, exemplos, decisões, prompts e manifesto foram revisados. A entrada do plano agora distingue a pasta de destino Wissian do checkout legado ClubAL e condiciona a implementação ao pedido de execução de Gui. Os arquivos originais foram preservados.

Arquitetura, stack, roadmap, IDs técnicos, 32 requisitos, 60 casos planejados e 28 itens de inventário foram preservados. O documento de estado observado e as evidências remotas do repositório permanecem históricos; o ClubAL, as branches e os serviços não foram inspecionados novamente ou modificados nesta revisão. As fontes públicas também não foram reconsultadas.

Os resultados abaixo se referem **ao pacote documental e ao seu verificador**, não à implementação da plataforma ou dos produtos.

## Verificações executadas

| Verificação | Resultado |
|---|---|
| Integridade do pacote original antes da revisão | PASS — 48 arquivos, sem inconsistências |
| Manifesto revisado, tamanhos e hashes SHA-256 | PASS — 48 arquivos cobertos; manifesto exclui a si próprio |
| JSON, caminhos relativos, links Markdown internos e rastreabilidade | PASS |
| Dependências e composições dos exemplos | PASS no escopo estrutural do verificador |
| Suite do próprio verificador, no Windows | 20 casos descobertos: 19 aprovados e 1 não executado por limitação do ambiente |
| Preservação das matrizes e evidências históricas do repositório | PASS — igualdade de bytes conferida com o pacote original |
| Nome anterior em arquivos e conteúdo do ZIP revisado | Nenhuma ocorrência |
| ZIP final | CRC e igualdade de conteúdo com a pasta validada conferidos |
| Testes do SaaS, .NET, React, PostgreSQL, browser ou dispositivos | NOT_RUN |
| Publicação, dados reais, implantação ou registro de marca | NÃO EXECUTADOS por esta revisão |

## Execução reproduzível

Na raiz extraída do pacote, utilizando Python 3:

```powershell
python -B ferramentas/validar_pacote.py --json
python -B ferramentas/testar_validador.py
```

Nesta revisão, os scripts foram chamados pelo caminho absoluto do Python 3.14.6 instalado no host. Ambos retornaram exit code 0. O verificador retornou `ok: true`, sem inconsistências. A suite retornou `OK (skipped=1)`.

O caso `test_20_symlink_rejected` não foi executado porque o ambiente Windows não permitiu criar o link simbólico da fixture. Não foram alteradas permissões ou configurações do sistema. Esse resultado não comprova o cenário de rejeição de symlink neste host; os demais 19 casos passaram. A lógica de segurança do verificador foi preservada.

O registro original da versão 1.0.0 relata execução em Linux/Python 3.13.5 com 20 casos aprovados. Esse resultado histórico não substitui a execução Windows descrita acima. Nenhuma dessas suites testa o software ClubAL ou Wissian.

## Inventário e rastreabilidade

O ZIP contém **49 arquivos**: 36 Markdown, 10 JSON, 2 scripts Python e 1 `Prompt.txt`. O manifesto cobre os outros 48 arquivos. Permanecem **32 requisitos, 60 casos de teste planejados, 28 itens de inventário inicial, 11 registros de inspeção remota e 10 referências públicas**. Todos os casos do SaaS permanecem `NOT_RUN`.

O campo `source_archive_sha256` do manifesto identifica os bytes do ZIP original utilizado na revisão. Os hashes do novo manifesto correspondem ao conteúdo revisado; não são reaproveitados da versão anterior.

## Limites e uso

A revisão não executou a reestruturação, migrações, mudanças no código do SaaS ou ações externas. Identificadores de compatibilidade ClubAL e referências históricas foram mantidos. A escolha interna do nome Wissian não comprova disponibilidade de domínio, registro de marca, constituição da empresa ou titularidade formalizada.

Use [o guia de entrada](01_LEIA_PRIMEIRO.md) e [o prompt de acompanhamento](Prompt.txt) para levar o plano à pasta dedicada. Extraia em uma subpasta de apoio, preservando a governança existente. O pacote documenta o trabalho futuro; a solicitação atual de Gui delimita o que poderá ser executado.
