# 02 — ANÁLISIS OPERATIVO

**Evidencia:** ORDER_OPERATIONAL 119/119 · PKS Kitchen 18/18 · Browser E2E 2026-07-28

---

## Flujo operativo verificado (happy path)

```
Mesa → Productos → Confirmar → Estación (KDS) → Marcar listo → Pago (parcial/total) → Mesa disponible
```

**Certificado:** API + browser en VPS (`164.68.99.83:8084`).

---

## Por rol operativo

### Mesero
| Capacidad | Estado |
|-----------|--------|
| Ver mesas asignadas | ✅ Asignación por área/mesa |
| Tomar pedido digital | ✅ |
| Enviar a cocina por estación | ✅ Admin elige estación; auto-routing por stock |
| Dividir cuenta | ✅ API Person/split certificada |
| Aplicar descuento | ❌ Bloqueado (403) — solo manager+ |
| Cobrar | ⚠️ Parcial vía cajero/admin |

### Chef / Cocina
| Capacidad | Estado |
|-----------|--------|
| KDS por estación | ✅ `/Order/StationOrders`, `/api/kitchen/current` |
| Marcar listo | ✅ Browser + API |
| Prioridad / VIP flag | ✅ Campo orden |
| Cancelar post-cocina | ✅ Con permisos |

### Bar
| Capacidad | Estado |
|-----------|--------|
| Estaciones bar separadas | ✅ Certificado routing 15/15 |
| Inventario por estación | ✅ ProductStockAssignment |

### Cajero
| Capacidad | Estado |
|-----------|--------|
| Pago efectivo/tarjeta/mixto | ✅ |
| Idempotencia anti-duplicado | ✅ |
| Reembolso | ✅ API refund |
| Apertura/cierre caja | ❌ **No existe** |
| Arqueo | ❌ **No existe** |
| Precuenta | ❌ **No existe** |

### Gerente
| Capacidad | Estado |
|-----------|--------|
| Reportes ventas | ✅ JSON APIs |
| Cancelar orden / supervisor | ✅ |
| Multisucursal | ✅ Branch isolation |
| Cierre de día formal | ❌ Solo reporte, sin ritual |

### Compras / almacén
| Capacidad | Estado |
|-----------|--------|
| Órdenes de compra | ❌ 404 |
| Proveedores | ❌ 404 |
| Entrada manual stock | ✅ `CreatePurchase` |
| Transferencias stock | ✅ Parcial (reject gap) |
| Recetas / BOM | ✅ Enterprise 2026-07-04 |

---

## Cuellos de botella detectados

1. **Sin caja:** imposible cuadrar efectivo vs sistema → riesgo operativo diario.
2. **Sin compras:** reabastecimiento fuera del sistema → Excel paralelo.
3. **MoveToTable sin re-routing KDS:** cambio de piso puede desincronizar cocina.
4. **Impresión solo HTML:** cocina/bar dependen de pantalla, no ticket físico.
5. **Onboarding manual:** tiempo alto en go-live sin consultor.

---

## Eficiencia cuantificable (estimación conservadora para piloto)

| Proceso | Mejora potencial | Condición |
|---------|------------------|-----------|
| Comunicación cocina | 15–25% menos errores reenvío | Con KDS adoptado |
| Tiempo cobro | 5–10% si pagos parciales usados | Sin caja, beneficio parcial |
| Inventario | 5–15% menos quiebres en estación | Requiere disciplina stock assignments |
| Back-office compras | **0%** | Módulo ausente |

**Nota:** cifras son rangos de industria aplicados solo donde hay capacidad verificada; no hay medición A/B en producción RestBar.
