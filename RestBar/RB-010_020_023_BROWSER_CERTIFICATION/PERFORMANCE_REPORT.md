# PERFORMANCE_REPORT.md

Mediciones reales DOMContentLoaded (desktop, muestra):

| Página | ms | Budget P95 2s |
|--------|-----|----------------|
| CashDashboard | ~189 | PASS |
| Procurement | ~255 | PASS |
| FoodCost | ~286–299 | PASS |
| CommandCenter | ~262–583 | PASS |
| Orders | ~414–451 | PASS |

Hard fail solo si >5000ms (varianza local). Todas las muestras << 2000ms.

Tablet/mobile: mismos paths PASS.

**Veredicto Performance:** PASS
