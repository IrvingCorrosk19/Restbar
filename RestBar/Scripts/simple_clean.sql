-- =====================================================
-- SCRIPT SIMPLE: DELETE + UPDATE
-- RestBar System - Solo eliminar y actualizar
-- =====================================================

-- ⚠️ ADVERTENCIA: Este script eliminará TODAS las órdenes activas
-- ⚠️ Solo ejecutar en ambiente de desarrollo

-- =====================================================
-- 1. DELETE - Eliminar pagos de órdenes activas
-- =====================================================
DELETE FROM "Payments" 
WHERE "OrderId" IN (
    SELECT "Id" FROM "Orders" 
    WHERE "Status" IN (0, 1, 2, 3, 4, 5)
);

-- =====================================================
-- 2. DELETE - Eliminar items de órdenes activas
-- =====================================================
DELETE FROM "OrderItems" 
WHERE "OrderId" IN (
    SELECT "Id" FROM "Orders" 
    WHERE "Status" IN (0, 1, 2, 3, 4, 5)
);

-- =====================================================
-- 3. DELETE - Eliminar órdenes activas
-- =====================================================
DELETE FROM "Orders" 
WHERE "Status" IN (0, 1, 2, 3, 4, 5);

-- =====================================================
-- 4. UPDATE - Todas las mesas a Disponible
-- =====================================================
UPDATE "Tables" 
SET "Status" = 0, "UpdatedAt" = NOW()
WHERE "IsActive" = true;

-- =====================================================
-- 5. VERIFICACIÓN FINAL
-- =====================================================
SELECT 'Limpieza completada' as "Resultado";
SELECT COUNT(*) as "Mesas Disponibles" FROM "Tables" WHERE "Status" = 0 AND "IsActive" = true;
