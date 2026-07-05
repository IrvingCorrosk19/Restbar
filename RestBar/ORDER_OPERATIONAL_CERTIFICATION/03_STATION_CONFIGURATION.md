# 03 — Station Configuration

## Estaciones Enterprise (SeedEnterpriseRouting)

### Piso 1
Cocina Piso 1, Bar Principal, Bar VIP, Parrilla, Horno, Horno B, Cocina Caliente, Cocina Fría, Pastelería, Cocina Express

### Piso 2
Cocina Piso 2, Bar Piso 2

### Piso 3
Cocina Piso 3, Bar Piso 3

## ProductStockAssignment

CRUD: `/ProductStockAssignment/GetAssignments`, Create, Update, Delete  
Modelo: `ProductId`, `StationId`, `Priority`, `Stock`, `BranchId`, `IsActive`

**OP-CFG-02:** 14 asignaciones activas — PASS

## Tipos de estación

`kitchen`, `bar`, `grill`, `oven`, `pastry` — filtrados en KDS por `stationType`.

## Usuarios por estación

Chef → `StationOrders?stationType=kitchen`  
Bartender → `StationOrders?stationType=bar`  
Policy: `KitchenAccess`
