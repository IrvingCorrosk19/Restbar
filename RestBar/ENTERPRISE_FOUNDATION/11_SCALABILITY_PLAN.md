# 11 — SCALABILITY PLAN

---

# Dimensiones de escala

| Dimensión | Hoy | 5 años | Estrategia |
|-----------|-----|--------|------------|
| Branches / company | Decenas | Cientos | Índices + CC aggregates + bi schema |
| Companies (SaaS) | Pocas | Miles | Tenant filters estrictos; no DB-per-tenant aún |
| Concurrent POS users | Baja | Alta | SignalR scaleout Redis (cuando haga falta) |
| Order volume | Medio | Alto | Particionar analytics; OLTP caliente limpio |
| Report concurrency | Bajo | Medio | Cache + jobs |
| KDS stations | Multi | Multi×N | Ya preparado por groups |

---

# Lo que NO hacer prematuro

- Kubernetes multi-service split  
- Event sourcing completo  
- Sharding por CompanyId  
- Cosmos/NoSQL paralelo  

---

# Escalones

1. **Vertical** — índices, AsNoTracking, projections (ahora)  
2. **Cache** — distributed cache ya registrado; usar en CC  
3. **Jobs** — hosted services BI/backup  
4. **SignalR Redis backplane** — cuando >1 nodo  
5. **Read replica** — reportes pesados  

---

# Franquicias / multi-país

- Company = franchisee; Brand policies después  
- Fiscal adapter por Company.CountryCode (campo futuro)  
- Un deployment multi-tenant preferido a forks por país  
