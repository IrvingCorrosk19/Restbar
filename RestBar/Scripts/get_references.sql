-- Obtener IDs de referencia para completar la orden
SELECT 'Company' as type, id, name FROM companies LIMIT 1;
SELECT 'Branch' as type, id, name FROM branches LIMIT 1;
SELECT 'Customer' as type, id, "FullName" as name FROM customers LIMIT 1;
SELECT 'Table' as type, id, "TableNumber" as name FROM tables LIMIT 1;

