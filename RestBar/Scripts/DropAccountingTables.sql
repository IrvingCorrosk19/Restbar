-- =====================================================
-- SCRIPT PARA ELIMINAR TABLAS DE CONTABILIDAD - RestBar
-- =====================================================
-- Este script elimina las tablas de contabilidad que ya no están en el contexto
-- Ejecutar con precaución en producción
-- =====================================================

-- 🔍 INICIO: Eliminación de tablas de contabilidad
DO $$
BEGIN
    RAISE NOTICE '🔍 [RestBar] Iniciando eliminación de tablas de contabilidad...';
END $$;

-- =====================================================
-- ELIMINACIÓN DE TABLAS DE CONTABILIDAD
-- =====================================================

-- 1. ELIMINAR TABLA journal_entry_details (si existe)
DO $$
DECLARE
    table_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'journal_entry_details'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '🗑️ [Drop] Eliminando tabla journal_entry_details...';
        DROP TABLE IF EXISTS journal_entry_details CASCADE;
        RAISE NOTICE '✅ [Drop] Tabla journal_entry_details eliminada exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Drop] Tabla journal_entry_details no existe';
    END IF;
END $$;

-- 2. ELIMINAR TABLA journal_entries (si existe)
DO $$
DECLARE
    table_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'journal_entries'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '🗑️ [Drop] Eliminando tabla journal_entries...';
        DROP TABLE IF EXISTS journal_entries CASCADE;
        RAISE NOTICE '✅ [Drop] Tabla journal_entries eliminada exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Drop] Tabla journal_entries no existe';
    END IF;
END $$;

-- 3. ELIMINAR TABLA accounts (si existe)
DO $$
DECLARE
    table_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'accounts'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '🗑️ [Drop] Eliminando tabla accounts...';
        DROP TABLE IF EXISTS accounts CASCADE;
        RAISE NOTICE '✅ [Drop] Tabla accounts eliminada exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Drop] Tabla accounts no existe';
    END IF;
END $$;

-- =====================================================
-- ELIMINACIÓN DE ENUMS DE CONTABILIDAD
-- =====================================================

-- 4. ELIMINAR ENUMS DE CONTABILIDAD (si existen)
DO $$
DECLARE
    enum_exists BOOLEAN;
BEGIN
    -- Verificar y eliminar journal_entry_status_enum
    SELECT EXISTS (
        SELECT FROM pg_type 
        WHERE typname = 'journal_entry_status_enum'
    ) INTO enum_exists;
    
    IF enum_exists THEN
        RAISE NOTICE '🗑️ [Drop] Eliminando enum journal_entry_status_enum...';
        DROP TYPE IF EXISTS journal_entry_status_enum CASCADE;
        RAISE NOTICE '✅ [Drop] Enum journal_entry_status_enum eliminado exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Drop] Enum journal_entry_status_enum no existe';
    END IF;
    
    -- Verificar y eliminar journal_entry_type_enum
    SELECT EXISTS (
        SELECT FROM pg_type 
        WHERE typname = 'journal_entry_type_enum'
    ) INTO enum_exists;
    
    IF enum_exists THEN
        RAISE NOTICE '🗑️ [Drop] Eliminando enum journal_entry_type_enum...';
        DROP TYPE IF EXISTS journal_entry_type_enum CASCADE;
        RAISE NOTICE '✅ [Drop] Enum journal_entry_type_enum eliminado exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Drop] Enum journal_entry_type_enum no existe';
    END IF;
    
    -- Verificar y eliminar account_type_enum
    SELECT EXISTS (
        SELECT FROM pg_type 
        WHERE typname = 'account_type_enum'
    ) INTO enum_exists;
    
    IF enum_exists THEN
        RAISE NOTICE '🗑️ [Drop] Eliminando enum account_type_enum...';
        DROP TYPE IF EXISTS account_type_enum CASCADE;
        RAISE NOTICE '✅ [Drop] Enum account_type_enum eliminado exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Drop] Enum account_type_enum no existe';
    END IF;
    
    -- Verificar y eliminar account_category_enum
    SELECT EXISTS (
        SELECT FROM pg_type 
        WHERE typname = 'account_category_enum'
    ) INTO enum_exists;
    
    IF enum_exists THEN
        RAISE NOTICE '🗑️ [Drop] Eliminando enum account_category_enum...';
        DROP TYPE IF EXISTS account_category_enum CASCADE;
        RAISE NOTICE '✅ [Drop] Enum account_category_enum eliminado exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Drop] Enum account_category_enum no existe';
    END IF;
    
    -- Verificar y eliminar account_nature_enum
    SELECT EXISTS (
        SELECT FROM pg_type 
        WHERE typname = 'account_nature_enum'
    ) INTO enum_exists;
    
    IF enum_exists THEN
        RAISE NOTICE '🗑️ [Drop] Eliminando enum account_nature_enum...';
        DROP TYPE IF EXISTS account_nature_enum CASCADE;
        RAISE NOTICE '✅ [Drop] Enum account_nature_enum eliminado exitosamente';
    ELSE
        RAISE NOTICE 'ℹ️ [Drop] Enum account_nature_enum no existe';
    END IF;
END $$;

-- =====================================================
-- VERIFICACIÓN FINAL
-- =====================================================
DO $$
DECLARE
    table_exists BOOLEAN;
BEGIN
    RAISE NOTICE '🔍 [RestBar] Verificación final de eliminación...';
    
    -- Verificar que las tablas ya no existen
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'accounts'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '❌ [Verify] Tabla accounts AÚN EXISTE';
    ELSE
        RAISE NOTICE '✅ [Verify] Tabla accounts eliminada correctamente';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'journal_entries'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '❌ [Verify] Tabla journal_entries AÚN EXISTE';
    ELSE
        RAISE NOTICE '✅ [Verify] Tabla journal_entries eliminada correctamente';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'journal_entry_details'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '❌ [Verify] Tabla journal_entry_details AÚN EXISTE';
    ELSE
        RAISE NOTICE '✅ [Verify] Tabla journal_entry_details eliminada correctamente';
    END IF;
END $$;

-- =====================================================
-- RESUMEN FINAL
-- =====================================================
DO $$
BEGIN
    RAISE NOTICE '🎯 [RestBar] =====================================================';
    RAISE NOTICE '🎯 [RestBar] ELIMINACIÓN DE TABLAS DE CONTABILIDAD COMPLETADA';
    RAISE NOTICE '🎯 [RestBar] =====================================================';
    RAISE NOTICE '✅ [RestBar] Tablas de contabilidad eliminadas';
    RAISE NOTICE '✅ [RestBar] Enums de contabilidad eliminados';
    RAISE NOTICE '✅ [RestBar] Base de datos homologada con RestBarContext';
    RAISE NOTICE '🎯 [RestBar] =====================================================';
END $$;

-- =====================================================
-- FIN DEL SCRIPT DE ELIMINACIÓN
-- =====================================================
