-- Script simple para verificar datos en las tablas
-- Ejecutar en PostgreSQL

-- Verificar companies
SELECT 'COMPANIES' as tabla, COUNT(*) as total FROM public.companies;

-- Verificar branches  
SELECT 'BRANCHES' as tabla, COUNT(*) as total FROM public.branches;

-- Mostrar algunas compañías si existen
SELECT id, name, is_active FROM public.companies LIMIT 5;

-- Mostrar algunas sucursales si existen
SELECT id, name, company_id, is_active FROM public.branches LIMIT 5;
