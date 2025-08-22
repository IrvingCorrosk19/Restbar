-- Script para eliminar las tablas de configuración avanzada que no existen
-- Ejecutar este script en la base de datos PostgreSQL

-- Eliminar las tablas de configuración avanzada si existen
DROP TABLE IF EXISTS public."Printers" CASCADE;
DROP TABLE IF EXISTS public."SystemSettings" CASCADE;
DROP TABLE IF EXISTS public."Currencies" CASCADE;
DROP TABLE IF EXISTS public."TaxRates" CASCADE;
DROP TABLE IF EXISTS public."DiscountPolicies" CASCADE;
DROP TABLE IF EXISTS public."OperatingHours" CASCADE;
DROP TABLE IF EXISTS public."NotificationSettings" CASCADE;
DROP TABLE IF EXISTS public."BackupSettings" CASCADE;

-- Verificar que las tablas fueron eliminadas
SELECT 'Configuración avanzada eliminada exitosamente' as resultado;
