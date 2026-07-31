# 03 — Competitive Analysis

**Regla:** no favorecer artificialmente a RestBar.

| Dimensión | RestBar | Toast | R365 | Oracle Hosp. | Aloha | Square | Lightspeed | TouchBistro | Revel |
|-----------|---------|-------|------|--------------|-------|--------|------------|-------------|-------|
| POS+KDS nativo | Fuerte | Fuerte | Débil/partner | Fuerte | Fuerte | Fuerte | Fuerte | Fuerte | Fuerte |
| Caja enterprise + audit hash | Fuerte | Medio | N/A | Fuerte | Fuerte | Medio | Medio | Medio | Medio |
| Inv+Compras+FC unificado | Fuerte | Medio | **Fuerte** | Fuerte | Medio | Débil | Medio | Débil | Medio |
| BI / Forecast / Rules | Medio (piloto) | Medio+ | Medio+ | Fuerte | Medio | Medio | Medio | Débil | Medio |
| Pagos / offline | **Débil** | **Excelente** | N/A | Fuerte | Fuerte | **Excelente** | Fuerte | Medio | Fuerte |
| Ecosistema / marca | Débil | Excelente | Fuerte | Excelente | Fuerte | Excelente | Fuerte | Medio | Medio |
| Multi-tenant self-host | Fuerte | SaaS | SaaS | On-prem/cloud | Variado | SaaS | SaaS | SaaS | Cloud |
| Costo / soberanía datos | **Ventaja** | Alto | Alto | Muy alto | Alto | Medio | Medio-Alto | Medio | Alto |
| Implantación | Media (partner) | Rápida US | Contable | Larga | Larga | Rápida | Media | Media | Media |
| Escalabilidad evidenciada | No 5k lab | Sí | Sí | Sí | Sí | Sí | Sí | Media | Sí |

## Lectura honesta

- **RestBar gana** en vertical integration self-hosted (POS→FC→BI→Rules) para mid-market LATAM con partner.  
- **RestBar pierde** frente a Toast/Square en pagos+offline+ecosistema; frente a Oracle/Aloha en enterprise hotelero global; frente a R365 en profundidad back-office contable US.  
- **No** reclamar paridad Toast/Oracle.
