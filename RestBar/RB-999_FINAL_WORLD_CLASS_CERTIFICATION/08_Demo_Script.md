# 08 — Demo Script (30 minutos)

**Objetivo:** dueño/gerente entiende valor en 30 min. Solo flujos de alto impacto. Ambiente: VPS o staging con flags Cash/Purchasing/FoodCost ON.

| Min | Bloque | Pantalla / acción | Mensaje de valor |
|-----|--------|-------------------|------------------|
| 0–2 | Contexto | Home | “Un solo sistema: operación + control” |
| 2–5 | Config mínima | Mesas / estaciones (ya seed) | Listo para operar sin 10 proveedores |
| 5–12 | POS | Mesa → producto → enviar cocina | Velocidad servicio |
| 12–16 | KDS | StationOrders kitchen → listo | Menos gritos / errores |
| 16–20 | Pago + Caja | Cobro + dashboard caja / movimiento | Control dinero |
| 20–23 | Inventario | Low stock / snapshot | Visibilidad stock |
| 23–25 | Compras | Lista PO / proveedor | Compra trazable |
| 25–27 | Food Cost | Dashboard FC% / menu eng | Rentabilidad plato |
| 27–29 | Analytics | Centro Ejecutivo + export CSV | Decidir sin Power BI |
| 29–30 | Cierre | Seguridad roles / audit | Quién hizo qué |

## No mostrar en demo de venta

Seed, Copilot off, Reports ExportPdf stub, AdvancedSettings densos, código, SuperAdmin a menos que cadena, Email roto, Payment view huérfana.

## Objeción rápida

| Objeción | Respuesta honesta |
|----------|-------------------|
| “¿Y si se cae internet?” | Hoy requiere conectividad; offline es gap conocido — planificar red/backup link |
| “¿Toast tiene tarjetas?” | Correcto; RestBar no reemplaza procesador aún — integrar partner o aceptar limitación |
| “¿Cuánto cuesta?” | Propuesta: licencia + implementación + hosting (definir números comerciales aparte) |
