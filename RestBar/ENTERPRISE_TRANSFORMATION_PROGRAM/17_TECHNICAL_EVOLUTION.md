# 17 — TECHNICAL EVOLUTION

Evolución técnica **sin romper** arquitectura, multitenant ni performance.

---

# 1. Principios

1. Extender Services/Entities existentes  
2. Un bounded context por capacidad (Cash, Purchasing, …)  
3. Tenant en **toda** query nueva  
4. No microservicios prematuros  
5. Jobs hosted para BI/backup (hoy cero)  
6. Feature flags para módulos incompletos  

---

# 2. Evolución por capa

| Capa | Hoy | Evolución |
|------|-----|-----------|
| Domain | Models + EnterpriseOperations | Cash*, Supplier, PO, Combo; cost fields |
| Services | Fat OrderService | Extraer domain services; no duplicar Order |
| API | MVC + algunos JSON | Versioned `/api/v1` para CC/BI/Copilot |
| UI | Razor + JS | Mantener; componentes CC; no rewrite SPA año 1 |
| Realtime | OrderHub | Grupos `branch_{id}`, `cash_{id}`, alerts |
| Data | Un Postgres | Schema `bi_` + materialized views |
| Jobs | Ninguno | `IHostedService` / Hangfire ligero |
| Auth | Policies | Permission fine-grained cash/po |
| Integraciones | Email | Fiscal, accounting, delivery adapters |

---

# 3. Multitenant — no romper

- Toda entidad nueva: `CompanyId` (+ `BranchId` si aplica)  
- Tests aislamiento en suite MT (ampliar casos PO/Cash)  
- Considerar EF global filters en fase F2+ (opt-in)  
- SuperAdmin only cross-tenant  

---

# 4. Performance

| Riesgo | Mitigación |
|--------|------------|
| CC N+1 | Snapshot agregado cache Redis/memory 30–60s |
| BI heavy | ETL nightly; no query OLTP crudo en UI pico |
| SignalR fanout | Groups por branch/station |
| Reportes | Async export job |

---

# 5. Modularidad recomendada (folders)

```
/Domain/Cash
/Domain/Purchasing
/Domain/Costing
/Domain/Promotions
/Application/CommandCenter
/Application/Bi
/Infrastructure/Jobs
/Infrastructure/Fiscal
```

Sin separar deployables hasta escala real.

---

# 6. Deuda a pagar (técnica = producto)

| Deuda | Acción |
|-------|--------|
| Controllers/services huérfanos | Wire o remove public surface |
| Dual User admin | Merge |
| Category dual | Deprecate ProductCategory |
| Export byte[0] | Implement or hide |
| Backup Delay | Real pg_dump/S3 |
| Seed anonymous | Env gate |

---

# 7. Testing evolution

- Extender PKS a Cash + Purchasing  
- MT cases por módulo nuevo  
- Contract tests fiscal adapters  
- Load test CC + KDS  

---

# 8. Qué NO hacer

- Reescribir en otro stack  
- Event-sourcing completo año 1  
- Multi-DB por tenant prematuro  
- Duplicar Order pipeline para combos (expandir líneas)
