-- Script ALTER para agregar company_id y branch_id a la tabla stations existente
-- Ejecutar este script en PostgreSQL

-- 1. Agregar la columna company_id
ALTER TABLE public.stations 
ADD COLUMN company_id uuid;

-- 2. Agregar la columna branch_id  
ALTER TABLE public.stations 
ADD COLUMN branch_id uuid;

-- 3. Crear índices para mejorar el rendimiento
CREATE INDEX IF NOT EXISTS "IX_stations_company_id" 
ON public.stations USING btree (company_id ASC NULLS LAST)
TABLESPACE pg_default;

CREATE INDEX IF NOT EXISTS "IX_stations_branch_id" 
ON public.stations USING btree (branch_id ASC NULLS LAST)
TABLESPACE pg_default;

-- 4. Agregar las restricciones de clave foránea
ALTER TABLE public.stations 
ADD CONSTRAINT "FK_stations_companies_company_id" 
FOREIGN KEY (company_id) 
REFERENCES public.companies (id) 
MATCH SIMPLE 
ON UPDATE NO ACTION 
ON DELETE SET NULL;

ALTER TABLE public.stations 
ADD CONSTRAINT "FK_stations_branches_branch_id" 
FOREIGN KEY (branch_id) 
REFERENCES public.branches (id) 
MATCH SIMPLE 
ON UPDATE NO ACTION 
ON DELETE SET NULL;

-- 5. Comentarios para documentar los cambios
COMMENT ON COLUMN public.stations.company_id IS 'Referencia a la compañía a la que pertenece la estación';
COMMENT ON COLUMN public.stations.branch_id IS 'Referencia a la sucursal a la que pertenece la estación';

-- 6. Verificar que las columnas se agregaron correctamente
SELECT 
    column_name, 
    data_type, 
    is_nullable, 
    column_default
FROM information_schema.columns 
WHERE table_name = 'stations' 
AND column_name IN ('company_id', 'branch_id')
ORDER BY column_name;

-- 7. Mostrar la estructura final de la tabla
\d public.stations;
