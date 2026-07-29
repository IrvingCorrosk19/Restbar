# 09 — MULTITENANT VALIDATION (PKS + Functional Cases)

**Fecha:** 2026-07-28  
**Actualizado con:** suite `Run-MultitenantFunctionalCases.ps1` (51 casos)

---

## Veredicto

**PASS** — aislamiento Company/Branch verificado en local (51/51) y VPS (retest OK).

---

## Evidencia viva

| Check | Resultado |
|-------|-----------|
| `admin.b@restbar.com` sesión | PASS |
| Mesas A ≠ B | PASS |
| Mesas Costa ≠ Norte ≠ Sur | PASS |
| Catálogo exclusivo por empresa | PASS |
| IDOR pago/cancel cross-company | PASS (403) |
| MoveToTable cross-company | PASS (400/403) |
| Branch Centro vs Norte | PASS (403) |
| Flujo pago A/B/Costa/Norte/Sur | PASS |

Detalle completo: `15_MULTITENANT_FUNCTIONAL_CASES.md`

---

## Gaps multitenant ligados a PKS

| Tema | Estado |
|------|--------|
| Inventario Purchase con CompanyId | Loguea movimiento; no hay documento PO por tenant |
| Rate-limit login en VPS | Puede bloquear suites masivas (429); no es fuga de datos |

---

## Conclusión

No se detectaron fugas de datos entre empresas/sucursales en los vectores funcionales ejecutados.
