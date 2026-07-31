# RB-027 — Permanent Quality Gate & Engineering Excellence

**Fecha:** 2026-07-30 · **Ámbito:** proceso de ingeniería (sin módulos de negocio nuevos)  
**Veredicto:** ver `10_Final_Engineering_Certification.md`

## Misión

Ningún cambio futuro debe integrarse sin pasar Quality Gates que protejan POS, Caja, Inventario, Compras, Food Cost, BI, Reportes, Exportaciones, Seguridad, RBAC, Multitenancy, Performance, API y Base de datos.

## Documentos

| # | Documento |
|---|-----------|
| 01 | [Quality Gates](01_Quality_Gates.md) |
| 02 | [Test Coverage](02_Test_Coverage.md) |
| 03 | [Static Analysis](03_Static_Analysis.md) |
| 04 | [Architecture Review](04_Architecture_Review.md) |
| 05 | [Code Quality](05_Code_Quality.md) |
| 06 | [Technical Debt](06_Technical_Debt.md) |
| 07 | [CI/CD Guide](07_CI_CD_Guide.md) |
| 08 | [Development Standards](08_Development_Standards.md) |
| 09 | [Contribution Guide](09_Contribution_Guide.md) |
| 10 | [Final Engineering Certification](10_Final_Engineering_Certification.md) |

## Ejecución local

```powershell
pwsh RestBar/Com/quality/run-quality-gates.ps1 -BaseUrl http://localhost:5001
# VPS:
$env:RESTBAR_BASE_URL='http://164.68.99.83:8084'
pwsh RestBar/Com/quality/run-quality-gates.ps1
```

## CI

Workflow: `.github/workflows/restbar-ci.yml`  
Check requerido en branch protection: **Quality Gate**

## Evidencia baseline (2026-07-30)

| Métrica | Valor |
|---------|-------|
| Unit tests | **77 PASS** (xUnit + InlineData) |
| Unit line coverage | **0.41%** (421 / 100452 líneas) |
| Playwright specs | **32** · ~**158** tests · 3 viewports |
| Controllers | **42** |
| Service files | **63** |
| EF migrations | **17** |
| CI previo RB-027 | Solo build + unit |
| CI post RB-027 | G1–G3 obligatorios + G4 browser si `RESTBAR_BASE_URL` |
