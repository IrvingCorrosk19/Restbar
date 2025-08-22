-- Script para agregar branch_id a la tabla categories
-- Ejecutar en PostgreSQL

-- 1. Agregar la columna branch_id
ALTER TABLE public.categories 
ADD COLUMN "BranchId" uuid;

-- 2. Crear índice para branch_id
CREATE INDEX IF NOT EXISTS "IX_categories_BranchId"
    ON public.categories USING btree
    ("BranchId" ASC NULLS LAST)
    TABLESPACE pg_default;

-- 3. Agregar foreign key constraint
ALTER TABLE public.categories
ADD CONSTRAINT "FK_categories_branches_BranchId"
    FOREIGN KEY ("BranchId")
    REFERENCES public.branches (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

-- 4. Crear índice compuesto para CompanyId y BranchId
CREATE INDEX IF NOT EXISTS "IX_categories_CompanyId_BranchId"
    ON public.categories USING btree
    ("CompanyId" ASC NULLS LAST, "BranchId" ASC NULLS LAST)
    TABLESPACE pg_default;

-- 5. Comentario para documentar el cambio
COMMENT ON COLUMN public.categories."BranchId" IS 'Referencia a la sucursal a la que pertenece la categoría';
