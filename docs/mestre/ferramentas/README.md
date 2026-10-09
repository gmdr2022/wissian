# Ferramentas documentais

Python 3 e biblioteca padrão; sem rede, credenciais, dependências externas ou execução do código de produto/fontes. Não escolhem a stack do SaaS.

Na raiz do workspace:

```powershell
python -B docs/mestre/ferramentas/validar_pacote.py --root . --workspace --json
python -B docs/mestre/ferramentas/atualizar_manifesto.py --root .
python -B docs/mestre/ferramentas/atualizar_manifesto.py --root . --apply
python -B docs/mestre/ferramentas/testar_validacao.py
```

- `validar_pacote.py`: somente leitura; hashes/conjunto do escopo documental, links, UTF-8/JSON, 75 fontes, rastreabilidade, composições e estados de preparação. `validar_continuity.py` é seu verificador de contrato específico.
- `atualizar_manifesto.py`: prévia por padrão; `--apply` publica o manifesto após validar a proposta inteira. `--add`/`--remove` declaram mudanças de conjunto; o atualizador não altera documentos nem aprova produto.
- `testar_validacao.py`: fixtures temporárias e negativos; não altera a origem, instala dependências ou executa testes SaaS. Um teste de symlink pode ficar SKIPPED no Windows sem privilégio; não elevar permissão só para mudar esse resultado.

O modo `--workspace` deixa código/dependências fora do escopo, preservando a fiscalização de documentos. O modo padrão estrito serve a uma distribuição documental limpa. `--installed` conserva compatibilidade com pacotes anteriores e a conferência da cópia histórica de nome exato; não é incentivo a duplicar o workspace.

[Procedimento completo de manutenção/recuperação](../14_INSTALACAO_E_RESTAURACAO.md). Record execution results in the change's existing sanitized evidence, following chapter 12; não manter relatórios estáticos que aparentem validar versões posteriores.
