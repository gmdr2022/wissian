# UX, marca, acessibilidade e qualidade de produto

Fonte principal: [guia fundador integral](../fontes/workspace-2026-09-24/Wissian-Guia-Fundador-da-Plataforma.md), sobretudo §§4–21, 25–33 e 37–40. Fluxos administrativos: [plano da operação](../fontes/workspace-2026-09-24/PLANO_WISSIAN_OPERACAO_BASE.md) §§4–6/11. Herança: [UX v1.1](../fontes/reestruturacao-v1.1/11_UX_MARCA_I18N.md), TEM e comparação Base44 em [absorção](04_ABSORCAO_CODEX_BASE44.md).

## Identidade e hierarquia

Wissian é a plataforma; Fultuma Tech Ltda. é a fornecedora indicada nas fontes. Produto vertical, organização cliente e preferências pessoais são camadas diferentes. Não declarar registro de marca, domínio disponível, titularidade de cada asset ou licença de terceiros como fato sem evidência. A origem linguística e as formulações oficiais permanecem no guia fundador; não recontar a etimologia de memória ou transformá-la em promessa técnica.

Usar os textos oficiais do guia onde fizerem sentido. Não criar nesta entrega um logotipo substituto para o que Gui alterou no Base44. Inventariar SVG/raster, fontes, licenças, variantes, estado aprovado e contexto antes de escolher. IDs técnicos novos do Wissian são separados de rótulos visuais; traduções e renomeações não mudam autorizações nem identidades de dados.

## Experiência administrativa útil

Entrada identifica organização ativa, tarefa principal e pendências relevantes. Navegação apresenta só capacidades utilizáveis naquele contexto. Composição neutra não mostra alojamentos, socioeducandos, telas especializadas, menus desabilitados de outras verticais ou cartões “em breve”. Catálogo comercial, quando construído, deve ficar num contexto deliberado de administração/contratação; não ocupar a rotina com propaganda.

M2 entrega localizar → abrir → relacionar documento/contraparte, com filtros preservados e retorno previsível. M3 entrega nota registrada → parcelas → baixa parcial/estorno. M4 entrega conciliação → revisão → pacote mensal versionado. Rótulos precisam diferenciar documento, registro fiscal, movimento financeiro e evidência de conferência. Exportar não é enviar; registrar nota não é emiti-la oficialmente; documento anexado não prova validade jurídica.

Não presumir que grid mais bonito ou frontend mais novo é melhor. Comparar a versão Base44 e a versão Codex no mesmo cenário, viewport e estado: passos necessários, perda de contexto, consistência, erros, recuperação e acessibilidade. O mapa de alojamentos, sua geometria, agrupamentos, ocupação e regras ficam na vertical adequada. Não reduzir a mudança de Gui a “tema” sem caracterizá-la.

## Estados obrigatórios por fluxo

| Estado | Comportamento verificável |
|---|---|
| Vazio inicial | Explicar o que é possível fazer e uma primeira ação permitida; não criar dados silenciosamente |
| Carregando | Progresso compreensível; não duplicar ação com múltiplos cliques; permitir sair quando seguro |
| Sem resultado | Preservar filtros e mostrar como ajustar; não confundir com falta de permissão |
| Erro parcial | Identificar itens afetados, o que foi salvo e como retomar; sem sucesso global falso |
| Conflito de edição | Preservar rascunho e pedir reconciliação informada; não sobrescrever versão mais nova |
| Permissão revogada | Bloquear a ação e limpar conteúdo indevido; manter saída/recuperação segura |
| Mudança de organização | Confirmar descarte/guardar rascunho de forma isolada; limpar caches e seleções do contexto anterior |
| Arquivado/desativado | Histórico autorizado recuperável; não sugerir exclusão de dados ou execução de jobs ativos |

## Acessibilidade e uso real

Critérios de implementação propostos: operação integral por teclado; ordem/foco visíveis e restauração após modal; nomes e relações semânticos; mensagens lidas por tecnologia assistiva; erro associado ao campo; contraste adequado; informação além de cor; zoom/reflow sem perda de ação; alvos e densidade úteis; movimento reduzido. Validar manualmente os fluxos principais junto de verificações automatizadas. Ferramenta automática sozinha não comprova acessibilidade.

Preservar temas e preferências úteis de TEM sem importar acoplamentos institucionais. Hierarquia proposta: identidade do produto, escolhas permitidas da organização, preferência da pessoa, acessibilidade. Tema não concede autoridade. Preferências legadas exigem mapeamento explícito e rollback, não renomeação global de chaves.

Preparar textos externalizáveis, locale, fuso por organização/usuário quando necessário e valores monetários exatos. Operar inicialmente no idioma validado; preparação internacional não autoriza declarar suporte em mercados, impostos ou idiomas não testados. Datas de competência, instante UTC e data civil são conceitos distintos.

## Critério de qualidade e validação humana

Escolher cenários reais de trabalho com dados sintéticos, medir capacidade de concluir e erros, registrar fricção e decisões. Não inventar metas de latência, sucesso ou conversão antes de baseline; definir orçamento e amostra em M0/M1. Diferenciação buscada vem de fluxos confiáveis, contratos, domínio, rastreabilidade e evolução consistente. Nenhum pacote documental garante exclusividade ou impede concorrentes.

Registrar aceite humano por fluxo/versão no mecanismo disponível; Codex Checkpoint, se ainda disponível como ferramenta legada, é auxiliar opcional. Pendências e pedidos de ajustes continuam pendentes até Gui decidir, sem exigir continuidade de desenvolvimento do CC. Screenshots de demonstração não aprovam comportamento, isolamento ou uso em dispositivo. Gamificação só se houver finalidade explícita e compatível com o guia: feedback contextual opcional, sem pressão ou ranking criado por padrão.

## Experiência Continuity

O [trio no capítulo 16](16_CONTINUIDADE_OPERACIONAL.md) herda estados de UX/acessibilidade deste capítulo. Agora explica motivo e ação adequada; Passagens separa fotografia e mudanças atuais; Verificações mostram instrução, resultado esperado, versão e decisões humanas claras. Adiado tem prazo/motivo; bloqueio indica responsável sem vazar dependência restrita. Ruído não vira urgência; telemetria repetida não cria novos cards/avisos.

Ordenação determinística, teclado/foco, leitor de tela, reflow, contraste, estados vazios/desatualizados, falhas e conflitos fazem parte de TC-13/14/17. Sem capability, não há card vazio ou menu promocional. Não expor tenant, handler, stable key ou marca técnica como requisito de compreensão. Naming final será validado no fluxo, sem alterar IDs técnicos.

O trio prioritário não apaga Pendências, Recursos/contexto, Saúde operacional, Histórico ou a informação futura “Depois”. Podem ser seções integradas ao fluxo, sem obrigar menus separados. Os cenários, públicos e expectativas da conversa estão no capítulo 16, seção 10, e na [matriz](matrizes/reaproveitamento-nexus-cc.json).
