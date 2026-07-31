# 23 — DATA INTEGRITY REPORT

| Check | Evidencia |
|-------|-----------|
| Unit tests | **77/77 PASS** |
| Order→kitchen flow | ORD-E2E PASS |
| Inventory after order | INV-ORD PASS |
| Cash session integrity | CASH-L* PASS |
| Payment foreign order | NEG-02 soft reject |

DB asserts SQL directos post-cada-flujo: no automatizados en Playwright (API shape checks sí).
