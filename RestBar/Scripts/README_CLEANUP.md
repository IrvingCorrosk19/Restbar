# 🧹 Scripts de Limpieza - RestBar System

Este directorio contiene scripts para limpiar órdenes y dejar todas las mesas en estado disponible.

## 📁 Archivos Disponibles

### 🗃️ Scripts SQL
- **`clean_orders_and_tables.sql`** - Limpieza completa con verificaciones y respaldos
- **`quick_clean_orders.sql`** - Limpieza rápida sin confirmaciones

### 🔧 Scripts de Ejecución
- **`clean_orders.ps1`** - Script PowerShell (recomendado)
- **`clean_orders.bat`** - Script Batch para Windows
- **`README_CLEANUP.md`** - Este archivo de instrucciones

## 🚀 Uso Rápido

### Opción 1: PowerShell (Recomendado)
```powershell
# Limpieza rápida
.\clean_orders.ps1

# Limpieza completa
.\clean_orders.ps1 -Mode full

# Solo respaldos
.\clean_orders.ps1 -Mode backup
```

### Opción 2: Batch (Windows)
```cmd
# Limpieza rápida
clean_orders.bat

# Limpieza completa
clean_orders.bat full

# Solo respaldos
clean_orders.bat backup
```

### Opción 3: SQL Directo
```sql
-- Ejecutar directamente en PostgreSQL
\i clean_orders_and_tables.sql
-- o
\i quick_clean_orders.sql
```

## ⚠️ Advertencias Importantes

### 🚨 ANTES DE EJECUTAR:
1. **Solo usar en ambiente de desarrollo**
2. **Hacer respaldo de la base de datos**
3. **Verificar que no hay usuarios activos**
4. **Confirmar que es necesario limpiar**

### 🗑️ QUÉ ELIMINA:
- ✅ Todas las órdenes con estado activo (Pending, SentToKitchen, Preparing, Ready, ReadyToPay, Served)
- ✅ Todos los items de esas órdenes
- ✅ Todos los pagos relacionados con esas órdenes
- ✅ Actualiza todas las mesas a estado "Disponible"

### 💾 QUÉ CONSERVA:
- ✅ Órdenes completadas (Completed)
- ✅ Órdenes canceladas (Cancelled)
- ✅ Datos de productos, categorías, usuarios, etc.
- ✅ Configuraciones del sistema

## 🔧 Configuración

### Connection String por Defecto:
```
Host=localhost;Database=restbar;Username=postgres;Password=postgres
```

### Personalizar Connection String:
```powershell
# PowerShell
.\clean_orders.ps1 -ConnectionString "Host=mi-servidor;Database=restbar;Username=mi-usuario;Password=mi-password"
```

## 📊 Estados de Órdenes

| Estado | Valor | Descripción |
|--------|-------|-------------|
| Pending | 0 | Pendiente de envío a cocina |
| SentToKitchen | 1 | Enviada a cocina |
| Preparing | 2 | En preparación |
| Ready | 3 | Lista |
| ReadyToPay | 4 | Lista para pagar |
| Served | 5 | Servida |
| **Cancelled** | 6 | **Cancelada (NO se elimina)** |
| **Completed** | 7 | **Completada (NO se elimina)** |

## 📊 Estados de Mesas

| Estado | Valor | Descripción |
|--------|-------|-------------|
| **Disponible** | 0 | **Estado final después de limpieza** |
| Ocupada | 1 | Mesa ocupada |
| Reservada | 2 | Mesa reservada |
| EnEspera | 3 | En espera |
| Atendida | 4 | Atendida |
| EnPreparacion | 5 | En preparación |
| Servida | 6 | Servida |
| ParaPago | 7 | Para pago |
| Pagada | 8 | Pagada |
| Bloqueada | 9 | Bloqueada |

## 🔍 Verificaciones Post-Limpieza

### Verificar Mesas Disponibles:
```sql
SELECT COUNT(*) as mesas_disponibles 
FROM "Tables" 
WHERE "Status" = 0 AND "IsActive" = true;
```

### Verificar Órdenes Restantes:
```sql
SELECT COUNT(*) as ordenes_restantes 
FROM "Orders" 
WHERE "Status" IN (0, 1, 2, 3, 4, 5);
```

### Verificar Items Restantes:
```sql
SELECT COUNT(*) as items_restantes 
FROM "OrderItems" oi
INNER JOIN "Orders" o ON oi."OrderId" = o."Id"
WHERE o."Status" IN (0, 1, 2, 3, 4, 5);
```

## 🆘 Solución de Problemas

### Error: "psql no encontrado"
- Instalar PostgreSQL client
- Agregar psql al PATH del sistema
- Verificar que PostgreSQL esté ejecutándose

### Error: "Connection refused"
- Verificar que PostgreSQL esté ejecutándose
- Verificar connection string
- Verificar credenciales de acceso

### Error: "Permission denied"
- Ejecutar como administrador
- Verificar permisos de usuario de base de datos
- Verificar que el usuario tenga permisos de DELETE y UPDATE

## 📞 Soporte

Si encuentras problemas con estos scripts:
1. Verifica los logs de PostgreSQL
2. Revisa la configuración de conexión
3. Confirma que tienes permisos necesarios
4. Contacta al administrador del sistema

---

**⚠️ RECORDATORIO: Estos scripts eliminan datos permanentemente. Úsalos con precaución.**
