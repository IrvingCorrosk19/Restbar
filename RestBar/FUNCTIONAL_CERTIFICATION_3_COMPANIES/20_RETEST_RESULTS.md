# 20 — Resultados de Re-prueba

**Fecha:** 2026-07-05  
**Ambiente:** localhost:5001, PostgreSQL 18, Release build

## Ejecución 1 (con bug $pid)

| Métrica | Valor |
|---------|-------|
| PASS | 35 |
| FAIL | 0 |
| Tests omitidos | TC3-SEC-*, TC3-MOV-SEC-01, TC3-CON-01 (error script) |

## Ejecución 2 (post-fix)

```
=== 3-COMPANIES SUMMARY ===
PASSED: 40  FAILED: 0  TOTAL: 40
FUNCTIONAL CERTIFICATION 3 COMPANIES: PASS
```

**CSV:** `TC3_TEST_RESULTS.csv`

## Regresión completa

```
scripts/Run-AllCertifications.ps1
ALL SUITES: PASS
```

| Suite | Casos |
|-------|-------|
| Functional | 43 |
| Order | 38 |
| Routing | 15 |
| Enterprise | 23 |
| **Subtotal regresión** | **119** |

## Compilación

```
dotnet build RestBar.sln -c Release
Build succeeded. 0 Error(s)
```

## Veredicto re-test

**FUNCTIONAL CERTIFICATION 3 COMPANIES: PASS**

No quedan defectos Critical ni High abiertos.
