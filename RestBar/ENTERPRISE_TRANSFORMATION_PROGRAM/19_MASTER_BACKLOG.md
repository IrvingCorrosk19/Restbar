# 19 — MASTER BACKLOG

Backlog maestro priorizado. ID estable para tracking.

**Valor:** G=Gana · A=Ahorra · C=Controla · D=Decide · S=SaaS RestBar  
**Esfuerzo:** S M L XL

---

# Ahora — P0 (hacer primero)

| ID | Ítem | Valor | Esf | Fase | Depende |
|----|------|-------|-----|------|---------|
| RB-001 | Ocultar/deshabilitar stubs (Supplier report, export vacío, settings rotas) | C | S | F0 | — |
| RB-002 | Unificar entry Payment → PaymentView; User admin | C | S | F0 | — |
| RB-003 | Lock Seed en no-Development | C | S | F0 | — |
| RB-004 | Conectar JS AdvancedReports faltantes | D | M | F0 | — |
| RB-010 | CashRegister + CashSession + movimientos | A C | L | F1 | Shift | **✅ Implementado 2026-07-29** (flag off) |
| RB-011 | Arqueo y diferencia caja | A C | M | F1 | RB-010 | **✅ Implementado** (Arqueo/Reconciliation) |
| RB-012 | Precuenta + impresión ticket | G C | L | F1 | Order/Invoice |
| RB-013 | Export Excel/PDF ventas e inventario | D | M | F1 | Reports |
| RB-014 | Z-Report / cierre día | A C D | M | F1 | RB-010 | **✅ Z Report implementado** |
| RB-015 | Shift UI Start/End/Handoff | C | M | F1 | Shift API |
| RB-020 | Supplier entity + controller + views (reusar JS) | A | M | F2 | —  | **✅ Implementado 2026-07-29** (flag off) |
| RB-021 | PurchaseOrder + lines + estados | A | L | F2 | RB-020  | **✅ Implementado** |
| RB-022 | Goods receipt → InventoryMovement + costo | A C | L | F2 | RB-021  | **✅ Implementado** |
| RB-023 | Recipe UI + costeo teorico | A D | L | F2 | Recipe | **OK Food Cost Engine + UI 2026-07-29** |
| RB-024 | Food cost % dashboard | A D | M | F2 | RB-022/023 | **OK Food Cost Command Center** |

---

# Siguiente — P1

> **RB-030 BI/Executive Command Center** implementado 2026-07-29 (EnableCommandCenter=false). Pack: RB-030_BUSINESS_INTELLIGENCE_ENTERPRISE.  
> **RB-040 AI Copilot / Director Operativo** implementado 2026-07-29 (EnableCopilot=false). Pack: RB-040_AI_COPILOT_ENTERPRISE. Cubre RB-080/081 v1 (rules + tools; LLM remoto pendiente).

| ID | Ítem | Valor | Esf | Fase |
|----|------|-------|-----|------|
| RB-030 | Merma con motivo y costo | A | M | F2  | **parcial WasteEvent RB-023** |
| RB-031 | Actual vs theoretical variance | A D | M | F2 |
| RB-032 | SupplierAnalysis con datos reales | D | S | F2 |
| RB-040 | Combos/bundles en POS | G | L | F3 | *(ID histórico POS; pack producto AI = RB-040_AI_COPILOT)* |
| RB-041 | Happy Hour / promo engine (PriceSchedule UI) | G | L | F3 |
| RB-042 | Fiscal adapter país piloto | C S | XL | F3 |
| RB-050 | Command Center snapshot API + UI | D | L | F4  | **OK via RB-030 BI CC** |
| RB-051 | Alert rules (stock, SLA, caja) | D C | M | F4 |

---

# Después — P2

| ID | Ítem | Valor | Esf | Fase |
|----|------|-------|-----|------|
| RB-060 | Customer POS capture + loyalty points usable | G | M | F3 |
| RB-061 | Upsell suggestions | G | S | F3 |
| RB-070 | BI schema + nightly aggregation job | D | L | F4  | **parcial ForecastSeed RB-030** |
| RB-071 | Forecast ventas + reorder suggestions | A D | L | F4 |
| RB-072 | SaaS plans + billing + onboarding | S | XL | F4 |
| RB-073 | Backup job real | C | S | F4 |
| RB-074 | Tips/commission admin UI | C | M | F3 |

---

# Más tarde — P3

| ID | Ítem | Valor | Esf | Fase |
|----|------|-------|-----|------|
| RB-080 | Copilot rules engine + action cards | D | L | F5 | **✅ v1 en RB-040** |
| RB-081 | Copilot LLM tools | D | XL | F5 | **parcial tools v1; LLM remoto pendiente** |
| RB-082 | Labor scheduling light | A | L | F5 |
| RB-083 | Delivery connectors | G | L | F5 |
| RB-084 | Franchise pack + royalties report | S D | L | F5 |
| RB-085 | Offline POS mode | C | XL | F5 |
| RB-086 | Accounting export/API | D | M | F5 |
| RB-087 | Reservations / waitlist | G | L | F5 |
| RB-088 | Gift cards | G | M | F5 |
| RB-089 | EF global tenant filters | C | M | F4–5 |

---

# Icebox (no construir salvo demanda pagada)

| ID | Ítem | Motivo |
|----|------|--------|
| RB-X01 | Hotel PMS / casino | Fuera ICP |
| RB-X02 | Payroll nativo | Integrar |
| RB-X03 | App marketplace | Prematuro |
| RB-X04 | Reescritura SPA total | No ROI 24m |
| RB-X05 | Multi-país fiscal simultáneo | Tras 1 país |
| RB-X06 | Deep ML stock | Tras forecast v1 |

---

# Eliminar / no invertir (deuda producto)

| ID | Acción |
|----|--------|
| RB-D01 | Deprecar menú a AdvancedSettings sin vista |
| RB-D02 | Deprecar ProductCategory legacy si Categories cubre |
| RB-D03 | Remover o gate SupplierAnalysis hasta RB-020 |
| RB-D04 | Fusionar UserManagement overlapping |
| RB-D05 | Quitar botones export stub o marcar “próximamente” honesto |

---

# Orden de ejecución sugerido (primeros 15)

1. RB-001 → 2. RB-003 → 3. RB-002 → 4. RB-004 →  
5. RB-010 → 6. RB-011 → 7. RB-012 → 8. RB-015 → 9. RB-014 → 10. RB-013 →  
11. RB-020 → 12. RB-021 → 13. RB-022 → 14. RB-023 → 15. RB-024
