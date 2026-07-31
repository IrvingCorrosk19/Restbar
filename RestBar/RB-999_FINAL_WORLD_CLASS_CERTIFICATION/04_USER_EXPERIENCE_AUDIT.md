# 04 — User Experience Audit

Simulación de recorridos (evidencia: Views existentes + FULL_BROWSER + certs módulo).

| Rol | Flujo crítico | Fricción observada | Severidad |
|-----|---------------|--------------------|-----------|
| Dueño | Command Center / DI Cockpit / Analytics | Muchas entradas (CC, BI, DI, Rules, Reports) — posible confusión de “dónde mirar” | Media |
| Gerente | Dashboard → Caja → Inventario → Compras | Flags/module disabled pages; navegación densa en Layout | Media |
| Cajero | Sesión caja → pagos → cierre | Flujo enterprise exige disciplina; bien documentado | Baja–Media |
| Mesero | Order / mesas | UI legacy dark; muchos scripts | Media |
| Cocina | Station / KDS | Depende timestamps; OK operativo | Baja |
| Compras | PO / Supplier | Bueno si módulo on | Baja |
| Inventario | Index + movimientos | DataTables CDN; export “próximamente” en partes | Media |
| Admin | Users / Branch / Flags | Potente pero técnico | Media |

## Hallazgos UX

1. **Demasiados “centros” analíticos** (BI, Executive, CC, DI, Rules) sin IA unificada de navegación.  
2. **Clicks** para publicar regla / aceptar recomendación aceptables para power users; no para cajero.  
3. **Responsive:** proyectos Playwright tablet/mobile existen; no todos los módulos deep-tested.  
4. **A11y:** suite parcial (`a11y-and-idor`); no WCAG AA certificado.  
5. **Tiempo percibido:** smoke PERF soft &lt;5s; P95 prod formal incompleto.

## Correcciones mínimas (recomendación — no implementar en este audit)

- Un solo hub “Insights” que enlace DI + CC + BI.  
- Wizard de primer día (caja+sucursal).  
- No ampliar módulos.
