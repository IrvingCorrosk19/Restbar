# 06 — Technical Debt

## Señales (app source, excl. lib/vendor/docs)

| Señal | Hallazgo | Prioridad |
|-------|----------|-----------|
| TODO/FIXME en Controllers/Services | Reports PDF/Excel; Email MinStock; Inventory export; Order/Kitchen/Person leftovers | **P1** Reports exports; **P2** resto |
| HACK | 0 | — |
| `[Obsolete]` | 0 | — |
| God services | `OrderService`, `AdvancedReportsService` | **P1** split gradual |
| DI factories `new` en Program | Varios | **P2** |
| Integration tests ausentes | Sistema completo | **P0** |
| Coverage 0.41% | Sistema completo | **P0** |
| Browser no siempre en CI | Sin `RESTBAR_BASE_URL` var | **P0** ops |
| Migraciones | 17 activas — no “obsoletas” detectadas; no borrar sin auditoría | P3 |
| Vistas/scripts | Cert folders masivos (docs) — no runtime | P3 tidy |
| Paquetes MailKit/MimeKit moderate | Supply chain | **P1** upgrade path |
| ~~SignalR 1.1.0 / Design 1.1.0 High transitive~~ | **Fixed RB-027** | done |
| EF package version mismatch warnings en Tests | 9.0.1 vs 9.0.5 | **P2** alinear |

## Backlog priorizado

### P0 — bloquea Engineering Excellence

1. Harness de integración API (`WebApplicationFactory`).
2. Variable CI `RESTBAR_BASE_URL` + branch protection en **Quality Gate**.
3. Unit tests Orders validation + Payment soft-fail paths.
4. Multitenant IDOR API tests reales (2 companies).

### P1

1. Completar exports Reports (cerrar TODOs o documentar deferred).
2. CodeQL / Semgrep en CI.
3. Subir cobertura Domain críticos ≥15%.
4. Upgrade MailKit/MimeKit cuando advisory lo exija.

### P2

1. Refactor incremental OrderService.
2. Alinear paquetes EF en Tests.
3. Eliminar TODOs cosméticos en PersonService.

### P3

1. Archivar certificaciones históricas en `docs/archive` (sin perder evidencia).
2. Limpiar scripts Com one-off.

## Política

- Todo TODO nuevo en código de dinero/stock debe tener issue ID.
- No se acepta “deuda nueva” en módulos Cash/Inventory/Payment sin plan de cierre en el mismo epic.
