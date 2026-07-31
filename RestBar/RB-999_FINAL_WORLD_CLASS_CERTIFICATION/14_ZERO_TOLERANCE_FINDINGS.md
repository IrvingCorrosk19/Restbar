# 14 — Zero-Tolerance Findings (Adenda)

Hallazgos que **por sí solos** impiden declarar “100% Enterprise Premium” o “proyecto terminado”.

## Bloqueantes absolutos (evidencia en repo)

| ID | Hallazgo | Evidencia | Módulos |
|----|----------|-----------|---------|
| ZT-01 | Export PDF/Excel de Reports **stub** (mensaje “en desarrollo”) | `ReportsController.ExportPdf/ExportExcel` TODO L240–266 | Reportes, Exportaciones |
| ZT-02 | Export inventario Excel/PDF **TODO** en JS | `inventory-management.js` L1244 | Inventario |
| ZT-03 | **Sin MFA** | No TwoFactor/MFA en codebase Auth | Autenticación |
| ZT-04 | **Sin offline POS** | Sin queue sync offline | POS |
| ZT-05 | **Sin payment gateway / PCI** | Pagos locales tip/split únicamente | Pagos |
| ZT-06 | Modifiers **sin UI/Controller admin** | Solo `ModifierService` + model | Modificadores |
| ZT-07 | Clientes **sin CRUD controller** | Customer entity + service; CRM reports only | Clientes |
| ZT-08 | Price lists / promos horarias enterprise **ausentes** | Precios en producto | Precios |
| ZT-09 | Cobertura unit ~**0.41%**; **0** integration API | RB-027 / test run 95 | Global |
| ZT-10 | SoD formal **no implementado** | RBAC policies sin matriz SoD | Roles |
| ZT-11 | MT IDOR deep **parcial** | RB-027 P0 | Multitenancy |
| ZT-12 | Diferencias UI↔API↔DB **no** validadas por harness universal | Sin dual-assert suite | Reportes/Export |
| ZT-13 | Forgot/Reset password **sin vistas** | AuthController sí; faltan `Views/Auth/ForgotPassword|ResetPassword.cshtml` | Autenticación |
| ZT-14 | Email admin **sin** `Views/Email/` | EmailController + templates; UI Index ausente | Email |
| ZT-15 | AdvancedSettings: acciones >> vistas | TaxRates/Discounts/Currencies etc. sin `.cshtml` | Configuración |
| ZT-16 | Shifts **API-only** (sin UI gestión) | `ShiftController` JSON Start/End | Turnos |
| ZT-17 | Price schedules **sin UI** | `PriceScheduleService` usado en Order; sin controller/views | Precios |

## Violaciones literales de “cero tolerancia”

- TODO en producción de reportes (acepta respuesta “éxito” sin archivo).  
- Funciones parcialmente implementadas (exports, CRM, precios avanzados).  
- Pantallas/admin incompletas (modifiers, customers).  
- Sin evidencia de ausencia de N+1/fugas en todos los módulos.

## Correcciones mínimas (no abrir productos nuevos)

Prioridad P0 calidad (remediación, no features cosméticas):

1. Completar o **retirar** endpoints stub ExportPdf/ExportExcel (no mentir “success”).  
2. Completar o ocultar export inventario TODO.  
3. Harness integration Orders/Payment/Cash/Inventory + asserts DB.  
4. Suite IDOR cross-company.  
5. Decidir alcance contractual: MFA/offline/gateway = roadmap explícito **o** fuera de alcance vendido.  
6. Admin mínimo Modifiers + Customer CRUD **solo si** se vende esos ítems en checklist comercial.

Hasta cerrar ZT-01…ZT-17 (o excluir por contrato escrito), el sello Enterprise Premium **queda denegado**.
