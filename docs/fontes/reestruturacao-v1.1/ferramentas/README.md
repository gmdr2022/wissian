# Ferramentas de integridade da entrega

O verificador usa apenas a biblioteca padrão do Python 3. Ele lê esta pasta e não faz chamadas de rede, não inspeciona o repositório ClubAL, não altera o banco e não executa Git, npm, dotnet ou deploy. O Codex pode utilizá-lo antes de consumir o pacote; Gui não precisa executá-lo manualmente.

A partir da raiz extraída, no Windows:

```powershell
py -3 -B ferramentas/validar_pacote.py --json
py -3 -B ferramentas/testar_validador.py
```

Em um ambiente com Python disponível como `python3`, substituir `py -3` por `python3`. Para apontar a outra pasta extraída, usar `--root "CAMINHO_DA_PASTA"` no primeiro comando. Os comandos acima verificam somente o pacote, não a aplicação.

## O que é verificado

Inventário de arquivos e hashes SHA-256; caminhos relativos seguros; ausência de links simbólicos; JSON legível; links Markdown locais; IDs e rastreabilidade entre requisitos, testes e evidências; todos os testes da aplicação ainda especificados como não executados; coerência estrutural dos exemplos de composição.

O manifesto lista todos os arquivos do pacote, **exceto ele próprio**, para evitar autorreferência de hash. Um hash comprova igualdade de bytes com o manifesto recebido, não autoria, segurança do software nem assinatura digital. Alterações em arquivos entregues devem causar diferença: mantenha o pacote original como referência e faça a implementação no repositório, sem atualizar o manifesto para esconder mudanças.

Os testes do verificador trabalham em cópias temporárias e simulam adulteração, arquivo faltante, caminhos indevidos, JSON inválido, referências quebradas e erros nos exemplos. Só essas cópias são modificadas. O verificador em si é somente leitura. Nenhum desses testes valida o ClubAL.

## Saída

Exit code 0: verificações previstas aprovadas. Exit code 1: inconsistência ou erro de leitura, com diagnóstico. `--json` produz um resumo estruturado. Links externos, direitos de marca e conteúdo do cofre C7 não são verificados pela ferramenta.

O relatório de execução efetiva desta entrega está em [99 — validação do pacote](../99_VALIDACAO_DO_PACOTE.md).
