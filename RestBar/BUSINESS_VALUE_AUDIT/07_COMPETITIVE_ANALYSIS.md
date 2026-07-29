# 07 — ANÁLISIS COMPETITIVO

**Metodología:** Comparar solo **capacidades verificadas de RestBar** vs **categorías estándar** de Toast, Oracle Hospitality (Simphony/Micros), NCR Aloha, Lightspeed, Square.  
**No se afirman features específicas de competidores** — solo gaps relativos donde RestBar está documentado como ausente o parcial.

---

## Matriz resumida

| Capacidad | RestBar | Implicación vs mercado |
|-----------|---------|------------------------|
| POS mesa + orden | ✅ Certificado | Paridad básica |
| KDS multi-estación | ✅ Fuerte | Diferenciador en mid-market |
| Multitenant nativo | ✅ 51/51 | Mejor que muchos POS single-tenant |
| Pagos parcial/mixto/idempotencia | ✅ | Paridad mid |
| Split bill | ✅ API | Paridad |
| SignalR tiempo real | ✅ | Paridad moderna |
| **Caja / arqueo** | ❌ | **Debilidad crítica vs todos** |
| **Fiscal / precuenta** | ❌ | **Debilidad crítica** |
| **Impresión térmica** | ❌ | Debilidad vs Toast/Square/Aloha |
| **Compras / PO / proveedores** | ❌ | Debilidad vs Oracle/Lightspeed back-office |
| Combos | ❌ | Debilidad vs Toast/Square |
| Happy hour pricing | ❌ | Debilidad |
| Onboarding / import POS | ❌ | Debilidad SaaS |
| Reportes export PDF/Excel | Stub | Debilidad |
| Forecast analytics | Vacío | Debilidad vs enterprise |
| Hardware ecosystem | No evidenciado | Debilidad vs Square/Toast |
| Offline mode | No certificado | Riesgo vs Aloha legacy |
| SaaS billing tiers | ❌ | Debilidad |
| Mobile waiter app | Web responsive | Parcial vs apps nativas |
| Delivery marketplace | ❌ UI | Debilidad |
| Hotel/PMS | ❌ | No compite con Oracle |

---

## Fortalezas RestBar (evidencia)

1. **Multitenant empresa/sucursal** listo para cadena regional sin reinstalar.
2. **KDS routing complejo** (multi-piso, multi-bar) certificado 15/15 routing.
3. **Costo de despliegue** potencialmente menor (open stack .NET + PostgreSQL).
4. **Personalización** — código propio vs configuración rígida (ventaja consultora, no cliente final).
5. **Auditoría y roles** — 11 roles, logs, aislamiento tenant.

---

## Debilidades vs cualquier líder

1. No es **sistema único** para operar restaurante regulado (caja+fiscal+compras).
2. No es **SaaS self-service** vendible en website con onboarding.
3. Reportes ejecutivos **a medias** (APIs sí, entrega ejecutiva no).
4. Marca/ecosistema **cero** vs Toast/Square (hardware, partners, integraciones).

---

## Oportunidades de posicionamiento

| Posición | Mensaje honesto |
|----------|-----------------|
| **Cadena regional LATAM** | "POS+KDS multitenant sin licencia Oracle" |
| **Dark kitchen / multi-marca** | Tenant aislado por marca en una plataforma |
| **Consultora implementadora** | Base customizable + certificación funcional propia |
| **NO competir** | Quick-service enterprise, hotel, franquicia US/EU fiscal |

---

## Funcionalidades faltantes para paridad mínima de mercado

1. Caja
2. Precuenta + ticket térmico
3. Fiscal (al menos un país piloto)
4. Combos
5. Export reportes
6. Import datos POS
