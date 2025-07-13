-- Script para agregar CompanyId y BranchId a la tabla tables
-- Ejecutar este script para actualizar la estructura de la tabla

-- Agregar columnas CompanyId y BranchId
ALTER TABLE public.tables 
ADD COLUMN "CompanyId" uuid,
ADD COLUMN "BranchId" uuid;

-- Crear índices para las nuevas columnas
CREATE INDEX IF NOT EXISTS "IX_tables_CompanyId" 
ON public.tables USING btree ("CompanyId" ASC NULLS LAST);

CREATE INDEX IF NOT EXISTS "IX_tables_BranchId" 
ON public.tables USING btree ("BranchId" ASC NULLS LAST);

-- Agregar foreign key constraints
ALTER TABLE public.tables 
ADD CONSTRAINT "FK_tables_Companies_CompanyId" 
FOREIGN KEY ("CompanyId") REFERENCES public.companies (id) 
ON DELETE CASCADE;

ALTER TABLE public.tables 
ADD CONSTRAINT "FK_tables_Branches_BranchId" 
FOREIGN KEY ("BranchId") REFERENCES public.branches (id) 
ON DELETE CASCADE;

-- Comentario para documentar los cambios
COMMENT ON COLUMN public.tables."CompanyId" IS 'ID de la compañía a la que pertenece la mesa';
COMMENT ON COLUMN public.tables."BranchId" IS 'ID de la sucursal a la que pertenece la mesa';

-- Verificar que las columnas se agregaron correctamente
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'tables' 
AND column_name IN ('CompanyId', 'BranchId');
