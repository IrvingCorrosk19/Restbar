-- Script para verificar que la columna paid_at tenga el tipo correcto
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'payments' 
AND column_name = 'paid_at';

-- Verificar que la tabla payments existe y tiene la estructura correcta
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'payments'
ORDER BY ordinal_position; 