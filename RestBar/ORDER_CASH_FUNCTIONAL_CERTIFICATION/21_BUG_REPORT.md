# 21 — Informe de Defectos

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Run previo** | 79 passed / 8 failed / 1 skipped → fallos corregidos en ciclo posterior |

## Defectos cerrados (verificados)

| ID | Severidad | Descripción | Fix | Verificación |
|----|-----------|-------------|-----|--------------|
| DEF-NAV-001 | **P0** | POS atrapaba usuario sin salida a Home | `14e12aa` — chrome Volver/Inicio + returnUrl seguro | ORD-NAV-01..06 **PASS** |
| DEF-CASH-OPEN-001 | P1 | Doble apertura sesión caja → crash/TempData | Manejo sesión activa en wizard | CASH-X04 **PASS** |
| DEF-ORD-STATUS-001 | P1 | UpdateItemStatus Guid vacío → HTTP 500 | `29f3e6e` BadRequest | STN-05 **PASS** |
| DEF-POS-SWAL-001 | P1 | Helper sendToKitchen no cerraba Swal → timeout tests | `33e47e2` helpers POS | ORD-E2E-01, PAY-03/04, KDS-03 **PASS** |

## Defectos abiertos / gaps (honestos)

| ID | Severidad | Descripción | Estado |
|----|-----------|-------------|--------|
| DEF-MT-SEED-001 | P2 | Usuario cross-tenant no sembrado | MT-02 SKIP |
| DEF-RBAC-SEED-001 | P2 | Roles mesero/cajero/chef no garantizados en VPS | WTR-02..04 SKIP condicional |
| DEF-COV-SPLIT-001 | P2 | Sin cobertura split bill E2E | NOT_COVERED |
| DEF-COV-CLOSE-001 | P2 | Sin cobertura cierre caja E2E | NOT_COVERED |
| DEF-COV-TRANSFER-001 | P2 | Transferencia mesa solo API negativa | PARTIAL |

## P0 abiertos

| Severidad | Count |
|-----------|-------|
| P0 | **0** |

## Veredicto

Defectos P0/P1 del ciclo actual **cerrados**; gaps restantes son cobertura de prueba, no bugs confirmados sin spec.
