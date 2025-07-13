-- =====================================================
-- SCRIPT RÁPIDO PARA LIMPIAR ÓRDENES Y MESAS
-- RestBar System - Limpieza Rápida
-- =====================================================

-- ⚠️ ADVERTENCIA: Este script eliminará TODAS las órdenes activas
-- ⚠️ Solo ejecutar en ambiente de desarrollo

-- =====================================================
-- LIMPIEZA RÁPIDA (SIN TRANSACCIÓN)
-- =====================================================

-- 1. Eliminar pagos
DELETE FROM "Payments" WHERE "OrderId" IN (
    SELECT "Id" FROM "Orders" WHERE "Status" IN (0, 1, 2, 3, 4, 5)
);

-- 2. Eliminar items
DELETE FROM "OrderItems" WHERE "OrderId" IN (
    SELECT "Id" FROM "Orders" WHERE "Status" IN (0, 1, 2, 3, 4, 5)
);

-- 3. Eliminar órdenes
DELETE FROM "Orders" WHERE "Status" IN (0, 1, 2, 3, 4, 5);

-- 4. Actualizar mesas a disponible
UPDATE "Tables" 
SET "Status" = 0, "UpdatedAt" = NOW(), "UpdatedBy" = 'CLEANUP'
WHERE "IsActive" = true;

-- 5. Verificar
SELECT 'Limpieza completada' as "Resultado";
SELECT COUNT(*) as "Mesas Disponibles" FROM "Tables" WHERE "Status" = 0 AND "IsActive" = true;
