# Pruebas Ejecutadas — Registro Formal

**Última ejecución:** 2026-07-04T13:19 (UTC-5)  
**Script:** `scripts/Run-FullCertification.ps1`  
**Target:** `http://localhost:5001`

## Resumen

| Resultado | Cantidad |
|-----------|----------|
| PASS | 43 |
| FAIL | 0 |
| **TOTAL** | **43** |

## Detalle por categoría

| Categoría | PASS |
|-----------|------|
| Setup | 2 |
| Login | 13 |
| Security | 10 |
| Navigation | 2 |
| POS | 3 |
| Payment | 5 |
| MultiTenant | 5 |
| Reports | 3 |

## Casos nuevos (fase extendida)

| ID | Descripción |
|----|-------------|
| AUTH-13 | Contraseña incorrecta usuario válido |
| NAV-02 | Bartender accede bar KDS |
| SEC-07..10 | Manager audit, supervisor/support permisos, waiter POS |
| PAY-04 | Sobrepago rechazado (400) |
| PAY-05 | Orden inexistente (404) |
| MT-04..05 | Sucursal Norte mesa N-01, sin productos B |
| RPT-03 | Manager accede reportes |

## Veredicto

```
VERDICT: PASS
```

Ver `VERDICT.md` para veredicto oficial.

## Comando de reproducción

```powershell
powershell -ExecutionPolicy Bypass -File "FUNCTIONAL_CERTIFICATION\scripts\Run-FullCertification.ps1"
```
