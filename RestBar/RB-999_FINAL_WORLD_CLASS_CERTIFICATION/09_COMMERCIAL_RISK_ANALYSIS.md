# 09 — Commercial Risk Analysis

## ¿Por qué un cliente NO compraría RestBar?

| # | Riesgo | Tipo | Evidencia | Impacto | Prob. | Recomendación mínima |
|---|--------|------|-----------|---------|-------|----------------------|
| R1 | Sin gateway de pagos / PCI | Comercial/Funcional | No processor | Alto | Alta | Partner de pagos + alcance contractual |
| R2 | Sin offline | Operativo | No sync queue | Alto | Alta | Solo locales con red estable; roadmap offline |
| R3 | Cobertura unit &lt;1% / sin integration | Técnico | RB-027 | Alto | Media | Harness API + unit Orders/Payment (calidad, no feature) |
| R4 | MT IDOR incompleto | Seguridad | RB-027 | Alto | Media | Suite IDOR cross-company antes de multi-tenant amplio |
| R5 | Demasiados hubs analíticos | UX | Layout | Medio | Alta | Hub único de insights (navegación) |
| R6 | Dependencia partner implantación | Comercial | Complejidad flags/caja | Medio | Alta | Paquete “piloto asistido” |
| R7 | MailKit moderate advisories | Seguridad | nuget audit | Bajo–Medio | Media | Upgrade path |
| R8 | No hiperescala evidenciada | Técnico | No 5k lab | Alto (si cliente lo exige) | Baja en piloto | No vender hyperscale |
| R9 | Support/SLA no productizado | Comercial | RB-026 Support parcial | Medio | Media | SLA escrito + canal |
| R10 | Expectativa Toast-parity | Comercial | Marketing risk | Alto | Media | Mensaje: vertical self-host mid-market |

## Posición de venta honesta

**Sí** para: cadena pequeña/mediana LATAM, online, partner, soberanía de datos, costo controlado.  
**No** para: QSR global, hotel Oracle, USA Toast-first, dark stores offline-first.
