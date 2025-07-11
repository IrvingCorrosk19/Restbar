-- Script manual para cambiar el tipo de columna paid_at
-- Ejecutar este script directamente en la base de datos PostgreSQL

-- 1. Verificar el tipo actual de la columna
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'payments' 
AND column_name = 'paid_at';

-- 2. Cambiar el tipo de columna de timestamp without time zone a timestamp with time zone
ALTER TABLE payments 
ALTER COLUMN paid_at TYPE timestamp with time zone 
USING paid_at AT TIME ZONE 'UTC';

-- 3. Verificar que el cambio se aplicó correctamente
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'payments' 
AND column_name = 'paid_at';

-- 4. Verificar que no hay datos corruptos
SELECT COUNT(*) as total_payments FROM payments;
SELECT COUNT(*) as payments_with_paid_at FROM payments WHERE paid_at IS NOT NULL;

-- 5. Insertar el registro de migración manualmente
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250711010756_ForceFixPaymentDateTimeColumn', '9.0.5')
ON CONFLICT ("MigrationId") DO NOTHING;

-- 6. Verificar que la migración se registró
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC LIMIT 5; 