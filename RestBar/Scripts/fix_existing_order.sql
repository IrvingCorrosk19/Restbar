-- Actualizar la orden existente para completar los campos faltantes
UPDATE orders
SET 
    "OrderNumber" = COALESCE("OrderNumber", '000001'),
    "CompanyId" = COALESCE("CompanyId", '770e8400-e29b-41d4-a716-446655440001'::uuid),
    "BranchId" = COALESCE("BranchId", '660e8400-e29b-41d4-a716-446655440001'::uuid),
    "CreatedAt" = COALESCE("CreatedAt", CURRENT_TIMESTAMP),
    "UpdatedAt" = COALESCE("UpdatedAt", CURRENT_TIMESTAMP),
    "CreatedBy" = COALESCE("CreatedBy", 'admin@restbar.com'),
    "UpdatedBy" = COALESCE("UpdatedBy", 'admin@restbar.com')
WHERE id = '715a76e2-d88a-4558-9f99-33a8e56e1ed8'::uuid;
