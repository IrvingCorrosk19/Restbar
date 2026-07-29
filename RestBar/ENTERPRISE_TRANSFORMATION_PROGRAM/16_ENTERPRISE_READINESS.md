# 16 — ENTERPRISE READINESS

Qué falta para RFPs de cadena / franquicia (no hotel Oracle).

---

# 1. Controles enterprise

| Control | Hoy | Target |
|---------|-----|--------|
| Multitenant isolation | ✅ Cert | Mantener + global filters EF opc. |
| RBAC | ✅ Roles/policies | + segregación caja vs mesero |
| Audit trail | ✅ | + cash & PO events |
| Soft delete / immutable payments | Parcial | Fortalecer |
| Environment seed lockdown | ❌ | ✅ F0 |
| Backup/restore | Stub | Job real F4 |
| Secrets management | Dev-ish | Vault/prod |
| Rate limiting | Existe (429 visto) | Tunear franquicia |

---

# 2. Operación multi-unidad

| Capacidad | Hoy | Target |
|-----------|-----|--------|
| Company/Branch hierarchy | ✅ | + Franchisee entity opcional |
| Cross-branch reports | Parcial | Command Center heatmap |
| Central menu policies | Débil | Brand lock F5 |
| Rollout config | Manual | Templates onboarding |

---

# 3. Cumplimiento

| Área | Target 24m |
|------|------------|
| Fiscal 1 país | F3 |
| Factura electrónica | Adapter |
| PCI (si card present) | Partner gateway; no store PAN |
| Privacidad datos clientes | Policy + retention CRM |
| SOC2 | Camino año 2–3 si enterprise US |

---

# 4. SLA / Non-funcionals

| Métrica | Target Chain |
|---------|--------------|
| Disponibilidad | 99.5% → 99.9% |
| RPO/RTO | <1h / <4h |
| Command Center | <5s |
| Concurrent users / branch | Probado load test |
| Offline | F5 |

---

# 5. Enterprise readiness score

| Área | Hoy | 12m | 24m |
|------|-----|-----|-----|
| Security | 55 | 70 | 85 |
| Multi-unit | 60 | 80 | 90 |
| Compliance | 20 | 55 | 75 |
| Reliability | 45 | 65 | 85 |
| Support model | 30 | 50 | 75 |
| **Enterprise overall** | **~35** | **~65** | **~82** |

---

# 6. Definition of Done “Enterprise LATAM”

- Business plan live en ≥3 cadenas  
- Caja+PO+FC+CC en prod  
- Fiscal 1 país  
- Audit pack CFO  
- Contratos SLA  
- Scorecard madurez ≥85
