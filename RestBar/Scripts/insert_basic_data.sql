-- Script para insertar datos básicos de prueba
-- Ejecutar en PostgreSQL si no hay datos

-- Insertar una compañía de prueba
INSERT INTO public.companies (id, name, legal_id, tax_id, address, phone, email, is_active, created_at, updated_at, created_by, updated_by)
VALUES 
    (gen_random_uuid(), 'RestBar Central', 'LEGAL001', 'TAX001', 'Calle Principal 123, Ciudad', '+507 123-4567', 'info@restbar.com', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Sistema', 'Sistema')
ON CONFLICT DO NOTHING;

-- Insertar una sucursal de prueba
INSERT INTO public.branches (id, name, address, phone, email, company_id, is_active, created_at, updated_at, created_by, updated_by)
SELECT 
    gen_random_uuid(), 
    'Sucursal Principal', 
    'Calle Principal 123, Ciudad', 
    '+507 123-4567', 
    'sucursal@restbar.com', 
    c.id, 
    true, 
    CURRENT_TIMESTAMP, 
    CURRENT_TIMESTAMP, 
    'Sistema', 
    'Sistema'
FROM public.companies c 
WHERE c.name = 'RestBar Central'
LIMIT 1
ON CONFLICT DO NOTHING;

-- Verificar que se insertaron los datos
SELECT 'COMPANIES' as tabla, COUNT(*) as total FROM public.companies;
SELECT 'BRANCHES' as tabla, COUNT(*) as total FROM public.branches;
