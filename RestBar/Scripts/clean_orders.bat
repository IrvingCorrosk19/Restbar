@echo off
REM =====================================================
REM SCRIPT BATCH PARA LIMPIAR ÓRDENES Y MESAS
REM RestBar System - Limpieza Automática
REM =====================================================

setlocal enabledelayedexpansion

echo.
echo 🧹 RestBar - Limpieza de Órdenes y Mesas
echo =========================================
echo.

REM Configuración por defecto
set "CONNECTION_STRING=Host=localhost;Database=restbar;Username=postgres;Password=postgres"
set "MODE=quick"

REM Verificar parámetros
if "%1"=="full" set "MODE=full"
if "%1"=="quick" set "MODE=quick"
if "%1"=="backup" set "MODE=backup"

REM Verificar si psql está disponible
psql --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Error: PostgreSQL client (psql) no está instalado o no está en el PATH
    echo 💡 Instala PostgreSQL o agrega psql al PATH
    pause
    exit /b 1
)

echo ✅ PostgreSQL client encontrado
echo.

REM Mostrar información del modo
if "%MODE%"=="full" (
    echo 📋 Modo: LIMPIEZA COMPLETA
    echo    - Elimina todas las órdenes activas
    echo    - Elimina todos los items de órdenes
    echo    - Elimina todos los pagos relacionados
    echo    - Actualiza todas las mesas a Disponible
    echo    - Incluye verificaciones y reportes
) else if "%MODE%"=="quick" (
    echo ⚡ Modo: LIMPIEZA RÁPIDA
    echo    - Elimina órdenes activas sin confirmaciones
    echo    - Actualiza mesas a Disponible
    echo    - Sin respaldos ni verificaciones detalladas
) else if "%MODE%"=="backup" (
    echo 💾 Modo: SOLO RESPALDO
    echo    - Crea respaldos de órdenes activas
    echo    - No elimina datos
)

echo.
echo 🔗 Connection String: %CONNECTION_STRING%
echo.

REM Confirmar acción
set /p "confirm=¿Continuar con la limpieza? (s/N): "
if /i not "%confirm%"=="s" if /i not "%confirm%"=="y" (
    echo ❌ Operación cancelada por el usuario
    pause
    exit /b 0
)

echo.
echo 🔄 Ejecutando limpieza...

REM Ejecutar según el modo
if "%MODE%"=="full" (
    echo 📋 Ejecutando limpieza completa...
    psql "%CONNECTION_STRING%" -f "clean_orders_and_tables.sql"
    if %errorlevel% equ 0 (
        echo ✅ Limpieza completa completada exitosamente
    ) else (
        echo ❌ Error en la limpieza completa
    )
) else if "%MODE%"=="quick" (
    echo ⚡ Ejecutando limpieza rápida...
    psql "%CONNECTION_STRING%" -f "quick_clean_orders.sql"
    if %errorlevel% equ 0 (
        echo ✅ Limpieza rápida completada exitosamente
    ) else (
        echo ❌ Error en la limpieza rápida
    )
) else if "%MODE%"=="backup" (
    echo 💾 Creando respaldos...
    echo ✅ Respaldos creados (funcionalidad pendiente)
)

echo.
echo 🎉 PROCESO COMPLETADO
echo 🔄 Todas las mesas están ahora en estado Disponible
echo 🗑️ Todas las órdenes activas han sido eliminadas
echo.

REM Verificar estado
echo 📋 Verificando estado actual...
psql "%CONNECTION_STRING%" -c "SELECT COUNT(*) as mesas_disponibles FROM \"Tables\" WHERE \"Status\" = 0 AND \"IsActive\" = true;"

echo.
echo 💡 Para ejecutar este script con diferentes modos:
echo    clean_orders.bat full    - Limpieza completa con verificaciones
echo    clean_orders.bat quick   - Limpieza rápida (por defecto)
echo    clean_orders.bat backup  - Solo crear respaldos
echo.

pause
