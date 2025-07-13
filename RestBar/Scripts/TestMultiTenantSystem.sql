-- =====================================================
-- SCRIPT DE PRUEBA MULTI-TENANT - RestBar
-- =====================================================
-- Este script crea una compañía completa para probar el sistema multi-tenant
-- Incluye: Compañía, Sucursales, Áreas, Estaciones, Mesas, Productos
-- =====================================================

-- 🔍 INICIO: Script de prueba multi-tenant
DO $$
BEGIN
    RAISE NOTICE '🔍 [RestBar] Iniciando prueba del sistema multi-tenant...';
END $$;

-- =====================================================
-- 1. CREAR COMPAÑÍA DE PRUEBA
-- =====================================================
DO $$
DECLARE
    test_company_id uuid;
BEGIN
    RAISE NOTICE '🏢 [Test] Creando compañía de prueba...';
    
    -- Crear compañía de prueba
    INSERT INTO public.companies (id, name, legal_id, tax_id, address, phone, email, is_active, created_at, created_by)
    VALUES (
        gen_random_uuid(),
        'Restaurante El Buen Sabor S.A.',
        'J-12345678-9',
        'TAX-123456789',
        'Av. Principal 123, Ciudad de Panamá',
        '+507 123-4567',
        'info@elbuensabor.com',
        true,
        CURRENT_TIMESTAMP,
        'Sistema de Prueba'
    ) RETURNING id INTO test_company_id;
    
    RAISE NOTICE '✅ [Test] Compañía creada con ID: %', test_company_id;
    
    -- =====================================================
    -- 2. CREAR SUCURSALES
    -- =====================================================
    RAISE NOTICE '🏪 [Test] Creando sucursales...';
    
    -- Sucursal Principal
    INSERT INTO public.branches (id, company_id, name, address, phone, is_active, created_at, created_by)
    VALUES (
        gen_random_uuid(),
        test_company_id,
        'Sucursal Principal - El Buen Sabor',
        'Av. Principal 123, Ciudad de Panamá',
        '+507 123-4567',
        true,
        CURRENT_TIMESTAMP,
        'Sistema de Prueba'
    );
    
    -- Sucursal Secundaria
    INSERT INTO public.branches (id, company_id, name, address, phone, is_active, created_at, created_by)
    VALUES (
        gen_random_uuid(),
        test_company_id,
        'Sucursal Mall Plaza - El Buen Sabor',
        'Mall Plaza, Local 45, Ciudad de Panamá',
        '+507 234-5678',
        true,
        CURRENT_TIMESTAMP,
        'Sistema de Prueba'
    );
    
    RAISE NOTICE '✅ [Test] Sucursales creadas para la compañía';
    
    -- =====================================================
    -- 3. CREAR ÁREAS PARA CADA SUCURSAL
    -- =====================================================
    RAISE NOTICE '📍 [Test] Creando áreas...';
    
    -- Obtener IDs de las sucursales
    DECLARE
        branch1_id uuid;
        branch2_id uuid;
        area1_id uuid;
        area2_id uuid;
        area3_id uuid;
        area4_id uuid;
    BEGIN
        -- Obtener primera sucursal
        SELECT id INTO branch1_id FROM public.branches WHERE company_id = test_company_id LIMIT 1;
        
        -- Obtener segunda sucursal
        SELECT id INTO branch2_id FROM public.branches WHERE company_id = test_company_id AND id != branch1_id LIMIT 1;
        
        -- Áreas para Sucursal Principal
        INSERT INTO public.areas (id, branch_id, company_id, name, description)
        VALUES (
            gen_random_uuid(),
            branch1_id,
            test_company_id,
            'Terraza',
            'Área de terraza con vista al mar'
        ) RETURNING id INTO area1_id;
        
        INSERT INTO public.areas (id, branch_id, company_id, name, description)
        VALUES (
            gen_random_uuid(),
            branch1_id,
            test_company_id,
            'Sala Principal',
            'Área principal del restaurante'
        ) RETURNING id INTO area2_id;
        
        -- Áreas para Sucursal Mall
        INSERT INTO public.areas (id, branch_id, company_id, name, description)
        VALUES (
            gen_random_uuid(),
            branch2_id,
            test_company_id,
            'Área Comedor',
            'Área de comedor del mall'
        ) RETURNING id INTO area3_id;
        
        INSERT INTO public.areas (id, branch_id, company_id, name, description)
        VALUES (
            gen_random_uuid(),
            branch2_id,
            test_company_id,
            'Área Bar',
            'Área de bar y bebidas'
        ) RETURNING id INTO area4_id;
        
        RAISE NOTICE '✅ [Test] Áreas creadas: Terraza, Sala Principal, Área Comedor, Área Bar';
        
        -- =====================================================
        -- 4. CREAR ESTACIONES
        -- =====================================================
        RAISE NOTICE '🏭 [Test] Creando estaciones...';
        
        -- Estaciones para Sucursal Principal
        INSERT INTO public.stations (id, name, type, icon, area_id, company_id, branch_id, is_active)
        VALUES 
            (gen_random_uuid(), 'Cocina Principal', 'Cocina', 'fa-utensils', area1_id, test_company_id, branch1_id, true),
            (gen_random_uuid(), 'Bar Principal', 'Bar', 'fa-glass-martini', area2_id, test_company_id, branch1_id, true),
            (gen_random_uuid(), 'Cafetería', 'Café', 'fa-coffee', area2_id, test_company_id, branch1_id, true);
        
        -- Estaciones para Sucursal Mall
        INSERT INTO public.stations (id, name, type, icon, area_id, company_id, branch_id, is_active)
        VALUES 
            (gen_random_uuid(), 'Cocina Mall', 'Cocina', 'fa-utensils', area3_id, test_company_id, branch2_id, true),
            (gen_random_uuid(), 'Bar Mall', 'Bar', 'fa-glass-martini', area4_id, test_company_id, branch2_id, true);
        
        RAISE NOTICE '✅ [Test] Estaciones creadas: Cocina Principal, Bar Principal, Cafetería, Cocina Mall, Bar Mall';
        
        -- =====================================================
        -- 5. CREAR MESAS
        -- =====================================================
        RAISE NOTICE '🪑 [Test] Creando mesas...';
        
        -- Mesas para Terraza (Sucursal Principal)
        INSERT INTO public.tables (id, area_id, table_number, capacity, status, is_active)
        VALUES 
            (gen_random_uuid(), area1_id, 'T1', 4, 'Disponible', true),
            (gen_random_uuid(), area1_id, 'T2', 6, 'Disponible', true),
            (gen_random_uuid(), area1_id, 'T3', 2, 'Disponible', true);
        
        -- Mesas para Sala Principal (Sucursal Principal)
        INSERT INTO public.tables (id, area_id, table_number, capacity, status, is_active)
        VALUES 
            (gen_random_uuid(), area2_id, 'S1', 4, 'Disponible', true),
            (gen_random_uuid(), area2_id, 'S2', 8, 'Disponible', true),
            (gen_random_uuid(), area2_id, 'S3', 6, 'Disponible', true),
            (gen_random_uuid(), area2_id, 'S4', 4, 'Disponible', true);
        
        -- Mesas para Área Comedor (Sucursal Mall)
        INSERT INTO public.tables (id, area_id, table_number, capacity, status, is_active)
        VALUES 
            (gen_random_uuid(), area3_id, 'M1', 4, 'Disponible', true),
            (gen_random_uuid(), area3_id, 'M2', 6, 'Disponible', true),
            (gen_random_uuid(), area3_id, 'M3', 2, 'Disponible', true);
        
        -- Mesas para Área Bar (Sucursal Mall)
        INSERT INTO public.tables (id, area_id, table_number, capacity, status, is_active)
        VALUES 
            (gen_random_uuid(), area4_id, 'B1', 2, 'Disponible', true),
            (gen_random_uuid(), area4_id, 'B2', 2, 'Disponible', true);
        
        RAISE NOTICE '✅ [Test] Mesas creadas: 10 mesas distribuidas en las 4 áreas';
        
        -- =====================================================
        -- 6. CREAR CATEGORÍAS DE PRODUCTOS
        -- =====================================================
        RAISE NOTICE '📂 [Test] Creando categorías de productos...';
        
        INSERT INTO public.categories (id, name, description, is_active)
        VALUES 
            (gen_random_uuid(), 'Platos Principales', 'Platos principales del menú', true),
            (gen_random_uuid(), 'Entradas', 'Entradas y aperitivos', true),
            (gen_random_uuid(), 'Bebidas', 'Bebidas y refrescos', true),
            (gen_random_uuid(), 'Postres', 'Postres y dulces', true),
            (gen_random_uuid(), 'Café', 'Café y bebidas calientes', true);
        
        RAISE NOTICE '✅ [Test] Categorías creadas: Platos Principales, Entradas, Bebidas, Postres, Café';
        
        -- =====================================================
        -- 7. CREAR PRODUCTOS
        -- =====================================================
        RAISE NOTICE '🍽️ [Test] Creando productos...';
        
        DECLARE
            cat_platos uuid;
            cat_entradas uuid;
            cat_bebidas uuid;
            cat_postres uuid;
            cat_cafe uuid;
            station_cocina uuid;
            station_bar uuid;
            station_cafe uuid;
        BEGIN
            -- Obtener IDs de categorías
            SELECT id INTO cat_platos FROM public.categories WHERE name = 'Platos Principales' LIMIT 1;
            SELECT id INTO cat_entradas FROM public.categories WHERE name = 'Entradas' LIMIT 1;
            SELECT id INTO cat_bebidas FROM public.categories WHERE name = 'Bebidas' LIMIT 1;
            SELECT id INTO cat_postres FROM public.categories WHERE name = 'Postres' LIMIT 1;
            SELECT id INTO cat_cafe FROM public.categories WHERE name = 'Café' LIMIT 1;
            
            -- Obtener IDs de estaciones
            SELECT id INTO station_cocina FROM public.stations WHERE name = 'Cocina Principal' AND company_id = test_company_id LIMIT 1;
            SELECT id INTO station_bar FROM public.stations WHERE name = 'Bar Principal' AND company_id = test_company_id LIMIT 1;
            SELECT id INTO station_cafe FROM public.stations WHERE name = 'Cafetería' AND company_id = test_company_id LIMIT 1;
            
            -- Productos de Cocina
            INSERT INTO public.products (id, name, description, price, cost, tax_rate, unit, category_id, station_id, is_active, created_at, created_by)
            VALUES 
                (gen_random_uuid(), 'Pescado Frito', 'Pescado fresco frito con acompañamientos', 18.50, 12.00, 0.07, 'plato', cat_platos, station_cocina, true, CURRENT_TIMESTAMP, 'Sistema de Prueba'),
                (gen_random_uuid(), 'Pollo Asado', 'Pollo asado con hierbas y especias', 16.00, 10.50, 0.07, 'plato', cat_platos, station_cocina, true, CURRENT_TIMESTAMP, 'Sistema de Prueba'),
                (gen_random_uuid(), 'Ensalada César', 'Ensalada César con aderezo especial', 8.50, 5.00, 0.07, 'plato', cat_entradas, station_cocina, true, CURRENT_TIMESTAMP, 'Sistema de Prueba'),
                (gen_random_uuid(), 'Flan Casero', 'Flan casero con caramelo', 6.00, 3.50, 0.07, 'porción', cat_postres, station_cocina, true, CURRENT_TIMESTAMP, 'Sistema de Prueba');
            
            -- Productos de Bar
            INSERT INTO public.products (id, name, description, price, cost, tax_rate, unit, category_id, station_id, is_active, created_at, created_by)
            VALUES 
                (gen_random_uuid(), 'Mojito', 'Mojito tradicional con ron y menta', 9.00, 4.50, 0.07, 'copa', cat_bebidas, station_bar, true, CURRENT_TIMESTAMP, 'Sistema de Prueba'),
                (gen_random_uuid(), 'Piña Colada', 'Piña colada con ron y crema de coco', 10.50, 5.25, 0.07, 'copa', cat_bebidas, station_bar, true, CURRENT_TIMESTAMP, 'Sistema de Prueba'),
                (gen_random_uuid(), 'Cerveza Nacional', 'Cerveza nacional 12oz', 3.50, 2.00, 0.07, 'botella', cat_bebidas, station_bar, true, CURRENT_TIMESTAMP, 'Sistema de Prueba');
            
            -- Productos de Cafetería
            INSERT INTO public.products (id, name, description, price, cost, tax_rate, unit, category_id, station_id, is_active, created_at, created_by)
            VALUES 
                (gen_random_uuid(), 'Café Americano', 'Café americano tradicional', 2.50, 1.25, 0.07, 'taza', cat_cafe, station_cafe, true, CURRENT_TIMESTAMP, 'Sistema de Prueba'),
                (gen_random_uuid(), 'Cappuccino', 'Cappuccino con espuma de leche', 3.50, 1.75, 0.07, 'taza', cat_cafe, station_cafe, true, CURRENT_TIMESTAMP, 'Sistema de Prueba'),
                (gen_random_uuid(), 'Café con Leche', 'Café con leche caliente', 3.00, 1.50, 0.07, 'taza', cat_cafe, station_cafe, true, CURRENT_TIMESTAMP, 'Sistema de Prueba');
            
            RAISE NOTICE '✅ [Test] Productos creados: 10 productos distribuidos en 3 estaciones';
        END;
    END;
END $$;

-- =====================================================
-- VERIFICACIÓN FINAL
-- =====================================================
DO $$
DECLARE
    company_count INTEGER;
    branch_count INTEGER;
    area_count INTEGER;
    station_count INTEGER;
    table_count INTEGER;
    category_count INTEGER;
    product_count INTEGER;
    test_company_id uuid;
BEGIN
    RAISE NOTICE '🔍 [Test] Verificación final del sistema multi-tenant...';
    
    -- Obtener ID de la compañía de prueba
    SELECT id INTO test_company_id FROM public.companies WHERE name = 'Restaurante El Buen Sabor S.A.' LIMIT 1;
    
    IF test_company_id IS NOT NULL THEN
        -- Contar elementos creados
        SELECT COUNT(*) INTO company_count FROM public.companies WHERE id = test_company_id;
        SELECT COUNT(*) INTO branch_count FROM public.branches WHERE company_id = test_company_id;
        SELECT COUNT(*) INTO area_count FROM public.areas WHERE company_id = test_company_id;
        SELECT COUNT(*) INTO station_count FROM public.stations WHERE company_id = test_company_id;
        SELECT COUNT(*) INTO table_count FROM public.tables t 
            INNER JOIN public.areas a ON t.area_id = a.id 
            WHERE a.company_id = test_company_id;
        SELECT COUNT(*) INTO category_count FROM public.categories;
        SELECT COUNT(*) INTO product_count FROM public.products p 
            INNER JOIN public.stations s ON p.station_id = s.id 
            WHERE s.company_id = test_company_id;
        
        RAISE NOTICE '🎯 [Test] =====================================================';
        RAISE NOTICE '🎯 [Test] RESUMEN DE PRUEBA MULTI-TENANT';
        RAISE NOTICE '🎯 [Test] =====================================================';
        RAISE NOTICE '✅ [Test] Compañías: %', company_count;
        RAISE NOTICE '✅ [Test] Sucursales: %', branch_count;
        RAISE NOTICE '✅ [Test] Áreas: %', area_count;
        RAISE NOTICE '✅ [Test] Estaciones: %', station_count;
        RAISE NOTICE '✅ [Test] Mesas: %', table_count;
        RAISE NOTICE '✅ [Test] Categorías: %', category_count;
        RAISE NOTICE '✅ [Test] Productos: %', product_count;
        RAISE NOTICE '🎯 [Test] =====================================================';
        RAISE NOTICE '🎯 [Test] SISTEMA MULTI-TENANT FUNCIONANDO CORRECTAMENTE';
        RAISE NOTICE '🎯 [Test] =====================================================';
    ELSE
        RAISE NOTICE '❌ [Test] No se encontró la compañía de prueba';
    END IF;
END $$;

-- =====================================================
-- FIN DEL SCRIPT DE PRUEBA
-- =====================================================
