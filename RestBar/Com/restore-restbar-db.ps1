# Restaura la base de datos RestBar en el VPS desde restbarIIC.sql
# Uso: .\restore-restbar-db.ps1
# Requiere: PuTTY (pscp + plink), contenedor restbar_postgres corriendo

$ErrorActionPreference = "Stop"
$pscp = "C:\Program Files\PuTTY\pscp.exe"
$plink = "C:\Program Files\PuTTY\plink.exe"
$hostname = "root@164.68.99.83"
$password = "DC26Y0U5ER6sWj"
$hostkey = "ssh-ed25519 SHA256:fXnxiWr5sqazM3xRId7HtcseAZ0XHcJ2BBIuPsLt2J0"
$remoteDir = "/opt/apps/restbar"

# Archivo SQL: en la raíz del repo (RestBar/restbarIIC.sql)
$sqlLocal = Join-Path $PSScriptRoot "..\restbarIIC.sql"
if (-not (Test-Path $sqlLocal)) {
    Write-Host "ERROR: No se encuentra $sqlLocal" -ForegroundColor Red
    Write-Host "Coloca restbarIIC.sql en la raíz del repo RestBar o indica otra ruta." -ForegroundColor Yellow
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RESTAURAR DB RESTBAR EN VPS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Archivo local: $sqlLocal" -ForegroundColor Gray
Write-Host "Tamano: $([math]::Round((Get-Item $sqlLocal).Length / 1MB, 2)) MB" -ForegroundColor Gray
Write-Host ""

# 1) Subir el SQL al VPS
Write-Host "PASO 1: Subiendo restbarIIC.sql al VPS..." -ForegroundColor Yellow
$remoteSql = "/tmp/restbarIIC.sql"
& $pscp -pw $password -hostkey $hostkey $sqlLocal "${hostname}:${remoteSql}" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al subir el archivo. ¿Tienes pscp (PuTTY) instalado?" -ForegroundColor Red
    exit 1
}
Write-Host "  OK: archivo en $remoteSql" -ForegroundColor Green
Write-Host ""

# 2) Restauración limpia: borrar DB, recrear, copiar dump (sin SET PG17), restaurar
Write-Host "PASO 2: Restauración limpia (drop + create + restore)..." -ForegroundColor Yellow
# En la cadena para el remoto, `" permite que las comillas dobles lleguen a psql (nombre case-sensitive)
$cmdRestore = "docker cp $remoteSql restbar_postgres:/tmp/restore.sql && docker exec restbar_postgres sed -i '/transaction_timeout/d' /tmp/restore.sql && docker exec restbar_postgres psql -U restbaruser -d postgres -c 'DROP DATABASE IF EXISTS `"RestBar`";' && docker exec restbar_postgres psql -U restbaruser -d postgres -c 'CREATE DATABASE `"RestBar`";' && docker exec restbar_postgres psql -U restbaruser -d RestBar -f /tmp/restore.sql && docker exec restbar_postgres rm -f /tmp/restore.sql && rm -f $remoteSql && echo RESTAURE OK"
$result = & $plink -ssh -pw $password -batch -hostkey $hostkey $hostname $cmdRestore 2>&1
Write-Host $result
if ($result -match "RESTAURE OK") {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  BASE DE DATOS RESTAURADA" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Reinicia la app si hace falta: cd $remoteDir && docker compose restart restbar_web" -ForegroundColor Gray
} else {
    Write-Host ""
    Write-Host "Revisa el mensaje anterior por errores de psql." -ForegroundColor Yellow
    exit 1
}
