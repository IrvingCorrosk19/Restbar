-- Script para corregir definitivamente los valores de TableStatus
-- Ejecutar este script en PostgreSQL para sincronizar con el enum C#

-- 1. Actualizar valores existentes de inglés a español
UPDATE public.tables SET status = 'Disponible'    WHERE UPPER(status) = 'AVAILABLE';
UPDATE public.tables SET status = 'Ocupada'       WHERE UPPER(status) = 'OCCUPIED';
UPDATE public.tables SET status = 'Reservada'     WHERE UPPER(status) = 'RESERVED';
UPDATE public.tables SET status = 'EnEspera'      WHERE UPPER(status) = 'WAITING';
UPDATE public.tables SET status = 'Atendida'      WHERE UPPER(status) = 'ATTENDED';
UPDATE public.tables SET status = 'EnPreparacion' WHERE UPPER(status) = 'PREPARING';
UPDATE public.tables SET status = 'Servida'       WHERE UPPER(status) = 'SERVED';
UPDATE public.tables SET status = 'ParaPago'      WHERE UPPER(status) = 'READY_FOR_PAYMENT';
UPDATE public.tables SET status = 'Pagada'        WHERE UPPER(status) = 'PAID';
UPDATE public.tables SET status = 'Bloqueada'     WHERE UPPER(status) = 'BLOCKED';

-- 2. Verificar los valores actualizados
SELECT DISTINCT status, COUNT(*) as count 
FROM public.tables 
GROUP BY status 
ORDER BY status;

-- 3. Agregar constraint de validación para prevenir valores inválidos
ALTER TABLE public.tables
  ADD CONSTRAINT ck_tables_status_allowed
  CHECK (status IN (
    'Disponible',
    'Ocupada',
    'Reservada',
    'EnEspera',
    'Atendida',
    'EnPreparacion',
    'Servida',
    'ParaPago',
    'Pagada',
    'Bloqueada'
  ));

-- 4. Verificar que el constraint se aplicó correctamente
SELECT conname, consrc 
FROM pg_constraint 
WHERE conname = 'ck_tables_status_allowed';
