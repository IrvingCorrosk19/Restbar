-- Script para verificar y agregar columnas faltantes en la tabla categories
-- Ejecutar este script en la base de datos PostgreSQL

-- Verificar si las columnas existen
DO $$
BEGIN
    -- Verificar si created_at existe
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'categories' AND column_name = 'created_at'
    ) THEN
        ALTER TABLE "categories" ADD COLUMN "created_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;
        RAISE NOTICE 'Columna created_at agregada';
    ELSE
        RAISE NOTICE 'Columna created_at ya existe';
    END IF;

    -- Verificar si updated_at existe
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'categories' AND column_name = 'updated_at'
    ) THEN
        ALTER TABLE "categories" ADD COLUMN "updated_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;
        RAISE NOTICE 'Columna updated_at agregada';
    ELSE
        RAISE NOTICE 'Columna updated_at ya existe';
    END IF;

    -- Verificar si created_by existe
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'categories' AND column_name = 'created_by'
    ) THEN
        ALTER TABLE "categories" ADD COLUMN "created_by" uuid;
        RAISE NOTICE 'Columna created_by agregada';
    ELSE
        RAISE NOTICE 'Columna created_by ya existe';
    END IF;

    -- Verificar si updated_by existe
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'categories' AND column_name = 'updated_by'
    ) THEN
        ALTER TABLE "categories" ADD COLUMN "updated_by" uuid;
        RAISE NOTICE 'Columna updated_by agregada';
    ELSE
        RAISE NOTICE 'Columna updated_by ya existe';
    END IF;

    -- Verificar si branch_id existe
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'categories' AND column_name = 'branch_id'
    ) THEN
        ALTER TABLE "categories" ADD COLUMN "branch_id" uuid;
        RAISE NOTICE 'Columna branch_id agregada';
    ELSE
        RAISE NOTICE 'Columna branch_id ya existe';
    END IF;

    -- Verificar si company_id existe
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'categories' AND column_name = 'company_id'
    ) THEN
        ALTER TABLE "categories" ADD COLUMN "company_id" uuid;
        RAISE NOTICE 'Columna company_id agregada';
    ELSE
        RAISE NOTICE 'Columna company_id ya existe';
    END IF;
END $$;

-- Crear índices si no existen
DO $$
BEGIN
    -- Índice para created_at
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes WHERE tablename = 'categories' AND indexname = 'ix_categories_created_at'
    ) THEN
        CREATE INDEX "ix_categories_created_at" ON "categories" ("created_at");
        RAISE NOTICE 'Índice ix_categories_created_at creado';
    END IF;

    -- Índice para updated_at
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes WHERE tablename = 'categories' AND indexname = 'ix_categories_updated_at'
    ) THEN
        CREATE INDEX "ix_categories_updated_at" ON "categories" ("updated_at");
        RAISE NOTICE 'Índice ix_categories_updated_at creado';
    END IF;

    -- Índice para updated_by
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes WHERE tablename = 'categories' AND indexname = 'ix_categories_updated_by'
    ) THEN
        CREATE INDEX "ix_categories_updated_by" ON "categories" ("updated_by");
        RAISE NOTICE 'Índice ix_categories_updated_by creado';
    END IF;

    -- Índice para company_id
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes WHERE tablename = 'categories' AND indexname = 'ix_categories_company_id'
    ) THEN
        CREATE INDEX "ix_categories_company_id" ON "categories" ("company_id");
        RAISE NOTICE 'Índice ix_categories_company_id creado';
    END IF;

    -- Índice para branch_id
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes WHERE tablename = 'categories' AND indexname = 'ix_categories_branch_id'
    ) THEN
        CREATE INDEX "ix_categories_branch_id" ON "categories" ("branch_id");
        RAISE NOTICE 'Índice ix_categories_branch_id creado';
    END IF;
END $$;

-- Crear trigger para updated_at si no existe
DO $$
BEGIN
    -- Verificar si la función existe
    IF NOT EXISTS (
        SELECT 1 FROM pg_proc WHERE proname = 'update_updated_at_column'
    ) THEN
        CREATE OR REPLACE FUNCTION update_updated_at_column()
        RETURNS TRIGGER AS $$
        BEGIN
            NEW.updated_at = CURRENT_TIMESTAMP;
            RETURN NEW;
        END;
        $$ language 'plpgsql';
        RAISE NOTICE 'Función update_updated_at_column creada';
    END IF;

    -- Verificar si el trigger existe
    IF NOT EXISTS (
        SELECT 1 FROM pg_trigger WHERE tgname = 'update_categories_updated_at'
    ) THEN
        CREATE TRIGGER update_categories_updated_at 
            BEFORE UPDATE ON "categories" 
            FOR EACH ROW 
            EXECUTE FUNCTION update_updated_at_column();
        RAISE NOTICE 'Trigger update_categories_updated_at creado';
    END IF;
END $$;

-- Mostrar la estructura final de la tabla
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'categories' 
ORDER BY ordinal_position;
