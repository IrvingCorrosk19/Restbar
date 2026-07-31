# 08 — Development Standards

Cada funcionalidad nueva o cambio material **debe** incluir:

| Requisito | Obligatorio | Notas |
|-----------|-------------|-------|
| Código | Sí | Alineado a arquitectura monolito modular |
| Pruebas unitarias | Sí si hay lógica de dominio/validación | xUnit en `RestBar.Tests` |
| Pruebas browser o API | Sí si hay UI/endpoint | Playwright bajo `tests/Browser/<Module>/` |
| Documentación | Sí si cambia flujo/contrato | Carpeta RB-* o changelog módulo |
| Permisos / RBAC | Sí | Policies + middleware |
| Auditoría | Sí si dinero/stock/config | AuditLog / hash chain Cash |
| i18n | Sí textos usuario | es-PA baseline; no hardcode inconsistente |
| Responsive | Sí UI | Verificar tablet/mobile project o checklist |
| Accesibilidad | Sí UI | No romper labels/roles; suite a11y cuando aplique |
| Exportaciones | Si el módulo exporta | CSV/XLSX/print; no dejar botón 500 |
| Validaciones | Sí | 4xx, no 500 |
| Manejo de errores | Sí | Mensajes no filtrar stack en Production |

## Prohibido

- Mergar con Quality Gate rojo.
- Añadir módulos de negocio “de paso” sin programa RB.
- Cambiar UX por gusto fuera de diseño existente.
- Introducir CDN/scripts sin CSP update + test.

## Naming tests

- Unit: `{Area}Tests.cs` · hechos descriptivos.
- Browser: `{MOD}-{nn} title` (ej. `INV-08`, `CASH-01`).

## Feature flags

Nuevos módulos enterprise → flag default **off** hasta certificación browser PASS.
