-- Script para crear usuario administrador por defecto
-- RestBar System - Admin User Creation

-- Verificar si ya existe un usuario admin
DO $$
DECLARE
    admin_exists BOOLEAN;
    admin_id UUID;
BEGIN
    -- Verificar si ya existe un usuario administrador activo
    SELECT EXISTS(
        SELECT 1 FROM users 
        WHERE role = 'admin' AND is_active = true
    ) INTO admin_exists;
    
    IF NOT admin_exists THEN
        -- Generar nuevo UUID para el admin
        admin_id := gen_random_uuid();
        
        -- Crear usuario administrador
        INSERT INTO users (
            id,
            branch_id,
            full_name,
            email,
            password_hash,
            role,
            is_active,
            created_at
        ) VALUES (
            admin_id,
            NULL, -- Sin sucursal específica (acceso global)
            'Administrador del Sistema',
            'admin@restbar.com',
            'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', -- Hash de "Admin123!"
            'admin',
            true,
            CURRENT_TIMESTAMP
        );
        
        RAISE NOTICE 'Usuario administrador creado exitosamente:';
        RAISE NOTICE 'Email: admin@restbar.com';
        RAISE NOTICE 'Contraseña: Admin123!';
        RAISE NOTICE 'ID: %', admin_id;
        
    ELSE
        RAISE NOTICE 'Ya existe un usuario administrador activo en el sistema.';
        
        -- Mostrar información de usuarios admin existentes
        RAISE NOTICE 'Usuarios administradores existentes:';
        FOR admin_id IN 
            SELECT id FROM users 
            WHERE role = 'admin' AND is_active = true
        LOOP
            RAISE NOTICE 'Admin ID: %', admin_id;
        END LOOP;
    END IF;
END $$;

-- Verificar el resultado
SELECT 
    id,
    full_name,
    email,
    role,
    is_active,
    created_at
FROM users 
WHERE role = 'admin' AND is_active = true
ORDER BY created_at DESC; 