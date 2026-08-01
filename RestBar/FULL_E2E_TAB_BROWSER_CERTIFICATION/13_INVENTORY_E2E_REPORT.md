# 13 — INVENTORY E2E REPORT (Tab Browser)

**Dominio:** Inventory index, movements, recipe consumption, stock transfer  
**Pack fecha:** 2026-08-01  
**Build:** Release deploy `871abc7`

| ID | Escenario | Estado | Notas |
|----|-----------|--------|-------|
| E2E-INV-01 | Inventory index + no stub | NOT STARTED (este pack) | Referencia: prior `inventory*` / RB packs — no re-run aquí |
| E2E-INV-02 | Recipe consumption post-order | NOT STARTED | Cadena POS→INV no ejecutada |
| E2E-INV-03 | Stock movement UI/API | NOT STARTED | — |
| StockTransfer | API-primary | NOT APPLICABLE (UI deep) | Primarily API; no deep browser cert este pack |
| Prior suite inventory | chromium-desktop | Referencia previa | 161 PASS baseline — global re-run IN PROGRESS |
| Unit Inventory recipe qty | Unit tests | Referencia | ~98 unit pass histórico |

## Gaps vs mandato

- Deep inventory E2E post-pedido no corrido  
- StockTransfer sin UI completa — no inventar PASS browser  
- Cross-tenant inventory IDOR pendiente

**Veredicto dominio Inventory:** FAIL vs mandato deep E2E (NOT STARTED en este pack).
