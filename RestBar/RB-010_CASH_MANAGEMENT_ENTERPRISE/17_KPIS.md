# 17 — KPIs

---

# Operativos

| KPI | Fórmula | Target |
|-----|---------|--------|
| **Cash Variance** | \|Counted - Expected\| / Cash Sales | < 0.5% |
| **Cash Variance $** | Counted - Expected | branch threshold |
| **Cash Accuracy** | 1 - (Variance$ / Expected) | > 99.5% |
| **Average Cash Balance** | Avg Expected during session | benchmark |
| **Opening Time** | First session open vs business open | < 15 min |
| **Closing Time** | Last close vs business close | < 30 min |
| **Avg Close Duration** | Close start → ClosedAt | < 5 min |
| **Sessions Open > 12h** | Count | 0 ideal |

---

# Riesgo / fraude

| KPI | Fórmula |
|-----|---------|
| **Refund %** | Refunds / Total Sales |
| **Void %** | Voids / Transactions |
| **Supervisor Overrides** | Count / day |
| **Manager Overrides** | Count / day |
| **Unapproved Variances** | Count blocked |
| **Cash Risk Score** | Weighted: variance + void% + overrides + stale sessions (0-100) |
| **Daily Cash Integrity** | Hash chain valid ? 100 : 0 |

---

# Productividad

| KPI | Fórmula |
|-----|---------|
| **Cashier Productivity** | Transactions / hour / cashier |
| **Incidents per 1000 txs** | Incidents / txs * 1000 |

---

# Command Center widgets (doc 07 transformation)

| Widget | Data |
|--------|------|
| Caja abierta | Count sessions OPER |
| Tiempo abierta max | Max(now - OpenedAt) |
| Diferencia acumulada hoy | SUM variance closed |
| Alertas | Stale, pending approval, high void% |
| Top riesgo cajero | Cash Risk Score rank |

---

# BI storage

Materialized view `mv_cash_kpi_daily(branch_id, business_date, ...)` — nightly job post-implementation.

---

# Copilot hooks (future)

"¿Qué caja tiene riesgo hoy?" → Cash Risk Score > 70
