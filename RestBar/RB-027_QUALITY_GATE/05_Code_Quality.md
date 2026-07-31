# 05 — Code Quality

## Estándares vigentes

| Tema | Estándar |
|------|----------|
| Lenguaje | C# 12 / .NET 8, nullable enable |
| UI | Razor + Bootstrap existente; no rediseñar sin RB |
| JS | Helpers por módulo en `wwwroot/js`; evitar globals nuevos |
| Errores API | Validación → 4xx JSON `{ success:false }`; no 500 por input malo |
| Logs | No secrets; Correlation ID (RB-026) |
| CSP | No ampliar CDNs sin necesidad + test browser |

## Métricas baseline

| Métrica | Valor |
|---------|-------|
| Unit PASS | 77 |
| Line coverage | 0.41% |
| Controllers | 42 |
| Service `.cs` | 63 |
| Playwright tests | ~158 |

## Definition of Done (calidad)

Un cambio está “Done” solo si:

1. Compila Release.
2. Tests existentes del área en verde.
3. Tests nuevos para lógica de dominio / validaciones.
4. Sin console CSP errors en páginas tocadas.
5. Permisos revisados.
6. Docs tocadas si cambia contrato o flujo.

## Anti-patrones prohibidos en PRs nuevos

- Hardcode de secrets / roles `postgres` en VPS (usar `restbaruser`).
- Scripts de browser-refresh / cache-buster en Production layouts.
- `catch` vacío que traga errores de caja/inventario/pagos.
- Feature flags default-on sin tests.
