# Segurança de Wissian

## Estado atual

O repositório contém preparação estrutural. Não há versão executável, ambiente publicado, canal público de suporte ou certificação de segurança. Os itens abaixo são requisitos para o desenvolvimento futuro.

## Relatar uma vulnerabilidade

Comunique pelo canal privado já estabelecido com o responsável pelo projeto/Fultuma. Não abrir issue pública com credenciais, dados pessoais, detalhes exploráveis ou arquivos de clientes. Antes de receber contribuições externas, a Fultuma deve definir um endereço/canal privado de segurança e a política de resposta; nenhum endereço foi presumido aqui.

Inclua componente afetado, impacto e reprodução com dados sintéticos. Se houver segredo exposto, informar a referência do incidente sem repetir o valor; o responsável deve coordenar revogação/rotação. Apagar a linha não revoga uma credencial comprometida.

## Requisitos de implementação

- Autenticação padronizada e autorização no servidor, com negação por padrão; sessão não substitui permissão sobre organização e recurso.
- Identificar a organização a partir de associação validada. Testar isolamento em banco, cache, arquivos, jobs, exportações e logs.
- Separar credenciais de runtime e migração. Nunca entregar segredos do servidor ao navegador; filtros Git/Docker são barreiras auxiliares, não scanners de segredos.
- Validar entradas e limites, parametrizar consultas, restringir uploads e saídas de rede de integrações, proteger sessões e limitar tentativas abusivas.
- Minimizar dados; definir retenção e exclusão conforme o produto. Logs operacionais não devem reproduzir payloads sensíveis. Auditoria de ações relevantes tem acesso restrito e retenção própria.
- Criptografia em trânsito/repouso, backups e restauração demonstrada antes de dados reais; privilégios e políticas de armazenamento devem ser verificáveis.
- Revisão de dependências, licenças, lockfile e artefatos de entrega. Exceções precisam de responsável, justificativa e prazo de revisão.

Requisitos específicos de mercados e produtos serão avaliados quando o plano os identificar. Esta estrutura não declara conformidade legal, regulatória ou certificação.
