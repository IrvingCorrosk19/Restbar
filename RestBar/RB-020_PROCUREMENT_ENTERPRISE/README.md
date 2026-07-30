# RB-020 — PROCUREMENT ENTERPRISE PLATFORM

**Supreme Edition · Fase 2 Cost Control**  
**Estado:** DISEÑO COMPLETO + **IMPLEMENTACIÓN v1 DESARROLLO PASS**  
**Fecha:** 2026-07-29

Ver `IMPLEMENTATION_PROGRESS.md` y `CERTIFICATION_RESULTS.md`.

Feature flag: `EnablePurchasingModule=false` hasta UAT.

---

## Regla suprema

No construir un módulo de compras. Construir el **cerebro financiero** que controla el costo del restaurante.

Cada línea de código debe ahorrar dinero, reducir desperdicio, mejorar margen, o prevenir fraude.

---

## Veredicto diseño

**APTO PARA IMPLEMENTACIÓN FASE A**

---

## Documentos

| # | Archivo |
|---|---------|
| 01 | [Business Analysis](01_BUSINESS_ANALYSIS.md) |
| 02 | [Requirements](02_REQUIREMENTS.md) |
| 03 | [Domain Model](03_DOMAIN_MODEL.md) |
| 04 | [Database Design](04_DATABASE_DESIGN.md) |
| 05 | [Architecture](05_ARCHITECTURE.md) |
| 06 | [Supplier Engine](06_SUPPLIER_ENGINE.md) |
| 07 | [Purchase Request](07_PURCHASE_REQUEST.md) |
| 08 | [Purchase Order](08_PURCHASE_ORDER.md) |
| 09 | [Goods Receipt](09_GOODS_RECEIPT.md) |
| 10 | [Cost Engine](10_COST_ENGINE.md) |
| 11 | [Supplier Score](11_SUPPLIER_SCORE.md) |
| 12 | [Approval Workflow](12_APPROVAL_WORKFLOW.md) |
| 13 | [Audit Model](13_AUDIT_MODEL.md) |
| 14 | [Security Model](14_SECURITY_MODEL.md) |
| 15 | [Procurement Command Center](15_PROCUREMENT_COMMAND_CENTER.md) |
| 16 | [Reports](16_REPORTS.md) |
| 17 | [KPIs](17_KPIS.md) |
| 18 | [Test Plan](18_TEST_PLAN.md) |
| 19 | [Implementation Plan](19_IMPLEMENTATION_PLAN.md) |
| 20 | [Final Certification](20_FINAL_CERTIFICATION.md) |

---

## Principios de integración

| Existente | Acción |
|-----------|--------|
| Product / Product.Cost | Extender — Cost Engine actualiza |
| InventoryMovement (Purchase) | Extender — FK a GoodsReceipt, UnitCost |
| Recipe / RecipeLine | Reusar — theoretical food cost |
| Station / ProductStockAssignment | Reusar como ubicación (NO Warehouse nuevo) |
| Supplier JS stub | Reemplazar con entidad real |
| SupplierAnalysis (ceros) | Conectar a datos reales |
| EnablePurchasingModule | Feature flag gate |
| PurchasingAccess / CostingAccess | Activar en controllers |
| RB-010 Cash | Intacta — sin acoplamiento |

---

## Backlog cubierto

RB-020 · RB-021 · RB-022 · RB-023 (costeo teórico) · RB-024 (food cost %) · RB-032 (SupplierAnalysis real)
