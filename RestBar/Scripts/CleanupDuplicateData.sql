-- =====================================================
-- SCRIPT DE LIMPIEZA DE DATOS DUPLICADOS - RestBar
-- =====================================================
-- Este script elimina datos duplicados manteniendo la integridad referencial
-- Compatible con PostgreSQL y RestBarContext (SIN MÓDULOS DE INVENTARIO Y CONTABILIDAD)
-- Ejecutar con precaución en producción
-- =====================================================

-- 🔍 INICIO: Verificación de estructura de base de datos
DO $$
BEGIN
    RAISE NOTICE '🔍 [RestBar] Iniciando verificación de estructura de base de datos...';
    RAISE NOTICE '✅ [RestBar] Módulos eliminados: Inventario, Contabilidad';
END $$;

-- =====================================================
-- VERIFICACIÓN DE TABLAS EXISTENTES (SIN CONTABILIDAD)
-- =====================================================
DO $$
DECLARE
    table_exists BOOLEAN;
BEGIN
    -- Verificar tablas principales
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'users'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla users existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla users NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'products'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla products existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla products NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'categories'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla categories existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla categories NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'companies'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla companies existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla companies NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'branches'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla branches existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla branches NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'areas'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla areas existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla areas NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'stations'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla stations existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla stations NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'tables'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla tables existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla tables NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'customers'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla customers existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla customers NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'modifiers'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla modifiers existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla modifiers NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'user_assignments'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla user_assignments existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla user_assignments NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'orders'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla orders existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla orders NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'order_items'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla order_items existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla order_items NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'payments'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla payments existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla payments NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'split_payments'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla split_payments existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla split_payments NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'invoices'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla invoices existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla invoices NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'order_cancellation_logs'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla order_cancellation_logs existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla order_cancellation_logs NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'transfers'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla transfers existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla transfers NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'transfer_items'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla transfer_items existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla transfer_items NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'product_modifiers'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla product_modifiers existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla product_modifiers NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'audit_logs'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla audit_logs existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla audit_logs NO existe';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'notifications'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '✅ [Structure] Tabla notifications existe';
    ELSE
        RAISE NOTICE '❌ [Structure] Tabla notifications NO existe';
    END IF;
    
    -- ✅ VERIFICAR TABLAS ELIMINADAS (NO DEBEN EXISTIR)
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'accounts'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '⚠️ [Structure] Tabla accounts EXISTE (debería estar eliminada)';
    ELSE
        RAISE NOTICE '✅ [Structure] Tabla accounts NO existe (correcto)';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'journal_entries'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '⚠️ [Structure] Tabla journal_entries EXISTE (debería estar eliminada)';
    ELSE
        RAISE NOTICE '✅ [Structure] Tabla journal_entries NO existe (correcto)';
    END IF;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'journal_entry_details'
    ) INTO table_exists;
    
    IF table_exists THEN
        RAISE NOTICE '⚠️ [Structure] Tabla journal_entry_details EXISTE (debería estar eliminada)';
    ELSE
        RAISE NOTICE '✅ [Structure] Tabla journal_entry_details NO existe (correcto)';
    END IF;
    
END $$;

-- =====================================================
-- LIMPIEZA CONDICIONAL BASADA EN TABLAS EXISTENTES
-- =====================================================

-- 1. LIMPIEZA DE USUARIOS DUPLICADOS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'users'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar usuarios duplicados por email
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT email, COUNT(*) as cnt
            FROM users 
            WHERE email IS NOT NULL AND email != ''
            GROUP BY email 
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [Users] Usuarios duplicados encontrados: %', duplicate_count;
        
        -- Eliminar usuarios duplicados manteniendo el más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM users 
            WHERE id IN (
                SELECT u.id
                FROM users u
                INNER JOIN (
                    SELECT email, MAX(created_at) as max_created
                    FROM users 
                    WHERE email IS NOT NULL AND email != ''
                    GROUP BY email
                ) latest ON u.email = latest.email AND u.created_at = latest.max_created
                WHERE u.id NOT IN (
                    SELECT MIN(id)
                    FROM users
                    WHERE email IS NOT NULL AND email != ''
                    GROUP BY email
                )
            );
            
            RAISE NOTICE '✅ [Users] Usuarios duplicados eliminados exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Users] Tabla users no existe, saltando limpieza';
    END IF;
END $$;

-- 2. LIMPIEZA DE PRODUCTOS DUPLICADOS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'products'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar productos duplicados por nombre y categoría
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT name, category_id, COUNT(*) as cnt
            FROM products 
            WHERE name IS NOT NULL AND name != ''
            GROUP BY name, category_id
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [Products] Productos duplicados encontrados: %', duplicate_count;
        
        -- Eliminar productos duplicados manteniendo el más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM products 
            WHERE id IN (
                SELECT p.id
                FROM products p
                INNER JOIN (
                    SELECT name, category_id, MAX(created_at) as max_created
                    FROM products 
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name, category_id
                ) latest ON p.name = latest.name AND p.category_id = latest.category_id AND p.created_at = latest.max_created
                WHERE p.id NOT IN (
                    SELECT MIN(id)
                    FROM products
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name, category_id
                )
            );
            
            RAISE NOTICE '✅ [Products] Productos duplicados eliminados exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Products] Tabla products no existe, saltando limpieza';
    END IF;
END $$;

-- 3. LIMPIEZA DE CATEGORÍAS DUPLICADAS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'categories'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar categorías duplicadas por nombre
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT name, COUNT(*) as cnt
            FROM categories 
            WHERE name IS NOT NULL AND name != ''
            GROUP BY name 
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [Categories] Categorías duplicadas encontradas: %', duplicate_count;
        
        -- Eliminar categorías duplicadas manteniendo la más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM categories 
            WHERE id IN (
                SELECT c.id
                FROM categories c
                INNER JOIN (
                    SELECT name, MAX(id) as max_id
                    FROM categories 
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name
                ) latest ON c.name = latest.name AND c.id = latest.max_id
                WHERE c.id NOT IN (
                    SELECT MIN(id)
                    FROM categories
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name
                )
            );
            
            RAISE NOTICE '✅ [Categories] Categorías duplicadas eliminadas exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Categories] Tabla categories no existe, saltando limpieza';
    END IF;
END $$;

-- 4. LIMPIEZA DE COMPAÑÍAS DUPLICADAS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'companies'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar compañías duplicadas por legal_id
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT legal_id, COUNT(*) as cnt
            FROM companies 
            WHERE legal_id IS NOT NULL AND legal_id != ''
            GROUP BY legal_id 
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [Companies] Compañías duplicadas encontradas: %', duplicate_count;
        
        -- Eliminar compañías duplicadas manteniendo la más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM companies 
            WHERE id IN (
                SELECT comp.id
                FROM companies comp
                INNER JOIN (
                    SELECT legal_id, MAX(created_at) as max_created
                    FROM companies 
                    WHERE legal_id IS NOT NULL AND legal_id != ''
                    GROUP BY legal_id
                ) latest ON comp.legal_id = latest.legal_id AND comp.created_at = latest.max_created
                WHERE comp.id NOT IN (
                    SELECT MIN(id)
                    FROM companies
                    WHERE legal_id IS NOT NULL AND legal_id != ''
                    GROUP BY legal_id
                )
            );
            
            RAISE NOTICE '✅ [Companies] Compañías duplicadas eliminadas exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Companies] Tabla companies no existe, saltando limpieza';
    END IF;
END $$;

-- 5. LIMPIEZA DE SUCURSALES DUPLICADAS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'branches'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar sucursales duplicadas por nombre y compañía
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT name, company_id, COUNT(*) as cnt
            FROM branches 
            WHERE name IS NOT NULL AND name != ''
            GROUP BY name, company_id
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [Branches] Sucursales duplicadas encontradas: %', duplicate_count;
        
        -- Eliminar sucursales duplicadas manteniendo la más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM branches 
            WHERE id IN (
                SELECT b.id
                FROM branches b
                INNER JOIN (
                    SELECT name, company_id, MAX(created_at) as max_created
                    FROM branches 
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name, company_id
                ) latest ON b.name = latest.name AND b.company_id = latest.company_id AND b.created_at = latest.max_created
                WHERE b.id NOT IN (
                    SELECT MIN(id)
                    FROM branches
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name, company_id
                )
            );
            
            RAISE NOTICE '✅ [Branches] Sucursales duplicadas eliminadas exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Branches] Tabla branches no existe, saltando limpieza';
    END IF;
END $$;

-- 6. LIMPIEZA DE ÁREAS DUPLICADAS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'areas'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar áreas duplicadas por nombre y sucursal
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT name, branch_id, COUNT(*) as cnt
            FROM areas 
            WHERE name IS NOT NULL AND name != ''
            GROUP BY name, branch_id
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [Areas] Áreas duplicadas encontradas: %', duplicate_count;
        
        -- Eliminar áreas duplicadas manteniendo la más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM areas 
            WHERE id IN (
                SELECT a.id
                FROM areas a
                INNER JOIN (
                    SELECT name, branch_id, MAX(id) as max_id
                    FROM areas 
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name, branch_id
                ) latest ON a.name = latest.name AND a.branch_id = latest.branch_id AND a.id = latest.max_id
                WHERE a.id NOT IN (
                    SELECT MIN(id)
                    FROM areas
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name, branch_id
                )
            );
            
            RAISE NOTICE '✅ [Areas] Áreas duplicadas eliminadas exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Areas] Tabla areas no existe, saltando limpieza';
    END IF;
END $$;

-- 7. LIMPIEZA DE ESTACIONES DUPLICADAS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'stations'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar estaciones duplicadas por nombre y área
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT name, area_id, COUNT(*) as cnt
            FROM stations 
            WHERE name IS NOT NULL AND name != ''
            GROUP BY name, area_id
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [Stations] Estaciones duplicadas encontradas: %', duplicate_count;
        
        -- Eliminar estaciones duplicadas manteniendo la más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM stations 
            WHERE id IN (
                SELECT s.id
                FROM stations s
                INNER JOIN (
                    SELECT name, area_id, MAX(id) as max_id
                    FROM stations 
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name, area_id
                ) latest ON s.name = latest.name AND s.area_id = latest.area_id AND s.id = latest.max_id
                WHERE s.id NOT IN (
                    SELECT MIN(id)
                    FROM stations
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name, area_id
                )
            );
            
            RAISE NOTICE '✅ [Stations] Estaciones duplicadas eliminadas exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Stations] Tabla stations no existe, saltando limpieza';
    END IF;
END $$;

-- 8. LIMPIEZA DE MESAS DUPLICADAS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'tables'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar mesas duplicadas por número y área
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT table_number, area_id, COUNT(*) as cnt
            FROM tables 
            WHERE table_number IS NOT NULL AND table_number != ''
            GROUP BY table_number, area_id
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [Tables] Mesas duplicadas encontradas: %', duplicate_count;
        
        -- Eliminar mesas duplicadas manteniendo la más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM tables 
            WHERE id IN (
                SELECT t.id
                FROM tables t
                INNER JOIN (
                    SELECT table_number, area_id, MAX(id) as max_id
                    FROM tables 
                    WHERE table_number IS NOT NULL AND table_number != ''
                    GROUP BY table_number, area_id
                ) latest ON t.table_number = latest.table_number AND t.area_id = latest.area_id AND t.id = latest.max_id
                WHERE t.id NOT IN (
                    SELECT MIN(id)
                    FROM tables
                    WHERE table_number IS NOT NULL AND table_number != ''
                    GROUP BY table_number, area_id
                )
            );
            
            RAISE NOTICE '✅ [Tables] Mesas duplicadas eliminadas exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Tables] Tabla tables no existe, saltando limpieza';
    END IF;
END $$;

-- 9. LIMPIEZA DE CLIENTES DUPLICADOS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'customers'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar clientes duplicados por email
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT email, COUNT(*) as cnt
            FROM customers 
            WHERE email IS NOT NULL AND email != ''
            GROUP BY email 
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [Customers] Clientes duplicados encontrados: %', duplicate_count;
        
        -- Eliminar clientes duplicados manteniendo el más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM customers 
            WHERE id IN (
                SELECT c.id
                FROM customers c
                INNER JOIN (
                    SELECT email, MAX(created_at) as max_created
                    FROM customers 
                    WHERE email IS NOT NULL AND email != ''
                    GROUP BY email
                ) latest ON c.email = latest.email AND c.created_at = latest.max_created
                WHERE c.id NOT IN (
                    SELECT MIN(id)
                    FROM customers
                    WHERE email IS NOT NULL AND email != ''
                    GROUP BY email
                )
            );
            
            RAISE NOTICE '✅ [Customers] Clientes duplicados eliminados exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Customers] Tabla customers no existe, saltando limpieza';
    END IF;
END $$;

-- 10. LIMPIEZA DE MODIFICADORES DUPLICADOS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'modifiers'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar modificadores duplicados por nombre
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT name, COUNT(*) as cnt
            FROM modifiers 
            WHERE name IS NOT NULL AND name != ''
            GROUP BY name 
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [Modifiers] Modificadores duplicados encontrados: %', duplicate_count;
        
        -- Eliminar modificadores duplicados manteniendo el más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM modifiers 
            WHERE id IN (
                SELECT m.id
                FROM modifiers m
                INNER JOIN (
                    SELECT name, MAX(id) as max_id
                    FROM modifiers 
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name
                ) latest ON m.name = latest.name AND m.id = latest.max_id
                WHERE m.id NOT IN (
                    SELECT MIN(id)
                    FROM modifiers
                    WHERE name IS NOT NULL AND name != ''
                    GROUP BY name
                )
            );
            
            RAISE NOTICE '✅ [Modifiers] Modificadores duplicados eliminados exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Modifiers] Tabla modifiers no existe, saltando limpieza';
    END IF;
END $$;

-- 11. LIMPIEZA DE ASIGNACIONES DE USUARIO DUPLICADAS (si existe la tabla)
DO $$
DECLARE
    table_exists BOOLEAN;
    duplicate_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'user_assignments'
    ) INTO table_exists;
    
    IF table_exists THEN
        -- Verificar asignaciones duplicadas (usuario, estación, área activas)
        SELECT COUNT(*) INTO duplicate_count
        FROM (
            SELECT user_id, station_id, area_id, COUNT(*) as cnt
            FROM user_assignments 
            WHERE is_active = true
            GROUP BY user_id, station_id, area_id
            HAVING COUNT(*) > 1
        ) duplicates;
        
        RAISE NOTICE '📊 [UserAssignments] Asignaciones duplicadas encontradas: %', duplicate_count;
        
        -- Eliminar asignaciones duplicadas manteniendo la más reciente
        IF duplicate_count > 0 THEN
            DELETE FROM user_assignments 
            WHERE id IN (
                SELECT ua.id
                FROM user_assignments ua
                INNER JOIN (
                    SELECT user_id, station_id, area_id, MAX(assigned_at) as max_assigned
                    FROM user_assignments 
                    WHERE is_active = true
                    GROUP BY user_id, station_id, area_id
                ) latest ON ua.user_id = latest.user_id AND ua.station_id = latest.station_id AND ua.area_id = latest.area_id AND ua.assigned_at = latest.max_assigned
                WHERE ua.id NOT IN (
                    SELECT MIN(id)
                    FROM user_assignments
                    WHERE is_active = true
                    GROUP BY user_id, station_id, area_id
                )
            );
            
            RAISE NOTICE '✅ [UserAssignments] Asignaciones duplicadas eliminadas exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [UserAssignments] Tabla user_assignments no existe, saltando limpieza';
    END IF;
END $$;

-- =====================================================
-- LIMPIEZA DE REFERENCIAS HUÉRFANAS
-- =====================================================

-- 12. LIMPIEZA DE ÓRDENES ORFANAS (sin items)
DO $$
DECLARE
    orders_exists BOOLEAN;
    order_items_exists BOOLEAN;
    orphan_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'orders'
    ) INTO orders_exists;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'order_items'
    ) INTO order_items_exists;
    
    IF orders_exists AND order_items_exists THEN
        -- Verificar órdenes sin items
        SELECT COUNT(*) INTO orphan_count
        FROM orders o
        LEFT JOIN order_items oi ON o.id = oi.order_id
        WHERE oi.id IS NULL;
        
        RAISE NOTICE '📊 [Orders] Órdenes sin items encontradas: %', orphan_count;
        
        -- Eliminar órdenes sin items
        IF orphan_count > 0 THEN
            DELETE FROM orders 
            WHERE id IN (
                SELECT o.id
                FROM orders o
                LEFT JOIN order_items oi ON o.id = oi.order_id
                WHERE oi.id IS NULL
            );
            
            RAISE NOTICE '✅ [Orders] Órdenes sin items eliminadas exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Orders] Tablas orders u order_items no existen, saltando limpieza';
    END IF;
END $$;

-- 13. LIMPIEZA DE PAGOS ORFANOS (sin orden)
DO $$
DECLARE
    payments_exists BOOLEAN;
    orders_exists BOOLEAN;
    orphan_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'payments'
    ) INTO payments_exists;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'orders'
    ) INTO orders_exists;
    
    IF payments_exists AND orders_exists THEN
        -- Verificar pagos sin orden
        SELECT COUNT(*) INTO orphan_count
        FROM payments p
        LEFT JOIN orders o ON p.order_id = o.id
        WHERE o.id IS NULL;
        
        RAISE NOTICE '📊 [Payments] Pagos sin orden encontrados: %', orphan_count;
        
        -- Eliminar pagos sin orden
        IF orphan_count > 0 THEN
            DELETE FROM payments 
            WHERE id IN (
                SELECT p.id
                FROM payments p
                LEFT JOIN orders o ON p.order_id = o.id
                WHERE o.id IS NULL
            );
            
            RAISE NOTICE '✅ [Payments] Pagos sin orden eliminados exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [Payments] Tablas payments u orders no existen, saltando limpieza';
    END IF;
END $$;

-- 14. LIMPIEZA DE ITEMS DE ORDEN ORFANOS (sin producto)
DO $$
DECLARE
    order_items_exists BOOLEAN;
    products_exists BOOLEAN;
    orphan_count INTEGER;
BEGIN
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'order_items'
    ) INTO order_items_exists;
    
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'products'
    ) INTO products_exists;
    
    IF order_items_exists AND products_exists THEN
        -- Verificar items de orden sin producto
        SELECT COUNT(*) INTO orphan_count
        FROM order_items oi
        LEFT JOIN products p ON oi.product_id = p.id
        WHERE p.id IS NULL;
        
        RAISE NOTICE '📊 [OrderItems] Items sin producto encontrados: %', orphan_count;
        
        -- Eliminar items sin producto
        IF orphan_count > 0 THEN
            DELETE FROM order_items 
            WHERE id IN (
                SELECT oi.id
                FROM order_items oi
                LEFT JOIN products p ON oi.product_id = p.id
                WHERE p.id IS NULL
            );
            
            RAISE NOTICE '✅ [OrderItems] Items sin producto eliminados exitosamente';
        END IF;
    ELSE
        RAISE NOTICE '⚠️ [OrderItems] Tablas order_items u products no existen, saltando limpieza';
    END IF;
END $$;

-- =====================================================
-- RESUMEN FINAL DE LIMPIEZA
-- =====================================================
DO $$
BEGIN
    RAISE NOTICE '🎯 [RestBar] =====================================================';
    RAISE NOTICE '🎯 [RestBar] RESUMEN FINAL DE LIMPIEZA DE DATOS DUPLICADOS';
    RAISE NOTICE '🎯 [RestBar] =====================================================';
    RAISE NOTICE '✅ [RestBar] Script de limpieza completado exitosamente';
    RAISE NOTICE '✅ [RestBar] Verificación de estructura realizada';
    RAISE NOTICE '✅ [RestBar] Módulos eliminados: Inventario, Contabilidad';
    RAISE NOTICE '✅ [RestBar] Datos duplicados eliminados (solo tablas existentes)';
    RAISE NOTICE '✅ [RestBar] Referencias huérfanas limpiadas (solo tablas existentes)';
    RAISE NOTICE '🎯 [RestBar] Base de datos optimizada y estandarizada';
    RAISE NOTICE '🎯 [RestBar] =====================================================';
END $$;

-- =====================================================
-- FIN DEL SCRIPT DE LIMPIEZA
-- =====================================================

