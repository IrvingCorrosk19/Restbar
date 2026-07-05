# 02 — Datos de Prueba — 3 Empresas

**Seeder:** `Services/ThreeCompaniesCertSeeder.cs`  
**Endpoint:** `GET /Seed/SeedThreeCompaniesCertification`  
**Password universal:** `123456`

---

## Empresa 1 — Restaurante Costa

| Campo | Valor |
|-------|-------|
| Sucursal | Costa Centro |
| Áreas | Piso 1 Salón, Piso 1 Terraza, Piso 2 Salón, Piso 2 Terraza |
| Estaciones | Cocina Piso 1, Bar Piso 1, Cocina Piso 2, Bar Piso 2 |
| Mesas | C-01 … C-10 (distribuidas en áreas) |
| Producto exclusivo | Producto Exclusivo Costa ($99) |

**Usuarios:**

| Rol | Email |
|-----|-------|
| Admin | admin@costa.restbar.com |
| Manager | manager@costa.restbar.com |
| Mesero 1 (Piso 1 Salón) | mesero1@costa.restbar.com |
| Mesero 2 (Piso 2 Salón) | mesero2@costa.restbar.com |
| Cajero | cajero@costa.restbar.com |
| Chef | chef@costa.restbar.com |
| Bartender | bartender@costa.restbar.com |

**Routing productos:** Hamburguesa→Parrilla/Cocina, Pizza/Postre→Cocina, Cerveza/Mojito→Bar

---

## Empresa 2 — Restaurante Norte

| Campo | Valor |
|-------|-------|
| Sucursal | Norte Mall |
| Áreas | Piso 1 Principal, Piso 2 VIP |
| Estaciones | Cocina Principal, Bar Principal, Parrilla Norte |
| Mesas | NM-01 … NM-10 |
| Producto exclusivo | Producto Exclusivo Norte ($99) |

**Usuarios:** `admin@`, `manager@`, `mesero1@`, `mesero2@`, `cajero@`, `chef@`, `bartender@` + dominio `norte.restbar.com`

**Asignaciones:** mesero1 → Piso 1 Principal; mesero2 → Piso 2 VIP

---

## Empresa 3 — Restaurante Sur

| Campo | Valor |
|-------|-------|
| Sucursal | Sur Hotel |
| Áreas | Piso 1 Hotel, Piso 2 Hotel, Piso 3 Rooftop |
| Estaciones | Cocina Principal Sur, Bar Hotel Sur, Cocina Rooftop, Bar Rooftop |
| Mesas | S-01 … S-15 |
| Producto exclusivo | Producto Exclusivo Sur ($99) |

**Usuarios:** dominio `sur.restbar.com` (misma estructura de roles)

---

## SuperAdmin

| Email | Password |
|-------|----------|
| superadmin@restbar.com | 123456 |

---

## Productos comunes por empresa

- Hamburguesa {Empresa}
- Pizza {Empresa}
- Cerveza {Empresa}
- Mojito {Empresa}
- Postre {Empresa}
- Producto Exclusivo {Empresa}

Categoría: **Menú Principal**  
Stock inicial: 100 unidades por producto; asignación por estación vía `ProductStockAssignment`.
