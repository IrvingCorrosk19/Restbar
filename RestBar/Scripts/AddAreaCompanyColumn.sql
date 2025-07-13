-- =====================================================
-- SCRIPT PARA AGREGAR COLUMNA COMPANY_ID A LA TABLA AREAS - RestBar
-- =====================================================
-- Este script agrega la columna company_id a la tabla areas
-- para mantener la integridad multi-tenant del sistema
-- =====================================================

-- 🔍 INICIO: Agregar columna company_id a la tabla areas
DO $$
BEGIN
    RAISE NOTICE '🔍 [RestBar] Iniciando agregado de columna company_id a tabla areas...';
END $$;

-- =====================================================
-- AGREGAR COLUMNA COMPANY_ID
-- =====================================================

-- 1. AGREGAR COLUMNA company_id (si no existe)
DO $$
DECLARE
    column_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'areas' 
        AND column_name = 'company_id'
    ) INTO column_exists;
    
    IF NOT column_exists THEN
        RAISE NOTICE '➕ [Areas] Agregando columna company_id...';
        ALTER TABLE public.areas ADD COLUMN company_id uuid;
        RAISE NOTICE '✅ [Areas] Columna company_id agregada exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Areas] Columna company_id ya existe';
    END IF;
END $$;

-- =====================================================
-- AGREGAR CONSTRAINT DE FOREIGN KEY
-- =====================================================

-- 2. AGREGAR FOREIGN KEY PARA company_id (si no existe)
DO $$
DECLARE
    constraint_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.table_constraints 
        WHERE table_schema = 'public' 
        AND table_name = 'areas' 
        AND constraint_name = 'FK_areas_companies_company_id'
    ) INTO constraint_exists;
    
    IF NOT constraint_exists THEN
        RAISE NOTICE '🔗 [Areas] Agregando FK para company_id...';
        ALTER TABLE public.areas 
        ADD CONSTRAINT "FK_areas_companies_company_id" 
        FOREIGN KEY (company_id) REFERENCES public.companies(id) 
        ON DELETE CASCADE;
        RAISE NOTICE '✅ [Areas] FK para company_id agregada exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Areas] FK para company_id ya existe';
    END IF;
END $$;

-- =====================================================
-- AGREGAR ÍNDICE PARA OPTIMIZACIÓN
-- =====================================================

-- 3. AGREGAR ÍNDICE PARA company_id (si no existe)
DO $$
DECLARE
    index_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM pg_indexes 
        WHERE tablename = 'areas' 
        AND indexname = 'IX_areas_company_id'
    ) INTO index_exists;
    
    IF NOT index_exists THEN
        RAISE NOTICE '📊 [Areas] Agregando índice para company_id...';
        CREATE INDEX "IX_areas_company_id" ON public.areas (company_id);
        RAISE NOTICE '✅ [Areas] Índice para company_id agregado exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Areas] Índice para company_id ya existe';
    END IF;
END $$;

-- =====================================================
-- ACTUALIZAR DATOS EXISTENTES (SI LOS HAY)
-- =====================================================

-- 4. ACTUALIZAR ÁREAS EXISTENTES CON DATOS POR DEFECTO
DO $$
DECLARE
    area_count INTEGER;
    default_company_id uuid;
BEGIN
    -- Contar áreas existentes
    SELECT COUNT(*) INTO area_count FROM public.areas;
    
    IF area_count > 0 THEN
        RAISE NOTICE '📊 [Areas] Encontradas % áreas existentes', area_count;
        
        -- Obtener la primera compañía como valor por defecto
        SELECT id INTO default_company_id FROM public.companies LIMIT 1;
        
        IF default_company_id IS NOT NULL THEN
            -- Actualizar áreas existentes
            UPDATE public.areas 
            SET company_id = default_company_id
            WHERE company_id IS NULL;
            
            RAISE NOTICE '✅ [Areas] Áreas existentes actualizadas con company_id por defecto';
        ELSE
            RAISE NOTICE '⚠️ [Areas] No se encontraron compañías para asignar por defecto';
        END IF;
    ELSE
        RAISE NOTICE 'ℹ️ [Areas] No hay áreas existentes para actualizar';
    END IF;
END $$;

-- =====================================================
-- VERIFICACIÓN FINAL
-- =====================================================
DO $$
DECLARE
    column_exists BOOLEAN;
BEGIN
    RAISE NOTICE '🔍 [RestBar] Verificación final de estructura...';
    
    -- Verificar que la columna existe
    SELECT EXISTS (
        SELECT FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'areas' 
        AND column_name = 'company_id'
    ) INTO column_exists;
    
    IF column_exists THEN
        RAISE NOTICE '✅ [Verify] Columna company_id agregada correctamente a areas';
    ELSE
        RAISE NOTICE '❌ [Verify] Columna company_id NO se agregó a areas';
    END IF;
END $$;

-- =====================================================
-- RESUMEN FINAL
-- =====================================================
DO $$
BEGIN
    RAISE NOTICE '🎯 [RestBar] =====================================================';
    RAISE NOTICE '🎯 [RestBar] AGREGADO DE COLUMNA COMPANY_ID A AREAS COMPLETADO';
    RAISE NOTICE '🎯 [RestBar] =====================================================';
    RAISE NOTICE '✅ [RestBar] Columna company_id agregada a areas';
    RAISE NOTICE '✅ [RestBar] Foreign Key configurada';
    RAISE NOTICE '✅ [RestBar] Índice creado para optimización';
    RAISE NOTICE '✅ [RestBar] Datos existentes actualizados';
    RAISE NOTICE '🎯 [RestBar] Tabla areas lista para multi-tenant';
    RAISE NOTICE '🎯 [RestBar] =====================================================';
END $$;

-- =====================================================
-- FIN DEL SCRIPT DE AGREGADO DE COLUMNA
-- =====================================================
