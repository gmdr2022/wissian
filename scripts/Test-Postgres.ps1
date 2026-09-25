[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PostgresBin,
    [string]$Dotnet = 'dotnet',
    [ValidateSet('Persistence', 'Api', 'Browser', 'Counterparties', 'CounterpartiesBrowser')][string]$Suite = 'Persistence'
)

$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$workParent = Join-Path $repoRoot '.local/postgres-tests'
$runRoot = Join-Path $workParent ([Guid]::NewGuid().ToString('N'))
$cluster = Join-Path $runRoot 'cluster'
$passwordFile = Join-Path $runRoot 'bootstrap-password.tmp'
$started = $false
$priorAdminConnection = $env:WISSIAN_TEST_ADMIN_CONNECTION

function Invoke-PgControl([string]$Action) {
    $arguments = @('-D', ('"' + $cluster + '"'), '-w', '-t', '45', $Action)
    if ($Action -eq 'start') {
        $arguments += @('-l', ('"' + (Join-Path $runRoot 'postgres.log') + '"'), '-o', ('"-h 127.0.0.1 -p ' + $port + ' -c shared_buffers=32MB -c max_connections=12"'))
    } else {
        $arguments += @('-m', 'fast')
    }
    $process = Start-Process -FilePath (Join-Path $PostgresBin 'pg_ctl.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runRoot ($Action + '.out')) -RedirectStandardError (Join-Path $runRoot ($Action + '.err'))
    # Wait for pg_ctl only; Start-Process -Wait would wait for the server's process tree.
    if (-not $process.WaitForExit(55000)) { throw "pg_ctl $Action excedeu o prazo; verificar o laboratório $runRoot." }
    if ($process.ExitCode -ne 0) { throw "pg_ctl $Action falhou; consultar logs em $runRoot." }
    $process.Dispose()
}

foreach ($binary in @('initdb.exe', 'pg_ctl.exe', 'postgres.exe')) {
    if (-not (Test-Path -LiteralPath (Join-Path $PostgresBin $binary) -PathType Leaf)) {
        throw "Binário PostgreSQL ausente: $binary"
    }
}
if ((& (Join-Path $PostgresBin 'postgres.exe') --version) -ne 'postgres (PostgreSQL) 18.6') {
    throw 'Laboratório fixado em PostgreSQL 18.6.'
}

New-Item -ItemType Directory -Path $runRoot | Out-Null
$listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()
$passwordBytes = New-Object byte[] 32
$random = [Security.Cryptography.RandomNumberGenerator]::Create()
$random.GetBytes($passwordBytes)
$random.Dispose()
$password = [BitConverter]::ToString($passwordBytes).Replace('-', '')
[Array]::Clear($passwordBytes, 0, $passwordBytes.Length)
try {
    [IO.File]::WriteAllText($passwordFile, $password, [Text.UTF8Encoding]::new($false))
    & (Join-Path $PostgresBin 'initdb.exe') -D $cluster -U wissian_test_admin --encoding=UTF8 --locale=C --auth=scram-sha-256 --pwfile=$passwordFile *> (Join-Path $runRoot 'initdb.log')
    if ($LASTEXITCODE -ne 0) { throw 'initdb falhou; consultar log local do laboratório.' }
    Remove-Item -LiteralPath $passwordFile
    Invoke-PgControl 'start'
    $started = $true
    $env:WISSIAN_TEST_ADMIN_CONNECTION = "Host=127.0.0.1;Port=$port;Database=postgres;Username=wissian_test_admin;Password=$password;Pooling=false;Timeout=10"
    $targetSuite = if ($Suite -in @('Browser', 'Counterparties', 'CounterpartiesBrowser')) { 'Api' } else { $Suite }
    $suiteArguments = switch ($Suite) {
        'Browser' { @('--', '--browser') }
        'Counterparties' { @('--', '--counterparties') }
        'CounterpartiesBrowser' { @('--', '--counterparties-browser') }
        default { @() }
    }
    & $Dotnet run --project (Join-Path $repoRoot ('tests/Wissian.' + $targetSuite + '.Tests')) --no-restore --disable-build-servers -p:BuildInParallel=false -p:UseSharedCompilation=false @suiteArguments
    if ($LASTEXITCODE -ne 0) { throw "Testes $Suite falharam." }
} finally {
    $env:WISSIAN_TEST_ADMIN_CONNECTION = $priorAdminConnection
    $password = $null
    if (Test-Path -LiteralPath $passwordFile) { Remove-Item -LiteralPath $passwordFile }
    # Only this invocation's new, dedicated cluster is stopped. Data remains for diagnosis.
    if ($started -or (Test-Path -LiteralPath (Join-Path $cluster 'postmaster.pid'))) {
        Invoke-PgControl 'stop'
    }
}
