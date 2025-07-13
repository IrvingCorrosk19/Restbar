@echo off
REM =====================================================
REM SCRIPT SIMPLE: DELETE + UPDATE
REM RestBar System - Solo eliminar y actualizar
REM =====================================================

echo.
echo 🧹 RestBar - Limpieza Simple (DELETE + UPDATE)
echo ==============================================
echo.

REM Configuración por defecto
set "CONNECTION_STRING=Host=localhost;Database=restbar;Username=postgres;Password=postgres"

REM Verificar si psql está disponible
psql --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Error: PostgreSQL client (psql) no está instalado
    pause
    exit /b 1
)

echo ✅ PostgreSQL client encontrado
echo.
echo 🗑️ Este script hará:
echo    - DELETE de Payments de órdenes activas
echo    - DELETE de OrderItems de órdenes activas  
echo    - DELETE de Orders activas
echo    - UPDATE de Tables a estado Disponible
echo.

REM Confirmar acción
set /p "confirm=¿Continuar? (s/N): "
if /i not "%confirm%"=="s" if /i not "%confirm%"=="y" (
    echo ❌ Operación cancelada
    pause
    exit /b 0
)

echo.
echo 🔄 Ejecutando limpieza simple...

REM Ejecutar script
psql "%CONNECTION_STRING%" -f "simple_clean.sql"

if %errorlevel% equ 0 (
    echo.
    echo ✅ LIMPIEZA COMPLETADA
    echo 🗑️ Órdenes activas eliminadas
    echo 🔄 Mesas actualizadas a Disponible
) else (
    echo.
    echo ❌ Error en la limpieza
)

echo.
pause
