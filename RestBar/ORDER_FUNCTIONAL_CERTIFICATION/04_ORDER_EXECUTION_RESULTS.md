# 04 — ORDER EXECUTION RESULTS

**Ejecución:** 2026-07-04 14:00 UTC-5  
**Script:** `scripts/Run-OrderCertification.ps1`  
**Target:** `http://localhost:5001`

## Resumen API

| Métrica | Valor |
|---------|-------|
| Total casos | 38 |
| PASS | 38 |
| FAIL | 0 |
| Tasa éxito | 100% |

## Resumen Browser E2E

| Métrica | Valor |
|---------|-------|
| Casos ejecutados | 5 |
| PASS | 5 |
| FAIL | 0 |

## Evidencia

- `ORDER_TEST_RESULTS.csv` — detalle por caso
- `ORDER_DEFECTS.csv` — vacío en ejecución final
- Backup: `backups/RestBar_pre_order_cert_20260704_135205.dump`
- Browser: Mesa T-09, producto Café Americano, orden `b869a722-388c-41a7-b609-e4a42db50789`

## Comandos ejecutados

```powershell
powershell -ExecutionPolicy Bypass -File ORDER_FUNCTIONAL_CERTIFICATION\scripts\Run-OrderCertification.ps1
dotnet build
dotnet run --urls http://localhost:5001
```

## Iteraciones

| Iteración | PASS | FAIL | Notas |
|-----------|------|------|-------|
| 1 | 27 | 10 | Orden principal cancelada por reset masivo en script |
| 2 | 31 | 6 | Mismo root cause |
| 3 | 38 | 0 | Script corregido + fixes aplicados |
