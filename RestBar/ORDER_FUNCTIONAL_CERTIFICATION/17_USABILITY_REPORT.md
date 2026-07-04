# 17 — USABILITY REPORT

Evaluación desde perspectiva operativa — viernes noche, hora pico.

## Flujos aprobados (baja fricción)

| Flujo | Clics estimados | Evaluación |
|-------|-----------------|------------|
| Seleccionar mesa → agregar producto → enviar cocina | 4-5 | Bueno |
| Pago parcial rápido | 3 | Bueno |
| Cambio de mesa | 2 (API) + confirmación UI | Aceptable |
| KDS por estación | 1 navegación | Bueno |

## Fricciones detectadas

| # | Problema | Impacto mesero | Propuesta |
|---|----------|----------------|-----------|
| U-01 | Modal estación en envío cocina (admin) | Confusión en primer uso | Auto-seleccionar estación del mesero asignado |
| U-02 | Cuentas separadas en modal separado | Curva aprendizaje | Badge visible "2 cuentas" en header orden |
| U-03 | Sin indicador visual mesa `EnPreparacion` vs `Ocupada` | Errores asignación | Colores distintos en mapa mesas |
| U-04 | Descuento requiere rol manager | Correcto pero lento | PIN supervisor inline en POS |
| U-05 | Sin modo offline | Riesgo en corte internet | Cola local + sync (fase 2) |
| U-06 | Refresh pierde selección mesa | Retrabajo | Persistir `tableId` en sessionStorage |

## Cocina / bar

- KDS paginado reduce saturación — positivo
- Falta sonido/alerta configurable por estación — recomendado
- Prioridad VIP no destacada visualmente en KDS — mejorar badge

## Caja

- Idempotencia previene doble cobro — excelente
- Falta pantalla "cuenta pendiente" consolidada multi-mesa — fase 2

## Gerente

- Auditoría accesible pero no linked desde Order/Index — agregar enlace contextual
- Reportes avanzados: 3 JS huérfanos detectados en análisis previo — limpiar o integrar

## Veredicto usabilidad

**Aceptable para piloto** con mejoras U-01 a U-04 recomendadas antes de rollout masivo.
