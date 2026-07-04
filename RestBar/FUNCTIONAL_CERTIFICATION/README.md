# RestBar — Certificación Funcional Enterprise

**Veredicto final:** `FUNCTIONAL CERTIFICATION: PASS`  
**API automatizada:** 43/43 | **Browser E2E:** 10/10

## Inicio rápido

```powershell
# 1. Servidor
cd c:\Proyectos\RestBar\RestBar\RestBar
dotnet run --urls "http://localhost:5001"

# 2. Certificación API (43 casos)
powershell -ExecutionPolicy Bypass -File "FUNCTIONAL_CERTIFICATION\scripts\Run-FullCertification.ps1"

# 3. Certificación Browser E2E (manual/MCP)
# Abrir http://localhost:5001/Auth/Login en navegador
# Ver 16_BROWSER_E2E_RESULTS.md
```

## Tipos de prueba

| Tipo | Casos | Archivo |
|------|-------|---------|
| API/HTTP automatizado | 43 | `04_EXECUTED_TESTS.csv` |
| Browser E2E (UI real) | 10 | `04_BROWSER_EXECUTED_TESTS.csv` |

## Estructura

| Archivo | Contenido |
|---------|-----------|
| `01_MASTER_FUNCTIONAL_CERTIFICATION_PLAN.md` | Plan y alcance |
| `02_TEST_SCENARIOS.md` | Escenarios |
| `03_TEST_CASES.md` | Casos detallados |
| `04_EXECUTED_TESTS.md` / `.csv` | Resultados |
| `05_DEFECT_LOG.md` / `.csv` | Defectos (vacío) |
| `06_ROOT_CAUSE_ANALYSIS.md` | RCA |
| `07_FIXES_APPLIED.md` | Correcciones código |
| `08_RETEST_RESULTS.md` | Historial retests |
| `09_MULTI_TENANT_CERTIFICATION.md` | Multi-tenant |
| `10_SECURITY_CERTIFICATION.md` | Seguridad |
| `11_POS_CERTIFICATION.md` | POS |
| `12_PAYMENT_CERTIFICATION.md` | Pagos |
| `13_PERFORMANCE_FUNCTIONAL_RESULTS.md` | Rendimiento |
| `14_PRODUCTION_READINESS_REPORT.md` | Pre-producción |
| `15_EXECUTIVE_FUNCTIONAL_SUMMARY.md` | Resumen ejecutivo |
| `16_BROWSER_E2E_RESULTS.md` | Pruebas en navegador real |
| `VERDICT.md` | Veredicto oficial |

## Backup

`backups/RestBar_pre_certification_20260704_125758.dump`

## Usuarios de prueba

Password universal seed: `123456` — ver plan maestro para emails por rol.
