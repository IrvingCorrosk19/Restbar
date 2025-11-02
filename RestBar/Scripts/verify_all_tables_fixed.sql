-- Script para verificar campos NULL o faltantes en todas las tablas principales
-- Usando nombres de columna correctos (snake_case)

-- 1. COMPANIES
SELECT 'COMPANIES' as tabla, 
       COUNT(*) FILTER (WHERE created_at IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE updated_at IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE created_by IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE updated_by IS NULL) as updated_by_null,
       COUNT(*) as total
FROM companies;

-- 2. BRANCHES (necesita company_id)
SELECT 'BRANCHES' as tabla,
       COUNT(*) FILTER (WHERE company_id IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE created_at IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE updated_at IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE created_by IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE updated_by IS NULL) as updated_by_null,
       COUNT(*) as total
FROM branches;

-- 3. AREAS (necesita company_id, branch_id)
SELECT 'AREAS' as tabla,
       COUNT(*) FILTER (WHERE company_id IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE branch_id IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE created_at IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE updated_at IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE created_by IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE updated_by IS NULL) as updated_by_null,
       COUNT(*) as total
FROM areas;

-- 4. TABLES (necesita company_id, branch_id, area_id)
SELECT 'TABLES' as tabla,
       COUNT(*) FILTER (WHERE company_id IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE branch_id IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE area_id IS NULL) as area_id_null,
       COUNT(*) FILTER (WHERE created_at IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE updated_at IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE created_by IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE updated_by IS NULL) as updated_by_null,
       COUNT(*) as total
FROM tables;

-- 5. CATEGORIES (necesita company_id, branch_id)
SELECT 'CATEGORIES' as tabla,
       COUNT(*) FILTER (WHERE company_id IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE branch_id IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE created_at IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE updated_at IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE created_by IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE updated_by IS NULL) as updated_by_null,
       COUNT(*) as total
FROM categories;

-- 6. STATIONS (necesita company_id, branch_id, area_id)
SELECT 'STATIONS' as tabla,
       COUNT(*) FILTER (WHERE company_id IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE branch_id IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE area_id IS NULL) as area_id_null,
       COUNT(*) FILTER (WHERE created_at IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE updated_at IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE created_by IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE updated_by IS NULL) as updated_by_null,
       COUNT(*) as total
FROM stations;

-- 7. PRODUCTS (necesita company_id, branch_id, category_id, station_id puede ser NULL)
SELECT 'PRODUCTS' as tabla,
       COUNT(*) FILTER (WHERE company_id IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE branch_id IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE category_id IS NULL) as category_id_null,
       COUNT(*) FILTER (WHERE created_at IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE updated_at IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE created_by IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE updated_by IS NULL) as updated_by_null,
       COUNT(*) as total
FROM products;

-- 8. USERS (necesita branch_id)
SELECT 'USERS' as tabla,
       COUNT(*) FILTER (WHERE branch_id IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE created_at IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE updated_at IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE created_by IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE updated_by IS NULL) as updated_by_null,
       COUNT(*) as total
FROM users;

-- 9. ORDERS (necesita "OrderNumber", company_id, branch_id)
SELECT 'ORDERS' as tabla,
       COUNT(*) FILTER (WHERE "OrderNumber" IS NULL OR "OrderNumber" = '') as order_number_null,
       COUNT(*) FILTER (WHERE "CompanyId" IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE "BranchId" IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE "CreatedAt" IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE "UpdatedAt" IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE "CreatedBy" IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE "UpdatedBy" IS NULL) as updated_by_null,
       COUNT(*) as total
FROM orders;

