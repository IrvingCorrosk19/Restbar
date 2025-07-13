-- Script para actualizar los valores de TableStatus de inglés a español
-- Ejecutar este script para corregir los valores existentes en la base de datos

-- Actualizar valores de estado de tablas de inglés a español
UPDATE tables 
SET status = CASE 
    WHEN status = 'AVAILABLE' THEN 'Disponible'
    WHEN status = 'OCCUPIED' THEN 'Ocupada'
    WHEN status = 'RESERVED' THEN 'Reservada'
    WHEN status = 'WAITING' THEN 'EnEspera'
    WHEN status = 'ATTENDED' THEN 'Atendida'
    WHEN status = 'PREPARING' THEN 'EnPreparacion'
    WHEN status = 'SERVED' THEN 'Servida'
    WHEN status = 'READY_FOR_PAYMENT' THEN 'ParaPago'
    WHEN status = 'PAID' THEN 'Pagada'
    WHEN status = 'BLOCKED' THEN 'Bloqueada'
    ELSE status -- Mantener valores que ya estén en español
END;

-- Verificar los valores actualizados
SELECT DISTINCT status, COUNT(*) as count 
FROM tables 
GROUP BY status 
ORDER BY status;
