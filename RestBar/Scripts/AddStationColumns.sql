-- =====================================================
-- SCRIPT PARA AGREGAR COLUMNAS A LA TABLA STATIONS - RestBar
-- =====================================================
-- Este script agrega las columnas CompanyId y BranchId a la tabla stations
-- para mantener la integridad multi-tenant del sistema
-- =====================================================

-- 🔍 INICIO: Agregar columnas a la tabla stations
DO $$
BEGIN
    RAISE NOTICE '🔍 [RestBar] Iniciando agregado de columnas a tabla stations...';
END $$;

-- =====================================================
-- AGREGAR COLUMNAS COMPANY_ID Y BRANCH_ID
-- =====================================================

-- 1. AGREGAR COLUMNA company_id (si no existe)
DO $$
DECLARE
    column_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'stations' 
        AND column_name = 'company_id'
    ) INTO column_exists;
    
    IF NOT column_exists THEN
        RAISE NOTICE '➕ [Stations] Agregando columna company_id...';
        ALTER TABLE public.stations ADD COLUMN company_id uuid;
        RAISE NOTICE '✅ [Stations] Columna company_id agregada exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Stations] Columna company_id ya existe';
    END IF;
END $$;

-- 2. AGREGAR COLUMNA branch_id (si no existe)
DO $$
DECLARE
    column_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'stations' 
        AND column_name = 'branch_id'
    ) INTO column_exists;
    
    IF NOT column_exists THEN
        RAISE NOTICE '➕ [Stations] Agregando columna branch_id...';
        ALTER TABLE public.stations ADD COLUMN branch_id uuid;
        RAISE NOTICE '✅ [Stations] Columna branch_id agregada exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Stations] Columna branch_id ya existe';
    END IF;
END $$;

-- =====================================================
-- AGREGAR CONSTRAINTS DE FOREIGN KEY
-- =====================================================

-- 3. AGREGAR FOREIGN KEY PARA company_id (si no existe)
DO $$
DECLARE
    constraint_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.table_constraints 
        WHERE table_schema = 'public' 
        AND table_name = 'stations' 
        AND constraint_name = 'FK_stations_companies_company_id'
    ) INTO constraint_exists;
    
    IF NOT constraint_exists THEN
        RAISE NOTICE '🔗 [Stations] Agregando FK para company_id...';
        ALTER TABLE public.stations 
        ADD CONSTRAINT "FK_stations_companies_company_id" 
        FOREIGN KEY (company_id) REFERENCES public.companies(id) 
        ON DELETE CASCADE;
        RAISE NOTICE '✅ [Stations] FK para company_id agregada exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Stations] FK para company_id ya existe';
    END IF;
END $$;

-- 4. AGREGAR FOREIGN KEY PARA branch_id (si no existe)
DO $$
DECLARE
    constraint_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.table_constraints 
        WHERE table_schema = 'public' 
        AND table_name = 'stations' 
        AND constraint_name = 'FK_stations_branches_branch_id'
    ) INTO constraint_exists;
    
    IF NOT constraint_exists THEN
        RAISE NOTICE '🔗 [Stations] Agregando FK para branch_id...';
        ALTER TABLE public.stations 
        ADD CONSTRAINT "FK_stations_branches_branch_id" 
        FOREIGN KEY (branch_id) REFERENCES public.branches(id) 
        ON DELETE CASCADE;
        RAISE NOTICE '✅ [Stations] FK para branch_id agregada exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Stations] FK para branch_id ya existe';
    END IF;
END $$;

-- =====================================================
-- AGREGAR ÍNDICES PARA OPTIMIZACIÓN
-- =====================================================

-- 5. AGREGAR ÍNDICE PARA company_id (si no existe)
DO $$
DECLARE
    index_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM pg_indexes 
        WHERE tablename = 'stations' 
        AND indexname = 'IX_stations_company_id'
    ) INTO index_exists;
    
    IF NOT index_exists THEN
        RAISE NOTICE '📊 [Stations] Agregando índice para company_id...';
        CREATE INDEX "IX_stations_company_id" ON public.stations (company_id);
        RAISE NOTICE '✅ [Stations] Índice para company_id agregado exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Stations] Índice para company_id ya existe';
    END IF;
END $$;

-- 6. AGREGAR ÍNDICE PARA branch_id (si no existe)
DO $$
DECLARE
    index_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM pg_indexes 
        WHERE tablename = 'stations' 
        AND indexname = 'IX_stations_branch_id'
    ) INTO index_exists;
    
    IF NOT index_exists THEN
        RAISE NOTICE '📊 [Stations] Agregando índice para branch_id...';
        CREATE INDEX "IX_stations_branch_id" ON public.stations (branch_id);
        RAISE NOTICE '✅ [Stations] Índice para branch_id agregado exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Stations] Índice para branch_id ya existe';
    END IF;
END $$;

-- =====================================================
-- ACTUALIZAR DATOS EXISTENTES (SI LOS HAY)
-- =====================================================

-- 7. ACTUALIZAR ESTACIONES EXISTENTES CON DATOS POR DEFECTO
DO $$
DECLARE
    station_count INTEGER;
    default_company_id uuid;
    default_branch_id uuid;
BEGIN
    -- Contar estaciones existentes
    SELECT COUNT(*) INTO station_count FROM public.stations;
    
    IF station_count > 0 THEN
        RAISE NOTICE '📊 [Stations] Encontradas % estaciones existentes', station_count;
        
        -- Obtener la primera compañía y sucursal como valores por defecto
        SELECT id INTO default_company_id FROM public.companies LIMIT 1;
        SELECT id INTO default_branch_id FROM public.branches LIMIT 1;
        
        IF default_company_id IS NOT NULL AND default_branch_id IS NOT NULL THEN
            -- Actualizar estaciones existentes
            UPDATE public.stations 
            SET company_id = default_company_id, 
                branch_id = default_branch_id
            WHERE company_id IS NULL OR branch_id IS NULL;
            
            RAISE NOTICE '✅ [Stations] Estaciones existentes actualizadas con company_id y branch_id por defecto';
        ELSE
            RAISE NOTICE '⚠️ [Stations] No se encontraron compañías o sucursales para asignar por defecto';
        END IF;
    ELSE
        RAISE NOTICE 'ℹ️ [Stations] No hay estaciones existentes para actualizar';
    END IF;
END $$;

-- =====================================================
-- VERIFICACIÓN FINAL
-- =====================================================
DO $$
DECLARE
    column_count INTEGER;
BEGIN
    RAISE NOTICE '🔍 [RestBar] Verificación final de estructura...';
    
    -- Verificar que las columnas existen
    SELECT COUNT(*) INTO column_count
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
    AND table_name = 'stations' 
    AND column_name IN ('company_id', 'branch_id');
    
    IF column_count = 2 THEN
        RAISE NOTICE '✅ [Verify] Columnas company_id y branch_id agregadas correctamente';
    ELSE
        RAISE NOTICE '❌ [Verify] Faltan columnas: se encontraron % de 2', column_count;
    END IF;
END $$;

-- =====================================================
-- RESUMEN FINAL
-- =====================================================
DO $$
BEGIN
    RAISE NOTICE '🎯 [RestBar] =====================================================';
    RAISE NOTICE '🎯 [RestBar] AGREGADO DE COLUMNAS A STATIONS COMPLETADO';
    RAISE NOTICE '🎯 [RestBar] =====================================================';
    RAISE NOTICE '✅ [RestBar] Columnas company_id y branch_id agregadas';
    RAISE NOTICE '✅ [RestBar] Foreign Keys configuradas';
    RAISE NOTICE '✅ [RestBar] Índices creados para optimización';
    RAISE NOTICE '✅ [RestBar] Datos existentes actualizados';
    RAISE NOTICE '🎯 [RestBar] Tabla stations lista para multi-tenant';
    RAISE NOTICE '🎯 [RestBar] =====================================================';
END $$;

-- =====================================================
-- FIN DEL SCRIPT DE AGREGADO DE COLUMNAS
-- =====================================================
