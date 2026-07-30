# 21 — Informe de Defectos

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Última suite estable** | 72 passed / 4 failed (timeout red VPS) / 2 skipped · CASH-L 4/4 PASS |

## Defectos cerrados (verificados)

| ID | Severidad | Descripción | Fix | Verificación |
|----|-----------|-------------|-----|--------------|
| DEF-NAV-001 | **P0** | POS atrapaba usuario sin salida a Home | chrome Volver/Inicio + returnUrl | ORD-NAV-01..06 |
| DEF-CASH-ROWVER-001 | **P0** | `cash_sessions.row_version` null en PostgreSQL → Open 500 | concurrency token + valor en insert (`eebc419`) | Open → Detail #1, dashboard 1 sesión |
| DEF-CASH-OPEN-001 | P1 | Doble apertura → excepción | TempData + redirect a sesión existente | CASH-X04 |
| DEF-CASH-DASH-001 | P1 | Dashboard sin links a sesiones activas | tabla tipada + Detalle | CASH-L01 |
| DEF-ORD-STATUS-001 | P1 | UpdateItemStatus Guid vacío → 500 | BadRequest | STN-05 |
| DEF-POS-SWAL-001 | P1 | Swal/overlays bloqueaban E2E | helpers POS | ORD-E2E-01, INV-ORD-01 |

## Gaps / P2 (cobertura o seed)

| ID | Severidad | Descripción | Estado |
|----|-----------|-------------|--------|
| DEF-MT-SEED-001 | P2 | Usuario cross-tenant no sembrado | MT-02 SKIP |
| DEF-RBAC-SEED-001 | P2 | Roles mesero/cajero opcionales en seed | WTR-02..03 SKIP condicional |
| DEF-COV-SPLIT-001 | P2 | Split bill UI profundo | PARTIAL (OPS-02 API) |
| DEF-NET-001 | P2 | Timeouts intermitentes VPS/GitHub en retest final | Infra (no producto) |

## P0 abiertos

| Severidad | Count |
|-----------|-------|
| P0 | **0** |

## Veredicto

P0/P1 de producto **cerrados**. Fallos finales de la suite (4) coincidieron con `ERR_CONNECTION_TIMED_OUT` al VPS — no con regresiones funcionales reproducibles cuando el host responde.
