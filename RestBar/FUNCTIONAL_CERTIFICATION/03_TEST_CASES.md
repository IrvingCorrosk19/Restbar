# Casos de Prueba Detallados

## Formato

| Campo | Descripción |
|-------|-------------|
| ID | Identificador único |
| Precondiciones | Estado requerido |
| Pasos | Acciones |
| Resultado esperado | Criterio PASS |
| Resultado obtenido | Última ejecución 2026-07-04 |

---

## AUTH-02 — Login Admin

**Precondiciones:** Seed demo ejecutado  
**Pasos:**
1. GET `/Auth/Login`
2. POST email=`admin@restbar.com`, password=`123456`
3. Verificar cookie `RestBarAuth`

**Esperado:** Sesión creada, redirect a dashboard  
**Obtenido:** ✅ PASS

---

## SEC-01 — Mesero denegado en Company

**Pasos:**
1. Login `mesero@restbar.com`
2. GET `/Company/Index`

**Esperado:** Redirect `/Auth/AccessDenied`  
**Obtenido:** ✅ PASS

---

## MT-02 — Aislamiento productos cross-tenant

**Pasos:**
1. Login `admin@restbar.com` (Empresa A)
2. GET `/Product/GetProducts`
3. Buscar "Producto Exclusivo B"

**Esperado:** 0 resultados  
**Obtenido:** ✅ PASS

---

## POS-02 — SendToKitchen

**Precondiciones:** Mesa y producto disponibles en sucursal A  
**Pasos:**
1. Login admin
2. GET `/Order/GetActiveTables` → tableId
3. GET `/Order/GetActiveCategories` → iterar categorías
4. GET `/Order/GetProductsByCategory/{id}` → productId
5. POST `/Order/SendToKitchen` con TableId, Items

**Esperado:** `{ success, orderId, status: SentToKitchen }`  
**Obtenido:** ✅ PASS — orderId `b578c29c-534f-41ee-aecc-71aab0b0aabf`

---

## PAY-01 — Pago parcial

**Precondiciones:** Orden activa con saldo pendiente  
**Pasos:**
1. Login `cajero@restbar.com`
2. POST `/api/Payment/partial` — Amount=1.00, Method=Efectivo, IdempotencyKey=UUID

**Esperado:** `{ success: true, isFullyPaid: false }`  
**Obtenido:** ✅ PASS

---

## PAY-03 — Idempotencia

**Pasos:**
1. Mismo IdempotencyKey en dos POST consecutivos
2. Ambos deben retornar éxito sin duplicar cobro

**Esperado:** Segundo request `isDuplicate: true` o mismo payment id  
**Obtenido:** ✅ PASS

---

## MT-03 — IDOR pago cross-tenant

**Pasos:**
1. Crear orden en Empresa A
2. Login `admin.b@restbar.com`
3. GET `/api/Payment/order/{orderIdA}/summary`

**Esperado:** HTTP 403 o 404  
**Obtenido:** ✅ PASS — Status 403

---

*Casos completos en `04_EXECUTED_TESTS.csv` (32 registros)*
