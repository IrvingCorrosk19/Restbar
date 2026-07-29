# 02 — KITCHEN TESTS (Cocina / KDS / Estaciones)

**Suite:** `scripts/Run-PurchasingKitchenSalesCertification.ps1`  
**Ejecución:** 2026-07-28 19:06:12 · `http://localhost:5001`  
**Resultado módulo:** **18/18 PASS** (escenarios ejecutados en vivo)

---

## Flujo verificado

```
Mesero → Orden → SendToKitchen → Estación (PSA) → Preparing → Ready → (Cancel opcional)
```

Estaciones seed: **17** · Cocinas ≥4 · Bares ≥3

---

## Matriz ejecutada

| ID | Escenario | Resultado | Notas |
|----|-----------|-----------|-------|
| KDS-01 | Stations configuradas | **PASS** | count=17 |
| KDS-02 | Chef StationOrders UI | **PASS** | HTTP 200 HTML |
| KDS-03 | Snapshot `/api/kitchen/current` | **PASS** | Recuperación post-reconnect |
| KDS-04 | Orden normal a cocina | **PASS** | SentToKitchen |
| KDS-05 | Observaciones | **PASS** | Item persistido |
| KDS-06 | Preparing | **PASS** | |
| KDS-07 | Mark ready | **PASS** | |
| KDS-08 | Doble MarkItemReady | **PASS** | Sin corrupción |
| KDS-09 | Cancel después de cocina | **PASS** | |
| KDS-10 | División cocina+bar | **PASS** | Hamburguesa + Cerveza |
| KDS-11 | Snapshot por estación | **PASS** | chef + bartender |
| KDS-12 | VIP / prioridad | **PASS** | |
| KDS-13 | Cambio de estación | **PASS** | `NewStationId` → Cocina Caliente |
| KDS-14 | Mesero sin KitchenAccess | **PASS** | Policy |
| KDS-15 | Orden grande 8 líneas | **PASS** | |
| KDS-16 | 2+ cocinas | **PASS** | 4 |
| KDS-17 | 2+ bares | **PASS** | 3 |
| KDS-SEC-01 | Mesa no asignada → 403 | **PASS** | Multitenant/asignación |

---

## Escenarios del prompt no cubiertos E2E en esta corrida

| Escenario | Evaluación |
|-----------|------------|
| Cocina/Bar pierde internet / SignalR offline E2E | Parcial — negotiate + snapshot OK; offline prolongado no simulado |
| Impresión duplicada térmica | Gap comercial (HTML only) |
| Producto listo sin preparar / never prepared edge | No forzado en esta suite |
| Reordenar cola visual | No ejercitado UI drag |
| Cambio de piso con re-routing automático | **Limitación conocida:** `MoveToTable` no reasigna estaciones |

---

## Validaciones

| Área | Resultado |
|------|-----------|
| Estados KitchenStatus / OrderItem | PASS en flujo principal |
| SignalR hub | negotiate OK; grupos usados por POS/KDS |
| Inventario en envío | Decremento vía flujo OrderService (cert previas + send OK) |
| Rendimiento 8 líneas | PASS funcional; no stress 150+ |

---

## Conclusión

**Cocina soporta operación real de restaurante** en flujo core KDS multi-estación. Limitaciones residuales: re-routing post cambio de mesa, stress/offline prolongado, impresión térmica.
