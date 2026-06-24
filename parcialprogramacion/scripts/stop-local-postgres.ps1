$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$postgresBin = "C:\Program Files\PostgreSQL\18\bin"
$dataDir = Join-Path $projectRoot ".postgres-data"

& (Join-Path $postgresBin "pg_ctl.exe") `
    -D $dataDir `
    stop
