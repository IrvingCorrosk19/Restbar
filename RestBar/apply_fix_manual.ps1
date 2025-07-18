# Script para aplicar el fix manual de la columna Date en order_cancellation_logs
Write-Host "Aplicando fix manual para columna Date en order_cancellation_logs..." -ForegroundColor Green

# 1. Intentar aplicar las migraciones pendientes
Write-Host "Paso 1: Aplicando migraciones pendientes..." -ForegroundColor Yellow
try {
    dotnet ef database update --no-build
    Write-Host "Migraciones aplicadas exitosamente" -ForegroundColor Green
} catch {
    Write-Host "Error al aplicar migraciones, continuando con fix manual..." -ForegroundColor Yellow
}

# 2. Aplicar el SQL manual usando dotnet ef
Write-Host "Paso 2: Ejecutando SQL manual..." -ForegroundColor Yellow

# Crear un archivo temporal con el SQL
$sqlContent = @"
-- Script para arreglar la columna Date en order_cancellation_logs
-- y agregar las columnas del sistema de pagos

-- 1. Cambiar el tipo de columna date a timestamp with time zone
ALTER TABLE order_cancellation_logs 
ALTER COLUMN date TYPE timestamp with time zone;

-- 2. Agregar columnas del sistema de pagos si no existen
DO `$`$
BEGIN
    -- Agregar columna is_shared a payments si no existe
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'payments' AND column_name = 'is_shared') THEN
        ALTER TABLE payments ADD COLUMN is_shared boolean DEFAULT FALSE;
    END IF;
    
    -- Agregar columna payer_name a payments si no existe
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'payments' AND column_name = 'payer_name') THEN
        ALTER TABLE payments ADD COLUMN payer_name character varying(100);
    END IF;
    
    -- Cambiar columna Method a method en split_payments si existe
    IF EXISTS (SELECT 1 FROM information_schema.columns 
               WHERE table_name = 'split_payments' AND column_name = 'Method') THEN
        ALTER TABLE split_payments RENAME COLUMN "Method" TO method;
    END IF;
    
    -- Cambiar tipo de columna method en split_payments si es necesario
    IF EXISTS (SELECT 1 FROM information_schema.columns 
               WHERE table_name = 'split_payments' AND column_name = 'method' AND data_type = 'text') THEN
        ALTER TABLE split_payments ALTER COLUMN method TYPE character varying(30);
    END IF;
END `$`$;

-- Verificar los cambios
SELECT 
    table_name,
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name IN ('order_cancellation_logs', 'payments', 'split_payments')
    AND column_name IN ('date', 'is_shared', 'payer_name', 'method')
ORDER BY table_name, column_name;
"@

# Escribir el SQL a un archivo temporal
$tempSqlFile = "temp_fix.sql"
$sqlContent | Out-File -FilePath $tempSqlFile -Encoding UTF8

Write-Host "Archivo SQL creado: $tempSqlFile" -ForegroundColor Green

# Instruir al usuario cómo ejecutar el SQL
Write-Host "Para ejecutar el SQL manualmente:" -ForegroundColor Cyan
Write-Host "1. Abrir pgAdmin o tu cliente PostgreSQL preferido" -ForegroundColor White
Write-Host "2. Conectarse a la base de datos RestBar" -ForegroundColor White
Write-Host "3. Ejecutar el contenido del archivo: $tempSqlFile" -ForegroundColor White
Write-Host ""
Write-Host "O usar psql si está disponible:" -ForegroundColor Cyan
Write-Host "psql -h localhost -p 5432 -U postgres -d RestBar -f $tempSqlFile" -ForegroundColor White

Write-Host ""
Write-Host "¡Fix manual preparado!" -ForegroundColor Green 