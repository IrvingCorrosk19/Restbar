# 03 — OPERATIONAL MODEL

**Objetivo:** Definir el modelo operativo que RestBar debe administrar de punta a punta.

---

# 1. Cadena de valor del restaurante

```
PLANEAR → COMPRAR → RECIBIR → ALMACENAR → PRODUCIR → SERVIR → COBRAR → CERRAR → ANALIZAR → DECIDIR
   │         │         │          │          │         │        │        │         │          │
  Forecast  PO      Recepción  Inventario  Receta    POS+KDS  Caja    Arqueo   BI      Copilot
  Labor     Supplier  Costo     Merma      Prep      Mesas    Fiscal  Shift    Alertas  Acciones
```

**Hoy RestBar cubre fuerte:** SERVIR + parte de COBRAR.  
**Parcial:** ALMACENAR, PRODUCIR (receta), ANALIZAR.  
**Ciego:** PLANEAR, COMPRAR, CERRAR formal, DECIDIR automático.

---

# 2. Procesos — matriz de administración

## Administra completamente

- Selección mesa / área  
- Pedido, modificadores básicos vía flujo orden  
- Envío cocina / bar multi-estación  
- Estados ítem KDS + SignalR  
- Pago parcial / mixto / split / void / refund API  
- Roles y permisos operativos  
- Multiempresa / multisucursal  

## Administra parcialmente

| Proceso | Qué hay | Qué falta |
|---------|---------|-----------|
| Inventario | Stock estación, transfer, movimiento | PO, caducidad, conteo cíclico |
| Recetas | BOM + deduct venta | UI chef, costeo, yield |
| Turnos | Shift API | Caja, arqueo, handoff UI |
| Precios | DiscountPolicy, PriceSchedule | Motor comercial HH/combo |
| Clientes | Modelo + puntos | CRM, captura POS, campañas |
| Facturación | InvoiceService | Precuenta, fiscal, UI |
| Reportes | APIs | Export, dashboard único |
| Tips | TipAllocation | Reglas UI + liquidación |

## No administra (debe)

1. Apertura/cierre caja y arqueo  
2. Órdenes de compra y proveedores  
3. Food Cost / Beverage Cost / Prime Cost  
4. Merma con motivo y costo  
5. Precuenta e impresión térmica estándar  
6. Combos y happy hour comerciales  
7. Labor scheduling y costo laboral  
8. Loyalty / recompra  
9. Delivery / agregadores  
10. Reservas / waitlist  
11. Franquicia (royalties, brand pack)  
12. Forecast demanda y producción  
13. Copiloto de decisiones  

## Debería administrar (prioridad por ROI)

| Prioridad | Proceso | Por qué |
|-----------|---------|---------|
| P0 | Caja + cierre | Control diario / anti-fraude |
| P0 | Precuenta | Rotación + CX |
| P0 | Compras + proveedor | Food cost |
| P0 | Food cost desde receta+PO | Margen |
| P1 | Combos + HH | Ingresos |
| P1 | Export + Command Center | Decisiones |
| P1 | Merma estructurada | Desperdicio |
| P2 | Loyalty + CRM mínimo | Recompra |
| P2 | Forecast + reorder | Automatización |
| P3 | Labor / Delivery / Franquicia pack | Escala |

---

# 3. Roles operativos — sistema ideal

| Rol | Job-to-be-done en RestBar |
|-----|---------------------------|
| Mesero | Tomar pedido rápido, ver estado cocina, precuenta, upsell sugerido |
| Chef / estación | KDS limpio, tiempos, merma, producción del día |
| Bartender | KDS bar + HH automático |
| Cajero | Cobrar + caja + arqueo |
| Supervisor | Floor control, alertas estación lenta |
| Gerente | Command Center <5s, acciones del día |
| Comprador | PO sugerido, comparar proveedores |
| Contador | Export fiscal/contable |
| Dueño / CEO cadena | Margen, sucursales, riesgos |
| Franquiciador | Benchmark locales, compliance |

---

# 4. Ritual diario que RestBar debe orquestar

### Apertura
1. Abrir turno + caja (fondo)  
2. Revisar alertas stock / estaciones  
3. Confirmar HH/promos del día  

### Servicio
4. POS → KDS → entrega  
5. Alertas SLA cocina  
6. Upsell / combo en POS  

### Cierre
7. Precuentas pendientes  
8. Cierre caja + arqueo  
9. Merma del día  
10. Reporte Z / export  

### Post-cierre (HQ)
11. Comparativo sucursales  
12. Reorder sugerido mañana  
13. Acciones Copilot  

**Hoy:** pasos 4 (parcial 5). El resto es Excel / WhatsApp.

---

# 5. Modelo multi-sucursal / franquicia

```
Holding / Franquiciador
  └── Company (marca / franchisee)
        └── Branch (local)
              └── Stations / Areas / Cash Registers
```

**Capacidades requeridas:**
- Benchmark entre branches  
- Catálogo central opcional  
- Políticas de descuento centralizadas  
- Pack reportes franquicia  
- (Futuro) royalty % sobre ventas  

**Ya existe:** jerarquía Company/Branch + SuperAdmin.  
**Falta:** capa Franquicia analítica y comercial.

---

# 6. Principio de diseño operativo

> Un solo flujo de verdad por proceso.  
> Extender entidades existentes (`Shift`, `InventoryMovement`, `Recipe`, `Invoice`, `DiscountPolicy`).  
> No crear módulos paralelos que dupliquen POS, inventario o reportes.
