# 02 — Competitive Matrix

**Method:** Capacidad RestBar = evidencia código/tests. Competidores = capacidades **públicamente conocidas** de categoría (no audit de código ajeno). Donde RestBar no tiene evidencia → competidor gana.

Leyenda impacto: **Alto** = decide compra · **Medio** · **Bajo**

| Capacidad | RestBar | Toast / Square / Lightspeed | Oracle Simphony / Aloha / Revel | R365 | Diferencia | Impacto | Prioridad |
|-----------|---------|-----------------------------|----------------------------------|------|------------|---------|-----------|
| POS mesa + KDS tiempo real | Sí (certificado browser) | Sí + hardware ecosystem | Sí enterprise | Débil (más back-office) | Paridad operativa core | Alto | — |
| Offline / flaky network | No | Sí (típico) | Sí (típico) | N/A | **Competidor gana** | **Crítico** | P0 comercial |
| Procesador de pagos integrado | No (manual/métodos string) | Sí nativo | Sí / partners | Integraciones | **Competidor gana** | **Crítico** | P0 |
| Caja con arqueo/X/Z/auditoría | Sí RB-010 | Sí | Sí | Parcial | Paridad | Alto | — |
| Inventario + merma | Sí | Variable | Sí | **Fuerte** | R365/Toast gana profundidad WMS | Alto | P1 |
| Compras / PO / proveedores | Sí RB-020 | Variable | Sí | **Fuerte** | Cerca R365 en mid-market | Alto | — |
| Food Cost / recetas / menu eng | Sí RB-023 | Add-ons / partners | Sí | **Fuerte** | Ventaja vs POS puros sin costing | Alto | — |
| Analytics nativo sin Power BI | Sí RB-025 | Dashboards cloud | BI enterprise caro | Fuerte reporting | **Ventaja costo/simplicidad** mid-market | Alto | — |
| Multisucursal / multi-empresa | Sí lógico | Sí cloud | Sí | Sí | Paridad lógica; escala no probada | Alto | P1 |
| App móvil nativa mesero | Web responsive | Apps nativas | Apps / thin | Apps | **Competidor gana UX móvil** | Alto | P1 |
| Marketplace / integraciones | No | Extenso | Extenso | Contabilidad | **Competidor gana** | Alto | P1 |
| Instalación on-prem / Docker propio | Sí (Docker VPS) | Cloud-first | On-prem/hybrid | Cloud | **Ventaja soberanía/datos** | Medio | — |
| Precio licencia opaque | Self-host costo infra | SaaS % + hardware | Enterprise contract | SaaS | RestBar puede ser más barato **si** ops propio | Alto | — |
| Soporte 24/7 / partner network | No evidenciado | Sí | Sí | Sí | **Competidor gana** | **Crítico** | P0 |
| Compliance PCI / certificaciones marca | Headers/hardening básico | PCI SAQ/partners | Enterprise | Contable | **Competidor gana** | Alto | P1 |
| Reservas / loyalty / marketing | No | Sí | Sí | Parcial | **Competidor gana** | Medio | P2 |
| Hotel / PMS | No | Partners | Fuerte Oracle | No | **Oracle gana** | Segmento | N/A |

## Conclusión competitiva (honesta)

- **No** supera a Oracle/Toast/Lightspeed/Square en ecosistema de pagos, offline, hardware y soporte global.  
- **Sí** puede competir en segmento **mid-market LATAM / self-hosted** donde el cliente valora: POS+KDS+Caja+Inventario+Compras+Food Cost+Analytics **en un solo producto** sin stack Power BI + R365 + POS separados.  
- Ventaja demostrable: **integración vertical nativa** (evidencia módulos RB-010→025 + browser PASS).  
- Desventaja decisiva de venta enterprise US: **pagos, offline, marca y red de partners**.
