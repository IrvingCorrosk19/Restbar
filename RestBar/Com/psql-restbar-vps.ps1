# RestBar VPS PostgreSQL helper — ALWAYS use restbaruser (role "postgres" does NOT exist on VPS).
# Examples:
#   .\RestBar\Com\psql-restbar-vps.ps1 -Sql "SELECT count(*) FROM branches;"
#   .\RestBar\Com\psql-restbar-vps.ps1 -SqlFile .\scripts\explain.sql
param(
    [string]$Sql = "",
    [string]$SqlFile = "",
    [string]$RemoteHost = "root@164.68.99.83"
)

$ErrorActionPreference = "Stop"
$plink = "C:\Program Files\PuTTY\plink.exe"
$pscp = "C:\Program Files\PuTTY\pscp.exe"
$password = $env:RESTBAR_SSH_PASSWORD
if (-not $password) { throw "Set RESTBAR_SSH_PASSWORD" }
$hostkey = $env:RESTBAR_SSH_HOSTKEY
$hk = if ($hostkey) { @("-hostkey", $hostkey) } else { @() }

# Hard rule: RestBar container DB user is restbaruser — never postgres.
$dbUser = "restbaruser"
$dbName = "RestBar"

if ($SqlFile) {
    if (-not (Test-Path $SqlFile)) { throw "File not found: $SqlFile" }
    $remote = "/tmp/restbar_query_$([guid]::NewGuid().ToString('N')).sql"
    & $pscp -pw $password -batch @hk $SqlFile "${RemoteHost}:$remote" | Out-Host
    & $plink -ssh -pw $password -batch @hk $RemoteHost "docker cp $remote restbar_postgres:$remote && docker exec restbar_postgres psql -U $dbUser -d $dbName -f $remote; rm -f $remote; docker exec restbar_postgres rm -f $remote" 2>&1
    return
}

if (-not $Sql) { throw "Pass -Sql or -SqlFile" }

# Escape for remote single-quoted bash -c via plink: write temp file approach is safer
$tmpLocal = [IO.Path]::GetTempFileName() + ".sql"
Set-Content -Path $tmpLocal -Value $Sql -Encoding UTF8
try {
    $remote = "/tmp/restbar_query_$([guid]::NewGuid().ToString('N')).sql"
    & $pscp -pw $password -batch @hk $tmpLocal "${RemoteHost}:$remote" | Out-Null
    & $plink -ssh -pw $password -batch @hk $RemoteHost "docker cp $remote restbar_postgres:$remote && docker exec restbar_postgres psql -U $dbUser -d $dbName -f $remote; rm -f $remote; docker exec restbar_postgres rm -f $remote" 2>&1
}
finally {
    Remove-Item $tmpLocal -Force -ErrorAction SilentlyContinue
}
