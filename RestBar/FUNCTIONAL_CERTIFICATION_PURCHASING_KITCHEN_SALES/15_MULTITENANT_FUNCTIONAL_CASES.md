# 15 — Multitenant Functional Cases

**Fecha:** 2026-07-28  
**Script:** `scripts/Run-MultitenantFunctionalCases.ps1`  
**Veredicto:** **PASS**

---

## Resumen ejecutivo

Se ejecutaron **51 casos funcionales multitenant** imaginando operación real SaaS multi-empresa:

| Ambiente | PASS | FAIL | BLOCKED | Veredicto |
|----------|------|------|---------|-----------|
| Local `http://localhost:5001` | **51** | **0** | **0** | **PASS** |
| VPS `http://164.68.99.83:8084` | **47** (+4 retest) | 1* | 3* | **PASS** tras retest |

\* El FAIL/BLOCKED de VPS fue por **HTTP 429 rate-limit de login** en `admin.norte@restbar.com` durante la corrida masiva. Retest aislado: login OK + aislamiento branch **PASS**.

---

## Tenants inventariados

| Empresa | Sucursal | Usuarios clave |
|---------|----------|----------------|
| RestBar Principal | RestBar Centro | `admin@restbar.com`, mesero/chef/cajero |
| RestBar Principal | RestBar Norte | `admin.norte@restbar.com` |
| RestBar Empresa B | Sucursal B Centro | `admin.b@restbar.com` |
| Restaurante Costa | Costa Centro | `admin@costa.restbar.com`, mesero1/2, cajero, chef |
| Restaurante Norte | Norte Mall | `admin@norte.restbar.com` |
| Restaurante Sur | Sur Hotel | `admin@sur.restbar.com` |
| — | — | `superadmin@restbar.com` |

Password cert: `123456`

---

## Escenarios (casos imaginarios)

### CASO 0 — Auth por tenant
Login A/B/Costa/Norte/Sur/SuperAdmin; rechazo de usuario inexistente.

### CASO 1 — Mesas no se mezclan
IDs de mesa A≠B, Costa≠Norte≠Sur, Centro≠Sucursal Norte. Prefijos C-*/NM-*/S-*.

### CASO 2 — Catálogo exclusivo
Cada empresa ve solo su `Producto Exclusivo *`; no hay fuga de catálogo.

### CASO 3 — Operación paralela A vs B
Dos restaurantes crean órdenes a la vez; ninguno ve la orden activa del otro (**403**).

### CASO 4 — IDOR cross-tenant
Pagar / cancelar / leer summary / leer mesa de otro tenant → **403/404**.

### CASO 5 — MoveToTable cross-company
No se puede mover orden a mesa de otra empresa (**400/403**).

### CASO 6 — Roles
Mesero/chef sin Company/Reports; mesero sí POS; SuperAdmin sí SuperAdmin.

### CASO 7 — Flujo venta por empresa
Orden → pago Efectivo en A, B, Costa, Norte, Sur → **PASS**.

### CASO 8 — Multi-sucursal misma empresa
`admin.norte` no puede pagar ni cancelar orden de Centro (**403**).

### CASO 9 — GUIDs inventados
Pago/orden con IDs falsos → **404/403** (sin filtración).

---

## Defectos encontrados y corregidos en esta corrida

| ID | Severidad | Hallazgo | Acción |
|----|-----------|----------|--------|
| DEF-MT-B-01 | High (ops) | Empresa B sin estación/stock → no podía `SendToKitchen` | Insert station `Cocina B` + `product_stock_assignments` (local + VPS) |
| DEF-MT-RL-01 | Medium (ops) | VPS rate-limit login **429** en corridas masivas | Suite con retry/backoff; retest manual OK |

**Ninguna fuga multitenant crítica abierta.**

---

## Artefactos

- `MT_FUNCTIONAL_RESULTS.csv` — última corrida
- `MT_FUNCTIONAL_REPORT.md` — resumen auto
- `scripts/Run-MultitenantFunctionalCases.ps1`

```powershell
# Local
.\scripts\Run-MultitenantFunctionalCases.ps1 -BaseUrl http://localhost:5001

# VPS (respetar rate-limit: no martillar logins)
.\scripts\Run-MultitenantFunctionalCases.ps1 -BaseUrl http://164.68.99.83:8084
```
