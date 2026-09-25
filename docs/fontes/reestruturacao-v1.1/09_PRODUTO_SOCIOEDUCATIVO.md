# Primeiro produto — socioeducativo

## Identidade e foco

Nome comercial a definir. `socioeducativo` é um identificador de trabalho nos exemplos, sujeito à compatibilidade com os IDs encontrados. Não batizar como “PEMSE SaaS”; PEMSE, Elo e outras instituições são possíveis clientes, não domínios hardcoded nem demonstração de demanda contratada.

O benefício inicial a validar é organizar e comunicar a programação operacional entre coordenação e equipes, reduzindo retrabalho e informação desatualizada. Trata-se de hipótese comercial, não resultado já medido. A reestruturação deve favorecer esse fluxo sem descartar recursos existentes mais amplos.

## Superfícies preservadas

**Operação:** edição, planejamento, controle e publicação conforme permissões. **Rotinas:** exibição de conteúdo aprovado/minimizado, incluindo resiliência offline prevista. **Consulta:** consumo autorizado e somente leitura, sem virar backoffice de relatórios.

Essas três experiências pertencem à composição socioeducativa. As capacidades técnicas de agenda, publicação, player e consulta podem ser compartilhadas com produtos futuros, sem levar regras específicas do socioeducativo.

## Domínio que não vai ao core

Instalações/ocupação, alocação de adolescentes, cenários planejados, movimentos, fluxo de confirmação, regras do layout, impedimentos, operação restrita, conteúdo normativo específico e modelos de documentos do setor. Os nomes são exemplos de capacidades já relatadas/identificadas, cuja cobertura integral deve ser inventariada antes de alterar.

Manter separação entre estado confirmado e cenário planejado; aprovação e execução; início e confirmação; decisão humana e validação automática. Preservar maker-checker, MFA, ACK, idempotência, revisão otimista e causalidade onde implementados. O Codex não deve simplificar fluxos críticos apenas para tornar a estrutura “genérica”.

Regras gerais de auditoria/permissões podem ser do core; a regra “quem pode aprovar determinada movimentação” continua no módulo socioeducativo. Não transformar alojamento em uma entidade universal `Resource` que perca garantias do domínio.

## Múltiplas instituições, mesma implementação

Configuração institucional deve suportar variações legítimas de unidades, marca, nomenclatura e políticas autorizadas sem branches por cliente. Não inferir que todas as instituições ou estados têm procedimentos idênticos. Regras com efeito institucional precisam de origem, vigência, versão e autoridade de aprovação, quando exigido.

Demonstrar dois clientes totalmente fictícios — por exemplo, Organização Aurora e Organização Horizonte — com pelo menos duas unidades no conjunto de testes. Não usar logotipos reais nem copiar políticas internas como fixture pública. Testar variação por configuração e isolamento no backend, cache, pesquisa, relatórios, dispositivos e notificações.

## Sequência funcional mínima a conservar

Entrar com usuário sintético → selecionar organização/unidade autorizada → consultar/alterar a programação conforme papel → publicar projeção minimizada → visualizar nas Rotinas → consultar pelo canal permitido → revisar auditoria e estado da publicação. Usuário somente leitor não deve editar nem exportar relatórios por chamadas diretas.

Manter recursos avançados sob os gates existentes; não habilitar ocupação/movimentação real, monitoramento, fonte normativa ou operação institucional apenas porque o produto agora tem um novo nome.

## Integrações governamentais e normas

Não prometer substituir SIPIA-SINASE nem importar/exportar automaticamente a sistemas oficiais. Qualquer integração depende de documentação, autorização, formato, responsabilidades e condições de acesso verificadas. Nenhuma API governamental foi integrada ou auditada neste pacote.

O plano não decide requisitos legais por estado. A arquitetura deve admitir adaptação delimitada, mas não anunciar conformidade nacional automática. Antes do piloto real: validação institucional e jurídica, finalidade dos dados, permissões, retenção, suporte e recuperação.

## Pronto para validação do produto não é “mercado validado”

Ao finalizar a reestruturação, produzir uma demonstração sintética reproduzível desse fluxo e um roteiro de medição: tempo para alterar/publicar rotina, erros/retrabalho observados, continuidade de uso e esforço de implantação. Não criar telemetria externa ou coletar dados pessoais sem aprovação. Investigar uso e disposição de pagar antes de iniciar outro produto completo.

## Aceite específico

Produto compõe a mesma fundação testada; não duplica autenticação/tenancy/auditoria; não exige PEMSE como organização especial; dois clientes sintéticos operam isolados; rotinas/consulta continuam autorizadas e minimizadas; nenhum gate humano/institucional mudou; funcionalidades preservadas são rastreáveis ao inventário.
