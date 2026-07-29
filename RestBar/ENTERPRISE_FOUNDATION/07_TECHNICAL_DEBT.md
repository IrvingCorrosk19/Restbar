# 07 — TECHNICAL DEBT

Priorizado por riesgo × bloqueo al roadmap.

---

# P0 — Pagar en Foundation 0.5

| ID | Deuda | Riesgo | Fix |
|----|-------|--------|-----|
| TD-01 | Menú Payment apunta a API | UX rota | Redirect menú PaymentView |
| TD-02 | Typo `/Report/Index` accountant | 404 | `/Reports/Index` |
| TD-03 | Seed AllowAnonymous fuera de Development | Seguridad | Gate Development only |
| TD-04 | Password en RestBarContext.OnConfiguring | Secrets | Remover; fail si no configurado |
| TD-05 | Sin proyecto de tests | Regresiones | Crear RestBar.Tests smoke |
| TD-06 | Sin policies Cash/Purchasing/Costing | Roadmap | Agregar policies vacías |
| TD-07 | Sin TenantScope helper | IDOR futuro | Infrastructure helper |
| TD-08 | Sin feature flags | Stubs visibles | IOptions FeatureFlags |
| TD-09 | Índices compuestos faltantes | Perf KDS/reports | Migration índices |

---

# P1 — Primer mes post-foundation

| ID | Deuda | Fix |
|----|-------|-----|
| TD-10 | OrderService God object | Extracción interna detrás facade |
| TD-11 | Filtros tenant inconsistentes GetById | Usar TenantScope en mutaciones |
| TD-12 | Dual DbContext registration | Un solo registro |
| TD-13 | Export PDF/Excel stub | Implementar o hide |
| TD-14 | Backup fake | HostedService real o hide UI |
| TD-15 | ProductCategory legacy | Migration deprecate |

---

# P2 — Trimestre

| ID | Deuda |
|----|-------|
| TD-16 | User admin dual UI merge |
| TD-17 | Controllers Shift/Recipe sin service layer |
| TD-18 | AdvancedReports split |
| TD-19 | AsNoTracking en listados Kitchen |
| TD-20 | SignalR group authorization by tenant |

---

# P3 — Después

| ID | Deuda |
|----|-------|
| TD-21 | EF global tenant filters |
| TD-22 | Soft-delete formal |
| TD-23 | Outbox / domain events |
| TD-24 | Read replica BI |

---

# Interés compuesto

Cada mes que OrderService crezca +200 LOC sin extracción, el costo de Cash/PO se multiplica. Foundation debe **congelar** nuevas features dentro de OrderService: solo hooks/interfaces.
