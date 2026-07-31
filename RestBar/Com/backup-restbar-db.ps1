# RestBar PostgreSQL backup (pg_dump custom format)
# Does NOT modify other VPS apps. Requires PuTTY plink/pscp OR local docker.
param(
    [string]$OutputDir = "",
    [string]$RemoteHost = "root@164.68.99.83",
    [switch]$LocalDocker
)

$ErrorActionPreference = "Stop"
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
if (-not $OutputDir) {
    $OutputDir = Join-Path $PSScriptRoot "..\..\backups"
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$fileName = "RestBar_backup_$stamp.dump"
$outPath = Join-Path $OutputDir $fileName

Write-Host "RestBar backup -> $outPath" -ForegroundColor Cyan

if ($LocalDocker) {
    docker exec restbar_postgres pg_dump -U restbaruser -d RestBar -Fc -f /tmp/$fileName
    docker cp "restbar_postgres:/tmp/$fileName" $outPath
    docker exec restbar_postgres rm -f /tmp/$fileName
} else {
    $plink = "C:\Program Files\PuTTY\plink.exe"
    $pscp = "C:\Program Files\PuTTY\pscp.exe"
    if (-not (Test-Path $plink)) { throw "plink.exe not found. Use -LocalDocker or install PuTTY." }
    # Prefer env RESTBAR_SSH_PASSWORD; do not hardcode secrets in repo scripts going forward.
    $password = $env:RESTBAR_SSH_PASSWORD
    if (-not $password) { throw "Set RESTBAR_SSH_PASSWORD for remote backup." }
    $hostkey = $env:RESTBAR_SSH_HOSTKEY
    $hk = if ($hostkey) { @("-hostkey", $hostkey) } else { @() }
    & $plink -ssh -pw $password -batch @hk $RemoteHost "docker exec restbar_postgres pg_dump -U restbaruser -d RestBar -Fc -f /tmp/$fileName" | Out-Host
    & $pscp -pw $password -batch @hk "${RemoteHost}:/tmp/$fileName" $outPath | Out-Host
    & $plink -ssh -pw $password -batch @hk $RemoteHost "rm -f /tmp/$fileName; docker exec restbar_postgres rm -f /tmp/$fileName" | Out-Null
}

if (-not (Test-Path $outPath)) { throw "Backup file missing: $outPath" }
$size = (Get-Item $outPath).Length
Write-Host "OK backup $fileName ($([math]::Round($size/1MB,2)) MB)" -ForegroundColor Green
Write-Host $outPath
