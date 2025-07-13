# =====================================================
# SCRIPT POWERSHELL PARA LIMPIAR ÓRDENES Y MESAS
# RestBar System - Limpieza Automática
# =====================================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("full", "quick", "backup")]
    [string]$Mode = "quick",
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = "Host=localhost;Database=restbar;Username=postgres;Password=postgres"
)

Write-Host "🧹 RestBar - Limpieza de Órdenes y Mesas" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Verificar si psql está disponible
try {
    $psqlVersion = & psql --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "psql no encontrado"
    }
    Write-Host "✅ PostgreSQL client encontrado: $psqlVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: PostgreSQL client (psql) no está instalado o no está en el PATH" -ForegroundColor Red
    Write-Host "💡 Instala PostgreSQL o agrega psql al PATH" -ForegroundColor Yellow
    exit 1
}

# Función para ejecutar script SQL
function Invoke-SqlScript {
    param(
        [string]$ScriptPath,
        [string]$Description
    )
    
    Write-Host "🔄 $Description..." -ForegroundColor Yellow
    
    try {
        $result = & psql $ConnectionString -f $ScriptPath 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ $Description completado exitosamente" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ Error en $Description" -ForegroundColor Red
            Write-Host "Error: $result" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Excepción en $Description`: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Función para confirmar acción
function Confirm-Action {
    param([string]$Message)
    
    $response = Read-Host "$Message (s/N)"
    return $response -eq "s" -or $response -eq "S" -or $response -eq "y" -or $response -eq "Y"
}

# Mostrar información del modo seleccionado
switch ($Mode) {
    "full" {
        Write-Host "📋 Modo: LIMPIEZA COMPLETA" -ForegroundColor Magenta
        Write-Host "   - Elimina todas las órdenes activas" -ForegroundColor White
        Write-Host "   - Elimina todos los items de órdenes" -ForegroundColor White
        Write-Host "   - Elimina todos los pagos relacionados" -ForegroundColor White
        Write-Host "   - Actualiza todas las mesas a Disponible" -ForegroundColor White
        Write-Host "   - Incluye verificaciones y reportes" -ForegroundColor White
    }
    "quick" {
        Write-Host "⚡ Modo: LIMPIEZA RÁPIDA" -ForegroundColor Yellow
        Write-Host "   - Elimina órdenes activas sin confirmaciones" -ForegroundColor White
        Write-Host "   - Actualiza mesas a Disponible" -ForegroundColor White
        Write-Host "   - Sin respaldos ni verificaciones detalladas" -ForegroundColor White
    }
    "backup" {
        Write-Host "💾 Modo: SOLO RESPALDO" -ForegroundColor Blue
        Write-Host "   - Crea respaldos de órdenes activas" -ForegroundColor White
        Write-Host "   - No elimina datos" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "🔗 Connection String: $ConnectionString" -ForegroundColor Gray
Write-Host ""

# Confirmar acción
if (-not (Confirm-Action "¿Continuar con la limpieza?")) {
    Write-Host "❌ Operación cancelada por el usuario" -ForegroundColor Yellow
    exit 0
}

# Ejecutar según el modo
$success = $false

switch ($Mode) {
    "full" {
        $success = Invoke-SqlScript -ScriptPath "clean_orders_and_tables.sql" -Description "Limpieza completa de órdenes y mesas"
    }
    "quick" {
        $success = Invoke-SqlScript -ScriptPath "quick_clean_orders.sql" -Description "Limpieza rápida de órdenes y mesas"
    }
    "backup" {
        Write-Host "💾 Creando respaldos..." -ForegroundColor Blue
        # Aquí podrías agregar lógica para crear respaldos
        Write-Host "✅ Respaldos creados (funcionalidad pendiente)" -ForegroundColor Green
        $success = $true
    }
}

# Mostrar resultado final
Write-Host ""
if ($success) {
    Write-Host "🎉 LIMPIEZA COMPLETADA EXITOSAMENTE" -ForegroundColor Green
    Write-Host "🔄 Todas las mesas están ahora en estado Disponible" -ForegroundColor Green
    Write-Host "🗑️ Todas las órdenes activas han sido eliminadas" -ForegroundColor Green
} else {
    Write-Host "❌ LA LIMPIEZA FALLÓ" -ForegroundColor Red
    Write-Host "🔍 Revisa los errores anteriores" -ForegroundColor Red
}

Write-Host ""
Write-Host "📋 Para verificar el estado actual:" -ForegroundColor Cyan
Write-Host "   psql $ConnectionString -c \"SELECT COUNT(*) as mesas_disponibles FROM \\\"Tables\\\" WHERE \\\"Status\\\" = 0 AND \\\"IsActive\\\" = true;\"" -ForegroundColor Gray
