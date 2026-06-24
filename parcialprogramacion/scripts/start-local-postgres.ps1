$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$postgresBin = "C:\Program Files\PostgreSQL\18\bin"
$postgresPort = 55433
$dataDir = Join-Path $projectRoot ".postgres-data"
$logFile = Join-Path $dataDir "postgres.log"
$postmasterPid = Join-Path $dataDir "postmaster.pid"

& (Join-Path $postgresBin "pg_ctl.exe") -D $dataDir status 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0 -and (Test-Path $postmasterPid)) {
    Remove-Item -LiteralPath $postmasterPid -Force
}

& (Join-Path $postgresBin "pg_isready.exe") -h 127.0.0.1 -p $postgresPort -U postgres | Out-Null
if ($LASTEXITCODE -ne 0) {
    & (Join-Path $postgresBin "pg_ctl.exe") `
        -D $dataDir `
        -o "`"-p $postgresPort -h 127.0.0.1`"" `
        -l $logFile `
        start

    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo iniciar PostgreSQL local en 127.0.0.1:$postgresPort."
    }
}
