# 09 — PERFORMANCE PLAN

---

# Hallazgos

| Área | Problema | Plan |
|------|----------|------|
| KDS | Includes profundos + filter in-memory | Proyectar DTOs; filtrar status en SQL; AsNoTracking |
| Order GetAll | Include graph completo | Paginación + projection |
| InventoryOps | Round-trips por línea | Batch stock updates |
| AdvancedReports | Queries repetidas | Snapshot tables / cache 60s |
| PaymentView stats | Multi-calls | Un aggregate query |
| SignalR | Fanout amplio | Groups por branch |
| EF tracking | Listados trackean | AsNoTracking default en queries read |
| Índices | Faltan compuestos | Migration F0.5 |
| Command Center futuro | N+1 widgets | `/api/command-center/snapshot` cached |

---

# Targets

| Métrica | Target |
|---------|--------|
| StationOrders P95 | < 500ms |
| POS add item | < 300ms |
| Command Center snapshot | < 5s (cache OK) |
| Report export | Async job |

---

# Concurrencia

- Order concurrency token — mantener  
- CashSession futuros: row version  
- No static mutable state en services (scoped OK)

---

# Compilación / alloc

- Evitar `ToList()` intermedios innecesarios en OrderService extracción  
- Reusar compiled queries EF para KDS hot path (fase F0.6+)
