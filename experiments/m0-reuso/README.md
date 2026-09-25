# M0 — ensaio de extração do domínio

Caracteriza fragmentos reais do ClubAL sem instalar pacotes ou executar a aplicação
de origem. `provenance.json` identifica commit, caminho, seleção e hashes. Namespace
original é preservado para não confundir ensaio com Core implementado do Wissian.
O recorte contém sessão, associação, organização, auditoria e seus dois auxiliares.
`Tenant.cs` inclui apenas `Tenant`; não inclui unidade/localidade ou suas regras.

Com SDK .NET 10 instalado, executar da raiz:

```powershell
dotnet restore experiments/m0-reuso/M0.ReuseProbe.csproj --configfile experiments/m0-reuso/NuGet.Config
dotnet run --project experiments/m0-reuso/M0.ReuseProbe.csproj --no-restore --disable-build-servers
```

As fontes de pacotes ficam vazias deliberadamente: o ensaio usa somente a biblioteca
padrão. O processo retorna código diferente de zero se qualquer caso falhar. Os 16
casos usam instante e identificadores sintéticos fixos. Não abrem servidor, banco,
navegador nem serviços externos. Não comprovam autenticação, isolamento persistente,
UI ou aceite do SaaS; esses gates pertencem a M1. `SelectContext` exige validação
prévia de associação pelo serviço; a entidade isolada não autoriza o tenant.

Não foi localizada licença explícita no ref de origem. A extração privada conserva
procedência e não atribui licença nova nem autoriza distribuição pública dos trechos.
Assets, SDK Base44, credenciais, seeds, migrations e bancos não são copiados.

Rollback: retirar este ensaio do consumo; nenhuma aplicação ou banco depende dele.
A origem permanece intacta. Não promover os arquivos `origin/` diretamente a M1:
adaptar contratos e limites conforme a ADR-M03 e provar acesso com persistência real.
