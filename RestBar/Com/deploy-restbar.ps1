# Despliega RestBar en el VPS - NO afecta otras aplicaciones
# Puertos: RestBar=8084 | CarnetQR=80 | FixHub=8081 | PanamaTravelHub=8082 | n8n=8083

$plink = "C:\Program Files\PuTTY\plink.exe"
$hostname = "root@164.68.99.83"
$password = "DC26Y0U5ER6sWj"
$hostkey = "ssh-ed25519 SHA256:fXnxiWr5sqazM3xRId7HtcseAZ0XHcJ2BBIuPsLt2J0"
$remoteDir = "/opt/apps/restbar"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  DESPLIEGUE RESTBAR - VPS" -ForegroundColor Cyan
Write-Host "  Sin afectar CarnetQR, FixHub, PanamaTravelHub, n8n" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificacion pre-deploy
Write-Host "Estado actual del VPS:" -ForegroundColor Yellow
$cmdCheck = "docker ps --format 'table {{.Names}}\t{{.Ports}}'"
$resultCheck = & $plink -ssh -pw $password -batch -hostkey $hostkey $hostname $cmdCheck 2>&1
Write-Host $resultCheck
Write-Host ""
Write-Host "RestBar usara: puerto 8084 (restbar), contenedores restbar_*, red restbar_net" -ForegroundColor Cyan
Write-Host ""

# PASO 1: Directorio y repo (clonar/actualizar Restbar desde GitHub)
Write-Host "PASO 1: Clonando/actualizando RestBar en $remoteDir..." -ForegroundColor Yellow
$cmd1 = "mkdir -p $remoteDir && cd $remoteDir && if [ -d .git ]; then git fetch origin; git reset --hard origin/main 2>/dev/null || git reset --hard origin/master; else git clone https://github.com/IrvingCorrosk19/Restbar.git .; fi"
$result1 = & $plink -ssh -pw $password -batch -hostkey $hostkey $hostname $cmd1 2>&1
Write-Host $result1
# Si no hay docker-compose.yml en el repo, subir el del proyecto local
$cmd1b = "cd $remoteDir && test -f docker-compose.yml && echo 'si' || echo 'no'"
$tieneCompose = & $plink -ssh -pw $password -batch -hostkey $hostkey $hostname $cmd1b 2>&1 | Select-Object -Last 1
if ($tieneCompose -match 'no') {
    Write-Host "  docker-compose.yml no esta en el repo; subiendo desde proyecto local..." -ForegroundColor Yellow
    $composePath = Join-Path $PSScriptRoot "..\docker-compose.yml"
    if (-not (Test-Path $composePath)) {
        Write-Host "  ERROR: No se encuentra $composePath" -ForegroundColor Red
        exit 1
    }
    $composeContent = Get-Content $composePath -Raw -Encoding UTF8
    $composeB64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($composeContent))
    $cmdUpload = "cd $remoteDir && echo $composeB64 | base64 -d > docker-compose.yml && echo 'docker-compose.yml OK'"
    $r = & $plink -ssh -pw $password -batch -hostkey $hostkey $hostname $cmdUpload 2>&1
    Write-Host $r
}
$remoteComposeDir = $remoteDir
Write-Host "  Directorio compose: $remoteComposeDir" -ForegroundColor Gray
Write-Host ""

# PASO 2: Crear .env (en la carpeta donde esta docker-compose para que lo vea)
Write-Host "PASO 2: Creando .env..." -ForegroundColor Yellow
$envContent = @"
POSTGRES_DB=RestBar
POSTGRES_USER=restbaruser
POSTGRES_PASSWORD=RestBar2024!Secure

ASPNETCORE_ENVIRONMENT=Production
"@
$envB64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($envContent))
$cmd2 = "cd $remoteComposeDir && echo '$envB64' | base64 -d > .env && echo '.env creado'"
$result2 = & $plink -ssh -pw $password -batch -hostkey $hostkey $hostname $cmd2 2>&1
Write-Host $result2
Write-Host ""

# PASO 3: Build y deploy (primera vez puede tardar 5-10 min)
Write-Host "PASO 3: Construyendo y levantando contenedores (5-10 min primera vez)..." -ForegroundColor Yellow
Write-Host "  Si hay timeout, ejecuta en el VPS: cd $remoteComposeDir && docker compose up -d --build" -ForegroundColor Gray
$cmd3 = "cd $remoteComposeDir && docker compose up -d --build 2>&1"
$result3 = & $plink -ssh -pw $password -batch -hostkey $hostkey $hostname $cmd3 2>&1
Write-Host $result3
Write-Host ""

# PASO 4: Verificar
Write-Host "PASO 4: Verificando RestBar..." -ForegroundColor Yellow
Start-Sleep -Seconds 5
$cmd4 = "docker ps --filter name=restbar --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'"
$result4 = & $plink -ssh -pw $password -batch -hostkey $hostkey $hostname $cmd4 2>&1
Write-Host $result4
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "  DESPLIEGUE COMPLETADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "RestBar:        http://164.68.99.83:8084  |  https://restbar.autonomousflow.lat (cuando nginx+SSL)" -ForegroundColor Green
Write-Host "Demas apps: carnet 80, fixhub 8081, travel 8082, n8n 8083 (no modificadas)" -ForegroundColor Gray
Write-Host ""
Write-Host "Comandos utiles:" -ForegroundColor Yellow
Write-Host "  docker logs -f restbar_web" -ForegroundColor White
Write-Host "  cd $remoteDir && docker compose down" -ForegroundColor White
Write-Host ""
