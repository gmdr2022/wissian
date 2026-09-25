# Exemplos de contratos — referência, não implementação

Estes arquivos ilustram o resultado lógico da composição. **Nenhum arquivo habilita uma feature, cria uma conta, concede acesso ou deve ser copiado como configuração de produção.** O Codex deve adaptar o contrato ao código existente, evitando um segundo cadastro de permissões ou versões paralelas de estruturas já prontas.

| Arquivo | Finalidade |
|---|---|
| [Catálogo](catalogo_modulos.exemplo.json) | IDs de exemplo, responsabilidades e dependências diretas. |
| [Fundação neutra](fundacao_neutra.exemplo.json) | Identidade/escopo/configuração/auditoria e shell sem domínio socioeducativo. |
| [Socioeducativo sintético](socioeducativo.exemplo.json) | Composição que consome o mesmo core e capacidades opcionais. |
| [Extensão técnica](extensao_tecnica.exemplo.json) | Prova limitada de reutilização sem construir outro negócio. |

## Tipos e semântica

`schema_version` é inteiro e vale 1 neste exemplo. `reference_only` é sempre verdadeiro. `module_ids` e `requires` são listas de IDs únicos. A versão `0.1.0-example` é ilustrativa, não a versão atual de uma biblioteca existente.

O catálogo usa as camadas `core`, `presentation`, `shared_capability`, `vertical` e `test_fixture`. A classificação é proposta; os caminhos reais, granularidade e dependências devem ser descobertos no checkout. Não criar onze novos projetos só porque o catálogo tem onze entradas. Preferir os menores limites necessários e verificáveis.

Cada composição deve conter todas as dependências declaradas de seus módulos. Não resolver uma referência ausente habilitando módulos adicionais silenciosamente. O núcleo não depende da vertical, da UI ou da fixture. O shell usa contratos neutros, não imports dos componentes específicos.

`runtime_activation` explicita que esses exemplos não definem o estado de feature flags. `real_data_authorized` falso e `authorization_grants` vazio impedem interpretar este pacote como autorização; o controle real deve permanecer server-side, nos mecanismos existentes. O produto socioeducativo não fica liberado para uso real pela existência do descriptor.

## Reserva do ClubAL

Não há composição executável ClubAL neste pacote. O produto futuro está reservado na documentação; capacidades de agenda/telas já utilizadas pelo socioeducativo continuam mantidas. Não criar uma aplicação vazia, um site ou um catálogo de preços para preencher essa reserva.

## Validação aqui versus validação no SaaS

O verificador do pacote checa estrutura básica, IDs, fechamento de dependências e ciclos dos exemplos. Isso **não** testa carregamento de módulos, compilação, autorização, migrations, browser ou runtime. Esses testes pertencem ao Codex após implementação, conforme [contratos](../06_CONTRATOS_DE_MODULOS.md) e [aceite](../13_TESTES_E_ACEITE.md).
