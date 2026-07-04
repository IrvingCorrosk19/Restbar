# 14 — ORDER PRODUCTION READINESS

## Checklist

| Criterio | Estado |
|----------|--------|
| Defectos críticos | ✅ 0 |
| Defectos altos | ✅ 0 |
| Multitenant POS | ✅ Corregido |
| Pagos parciales | ✅ Validado |
| Cambio de mesa | ✅ Implementado |
| Cuentas separadas JS | ✅ Cargado |
| GetActiveOrder binding | ✅ Corregido |
| Debug scripts removidos | ✅ |
| Backup pre-certificación | ✅ |
| Build limpio | ✅ |

## Pendiente pre-producción (no bloqueante)

1. Prueba carga 500–1000 órdenes
2. E2E SignalR reconexión 2 pantallas
3. Flujo supervisor cancelación post-preparado en UI
4. Persistir descuentos en servidor
5. Prueba 12 roles individuales en browser Order/Index

## Recomendaciones

- Mantener índice único orden activa por mesa (aplicado en certificación app-wide)
- Rate limiter dev 500 req/min (suficiente para certificación)
- Ejecutar `Run-OrderCertification.ps1` en CI pre-deploy

## Riesgo de despliegue

**BAJO** para operación POS estándar en entorno multitenant.
