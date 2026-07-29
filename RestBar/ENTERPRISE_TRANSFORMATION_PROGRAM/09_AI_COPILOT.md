# 09 — AI COPILOT

**Producto:** RestBar Copilot  
**Diseño only.** No IA decorativa: acciones sustentadas en datos reales del tenant.

---

# 1. Principios

1. **Datos primero** — sin caja/compras/FC limpios, el copiloto miente  
2. **Prescriptivo** — cada respuesta incluye acción ejecutable  
3. **Multitenant** — contexto solo Company/Branch del usuario  
4. **Explicable** — cita KPI/fuente (“Food cost Grill 38% vs meta 32%”)  
5. **Humano en el loop** — no auto-PO sin confirmación en v1  
6. **Español LATAM** nativo  

---

# 2. Preguntas que debe responder

| Pregunta | Motor v1 (reglas) | Motor v2 (LLM+tools) |
|----------|-------------------|----------------------|
| ¿Qué debo hacer hoy? | Top 3 alertas score | Narrativa + plan |
| ¿Qué comprar? | Reorder algorithm | + negociación hint |
| ¿Qué vender/promocionar? | Menu engineering matriz | Copy promo |
| ¿Qué eliminar? | Low margin + low velocity | |
| ¿Qué producir? | Forecast × recipe | |
| ¿Cambiar proveedor? | Scorecard precio/fill | |
| ¿Sucursal ayuda? | Z-score ventas/FC/SLA | |
| ¿Gerente capacitar? | Gap vs peer KPIs | |
| ¿Empleado bajo? | Sales/time metrics | |
| ¿Riesgos semana? | Stockout, caja, SLA trend | |
| ¿Oportunidades hoy? | HH gap, upsell, mesa lenta | |
| ¿Acciones ejecutar? | Deep links módulos | |

---

# 3. Arquitectura

```
User question
    ▼
Intent router (rules / small classifier)
    ▼
Tool layer (solo lecturas + propuestas):
  - GetCommandCenterSnapshot
  - GetFoodCostVariance
  - GetReorderSuggestions
  - GetMenuEngineering
  - GetBranchBenchmark
  - GetStationSLA
  - GetCustomerChurn
    ▼
Reasoning (rules engine → later LLM)
    ▼
Action cards: [Crear PO borrador] [86 producto] [Abrir alerta] [Ignorar]
    ▼
Audit log de recomendaciones y aceptación
```

**Año 1:** Rules engine + templates (sin dependencia LLM cara).  
**Año 2:** LLM con function calling sobre las mismas tools.

---

# 4. Action cards (UX)

```
⚠️ RIESGO: Stock 0 en “Lomo” mañana (velocity 18/día, stock 12)
Impacto: ~$420 ventas perdidas
Acción: Crear PO a Proveedor X — 40 ud — ETA jueves
[Crear borrador PO] [Ver recetas afectadas] [Descartar]
```

---

# 5. Guardrails

- No inventar proveedores/precios  
- No ejecutar pagos/cancellations  
- No cruzar tenants  
- Rate limit + costo token (si LLM)  
- Modo “solo gerente+”  

---

# 6. Prerrequisitos (bloqueantes)

| Prerrequisito | Por qué |
|---------------|---------|
| Caja | Riesgos financieros reales |
| PO + recepción | Comprar con sentido |
| Recipe costing | Eliminar/promocionar platos |
| Command Center | Contexto unificado |
| BI aggregates | Latencia y consistencia |
| Export/audit | Confianza |

**No lanzar Copilot comercial antes de Business plan capabilities.**

---

# 7. Métricas de éxito Copilot

| Métrica | Meta |
|---------|------|
| % recomendaciones aceptadas | >35% a 90 días |
| Tiempo a decisión gerente | −50% |
| Stockouts evitados | Medible |
| NPS gerente | +10 pts |

---

# 8. Diferenciación competitiva

Toast/Square tienen insights; pocos en LATAM ofrecen **copiloto accionable en español** atado a food cost + multi-sucursal. Esa es la apuesta de marca — **después** del core.
