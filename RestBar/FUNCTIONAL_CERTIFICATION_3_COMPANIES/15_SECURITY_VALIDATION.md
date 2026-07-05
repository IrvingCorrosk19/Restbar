# 15 — Validación de Seguridad Funcional

**Fecha:** 2026-07-05  
**Resultado:** PASS

## IDOR y manipulación de IDs

| ID | Ataque | Resultado |
|----|--------|-----------|
| TC3-SEC-01 | OrderId Costa desde sesión Norte | 403 PASS |
| TC3-SEC-02 | Cancel OrderId ajeno | 403 PASS |
| TC3-SEC-03 | GetActiveOrder tableId ajeno | PASS |
| TC3-SEC-04 | Payment OrderId GUID falso | 404 PASS |
| TC3-MOV-SEC-01 | TargetTableId otra empresa | 403 PASS |

## Control de rutas por rol

| ID | Resultado |
|----|-----------|
| TC3-ROL-01 Mesero → Company | PASS (denegado) |
| TC3-ROL-02 Chef → Reports | PASS (denegado) |

## Cobertura Order cert

- Mesero → descuentos bloqueado
- Cajero → cocina bloqueado
- Manipulación BranchId/CompanyId en request

## Conclusión

Controles de seguridad funcional adecuados para operación multitenant en desarrollo certificado.
