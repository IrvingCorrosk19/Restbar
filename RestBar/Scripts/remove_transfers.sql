-- Script para eliminar el módulo de transferencias completamente
-- Ejecutar este script en la base de datos PostgreSQL

-- Eliminar las tablas de transferencias
DROP TABLE IF EXISTS public."TransferItems" CASCADE;
DROP TABLE IF EXISTS public."Transfers" CASCADE;

-- Eliminar el enum de transfer_status si existe
DROP TYPE IF EXISTS public.transfer_status_enum CASCADE;

-- Verificar que las tablas fueron eliminadas
SELECT 'Transferencias eliminadas exitosamente' as resultado;
