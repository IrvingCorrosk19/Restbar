-- Script para marcar el último item pendiente como listo
-- Ejecutar este script para resolver el problema del item que no se marca como listo

-- 1. Marcar el último item pendiente como listo
UPDATE order_items 
SET 
    status = 'Ready',
    prepared_by_station_id = '1e3e5c7f-84b9-4c4b-8f9a-1a6e52c61ec1', -- ID de la estación Cocina
    prepared_at = NOW()
WHERE 
    order_id = '5e7837d2-b427-4424-b7d9-1c99bfeb8393' 
    AND status = 'Pending'
    AND id = 'c739843c-f3ac-480b-b163-9d02bae57ea5';

-- 2. Verificar que todos los items de la orden estén listos
SELECT 
    order_id,
    COUNT(*) as total_items,
    COUNT(CASE WHEN status = 'Ready' THEN 1 END) as ready_items,
    COUNT(CASE WHEN status = 'Pending' THEN 1 END) as pending_items
FROM order_items 
WHERE order_id = '5e7837d2-b427-4424-b7d9-1c99bfeb8393'
GROUP BY order_id;

-- 3. Si todos los items están listos, actualizar el estado de la orden
UPDATE orders 
SET 
    status = 'Ready',
    closed_at = NOW()
WHERE 
    id = '5e7837d2-b427-4424-b7d9-1c99bfeb8393'
    AND (
        SELECT COUNT(*) 
        FROM order_items 
        WHERE order_id = '5e7837d2-b427-4424-b7d9-1c99bfeb8393' 
        AND status = 'Pending'
    ) = 0;

-- 4. Verificar el resultado final
SELECT 
    o.id as order_id,
    o.status as order_status,
    oi.id as item_id,
    oi.status as item_status,
    oi.prepared_at,
    oi.prepared_by_station_id
FROM orders o
JOIN order_items oi ON o.id = oi.order_id
WHERE o.id = '5e7837d2-b427-4424-b7d9-1c99bfeb8393'
ORDER BY oi.prepared_at; 