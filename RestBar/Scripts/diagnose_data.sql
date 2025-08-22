-- Script de diagnóstico para verificar datos en las tablas principales
-- Ejecutar este script en PostgreSQL para diagnosticar problemas

-- 1. Verificar datos en companies
SELECT 
    'COMPANIES' as tabla,
    COUNT(*) as total_registros,
    COUNT(CASE WHEN is_active = true THEN 1 END) as activos,
    COUNT(CASE WHEN is_active = false THEN 1 END) as inactivos
FROM public.companies;

-- 2. Mostrar todas las compañías
SELECT 
    id,
    name,
    legal_id,
    is_active,
    created_at,
    updated_at
FROM public.companies
ORDER BY name;

-- 3. Verificar datos en branches
SELECT 
    'BRANCHES' as tabla,
    COUNT(*) as total_registros,
    COUNT(CASE WHEN is_active = true THEN 1 END) as activos,
    COUNT(CASE WHEN is_active = false THEN 1 END) as inactivos
FROM public.branches;

-- 4. Mostrar todas las sucursales con su compañía
SELECT 
    b.id,
    b.name as branch_name,
    c.name as company_name,
    b.is_active,
    b.created_at
FROM public.branches b
LEFT JOIN public.companies c ON b.company_id = c.id
ORDER BY c.name, b.name;

-- 5. Verificar datos en areas
SELECT 
    'AREAS' as tabla,
    COUNT(*) as total_registros
FROM public.areas;

-- 6. Mostrar todas las áreas con su compañía y sucursal
SELECT 
    a.id,
    a.name as area_name,
    c.name as company_name,
    b.name as branch_name,
    a.created_at
FROM public.areas a
LEFT JOIN public.companies c ON a.company_id = c.id
LEFT JOIN public.branches b ON a.branch_id = b.id
ORDER BY c.name, b.name, a.name;

-- 7. Verificar datos en stations
SELECT 
    'STATIONS' as tabla,
    COUNT(*) as total_registros,
    COUNT(CASE WHEN is_active = true THEN 1 END) as activos,
    COUNT(CASE WHEN is_active = false THEN 1 END) as inactivos
FROM public.stations;

-- 8. Mostrar todas las estaciones con su área, compañía y sucursal
SELECT 
    s.id,
    s.name as station_name,
    s.type,
    a.name as area_name,
    c.name as company_name,
    b.name as branch_name,
    s.is_active,
    s.created_at
FROM public.stations s
LEFT JOIN public.areas a ON s.area_id = a.id
LEFT JOIN public.companies c ON s.company_id = c.id
LEFT JOIN public.branches b ON s.branch_id = b.id
ORDER BY c.name, b.name, a.name, s.name;

-- 9. Verificar usuarios con sus compañías y sucursales
SELECT 
    'USERS' as tabla,
    COUNT(*) as total_registros,
    COUNT(CASE WHEN is_active = true THEN 1 END) as activos,
    COUNT(CASE WHEN is_active = false THEN 1 END) as inactivos
FROM public.users;

-- 10. Mostrar usuarios con sus compañías y sucursales
SELECT 
    u.id,
    u.full_name,
    u.email,
    u.role,
    c.name as company_name,
    b.name as branch_name,
    u.is_active,
    u.created_at
FROM public.users u
LEFT JOIN public.companies c ON u.company_id = c.id
LEFT JOIN public.branches b ON u.branch_id = b.id
ORDER BY c.name, b.name, u.full_name;

-- 11. Verificar si hay compañías sin sucursales
SELECT 
    'COMPANIES WITHOUT BRANCHES' as issue,
    c.id,
    c.name,
    COUNT(b.id) as branch_count
FROM public.companies c
LEFT JOIN public.branches b ON c.id = b.company_id
GROUP BY c.id, c.name
HAVING COUNT(b.id) = 0;

-- 12. Verificar si hay sucursales sin compañía
SELECT 
    'BRANCHES WITHOUT COMPANY' as issue,
    b.id,
    b.name,
    b.company_id
FROM public.branches b
WHERE b.company_id IS NULL;

-- 13. Verificar si hay áreas sin compañía o sucursal
SELECT 
    'AREAS WITHOUT COMPANY OR BRANCH' as issue,
    a.id,
    a.name,
    a.company_id,
    a.branch_id
FROM public.areas a
WHERE a.company_id IS NULL OR a.branch_id IS NULL;
