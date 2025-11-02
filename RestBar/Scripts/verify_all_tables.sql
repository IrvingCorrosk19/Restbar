-- Script para verificar campos NULL o faltantes en todas las tablas principales
-- Tablas CATÁLOGOS (sin CompanyId/BranchId - tienen su propia estructura)

-- 1. COMPANIES
SELECT 'COMPANIES' as tabla, 
       COUNT(*) FILTER (WHERE "CreatedAt" IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE "UpdatedAt" IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE "CreatedBy" IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE "UpdatedBy" IS NULL) as updated_by_null,
       COUNT(*) as total
FROM companies;

-- 2. BRANCHES (necesita CompanyId)
SELECT 'BRANCHES' as tabla,
       COUNT(*) FILTER (WHERE "CompanyId" IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE "CreatedAt" IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE "UpdatedAt" IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE "CreatedBy" IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE "UpdatedBy" IS NULL) as updated_by_null,
       COUNT(*) as total
FROM branches;

-- 3. AREAS (necesita CompanyId, BranchId)
SELECT 'AREAS' as tabla,
       COUNT(*) FILTER (WHERE "CompanyId" IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE "BranchId" IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE "CreatedAt" IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE "UpdatedAt" IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE "CreatedBy" IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE "UpdatedBy" IS NULL) as updated_by_null,
       COUNT(*) as total
FROM areas;

-- 4. TABLES (necesita CompanyId, BranchId, AreaId)
SELECT 'TABLES' as tabla,
       COUNT(*) FILTER (WHERE "CompanyId" IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE "BranchId" IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE "AreaId" IS NULL) as area_id_null,
       COUNT(*) FILTER (WHERE "CreatedAt" IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE "UpdatedAt" IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE "CreatedBy" IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE "UpdatedBy" IS NULL) as updated_by_null,
       COUNT(*) as total
FROM tables;

-- 5. CATEGORIES (necesita CompanyId, BranchId)
SELECT 'CATEGORIES' as tabla,
       COUNT(*) FILTER (WHERE "CompanyId" IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE "BranchId" IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE "CreatedAt" IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE "UpdatedAt" IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE "CreatedBy" IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE "UpdatedBy" IS NULL) as updated_by_null,
       COUNT(*) as total
FROM categories;

-- 6. STATIONS (necesita CompanyId, BranchId, AreaId)
SELECT 'STATIONS' as tabla,
       COUNT(*) FILTER (WHERE "CompanyId" IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE "BranchId" IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE "AreaId" IS NULL) as area_id_null,
       COUNT(*) FILTER (WHERE "CreatedAt" IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE "UpdatedAt" IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE "CreatedBy" IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE "UpdatedBy" IS NULL) as updated_by_null,
       COUNT(*) as total
FROM stations;

-- 7. PRODUCTS (necesita CompanyId, BranchId, CategoryId, StationId puede ser NULL)
SELECT 'PRODUCTS' as tabla,
       COUNT(*) FILTER (WHERE "CompanyId" IS NULL) as company_id_null,
       COUNT(*) FILTER (WHERE "BranchId" IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE "CategoryId" IS NULL) as category_id_null,
       COUNT(*) FILTER (WHERE "CreatedAt" IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE "UpdatedAt" IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE "CreatedBy" IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE "UpdatedBy" IS NULL) as updated_by_null,
       COUNT(*) as total
FROM products;

-- 8. USERS (necesita BranchId)
SELECT 'USERS' as tabla,
       COUNT(*) FILTER (WHERE "BranchId" IS NULL) as branch_id_null,
       COUNT(*) FILTER (WHERE "CreatedAt" IS NULL) as created_at_null,
       COUNT(*) FILTER (WHERE "UpdatedAt" IS NULL) as updated_at_null,
       COUNT(*) FILTER (WHERE "CreatedBy" IS NULL) as created_by_null,
       COUNT(*) FILTER (WHERE "UpdatedBy" IS NULL) as updated_by_null,
       COUNT(*) as total
FROM users;

-- 9. ORDERS (necesita OrderNumber, CompanyId, BranchId)
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

