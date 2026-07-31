# 01 — Quality Gates

**Obligatorios** para merge a `main` / `master`. Si un gate falla → **no aprobar**.

| Gate | Nombre | Criterio PASS | Enforced hoy |
|------|--------|---------------|--------------|
| G1 | Compila | `dotnet build -c Release` exit 0 | **CI** job `G1 Build` |
| G2 | Unitarias | `dotnet test RestBar.Tests` 100% PASS | **CI** job `G2 Unit tests` |
| G3 | API | Suite API/integration PASS (cuando exista) | **Parcial** — hoy cubierto por endpoints vía Browser + smoke; harness API formal = backlog P1 |
| G4 | Browser | Playwright smoke/security/MT/perf PASS | **CI** si variable `RESTBAR_BASE_URL`; **local** vía `run-quality-gates.ps1` |
| G5 | Performance | DOMContentLoaded &lt; 5s hard / target 2s en páginas críticas | Browser `Performance/*.spec.js` dentro de G4 |
| G6 | Sin regresión funcional | Specs del módulo tocado + Regression suite | **Proceso** (PR checklist) + suites existentes |
| G7 | Sin fuga multitenant | `Multitenant` + TenantScope unit + middleware | Unit TenantScope + browser MT; **profundidad cross-tenant limitada** |
| G8 | Sin vulnerabilidades críticas | Sin High/Critical en audit; SAST advisory | `dotnet list package --vulnerable` en CI (advisory); High/Critical falla script local |
| G9 | Cobertura mínima | No bajar del baseline; subir en código nuevo | Baseline **0.41%** líneas (2026-07-30). Floor duro alto = prematuro. **Regla:** lógica de dominio nueva → unit tests obligatorios |
| G10 | Documentación | PR template + docs RB-* si cambia comportamiento | Checklist PR + assets policy en CI |

## Política de bloqueo

1. GitHub branch protection debe exigir el check **`Quality Gate`**.
2. Deploy VPS (`Com/deploy-restbar.ps1`) solo con CI verde en el commit a desplegar.
3. Excepciones temporales requieren issue `QG-EXCEPTION` + fecha de caducidad ≤ 14 días.

## Mapa a módulos críticos

| Módulo | Gates mínimos antes de merge |
|--------|------------------------------|
| POS / Orders | G1 G2 G4 (Orders/Smoke) G6 G7 |
| Cash | G1 G2 (state machine) G4 Cash G6 |
| Inventory | G1 G2 (math) G4 Inventory G6 |
| Purchases | G1 G2 PO SM G4 Procurement G6 |
| Food Cost | G1 G2 math G4 FoodCost G6 |
| BI | G1 G2 catalog/math G4 Analytics G6 |
| Reports / Exports | G1 G4 Reports/Analytics export G6 |
| Security / RBAC | G1 G4 Security/Auth G8 |
| Multitenancy | G1 G2 TenantScope G4 MT G7 |
| Performance | G4 Performance G5 |
| DB / migraciones | G1 + revisión migración + backup antes deploy |

## Fallo → acción

| Fallo | Acción |
|-------|--------|
| G1/G2 | Fix inmediato; no squash-merge |
| G4 flaky | Re-run 1×; si persiste → bug producto o test, no “ignore” |
| G8 High/Critical | Upgrade o waivers documentados en Security report |
| G9 sin tests en lógica nueva | Rechazar PR |
