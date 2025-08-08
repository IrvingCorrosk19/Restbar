-- Script para inicializar el catálogo de cuentas básico para RestBar
-- Ejecutar después de crear las tablas de contabilidad

-- Limpiar datos existentes (opcional)
-- DELETE FROM journal_entry_details;
-- DELETE FROM journal_entries;
-- DELETE FROM accounts;

-- Insertar cuentas principales del catálogo

-- 1. ACTIVO (1000-1999)
INSERT INTO accounts (id, code, name, description, type, category, nature, is_active, is_system, created_at, created_by) VALUES
-- Activo Circulante
(gen_random_uuid(), '1101', 'Caja', 'Efectivo en caja', 1, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '1102', 'Bancos', 'Cuentas bancarias', 1, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '1201', 'Cuentas por Cobrar', 'Cuentas por cobrar a clientes', 1, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '1301', 'Inventario de Mercancías', 'Inventario de productos', 1, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '1401', 'Gastos Pagados por Anticipado', 'Gastos pagados por anticipado', 1, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),

-- Activo No Circulante
(gen_random_uuid(), '1501', 'Equipo de Cocina', 'Equipos de cocina', 1, 2, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '1502', 'Mobiliario y Equipos', 'Mobiliario y equipos de oficina', 1, 2, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '1503', 'Equipos de Computación', 'Computadoras y equipos informáticos', 1, 2, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '1601', 'Depreciación Acumulada', 'Depreciación acumulada de activos fijos', 1, 2, 2, true, true, CURRENT_TIMESTAMP, 'Sistema');

-- 2. PASIVO (2000-2999)
INSERT INTO accounts (id, code, name, description, type, category, nature, is_active, is_system, created_at, created_by) VALUES
-- Pasivo Circulante
(gen_random_uuid(), '2101', 'IVA por Cobrar', 'IVA cobrado a clientes', 2, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '2102', 'IVA por Pagar', 'IVA por pagar a proveedores', 2, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '2201', 'Cuentas por Pagar', 'Cuentas por pagar a proveedores', 2, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '2301', 'Impuestos por Pagar', 'Impuestos retenidos por pagar', 2, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '2401', 'Salarios por Pagar', 'Salarios pendientes de pago', 2, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '2501', 'Préstamos Bancarios', 'Préstamos bancarios a corto plazo', 2, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),

-- Pasivo No Circulante
(gen_random_uuid(), '2601', 'Préstamos Bancarios Largo Plazo', 'Préstamos bancarios a largo plazo', 2, 2, 2, true, true, CURRENT_TIMESTAMP, 'Sistema');

-- 3. CAPITAL (3000-3999)
INSERT INTO accounts (id, code, name, description, type, category, nature, is_active, is_system, created_at, created_by) VALUES
(gen_random_uuid(), '3101', 'Capital Social', 'Capital social de la empresa', 3, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '3201', 'Utilidades Retenidas', 'Utilidades retenidas de ejercicios anteriores', 3, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '3301', 'Utilidad del Ejercicio', 'Utilidad del ejercicio actual', 3, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema');

-- 4. INGRESOS (4000-4999)
INSERT INTO accounts (id, code, name, description, type, category, nature, is_active, is_system, created_at, created_by) VALUES
(gen_random_uuid(), '4101', 'Ventas de Comida', 'Ingresos por venta de comida', 4, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '4102', 'Ventas de Bebidas', 'Ingresos por venta de bebidas', 4, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '4103', 'Ventas de Postres', 'Ingresos por venta de postres', 4, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '4104', 'Servicios de Catering', 'Ingresos por servicios de catering', 4, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '4201', 'Otros Ingresos', 'Otros ingresos operativos', 4, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '4301', 'Ingresos Financieros', 'Ingresos por intereses y rendimientos', 4, 2, 2, true, true, CURRENT_TIMESTAMP, 'Sistema');

-- 5. GASTOS (5000-5999)
INSERT INTO accounts (id, code, name, description, type, category, nature, is_active, is_system, created_at, created_by) VALUES
-- Gastos de Operación
(gen_random_uuid(), '5101', 'Costo de Ventas - Comida', 'Costo de los ingredientes para comida', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5102', 'Costo de Ventas - Bebidas', 'Costo de las bebidas vendidas', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5103', 'Costo de Ventas - Postres', 'Costo de los postres vendidos', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5201', 'Gastos de Personal', 'Salarios y prestaciones del personal', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5202', 'Gastos de Renta', 'Renta del local', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5203', 'Servicios Públicos', 'Luz, agua, gas, teléfono', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5204', 'Gastos de Mantenimiento', 'Mantenimiento de equipos y local', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5205', 'Gastos de Limpieza', 'Productos de limpieza y servicios', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5206', 'Gastos de Marketing', 'Publicidad y promociones', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5207', 'Gastos Administrativos', 'Gastos de oficina y administración', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5208', 'Gastos de Seguridad', 'Seguridad y vigilancia', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5209', 'Gastos de Seguros', 'Pólizas de seguro', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5210', 'Gastos de Licencias', 'Licencias y permisos', 5, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),

-- Gastos No Operacionales
(gen_random_uuid(), '5301', 'Gastos Financieros', 'Intereses y comisiones bancarias', 5, 2, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '5401', 'Gastos Extraordinarios', 'Gastos no recurrentes', 5, 2, 1, true, true, CURRENT_TIMESTAMP, 'Sistema');

-- 6. CUENTAS DE ORDEN (6000-6999)
INSERT INTO accounts (id, code, name, description, type, category, nature, is_active, is_system, created_at, created_by) VALUES
(gen_random_uuid(), '6101', 'Compromisos', 'Compromisos adquiridos', 6, 1, 1, true, true, CURRENT_TIMESTAMP, 'Sistema'),
(gen_random_uuid(), '6201', 'Contingentes', 'Contingentes y garantías', 6, 1, 2, true, true, CURRENT_TIMESTAMP, 'Sistema');

-- Actualizar las fechas de creación
UPDATE accounts SET created_at = CURRENT_TIMESTAMP WHERE created_by = 'Sistema';

-- Verificar que se insertaron correctamente
SELECT COUNT(*) as total_accounts FROM accounts WHERE created_by = 'Sistema'; 