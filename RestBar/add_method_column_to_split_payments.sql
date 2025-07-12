-- Agregar columna Method a la tabla split_payments
-- Esta columna permitirá que cada persona en un split payment tenga su propio método de pago

ALTER TABLE split_payments 
ADD COLUMN method VARCHAR(30);

-- Comentario explicativo sobre la columna
COMMENT ON COLUMN split_payments.method IS 'Método de pago específico para cada persona en el split payment (efectivo, tarjeta, etc.)';

-- Verificar la estructura de la tabla después del cambio
SELECT column_name, data_type, character_maximum_length, is_nullable
FROM information_schema.columns 
WHERE table_name = 'split_payments' 
ORDER BY ordinal_position; 