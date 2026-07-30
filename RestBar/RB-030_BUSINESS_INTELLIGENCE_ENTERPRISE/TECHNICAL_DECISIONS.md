# 19 — TECHNICAL DECISIONS

1. **Orquestar** SalesReport + Cash + Procurement + FoodCost — cero motores de costo/caja nuevos.  
2. **Insight/Alert/Score** son reglas determinísticas (prep para Copilot RB-040).  
3. **Cache** ConcurrentDictionary 30s por BranchId.  
4. **ForecastSeed** solo histórico — sin ML.  
5. **EnableCommandCenter** (flag existente Foundation) — no crear flag duplicado.  
6. Persistencia best-effort: fallo de save no tumba el CC.  
7. AdvancedReports God object **no** expandido.
