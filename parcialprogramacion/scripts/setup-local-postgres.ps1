$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$postgresBin = "C:\Program Files\PostgreSQL\18\bin"
$postgresPort = 55433
$dataDir = Join-Path $projectRoot ".postgres-data"
$logFile = Join-Path $dataDir "postgres.log"
$schemaFile = Join-Path $projectRoot "sql\01_create_tables.sql"
$postmasterPid = Join-Path $dataDir "postmaster.pid"
$env:PGPASSWORD = "postgres"

if (-not (Test-Path $dataDir)) {
    & (Join-Path $postgresBin "initdb.exe") `
        --pgdata $dataDir `
        --username postgres `
        --auth trust `
        --encoding UTF8
}

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

$databaseExists = & (Join-Path $postgresBin "psql.exe") `
    -h 127.0.0.1 `
    -p $postgresPort `
    -U postgres `
    -d postgres `
    -w `
    -tAc "SELECT 1 FROM pg_database WHERE datname = 'dron_parcial';"

if ($LASTEXITCODE -ne 0) {
    throw "No se pudo consultar PostgreSQL local en 127.0.0.1:$postgresPort."
}

if ($databaseExists.Trim() -ne "1") {
    & (Join-Path $postgresBin "createdb.exe") `
        -h 127.0.0.1 `
        -p $postgresPort `
        -U postgres `
        -w `
        dron_parcial

    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo crear la base dron_parcial."
    }
}

& (Join-Path $postgresBin "psql.exe") `
    -h 127.0.0.1 `
    -p $postgresPort `
    -U postgres `
    -d dron_parcial `
    -w `
    -f $schemaFile

if ($LASTEXITCODE -ne 0) {
    throw "No se pudo aplicar el script SQL $schemaFile."
}
