# Manutenção documental, arquivo e recuperação

## Escopo de trabalho

A entrada vigente é `README.md`; as regras locais estão em `AGENTS.md`. Contratos, matrizes e ferramentas permanecem em `docs/mestre/`; 75 fontes originais permanecem em `docs/fontes/`, com bytes e caminhos preservados. A reorganização não escolhe stack, implementa SaaS ou altera os repositórios de origem.

Backups, ZIPs, recibos e relatórios de sessões anteriores ficam fora do checkout, em arquivo recuperável registrado no C7. O inventário de preservação informa caminho original, tamanho, SHA-256 e atributos; o recibo de reorganização informa cada destino. Esses arquivos não são dependências da aplicação nem da leitura dos contratos. Não recriar cópias aninhadas para trabalhar.

## Validar no repositório em evolução

```powershell
python -B docs/mestre/ferramentas/validar_pacote.py --root . --workspace --json
```

`--workspace` confere `docs/`, `README.md`, `AGENTS.md`, `WISSIAN_CODEX_REGRA_MESTRA.md` e as configurações documentais/Git declaradas no verificador. A regra mestre integra o pacote documental e sua cópia de teste, pois é entrada operacional obrigatória. Código, dependências, builds e configurações de runtime fora desse escopo não entram no manifesto documental. Um arquivo novo ou alterado em `docs/` continua sujeito à validação; o modo não dispensa integridade das fontes, vínculos, matrizes ou exemplos. Links da camada documental devem ser autocontidos; caminhos de experimentos/produto fora do pacote são referências textuais, com evidências compartilháveis em `docs/`.

Sem `--workspace` ou `--installed`, o modo é estrito: verifica o conjunto inteiro de uma distribuição documental extraída. `--installed` permanece compatibilidade com os pacotes anteriores; suas exclusões antigas não são recomendação para voltar a manter backups no checkout. Nenhum modo comprova funcionamento ou segurança da aplicação.

## Atualizar um contrato sem trabalho manual repetido

Editar a fonte canônica, revisar o diff e usar:

```powershell
python -B docs/mestre/ferramentas/atualizar_manifesto.py --root .
python -B docs/mestre/ferramentas/atualizar_manifesto.py --root . --apply
```

A primeira chamada mostra a proposta; a segunda só grava se todo o contrato candidato for válido. Inclusões e retiradas de documentos exigem `--add caminho/relativo` e `--remove caminho/relativo` explícitos, repetidos quando necessário. Alterações de layout não autorizam modificar fontes históricas. O utilitário rejeita fonte adulterada, link quebrado, perda de rastreabilidade, inclusão fora do escopo e alterações concorrentes detectadas; uma falha não regrava o manifesto.

Executar depois o verificador no modo workspace. Quando mudar o verificador ou um contrato coberto por seus negativos, rodar a [suíte](ferramentas/README.md):

```powershell
python -B docs/mestre/ferramentas/testar_validacao.py
```

As matrizes de preparação e os exemplos permanecem planos, com estados NOT_RUN/NOT_IMPLEMENTED. Evidências reais do futuro produto serão registradas por caso, versão, ambiente e comando, conforme [aceite](10_TESTES_E_ACEITE.md). Não converter a preparação inteira em PASS, nem executar testes indiscriminadamente em cada consulta.

## Recuperar sem sobrescrever trabalho posterior

1. Localizar o arquivo de preservação e seu inventário no registro C7/recibo da reorganização. Confirmar hash do ZIP e qual estado ele representa.
2. Extrair em destino vazio separado e conferir caminhos, contagens, tamanhos e hashes. Os atributos originais também estão registrados; extração ZIP sozinha não prova sua restauração.
3. Para uma fonte original, preferir sua cópia íntegra no acervo. Para um documento ativo, comparar com a versão atual antes de repor.
4. Preservar alterações posteriores e aplicar somente os itens necessários. Recuperação integral, troca de diretório ou sobrescrita em massa exige escopo/autorização correspondente; não usar reset ou exclusão ampla como atalho.

O backup integral anterior e seu ensaio de restauração preservam inclusive o estado Git. A reorganização mantém `.git` no local, sem troca de branch ou histórico. Recibos de entregas antigas são históricos e não devem orientar uma nova instalação como se fossem o estado atual.

## Regras para manter a organização

Uma decisão tem uma fonte canônica; outros arquivos apontam para ela. Guias de entrada não repetem contratos longos. Históricos não são instruções. A matriz de fontes é a chave para busca dirigida no acervo. Relatórios/diários de sessão vão ao C7; snapshots e backups ficam fora da pasta ativa. Novos documentos entram apenas quando têm consumidor e responsabilidade claros.
