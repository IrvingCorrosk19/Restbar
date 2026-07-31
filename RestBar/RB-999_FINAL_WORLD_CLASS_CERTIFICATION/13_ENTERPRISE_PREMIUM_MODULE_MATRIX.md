# 13 — Enterprise Premium Module Certification Matrix (Adenda)

**Fecha:** 2026-07-31  
**Estándar exigido:** Enterprise Premium · 100% · cero tolerancia  
**Método:** evidencia de código + suites existentes (unit 95 PASS, browser specs ~34).  
**Regla:** no certificar por compilación, ni por prueba parcial, ni por “funciona en demo”.

## Leyenda de resultado por módulo

| Resultado | Significado |
|-----------|-------------|
| **EP CERTIFIED** | Cumple checklist adenda + verificaciones globales con evidencia |
| **PARTIAL** | Existe y opera, pero falla cero tolerancia o faltan pruebas globales |
| **FAIL** | Gap crítico / stub / ausente / no certificable |

**Ningún módulo recibe EP CERTIFIED en esta corrida** — la cobertura unitaria global (~0.41%), la ausencia de harness de integración API y gaps funcionales explícitos (MFA, offline, SoD formal, exports stub) invalidan el sello Enterprise Premium completo.

---

## Matriz módulo × checklist adenda

| # | Módulo | Existencia | Unit | Browser | Gaps cero tolerancia (evidencia) | Resultado |
|---|--------|------------|------|---------|----------------------------------|-----------|
| 1 | Autenticación | Login/Logout/Forgot/Reset | Débil | auth-extended | Sin MFA; **sin vistas** Forgot/Reset; lockout formal no evidenciado | **FAIL** EP |
| 2 | Usuarios | User/UserManagement/Assignment | Débil | admin | CRUD/roles/estados sí; búsqueda/filtros enterprise incompletos vs checklist | **PARTIAL** |
| 3 | Roles y permisos | Policies + AuthorizationHelper | Parcial foundation | security | Sin SoD formal; herencia limitada | **PARTIAL** |
| 4 | SuperAdmin | SuperAdminController + views | Débil | admin | Multiempresa sí; auditoría/seguridad deep parcial | **PARTIAL** |
| 5 | Empresas | Company + SuperAdmin | Débil | admin | Branding/TZ/moneda — verificar settings; no EP completo | **PARTIAL** |
| 6 | Sucursales | Branch + SuperAdmin | Débil | admin | Horarios enterprise no plenamente evidenciados | **PARTIAL** |
| 7 | Configuración avanzada | AdvancedSettings | Débil | — | Acciones sin vistas (Tax/Discount/Currency); i18n no EP | **FAIL** EP |
| 8 | Email | EmailController + EmailService + templates | Débil | — | **Sin Views/Email/**; TODO MinStock en plantillas | **FAIL** EP |
| 9 | Áreas | AreaController | Débil | floors | CRUD básico | **PARTIAL** |
| 10 | Pisos | Modelado vía Areas (seed Piso N) | Débil | floors.spec | No entidad Floor dedicada; organización = áreas | **PARTIAL** |
| 11 | Mesas | TableController | Débil | tables | Transferencias/estados OK operacional prev.; no unit EP | **PARTIAL** |
| 12 | Estaciones | StationController | Débil | stations | Kitchen/Bar types | **PARTIAL** |
| 13 | Turnos | ShiftController | Débil | shifts | **API-only** (sin UI); sin labor cost | **FAIL** EP |
| 14 | POS | Order + layouts + JS | Débil Order | orders-e2e, concurrency | **Sin offline**; sin gateway | **FAIL** EP |
| 15 | Pedidos | OrderController/Service | Débil | orders-* | Sin unit profunda OrderService | **PARTIAL** |
| 16 | KDS Cocina | KitchenApi + StationOrders | Débil | kitchen | Rendimiento/prioridades EP no lab | **PARTIAL** |
| 17 | KDS Bar | Station type bar | Débil | kitchen/stations | Sync OK diseño; no suite bar dedicada EP | **PARTIAL** |
| 18 | Productos | ProductController | Débil | ops | Imágenes/costos OK parcial | **PARTIAL** |
| 19 | Categorías | CategoryController | Débil | — | CRUD | **PARTIAL** |
| 20 | Modificadores | ModifierService + model **sin Controller/UI admin** | — | — | UI admin ausente; solo service/seed | **FAIL** EP |
| 21 | Precios | Product + PriceScheduleService | — | — | Schedule en Order; **sin UI** listas/promos | **FAIL** EP |
| 22 | Impuestos | Cálculo en orden/settings | Débil | — | Exenciones enterprise no certificadas | **PARTIAL** |
| 23 | Descuentos | Order UI + reports | Débil | — | Reglas/límites/auditoría EP incompletos | **PARTIAL** |
| 24 | Clientes | Customer model + service; **sin CustomerController CRUD** | — | — | CRM/segmentación débil; solo uso en Order/Reports | **FAIL** EP |
| 25 | Pagos | Payment* + tip/split | Débil | payments | Efectivo/tarjeta/mixto UI; **sin procesador PCI** | **FAIL** EP |
| 26 | División de cuentas | PersonService + separate-accounts.js | Débil | payments/orders | Exactitud necesita harness API | **PARTIAL** |
| 27 | Caja | Cash* + SM + X/Z | Cash SM tests | cash* | Mejor del producto; aún sin integration DB harness | **PARTIAL** |
| 28 | Inventario | Inventory* + PSA + transfers | recipe qty | inventory* | Conteo físico WMS gap; TODO export Excel/PDF en JS | **PARTIAL** |
| 29 | Recetas | RecipeController | foodcost/inv | foodcost | Versiones EP limitadas | **PARTIAL** |
| 30 | Food Cost | FoodCostDashboard + math | FoodCostMath | foodcost | Depende snapshots | **PARTIAL** |
| 31 | Menu Engineering | MenuEngineering.cshtml + classifier | FoodCost | foodcost | Stars/Plowhorses/Puzzles/Dogs implementados en dominio | **PARTIAL** |
| 32 | Compras | PO + Procurement | PO SM | procurement | Devoluciones/solicitudes parciales vs checklist | **PARTIAL** |
| 33 | Proveedores | SupplierController | — | procurement | Evaluación scores; cumplimiento EP parcial | **PARTIAL** |
| 34 | Decision Intelligence | DI controllers + forecast | ForecastEngine | di.spec | PILOT; no EP accuracy multi-branch | **PARTIAL** |
| 35 | Command Center | ExecutiveCommandCenter | — | analytics | Flag; real-time parcial | **PARTIAL** |
| 36 | Business Intelligence | BiNative + AdvancedReports | BiDecisionMath | analytics | | **PARTIAL** |
| 37 | Reportes | Reports + AdvancedReports | — | reports | **ExportPdf/ExportExcel stub TODO** → mensaje “en desarrollo” | **FAIL** EP |
| 38 | Exportaciones | Varios flags | — | — | Exactitud UI↔DB no harness universal; stubs PDF/XLS | **FAIL** EP |
| 39 | Auditoría | AuditController | — | — | Operaciones críticas no 100% cubiertas | **PARTIAL** |
| 40 | SignalR | OrderHub + layouts | — | concurrency | Reconexión E2E offline histórico GAP | **PARTIAL** |
| 41 | Home | HomeController | — | smoke | Dashboard básico | **PARTIAL** |
| 42 | Business Rules | BR controllers | RuleCondition | br.spec | Safe actions only; PILOT | **PARTIAL** |
| 43 | Copilot | Flag **false** | CopilotEngine | — | Correctamente off; no EP | **N/A / FAIL** venta |

---

## Verificaciones globales (adenda) — estado producto

| Verificación | Estado |
|--------------|--------|
| Funcionalidad | Parcial por módulo |
| UX / UI | Inconsistente (PO Create GUID crudo, hubs analíticos múltiples) |
| Responsive | Spec existe; no EP 100% |
| Accesibilidad | Spec a11y parcial |
| Rendimiento | Soft budgets; sin lab hiperescala |
| Seguridad | Hardening + gaps MT IDOR / CSRF JSON |
| Multitenancy | Bueno con condiciones |
| RBAC | Bueno; sin SoD |
| Integridad datos | Sin dual-check UI/API/DB automatizado |
| Exportaciones / Reportes | **Falla cero tolerancia** (stubs) |
| Auditoría | Parcial |
| Localización | No i18n enterprise |
| Unit | 95 PASS / ~0.41% líneas — **falla EP** |
| Integración | **0 harness** — **falla EP** |
| Browser | Amplio pero no 100% módulos |
| Performance tests | Soft / parcial |

---

## Conteo

| Resultado | Cantidad |
|-----------|---------:|
| EP CERTIFIED | **0** |
| PARTIAL | ~28 |
| FAIL EP | ~14 |
| N/A | 1 (Copilot off) |

**Conclusión de matriz:** bajo la adenda de cero tolerancia, **RestBar no puede declararse terminado ni Enterprise Premium certificado**.
