# 05 — Business Value

Cada afirmación: módulo + evidencia + KPI posible. Sin humo.

| Promesa | ¿RestBar ayuda? | Módulo | Evidencia | KPI demostrable |
|---------|-----------------|--------|-----------|-----------------|
| Ganar más dinero | **Indirecto** | POS+KDS+Analytics | Menos errores cocina (KDS PASS); no prueba A/B ventas | Ticket medio, % remakes |
| Reducir desperdicio | **Sí** | Food Cost waste + Inv movements | FC/INV tests; snapshots | Waste $ / FC% |
| Comprar mejor | **Sí** | Procurement + price history | PO/Supplier PASS | Precio unitario, OTIF |
| Controlar inventario | **Sí (con límites)** | Inventory | INV-* PASS; sin conteo físico | Stockouts, variance |
| Reducir fraude caja | **Sí** | Cash hash/approvals/audit | RB-010 | Variance caja, overrides |
| Cerrar caja más rápido | **Sí** | CashSession arqueo/X/Z | CASH-L* PASS | Minutos cierre |
| Controlar sucursales | **Sí lógico** | Multi-branch + SuperAdmin | MT smoke | Comparativo branch Analytics |
| Reducir tiempos cocina | **Parcial** | KDS + timestamps | Prep times necesitan datos | Min prep, tickets delayed |
| Mejorar servicio | **Parcial** | POS+KDS SignalR | E2E PASS | Tiempo mesa→servido |
| Tomar decisiones | **Sí** | Executive Analytics | AN-* PASS | Uso semanal dashboards |

## Segmentos — ¿cambiarían a RestBar?

| Segmento | ¿Cambiarían? | Por qué (evidencia) |
|----------|--------------|---------------------|
| Pequeño | **Tal vez** | Costo bajo self-host; si aceptan online-only y pagos manuales |
| Mediano | **Mejor fit** | Vertical POS+caja+inv+compras+FC+BI nativo |
| Cadena | **Piloto sí / scale no** | Multibranch sí; load/support no probados |
| Franquicia | **No aún** | Falta portal franquicia, billing, compliance pack |
| Dark kitchen | **Parcial** | KDS fuerte; delivery/integrations débiles |
| Bar | **Sí operativo** | Bar KDS + caja |
| Café | **Sí** si menú simple | Sin loyalty/reservas |
| Hotel | **No** | Sin PMS |
| Food court | **Difícil** | Multi-vendor settlements no existen |

**ROI realista:** control (caja/inv/FC) + unificación de herramientas, no “aumentar ventas 20%” sin medición.
