# 04 — User Experience

**Evidencia:** browser suites (ORD-NAV, RSP, A11Y-01), layouts `_OrderLayout`/`_KitchenLayout`, no Dark Mode.

| Dimensión | Hallazgo | Evidencia | Acción comercial |
|-----------|----------|-----------|------------------|
| Clics POS | Mesa → categoría → producto → enviar: flujo corto | ORD-E2E PASS | Mantener; no reinventar |
| Curva mesero | Media; UI densa | Order JS multi-file | Capacitación 2–4 h asistida |
| Cajero | Caja wizard + PaymentView | CASH/PAY PASS | 2–4 h |
| Gerente | Muchos menús Config/Ops | `_Layout` | Necesita mapa de roles en venta |
| Responsive | Tablet/móvil parcial | RSP PASS; no app nativa | Condición |
| Dark Mode | No existe | Inventario | Cosmético |
| Errores | Prod oculta stack; CorrelationId | RB-026 | Bien para soporte |
| Consistencia | Payment huérfano; Email sin vista | Gaps | Limpieza cosmética |
| Accesibilidad | Smoke login only | A11Y-01 | No WCAG certificado |

## Entrenamiento estimado (nuevo usuario)

| Rol | Tiempo a autonomía básica |
|-----|---------------------------|
| Mesero | 2–4 horas |
| Cajero | 3–6 horas |
| Cocina/KDS | 1–2 horas |
| Gerente/Admin | 1–2 días |
| Comprador | 0.5–1 día |
| Auditor | 0.5 día |

**Fricción que sí conviene remediar (ROI):** wizard de onboarding inicial (empresa→sucursal→mesas→productos→caja); no más módulos de negocio.
