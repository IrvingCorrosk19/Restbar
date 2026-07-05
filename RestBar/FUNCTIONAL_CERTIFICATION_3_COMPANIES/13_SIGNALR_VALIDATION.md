# 13 — Validación SignalR

**Fecha:** 2026-07-05  
**Resultado:** PASS (indirecto) / Limitación documentada

## TC3

El script TC3 **no incluye pruebas de navegador** en tiempo real. Valida APIs y estados resultantes.

## Cobertura ORDER_FUNCTIONAL_CERTIFICATION

`16_SIGNALR_REPORT.md` — hub `OrderHub`:
- Cambio estado mesa
- Orden enviada a cocina
- Producto listo
- Pago realizado
- Grupos por sucursal/estación

## Aislamiento multitenant SignalR

**Riesgo residual (Medium):** No se ejecutó prueba automatizada con 3 empresas conectadas simultáneamente en browser para confirmar que eventos no cruzan tenants.

**Mitigación existente:** Hubs filtran por `BranchId` / `CompanyId` en grupos (validado en código y reporte Order cert single-tenant).

## Recomendación pre-producción

Ejecutar prueba manual:
1. Mesero Costa + Mesero Norte en ventanas separadas
2. Cambiar mesa Costa → verificar que Norte no recibe evento

## Conclusión

SignalR operativo para operación estándar. Aislamiento multitenant en tiempo real requiere smoke test manual antes de producción multi-cliente.
