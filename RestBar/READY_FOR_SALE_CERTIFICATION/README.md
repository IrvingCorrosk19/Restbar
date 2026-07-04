# README — Ready for Sale Certification

Certificación comercial escenarios **29-67** del prompt master.

## Ejecutar

```powershell
powershell -File READY_FOR_SALE_CERTIFICATION\scripts\Run-ReadyForSaleCertification.ps1
```

## Demo comercial

```powershell
Invoke-RestMethod http://localhost:5001/Seed/SeedDemoData
Invoke-RestMethod http://localhost:5001/Seed/SeedEnterpriseRouting
Invoke-RestMethod http://localhost:5001/Seed/SeedCommercialDemo
```

Login demo: `admin@restbar.com` / `123456`

## Documentos

| Archivo | Contenido |
|---------|-----------|
| `COMMERCIAL_VERDICT.md` | Veredicto ¿se puede vender? |
| `SALE_BLOCKERS.md` | 13 bloqueos de venta |
| `RFS_TEST_RESULTS.csv` | Evidencia ejecución |
| `RFS_SALE_BLOCKERS.csv` | Lista blockers |

## Fixes aplicados (fase comercial)

- `TenantSubscriptionMiddleware` — bloqueo operación tenant suspendido
- `AuthService` — login bloqueado si empresa/sucursal inactiva
- `CommercialDemoSeeder` — 30 mesas, 100 productos, historial
- `ApplyDiscount` — solo manager/supervisor/admin
