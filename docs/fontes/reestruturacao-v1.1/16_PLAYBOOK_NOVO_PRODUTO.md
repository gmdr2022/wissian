# Como conceber o próximo produto sobre a fundação

**Aplicação comercial posterior.** Nesta reestruturação, executar apenas a prova técnica fictícia descrita abaixo; não iniciar ClubAL ou outro nicho completo.

## Receita de reutilização

Definir público/necessidade e fluxo mínimo; selecionar capacidades existentes; declarar composição e apresentação; implementar somente o domínio distinto; configurar permissões e projeções; criar fixtures e testes de isolamento; produzir artefato versionado; validar upgrade/rollback e implantação antes de uso real.

O produto não copia auth, tenancy, auditoria ou shell. Ele consome a mesma implementação do core, com versão e contratos explícitos. Customização de cliente fica em configuração suportada, não em branch ou clone permanente.

## Prova fictícia desta fase

Criar no escopo de teste uma extensão mínima, por exemplo “Anotações sintéticas de demonstração”, com um DTO sem pessoas reais e uma operação autorizada. Esse nome é apenas um exemplo técnico, não novo requisito de negócio nem produto.

Ela deve: registrar-se pela interface pública; usar escopo/identidade fornecidos pela fundação; emitir auditoria conforme contrato; expor uma superfície mínima; ser removível da composição sem editar lógica interna do core; rejeitar outro cliente; e compartilhar a mesma versão do núcleo consumida pelo produto socioeducativo.

Não publicar a extensão nem adicionar serviço/banco/SDK para ela. Se um módulo existente e puramente sintético já cumpre a prova, reutilizá-lo. Testar um novo módulo é diferente de multiplicar sistemas comerciais.

## Pacote local reutilizável

Ao fechar a v1, produzir receita de consumo contendo: versão do core/capacidades; matriz de compatibilidade realmente testada; arquivos/artefatos necessários; configuração mínima sem secrets; método de bootstrap; testes de contrato; como registrar um módulo; como atualizar; como preservar IDs e dados; como verificar integridade.

Escolher formato de consumo coerente com o monorepo/toolchain atual. Não publicar NuGet/npm nem configurar registry privado pago sem necessidade e autorização. Arquivar uma cópia de fonte não substitui versão e política de atualização.

## Quando separar repositórios

Reavaliar só quando equipes/ciclos de release, confidencialidade ou operação justificarem. Se separar, consumir pacotes/versionamento compartilhados, definir processo de atualização e rodar testes dos consumidores. Template GitHub gera uma nova base de arquivos, não “herança viva” do núcleo [S04].

## Critério de benefício

A fundação melhora velocidade quando a criação de uma composição nova não exige reconstruir infraestrutura comum. Demonstrar isso com a prova e os passos executados, não com porcentagem inventada de economia de tempo. A qualidade do segundo produto ainda dependerá de compreender seu usuário e domínio.
