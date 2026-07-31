# Restore RestBar from pg_dump -Fc file into restbar_postgres (DESTRUCTIVE for RestBar DB only).
# Requires explicit -ConfirmRestore YES
param(
    [Parameter(Mandatory = $true)][string]$DumpPath,
    [string]$RemoteHost = "root@164.68.99.83",
    [switch]$LocalDocker,
    [string]$ConfirmRestore = ""
)

$ErrorActionPreference = "Stop"
if ($ConfirmRestore -ne "YES") {
    throw "Refusing restore. Pass -ConfirmRestore YES after verifying dump and downtime window."
}
if (-not (Test-Path $DumpPath)) { throw "Dump not found: $DumpPath" }

$fileName = Split-Path $DumpPath -Leaf
Write-Host "WARNING: This REPLACES database RestBar from $DumpPath" -ForegroundColor Yellow

if ($LocalDocker) {
    docker cp $DumpPath "restbar_postgres:/tmp/$fileName"
    docker exec restbar_postgres pg_restore -U restbaruser -d RestBar --clean --if-exists "/tmp/$fileName"
} else {
    $plink = "C:\Program Files\PuTTY\plink.exe"
    $pscp = "C:\Program Files\PuTTY\pscp.exe"
    $password = $env:RESTBAR_SSH_PASSWORD
    if (-not $password) { throw "Set RESTBAR_SSH_PASSWORD" }
    $hostkey = $env:RESTBAR_SSH_HOSTKEY
    $hk = if ($hostkey) { @("-hostkey", $hostkey) } else { @() }
    & $pscp -pw $password -batch @hk $DumpPath "${RemoteHost}:/tmp/$fileName" | Out-Host
    & $plink -ssh -pw $password -batch @hk $RemoteHost "docker cp /tmp/$fileName restbar_postgres:/tmp/$fileName && docker exec restbar_postgres pg_restore -U restbaruser -d RestBar --clean --if-exists /tmp/$fileName" | Out-Host
}

Write-Host "Restore finished. Verify /health/ready and login." -ForegroundColor Green
