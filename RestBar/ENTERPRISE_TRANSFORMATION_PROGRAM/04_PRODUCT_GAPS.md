# 04 — PRODUCT GAPS

Brechas clasificadas por impacto en **dinero / operación / decisión**. Solo gaps que fallan la regla de valor.

---

# 1. Gaps críticos (bloquean 41→70)

| ID | Gap | Tipo | Impacto | Building block existente |
|----|-----|------|---------|--------------------------|
| G01 | Caja / arqueo / cierre Z | Operativo + Financiero + Legal | Fraude, no cierre diario | Extender `Shift` |
| G02 | Precuenta + impresión | Operativo + CX | −rotación mesa | Extender `Invoice` / Order |
| G03 | Fiscal configurable | Legal + Comercial | No vende regulado | `InvoiceService` |
| G04 | Proveedor + PO + recepción | Financiero + Compras | Food cost ciego | `InventoryMovement` + JS Supplier |
| G05 | Food Cost / recipe costing | Financiero | Margen invisible | `Recipe` + PO cost |
| G06 | Export PDF/Excel real | Gerencial | Excel paralelo | Reports stubs |
| G07 | Combos | Comercial + Ingresos | Ticket plano | Nuevo ligero + Order |
| G08 | Happy Hour / promo engine | Comercial | Bar underperforms | `PriceSchedule` + `DiscountPolicy` |

---

# 2. Gaps altos (70→82)

| ID | Gap | Audiencia |
|----|-----|-----------|
| G09 | Merma con motivo/costo | Chef, CFO |
| G10 | Command Center ejecutivo | Dueño, gerente |
| G11 | UI Recetas / costeo | Chef |
| G12 | UI Shift + handoff | Supervisor |
| G13 | Conectar AdvancedReports JS | Gerente |
| G14 | CRM mínimo + loyalty usable | Marketing, dueño |
| G15 | Alertas proactivas (stock, SLA, caja) | Ops |
| G16 | Reorder sugerido | Compras |

---

# 3. Gaps medios (82→90)

| ID | Gap |
|----|-----|
| G17 | Forecast ventas / demanda |
| G18 | Labor scheduling + labor cost |
| G19 | Delivery / agregadores |
| G20 | Reservas / waitlist |
| G21 | Gift cards |
| G22 | Pack franquicia / royalties |
| G23 | Offline POS |
| G24 | Copilot IA |

---

# 4. Gaps por experiencia de persona

| Persona | Gap principal | Severidad |
|---------|---------------|-----------|
| Propietario | No ve margen ni “qué hacer hoy” | Crítica |
| Gerente | Reportes sin export / UI rota | Alta |
| Franquiciador | Sin benchmark pack | Alta |
| Contador | Sin fiscal/export | Crítica |
| Auditor | Audit OK; sin cierre caja | Alta |
| Chef | KDS OK; sin receta UI / merma | Media-Alta |
| Comprador | Módulo 404 | Crítica |
| Supervisor | Sin floor command | Media |
| Mesero | Sin precuenta / upsell | Alta |
| Cajero | Sin caja | Crítica |
| Inversionista RestBar | Sin SaaS billing/onboarding | Alta |
| Cliente final | Sin loyalty / tiempo estimado | Media |

---

# 5. Gaps competitivos (vs mercado)

| Capacidad table-stakes Toast/Square/R365 | RestBar |
|------------------------------------------|---------|
| Cash management | ❌ |
| Invoicing / fiscal | ❌/stub |
| Purchasing | ❌ |
| Recipe costing | Parcial schema |
| Promotions | Parcial |
| Loyalty | Campo muerto |
| Labor | ❌ |
| Inventory + waste | Parcial |
| Multi-location analytics | Parcial |
| Open API / marketplace | ❌ |

---

# 6. Gaps de experiencia (UX producto)

- Menús a pantallas rotas (Supplier, AdvancedSettings)  
- Duplicidad Payment / PaymentView / User admin  
- Export botones que no exportan (rompe confianza)  
- Seed expuesto  
- Sin onboarding día-1  

**Principio:** ocultar o reparar superficies rotas **antes** de marketing.

---

# 7. Qué NO es gap prioritario

| Idea | Por qué posponer |
|------|------------------|
| Casino / hotel / crucero | Fuera de ICP 24m |
| Payroll nativo | Integrar, no construir |
| ML avanzado día 1 | Sin data warehouse limpio |
| Marketplace apps | Después de core Business plan |
| Reescribir frontend SPA | No acelera ROI vs completar módulos |

---

# 8. Mapa gap → valor

```
G01 Caja ──────────► Ahorro fraude + control
G02 Precuenta ─────► Ingreso (rotación)
G04+G05 Compras/FC ► Ahorro margen
G07+G08 Combos/HH ─► Ingreso ticket
G10+G15 Dashboard ─► Decisiones
G17+G24 Forecast/IA► Automatización (después)
```
