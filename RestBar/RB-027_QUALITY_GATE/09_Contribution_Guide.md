# 09 — Contribution Guide

## Flujo

1. Branch desde `main`: `feat/…`, `fix/…`, `chore/rb-027-…`
2. Implementar + tests + docs mínimas.
3. Correr local:
   ```powershell
   pwsh RestBar/Com/quality/run-quality-gates.ps1 -BaseUrl http://localhost:5001
   ```
4. Abrir PR usando plantilla `.github/PULL_REQUEST_TEMPLATE.md`.
5. Esperar check **Quality Gate** verde.
6. Review humano: módulos tocados + riesgos tenant/seguridad.
7. Squash o merge según política del repo (sin force a main).

## Roles DB / entornos

| Entorno | URL típica | DB user |
|---------|------------|---------|
| Local | `http://localhost:5001` | según connection string |
| VPS | `http://164.68.99.83:8084` | **`restbaruser`** |

## Credencial smoke (no producción cliente)

`admin@restbar.com` — solo labs/cert. No documentar passwords en issues públicos.

## Dónde poner qué

| Cambio | Ubicación |
|--------|-----------|
| Lógica dominio | `Services/`, `Domain/` |
| Unit test | `RestBar.Tests/<Area>/` |
| Browser | `RestBar/tests/Browser/<Area>/` |
| Calidad proceso | `RestBar/RB-027_QUALITY_GATE/` |
| Deploy | `RestBar/Com/` |

## Rechazo típico de PR

- Sin tests en lógica nueva.
- 500 en validación negativa.
- Rompe Inventory/Cash/Orders browser.
- Secrets en repo.
- Quality Gate rojo.
