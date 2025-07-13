-- =====================================================
-- SCRIPT PARA LIMPIAR ÓRDENES Y DEJAR MESAS DISPONIBLES
-- RestBar System - Limpieza Completa
-- =====================================================

-- ⚠️ ADVERTENCIA: Este script eliminará TODAS las órdenes y sus items
-- ⚠️ Solo ejecutar en ambiente de desarrollo o cuando sea necesario

BEGIN TRANSACTION;

-- =====================================================
-- 1. BACKUP DE DATOS (OPCIONAL - DESCOMENTAR SI NECESITAS)
-- =====================================================
/*
-- Crear tabla de respaldo de órdenes
CREATE TABLE IF NOT EXISTS orders_backup AS 
SELECT * FROM "Orders" WHERE "Status" IN (0, 1, 2, 3, 4, 5); -- Estados activos

-- Crear tabla de respaldo de items
CREATE TABLE IF NOT EXISTS order_items_backup AS 
SELECT oi.* FROM "OrderItems" oi
INNER JOIN "Orders" o ON oi."OrderId" = o."Id"
WHERE o."Status" IN (0, 1, 2, 3, 4, 5); -- Estados activos

-- Crear tabla de respaldo de pagos
CREATE TABLE IF NOT EXISTS payments_backup AS 
SELECT p.* FROM "Payments" p
INNER JOIN "Orders" o ON p."OrderId" = o."Id"
WHERE o."Status" IN (0, 1, 2, 3, 4, 5); -- Estados activos
*/

-- =====================================================
-- 2. ELIMINAR PAGOS RELACIONADOS CON ÓRDENES ACTIVAS
-- =====================================================
PRINT '🗑️ Eliminando pagos de órdenes activas...';

DELETE FROM "Payments" 
WHERE "OrderId" IN (
    SELECT "Id" FROM "Orders" 
    WHERE "Status" IN (0, 1, 2, 3, 4, 5) -- Estados: Pending, SentToKitchen, Preparing, Ready, ReadyToPay, Served
);

-- =====================================================
-- 3. ELIMINAR ITEMS DE ÓRDENES ACTIVAS
-- =====================================================
PRINT '🗑️ Eliminando items de órdenes activas...';

DELETE FROM "OrderItems" 
WHERE "OrderId" IN (
    SELECT "Id" FROM "Orders" 
    WHERE "Status" IN (0, 1, 2, 3, 4, 5) -- Estados activos
);

-- =====================================================
-- 4. ELIMINAR ÓRDENES ACTIVAS
-- =====================================================
PRINT '🗑️ Eliminando órdenes activas...';

DELETE FROM "Orders" 
WHERE "Status" IN (0, 1, 2, 3, 4, 5); -- Estados activos

-- =====================================================
-- 5. ACTUALIZAR TODAS LAS MESAS A DISPONIBLE
-- =====================================================
PRINT '🔄 Actualizando todas las mesas a estado Disponible...';

UPDATE "Tables" 
SET 
    "Status" = 0, -- TableStatus.Disponible
    "UpdatedAt" = NOW(),
    "UpdatedBy" = 'SYSTEM_CLEANUP'
WHERE "IsActive" = true;

-- =====================================================
-- 6. VERIFICAR RESULTADOS
-- =====================================================
PRINT '📊 Verificando resultados...';

-- Contar órdenes restantes
SELECT 
    COUNT(*) as "Órdenes Restantes",
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ Todas las órdenes activas eliminadas'
        ELSE '⚠️ Aún quedan órdenes activas'
    END as "Estado"
FROM "Orders" 
WHERE "Status" IN (0, 1, 2, 3, 4, 5);

-- Contar items restantes
SELECT 
    COUNT(*) as "Items Restantes",
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ Todos los items eliminados'
        ELSE '⚠️ Aún quedan items'
    END as "Estado"
FROM "OrderItems" oi
INNER JOIN "Orders" o ON oi."OrderId" = o."Id"
WHERE o."Status" IN (0, 1, 2, 3, 4, 5);

-- Verificar estado de mesas
SELECT 
    "Status",
    COUNT(*) as "Cantidad",
    CASE 
        WHEN "Status" = 0 THEN '✅ Disponible'
        ELSE '⚠️ No disponible'
    END as "Estado"
FROM "Tables" 
WHERE "IsActive" = true
GROUP BY "Status"
ORDER BY "Status";

-- =====================================================
-- 7. MOSTRAR RESUMEN FINAL
-- =====================================================
PRINT '📋 RESUMEN DE LIMPIEZA:';
PRINT '✅ Pagos eliminados';
PRINT '✅ Items de órdenes eliminados';
PRINT '✅ Órdenes activas eliminadas';
PRINT '✅ Todas las mesas actualizadas a Disponible';

-- =====================================================
-- 8. CONFIRMAR TRANSACCIÓN
-- =====================================================
COMMIT TRANSACTION;

PRINT '🎉 LIMPIEZA COMPLETADA EXITOSAMENTE';
PRINT '🔄 Todas las mesas están ahora en estado Disponible';
PRINT '🗑️ Todas las órdenes activas han sido eliminadas';

-- =====================================================
-- SCRIPT ALTERNATIVO (MÁS CONSERVADOR)
-- =====================================================
/*
-- Si prefieres solo cancelar las órdenes en lugar de eliminarlas:

BEGIN TRANSACTION;

-- Cancelar todas las órdenes activas
UPDATE "Orders" 
SET 
    "Status" = 6, -- OrderStatus.Cancelled
    "ClosedAt" = NOW(),
    "UpdatedAt" = NOW(),
    "UpdatedBy" = 'SYSTEM_CLEANUP'
WHERE "Status" IN (0, 1, 2, 3, 4, 5);

-- Actualizar todas las mesas a disponible
UPDATE "Tables" 
SET 
    "Status" = 0, -- TableStatus.Disponible
    "UpdatedAt" = NOW(),
    "UpdatedBy" = 'SYSTEM_CLEANUP'
WHERE "IsActive" = true;

COMMIT TRANSACTION;
*/
