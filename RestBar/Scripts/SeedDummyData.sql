-- Script de seeding para crear datos dummy completos
-- Ejecutar: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d RestBar -f Scripts/SeedDummyData.sql

BEGIN;

-- 1. COMPAÑÍA
INSERT INTO companies (id, name, legal_id, is_active, created_at, updated_at, created_by)
VALUES ('770e8400-e29b-41d4-a716-446655440001', 'RestBar Principal', '123456789', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder')
ON CONFLICT (id) DO NOTHING;

-- 2. SUCURSAL
INSERT INTO branches (id, company_id, name, address, phone, is_active, created_at, "UpdatedAt", "CreatedBy")
VALUES ('660e8400-e29b-41d4-a716-446655440001', '770e8400-e29b-41d4-a716-446655440001', 'RestBar Centro', 'Calle Principal #123', '+507 123-4567', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder')
ON CONFLICT (id) DO NOTHING;

-- 3. ÁREAS
DO $$
DECLARE
    area_salon_id uuid;
    area_terraza_id uuid;
BEGIN
    -- Crear áreas
    INSERT INTO areas (id, company_id, branch_id, name, description, is_active, created_at, updated_at, created_by)
    VALUES 
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 'Salón', 'Área principal', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 'Terraza', 'Área exterior', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder')
    ON CONFLICT DO NOTHING;

    -- Obtener IDs de áreas
    SELECT id INTO area_salon_id FROM areas WHERE name = 'Salón' AND company_id = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;
    SELECT id INTO area_terraza_id FROM areas WHERE name = 'Terraza' AND company_id = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;

    -- 4. ESTACIONES
    INSERT INTO stations (id, company_id, branch_id, area_id, name, type, icon, is_active, "CreatedAt", "UpdatedAt", "CreatedBy")
    VALUES 
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_salon_id, 'Cocina Principal', 'kitchen', '🍽️', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_salon_id, 'Bar Principal', 'bar', '🍹', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder')
    ON CONFLICT DO NOTHING;

    -- 5. MESAS
    INSERT INTO tables (id, company_id, branch_id, area_id, table_number, capacity, status, is_active, created_at, updated_at, created_by)
    VALUES 
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_salon_id, 'T-01', 4, 'disponible', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_salon_id, 'T-02', 4, 'disponible', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_terraza_id, 'T-03', 4, 'disponible', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_salon_id, 'T-04', 4, 'disponible', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_terraza_id, 'T-05', 4, 'disponible', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_salon_id, 'T-06', 6, 'disponible', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_terraza_id, 'T-07', 6, 'disponible', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_salon_id, 'T-08', 4, 'disponible', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_terraza_id, 'T-09', 4, 'disponible', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', area_salon_id, 'T-10', 8, 'disponible', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder')
    ON CONFLICT DO NOTHING;

END $$;

    -- 6. CATEGORÍAS
    INSERT INTO categories (id, "CompanyId", branch_id, name, description, is_active, "CreatedAt", "UpdatedAt", "CreatedBy")
    VALUES 
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 'Bebidas', 'Bebidas no alcohólicas', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 'Platos', 'Platos principales', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 'Postres', 'Postres y dulces', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 'Bebidas Alcohólicas', 'Bebidas alcohólicas', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder')
    ON CONFLICT DO NOTHING;

    -- 7. PRODUCTOS
    DO $$
    DECLARE
        cat_bebidas_id uuid;
        cat_platos_id uuid;
        cat_postres_id uuid;
        cat_alcohol_id uuid;
        station_kitchen_id uuid;
        station_bar_id uuid;
    BEGIN
        -- Obtener IDs
        SELECT id INTO cat_bebidas_id FROM categories WHERE name = 'Bebidas' AND "CompanyId" = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;
        SELECT id INTO cat_platos_id FROM categories WHERE name = 'Platos' AND "CompanyId" = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;
        SELECT id INTO cat_postres_id FROM categories WHERE name = 'Postres' AND "CompanyId" = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;
        SELECT id INTO cat_alcohol_id FROM categories WHERE name = 'Bebidas Alcohólicas' AND "CompanyId" = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;
        SELECT id INTO station_kitchen_id FROM stations WHERE name = 'Cocina Principal' AND company_id = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;
        SELECT id INTO station_bar_id FROM stations WHERE name = 'Bar Principal' AND company_id = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;

    -- Insertar productos
    INSERT INTO products (id, company_id, branch_id, category_id, station_id, name, description, price, tax_rate, unit, is_active, "CreatedAt", "UpdatedAt", "CreatedBy")
    VALUES 
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', cat_bebidas_id, station_bar_id, 'Café Americano', 'Café americano caliente', 2.50, 0.07, 'unit', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', cat_bebidas_id, station_bar_id, 'Jugo de Naranja', 'Jugo natural de naranja', 3.00, 0.07, 'unit', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', cat_platos_id, station_kitchen_id, 'Hamburguesa Clásica', 'Hamburguesa con papas', 8.99, 0.07, 'unit', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', cat_platos_id, station_kitchen_id, 'Pasta Alfredo', 'Pasta con salsa alfredo', 9.99, 0.07, 'unit', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', cat_postres_id, station_kitchen_id, 'Tiramisú', 'Postre italiano', 6.50, 0.07, 'unit', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', cat_postres_id, station_kitchen_id, 'Brownie con Helado', 'Brownie caliente con helado', 7.00, 0.07, 'unit', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', cat_alcohol_id, station_bar_id, 'Cerveza Nacional', 'Cerveza local', 4.50, 0.07, 'unit', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
        (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', cat_alcohol_id, station_bar_id, 'Cóctel Mojito', 'Mojito clásico', 8.00, 0.07, 'unit', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder')
    ON CONFLICT DO NOTHING;
END $$;

-- 8. USUARIOS (con hash SHA256 de "123456" en base64)
-- Hash SHA256 de "123456" en base64: "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI="
INSERT INTO users (id, branch_id, full_name, email, password_hash, role, is_active, created_at, updated_at, created_by)
VALUES 
    ('770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 'Administrador del Sistema', 'admin@restbar.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'admin', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
    (gen_random_uuid(), '660e8400-e29b-41d4-a716-446655440001', 'Gerente General', 'gerente@restbar.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'manager', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
    (gen_random_uuid(), '660e8400-e29b-41d4-a716-446655440001', 'Supervisor de Turno', 'supervisor@restbar.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'supervisor', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
    (gen_random_uuid(), '660e8400-e29b-41d4-a716-446655440001', 'Mesero Principal', 'mesero@restbar.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'waiter', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
    (gen_random_uuid(), '660e8400-e29b-41d4-a716-446655440001', 'Cajero Principal', 'cajero@restbar.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'cashier', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
    (gen_random_uuid(), '660e8400-e29b-41d4-a716-446655440001', 'Chef Principal', 'chef@restbar.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'chef', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
    (gen_random_uuid(), '660e8400-e29b-41d4-a716-446655440001', 'Bartender Principal', 'bartender@restbar.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'bartender', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
    (gen_random_uuid(), '660e8400-e29b-41d4-a716-446655440001', 'Contador', 'contador@restbar.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'accountant', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder'),
    (gen_random_uuid(), '660e8400-e29b-41d4-a716-446655440001', 'Soporte Técnico', 'soporte@restbar.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'support', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder')
ON CONFLICT (email) DO NOTHING;

-- 9. CLIENTE DE PRUEBA
INSERT INTO customers (id, "CompanyId", "BranchId", full_name, email, phone, loyalty_points, created_at, "UpdatedAt", "CreatedBy")
VALUES (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 'Cliente Demo', 'cliente@example.com', '+507 123-4567', 0, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder')
ON CONFLICT DO NOTHING;

-- 10. ASIGNACIONES DE USUARIOS
DO $$
DECLARE
    mesero_id uuid;
    mesa1_id uuid;
    mesa2_id uuid;
    area_salon_id uuid;
BEGIN
    SELECT id INTO mesero_id FROM users WHERE email = 'mesero@restbar.com' LIMIT 1;
    SELECT id INTO mesa1_id FROM tables WHERE table_number = 'T-01' AND company_id = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;
    SELECT id INTO mesa2_id FROM tables WHERE table_number = 'T-02' AND company_id = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;
    SELECT id INTO area_salon_id FROM areas WHERE name = 'Salón' AND company_id = '770e8400-e29b-41d4-a716-446655440001' LIMIT 1;

    IF mesero_id IS NOT NULL AND area_salon_id IS NOT NULL AND mesa1_id IS NOT NULL AND mesa2_id IS NOT NULL THEN
        INSERT INTO user_assignments (id, user_id, area_id, assigned_table_ids, "CompanyId", "BranchId", is_active, "CreatedAt", "UpdatedAt", "CreatedBy")
        VALUES (gen_random_uuid(), mesero_id, area_salon_id, jsonb_build_array(mesa1_id, mesa2_id), '770e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'Seeder')
        ON CONFLICT DO NOTHING;
    END IF;
END $$;

-- 11. PLANTILLAS DE EMAIL
INSERT INTO email_templates (id, company_id, name, subject, body, description, category, placeholders, is_active, created_at)
VALUES 
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'OrderConfirmation', 'Confirmación de Orden #{{OrderNumber}}', 
     '<html><body style="font-family: Arial, sans-serif;"><h2>Confirmación de Orden</h2><p>Estimado/a cliente,</p><p>Su orden ha sido confirmada exitosamente.</p><p><strong>Número de Orden:</strong> {{OrderNumber}}</p><p><strong>Fecha:</strong> {{OrderDate}}</p><p><strong>Total:</strong> {{TotalAmount}}</p><h3>Items:</h3><div>{{Items}}</div><p>Gracias por su preferencia.</p></body></html>',
     'Template para confirmación de orden', 'Orders', 'OrderNumber,OrderDate,TotalAmount,Items', true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'PasswordRecovery', 'Recuperación de Contraseña',
     '<html><body style="font-family: Arial, sans-serif;"><h2>Recuperación de Contraseña</h2><p>Hola {{UserName}},</p><p>Has solicitado recuperar tu contraseña. Haz clic en el siguiente enlace:</p><p><a href="{{ResetLink}}">Recuperar Contraseña</a></p><p>O copia y pega este token: {{ResetToken}}</p><p>Este enlace expirará en {{ExpirationMinutes}} minutos.</p><p>Si no solicitaste esto, ignora este email.</p></body></html>',
     'Template para recuperación de contraseña', 'Auth', 'UserName,ResetLink,ResetToken,ExpirationMinutes', true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'Welcome', 'Bienvenido a RestBar',
     '<html><body style="font-family: Arial, sans-serif;"><h2>Bienvenido a RestBar</h2><p>Hola {{UserName}},</p><p>Tu cuenta ha sido creada exitosamente.</p><p><strong>Email:</strong> {{Email}}</p><p><strong>Contraseña Temporal:</strong> {{TemporaryPassword}}</p><p>Por favor cambia tu contraseña después del primer inicio de sesión.</p><p><a href="{{LoginUrl}}">Iniciar Sesión</a></p><p>Bienvenido a {{CompanyName}}!</p></body></html>',
     'Template de bienvenida', 'Auth', 'UserName,Email,TemporaryPassword,LoginUrl,CompanyName', true, CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

-- 12. CONFIGURACIONES DEL SISTEMA
INSERT INTO "SystemSettings" ("Id", "CompanyId", "SettingKey", "SettingValue", "Description", "Category", "IsActive", "CreatedAt", "UpdatedAt")
VALUES 
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'AppName', 'RestBar Sistema', 'Nombre de la aplicación', 'General', true, CURRENT_TIMESTAMP, NULL),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'Currency', 'USD', 'Moneda principal', 'General', true, CURRENT_TIMESTAMP, NULL),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'TaxRate', '0.07', 'Tasa de impuesto por defecto', 'General', true, CURRENT_TIMESTAMP, NULL),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'AllowSplitPayments', 'true', 'Permitir pagos divididos', 'Payments', true, CURRENT_TIMESTAMP, NULL),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'EmailNotifications', 'true', 'Activar notificaciones por email', 'Notifications', true, CURRENT_TIMESTAMP, NULL)
ON CONFLICT DO NOTHING;

-- 13. MONEDA
INSERT INTO "Currencies" ("Id", "CompanyId", "Code", "Name", "Symbol", "ExchangeRate", "IsDefault", "IsActive", "CreatedAt", "UpdatedAt")
VALUES (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'USD', 'Dólar Estadounidense', '$', 1.0, true, true, CURRENT_TIMESTAMP, NULL)
ON CONFLICT DO NOTHING;

-- 14. HORARIOS DE OPERACIÓN
INSERT INTO "OperatingHours" ("Id", "CompanyId", "DayOfWeek", "OpenTime", "CloseTime", "IsOpen", "Notes", "CreatedAt", "UpdatedAt")
VALUES 
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'Monday', '09:00:00', '22:00:00', true, 'Horario estándar para Monday', CURRENT_TIMESTAMP, NULL),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'Tuesday', '09:00:00', '22:00:00', true, 'Horario estándar para Tuesday', CURRENT_TIMESTAMP, NULL),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'Wednesday', '09:00:00', '22:00:00', true, 'Horario estándar para Wednesday', CURRENT_TIMESTAMP, NULL),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'Thursday', '09:00:00', '22:00:00', true, 'Horario estándar para Thursday', CURRENT_TIMESTAMP, NULL),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'Friday', '09:00:00', '22:00:00', true, 'Horario estándar para Friday', CURRENT_TIMESTAMP, NULL),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'Saturday', '09:00:00', '22:00:00', true, 'Horario estándar para Saturday', CURRENT_TIMESTAMP, NULL),
    (gen_random_uuid(), '770e8400-e29b-41d4-a716-446655440001', 'Sunday', '09:00:00', '22:00:00', true, 'Horario estándar para Sunday', CURRENT_TIMESTAMP, NULL)
ON CONFLICT DO NOTHING;

COMMIT;

-- Mensaje de confirmación
SELECT '✅ Datos dummy creados exitosamente!' as message;
