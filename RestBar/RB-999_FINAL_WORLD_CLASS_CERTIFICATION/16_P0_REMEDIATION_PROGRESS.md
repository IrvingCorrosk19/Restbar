# 16 — P0 Remediation Progress (2026-07-31)

## Completado en código (build 0 errores · unit 95 PASS)

| ID | Antes | Después |
|----|-------|---------|
| ZT-01 | Reports ExportPdf/Excel stub `success` falso | Excel ClosedXML + HTML imprimible reales |
| ZT-02 | Inventory export “Próximamente” | CSV download del reporte |
| ZT-13 | Sin vistas Forgot/Reset | Vistas + link en Login |
| Auth bug | ForgotPassword **sobrescribía PasswordHash** | Token en `IMemoryCache` (30 min) |
| ZT-14 | Sin Views/Email | `Email/Index` |
| ZT-15 | AdvancedSettings sin vistas | Currencies/Tax/Discount/Hours/Notify/Backup + Create |
| ZT-16 | Shifts API-only | `Shift/Index` + `Status` |
| ZT-06 | Modifiers sin UI | `ModifierController` + Index |
| ZT-07 | Customers sin CRUD | `CustomerController` + Index |
| ZT-17 | Precios horarios sin UI | Cubierto vía `DiscountPolicies` (ValidFrom/Until) |
| Accounting JS | “en desarrollo” | Redirect a reportes reales |
| PaymentView export | JSON stub | Excel + HTML print |
| Email MinStock TODO | hardcode 0 | `product.MinStock` |
| Nav | — | Links Modifier/Customer/Shift/Email/AdvancedSettings |

## Aún NO cerrado (producto ≠ 100% EP)

| ID | Gap |
|----|-----|
| ZT-03 | MFA |
| ZT-04 | Offline POS |
| ZT-05 | Payment gateway / PCI |
| ZT-09 | Coverage ~0.41% / integration harness |
| ZT-10 | SoD formal |
| ZT-11 | MT IDOR deep |
| ZT-12 | Dual assert UI/API/DB universal |

**Veredicto:** remediación P0 de stubs/vistas **sí**. Enterprise Premium 100% **no**.
