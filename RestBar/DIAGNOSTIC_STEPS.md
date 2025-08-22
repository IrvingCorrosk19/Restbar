# 🔍 DIAGNÓSTICO: Dropdowns Vacíos en Modal de Áreas

## 🎯 **PROBLEMA:**
Los dropdowns de Compañía y Sucursal están vacíos en el modal de creación/edición de áreas.

## 📋 **PASOS DE DIAGNÓSTICO:**

### **1. Verificar Datos en Base de Datos:**
```bash
# Ejecutar en PostgreSQL:
psql -U postgres -d RestBar -f Scripts/check_data.sql
```

**Resultado esperado:**
- Debe mostrar al menos 1 compañía y 1 sucursal
- Si muestra 0, ejecutar: `psql -U postgres -d RestBar -f Scripts/insert_basic_data.sql`

### **2. Verificar Logs del Backend:**
1. Abrir la consola donde está ejecutándose `dotnet run`
2. Navegar a `https://localhost:7247/Area/Index`
3. Abrir el modal de creación
4. Buscar estos logs específicos:

**Para Compañías:**
```
🔍 [AreaController] GetCompanies() - Iniciando carga de compañías...
🔍 [AreaController] GetCompanies() - Llamando a _companyService.GetAllAsync()...
✅ [AreaController] GetCompanies() - Compañías cargadas: X
📋 [AreaController] GetCompanies() - Detalles de las compañías:
  - ID: xxx, Nombre: RestBar Central, Activa: True
📊 [AreaController] GetCompanies() - Datos procesados: X
📤 [AreaController] GetCompanies() - Enviando respuesta: {"success":true,"data":[...]}
```

**Para Sucursales:**
```
🔍 [AreaController] GetBranches() - Iniciando carga de sucursales...
🔍 [AreaController] GetBranches() - Llamando a _branchService.GetAllAsync()...
✅ [AreaController] GetBranches() - Sucursales cargadas: X
📋 [AreaController] GetBranches() - Detalles de las sucursales:
  - ID: xxx, Nombre: Sucursal Principal, Activa: True, CompanyId: xxx
📊 [AreaController] GetBranches() - Datos procesados: X
📤 [AreaController] GetBranches() - Enviando respuesta: {"success":true,"data":[...]}
```

### **3. Verificar Logs del Frontend:**
1. Abrir DevTools (F12)
2. Ir a la pestaña Console
3. Navegar a la página de áreas
4. Abrir el modal
5. Buscar estos logs:

```
🚀 [Area/Index] Inicializando página de áreas...
📋 [Area/Index] Modal de creación abierto, cargando datos...
🏢 [Area/Index] loadCompanies() - Iniciando carga de compañías...
📡 [Area/Index] loadCompanies() - Respuesta recibida: {success: true, data: [...]}
✅ [Area/Index] loadCompanies() - X compañías cargadas en #areaCompanySelect
```

### **4. Verificar Peticiones AJAX:**
1. En DevTools, ir a la pestaña Network
2. Filtrar por XHR/Fetch
3. Abrir el modal
4. Buscar estas peticiones:
   - `GET /Area/GetCompanies` (debe devolver 200 OK)
   - `GET /Area/GetBranches` (debe devolver 200 OK)

## 🔍 **POSIBLES CAUSAS:**

### **A. No hay datos en la BD:**
- **Síntoma:** Logs muestran "Compañías cargadas: 0"
- **Solución:** Ejecutar `Scripts/insert_basic_data.sql`

### **B. Error en el servicio:**
- **Síntoma:** Logs muestran error en `CompanyService.GetAllAsync()`
- **Solución:** Revisar conexión a BD y configuración

### **C. Error en el controlador:**
- **Síntoma:** Logs muestran error en `AreaController.GetCompanies()`
- **Solución:** Revisar inyección de dependencias

### **D. Error en el frontend:**
- **Síntoma:** Petición AJAX falla (404, 500, etc.)
- **Solución:** Revisar rutas y JavaScript

### **E. Error de JavaScript:**
- **Síntoma:** Logs muestran error en `loadCompanies()`
- **Solución:** Revisar código JavaScript

## 🚀 **SOLUCIÓN RÁPIDA:**

Si no hay datos, ejecutar:
```bash
psql -U postgres -d RestBar -f Scripts/insert_basic_data.sql
```

Luego recargar la página y probar nuevamente.

## 📊 **INFORMACIÓN A REPORTAR:**

1. **Resultado del script check_data.sql**
2. **Logs del backend (consola de dotnet run)**
3. **Logs del frontend (DevTools Console)**
4. **Peticiones AJAX (DevTools Network)**
5. **Cualquier error que aparezca**

Con esta información podremos identificar exactamente dónde está el problema y solucionarlo.
