-- Script para eliminar las tablas no utilizadas que están en la BD pero no referenciadas en el código
-- Ejecutar este script en la base de datos PostgreSQL

-- Eliminar las tablas de contabilidad que no se usan
DROP TABLE IF EXISTS public."journal_entry_details" CASCADE;
DROP TABLE IF EXISTS public."journal_entries" CASCADE;
DROP TABLE IF EXISTS public."accounts" CASCADE;

-- Eliminar las tablas de inventario que no se usan
DROP TABLE IF EXISTS public."inventory_movements" CASCADE;
DROP TABLE IF EXISTS public."inventory" CASCADE;

-- Eliminar las tablas de compras que no se usan
DROP TABLE IF EXISTS public."purchase_order_receipt_items" CASCADE;
DROP TABLE IF EXISTS public."purchase_order_receipts" CASCADE;
DROP TABLE IF EXISTS public."purchase_order_items" CASCADE;
DROP TABLE IF EXISTS public."purchase_orders" CASCADE;

-- Eliminar las tablas de proveedores que no se usan
DROP TABLE IF EXISTS public."suppliers" CASCADE;

-- Eliminar las tablas de configuración de empresa que no se usan
DROP TABLE IF EXISTS public."company_subscriptions" CASCADE;
DROP TABLE IF EXISTS public."company_settings" CASCADE;

-- Eliminar enums que no se usan
DROP TYPE IF EXISTS public.purchase_order_status_enum CASCADE;
DROP TYPE IF EXISTS public.movement_type_enum CASCADE;

-- Verificar que las tablas fueron eliminadas
SELECT 'Tablas no utilizadas eliminadas exitosamente' as resultado;
