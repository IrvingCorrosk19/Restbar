-- Agregar columnas is_shared y payer_name a la tabla payments
-- is_shared indica si el pago es compartido entre varias personas
-- payer_name guarda el nombre del pagador cuando el pago no es compartido

ALTER TABLE payments 
ADD COLUMN is_shared BOOLEAN DEFAULT FALSE;

ALTER TABLE payments 
ADD COLUMN payer_name VARCHAR(100);

-- Comentarios explicativos sobre las columnas
COMMENT ON COLUMN payments.is_shared IS 'Indica si el pago es compartido entre varias personas (true) o individual (false)';
COMMENT ON COLUMN payments.payer_name IS 'Nombre del pagador cuando el pago no es compartido (opcional)';

-- Verificar la estructura de la tabla después del cambio
SELECT column_name, data_type, character_maximum_length, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'payments' 
ORDER BY ordinal_position; 