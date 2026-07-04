# 18 — PRODUCTION READINESS

## Checklist pre-producción

| Criterio | Estado |
|----------|--------|
| Defectos críticos abiertos | ✅ 0 |
| Defectos altos abiertos | ✅ 0 |
| Certificación automatizada | ✅ 119/119 PASS |
| Backup pre-certificación | ✅ `backups/RestBar_pre_order_enterprise_20260704_171831.dump` |
| Multitenant POS | ✅ Validado |
| Enrutamiento cocina/bar | ✅ Validado |
| Pagos + reembolsos | ✅ Validado |
| Inventario + recetas | ✅ Validado |
| Build limpio | ✅ 0 errores |
| Reset entre suites CI | ✅ `Cert-Common.ps1` |

## Bloqueantes producción internacional

| Item | Severidad | ETA sugerido |
|------|-----------|--------------|
| Load test 500+ concurrentes | Alta | Pre-expansión |
| Pasarela tarjeta | Alta | Pre-caja electrónica |
| Facturación electrónica multi-país | Alta | Por mercado |
| Impresión térmica E2E | Media | Pre-go-live cocina |
| SignalR offline E2E | Media | Pre-go-live |
| Delivery UI | Media | Si aplica modelo |

## Recomendaciones despliegue

1. **Piloto:** 1-5 sucursales RestBar Centro — riesgo **BAJO**
2. **Cadena nacional:** tras load test — riesgo **MEDIO**
3. **Franquicia internacional:** requiere fase 2 — riesgo **ALTO** sin completar gaps

## CI/CD

```powershell
# Pre-deploy gate
powershell -File scripts\Run-AllCertifications.ps1
if ($LASTEXITCODE -ne 0) { exit 1 }
```

## Veredicto production readiness

**LISTO para piloto controlado.** No listo para hiperscale internacional sin fase 2.
