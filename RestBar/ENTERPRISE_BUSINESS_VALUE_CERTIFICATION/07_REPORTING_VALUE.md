# 07 — VALOR DE REPORTES (Fase 7)

---

# REPORTES POR ROL — EXISTENCIA Y UTILIDAD

| Rol | ¿Existe? | ¿Ayuda a decidir? | Evidencia |
|-----|----------|-------------------|-----------|
| **CEO** | ⚠️ | Parcial | DashboardStats, branch sales — sin P&L ni forecast |
| **CFO** | ❌ | No | Sin prime cost, EBITDA, export contable |
| **COO** | ⚠️ | Parcial | Station performance, table util — sin SLA |
| **CTO** | ⚠️ | N/A producto | Audit logs; sin métricas SaaS |
| **Gerente General** | ⚠️ | Parcial | Sales + inventory API; UI AdvancedReports 3 sin JS |
| **Operaciones** | ⚠️ | Parcial | KDS ops; sin labor scheduling |
| **Compras** | ❌ | No | 404 PurchaseOrder/Supplier |
| **Inventario** | ⚠️ | Parcial | InventoryAnalysis; sin PO |
| **Chef Ejecutivo** | ⚠️ | Parcial | Station + top products; sin food cost real |
| **Supervisor** | ⚠️ | Parcial | Order status; sin floor map analytics |
| **Auditor** | ⚠️ | Parcial | AuditLog; sin SOX pack |
| **Franquicias** | ⚠️ | Parcial | Multitenant branch compare |
| **Contabilidad** | ❌ | No | Export PDF/Excel stub |
| **Ventas** | ⚠️ | Parcial | Employee/category sales |
| **Marketing** | ❌ | No | Sin promo analytics |

---

# HALLAZGOS CRÍTICOS

1. **APIs reales vs UI:** AdvancedReports tiene endpoints funcionales pero **3 vistas sin JavaScript** conectado.
2. **Export:** Stub — gerente **no puede** sacar Excel/PDF producción sin dev.
3. **SupplierAnalysis:** Stub documentado.
4. **Utilidad real:** Reportes **sirven para piloto** (ventas, mesero, sucursal). **No sirven** para CFO ni compras.

**Score reportes: 38/100**
