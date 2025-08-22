-- Script para insertar datos de prueba
-- Ejecutar este script en PostgreSQL si no hay datos

-- 1. Insertar compañías de prueba
INSERT INTO public.companies (id, name, legal_id, tax_id, address, phone, email, is_active, created_at, updated_at, created_by, updated_by)
VALUES 
    (gen_random_uuid(), 'RestBar Central', 'LEGAL001', 'TAX001', 'Calle Principal 123, Ciudad', '+507 123-4567', 'info@restbar.com', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema'),
    (gen_random_uuid(), 'RestBar Norte', 'LEGAL002', 'TAX002', 'Avenida Norte 456, Ciudad', '+507 234-5678', 'norte@restbar.com', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema'),
    (gen_random_uuid(), 'RestBar Sur', 'LEGAL003', 'TAX003', 'Calle Sur 789, Ciudad', '+507 345-6789', 'sur@restbar.com', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema')
ON CONFLICT (legal_id) DO NOTHING;

-- 2. Obtener los IDs de las compañías insertadas
DO $$
DECLARE
    company1_id uuid;
    company2_id uuid;
    company3_id uuid;
BEGIN
    -- Obtener IDs de las compañías
    SELECT id INTO company1_id FROM public.companies WHERE name = 'RestBar Central' LIMIT 1;
    SELECT id INTO company2_id FROM public.companies WHERE name = 'RestBar Norte' LIMIT 1;
    SELECT id INTO company3_id FROM public.companies WHERE name = 'RestBar Sur' LIMIT 1;

    -- 3. Insertar sucursales de prueba
    INSERT INTO public.branches (id, name, address, phone, company_id, is_active, created_at, updated_at, created_by, updated_by)
    VALUES 
        (gen_random_uuid(), 'Sucursal Centro', 'Calle Principal 123, Ciudad', '+507 123-4567', company1_id, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema'),
        (gen_random_uuid(), 'Sucursal Norte', 'Avenida Norte 456, Ciudad', '+507 234-5678', company2_id, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema'),
        (gen_random_uuid(), 'Sucursal Sur', 'Calle Sur 789, Ciudad', '+507 345-6789', company3_id, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema')
    ON CONFLICT DO NOTHING;

    -- 4. Obtener los IDs de las sucursales insertadas
    DECLARE
        branch1_id uuid;
        branch2_id uuid;
        branch3_id uuid;
    BEGIN
        SELECT id INTO branch1_id FROM public.branches WHERE name = 'Sucursal Centro' LIMIT 1;
        SELECT id INTO branch2_id FROM public.branches WHERE name = 'Sucursal Norte' LIMIT 1;
        SELECT id INTO branch3_id FROM public.branches WHERE name = 'Sucursal Sur' LIMIT 1;

        -- 5. Insertar áreas de prueba
        INSERT INTO public.areas (id, name, description, branch_id, company_id, created_at, updated_at, created_by, updated_by)
        VALUES 
            (gen_random_uuid(), 'Terraza', 'Área de terraza al aire libre', branch1_id, company1_id, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema'),
            (gen_random_uuid(), 'Sala Principal', 'Sala principal del restaurante', branch1_id, company1_id, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema'),
            (gen_random_uuid(), 'Bar', 'Área del bar', branch2_id, company2_id, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema'),
            (gen_random_uuid(), 'Cocina', 'Área de cocina', branch3_id, company3_id, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema')
        ON CONFLICT DO NOTHING;

        -- 6. Obtener los IDs de las áreas insertadas
        DECLARE
            area1_id uuid;
            area2_id uuid;
            area3_id uuid;
            area4_id uuid;
        BEGIN
            SELECT id INTO area1_id FROM public.areas WHERE name = 'Terraza' LIMIT 1;
            SELECT id INTO area2_id FROM public.areas WHERE name = 'Sala Principal' LIMIT 1;
            SELECT id INTO area3_id FROM public.areas WHERE name = 'Bar' LIMIT 1;
            SELECT id INTO area4_id FROM public.areas WHERE name = 'Cocina' LIMIT 1;

            -- 7. Insertar estaciones de prueba
            INSERT INTO public.stations (id, name, type, icon, area_id, company_id, branch_id, is_active, "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
            VALUES 
                (gen_random_uuid(), 'Cocina Principal', 'Cocina', 'fas fa-utensils', area4_id, company3_id, branch3_id, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema'),
                (gen_random_uuid(), 'Bar Principal', 'Bar', 'fas fa-glass-martini', area3_id, company2_id, branch2_id, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema'),
                (gen_random_uuid(), 'Café', 'Café', 'fas fa-coffee', area2_id, company1_id, branch1_id, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema'),
                (gen_random_uuid(), 'Postres', 'Postres', 'fas fa-birthday-cake', area1_id, company1_id, branch1_id, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema')
            ON CONFLICT DO NOTHING;
        END;
    END;
END $$;

-- 8. Verificar los datos insertados
SELECT 'COMPANIES INSERTED' as status, COUNT(*) as count FROM public.companies;
SELECT 'BRANCHES INSERTED' as status, COUNT(*) as count FROM public.branches;
SELECT 'AREAS INSERTED' as status, COUNT(*) as count FROM public.areas;
SELECT 'STATIONS INSERTED' as status, COUNT(*) as count FROM public.stations;

-- 9. Mostrar resumen de datos
SELECT 
    c.name as company_name,
    COUNT(DISTINCT b.id) as branches_count,
    COUNT(DISTINCT a.id) as areas_count,
    COUNT(DISTINCT s.id) as stations_count
FROM public.companies c
LEFT JOIN public.branches b ON c.id = b.company_id
LEFT JOIN public.areas a ON b.id = a.branch_id
LEFT JOIN public.stations s ON a.id = s.area_id
GROUP BY c.id, c.name
ORDER BY c.name;
