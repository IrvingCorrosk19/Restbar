# 19 — Correcciones Aplicadas

**Fecha:** 2026-07-05

## FIX-001 — Variable `$pid` en script de certificación

**Archivo:** `FUNCTIONAL_CERTIFICATION_3_COMPANIES/scripts/Run-ThreeCompaniesCertification.ps1`  
**Causa:** PowerShell reserva `$PID` (process ID). Asignar `$pid` lanza `VariableNotWritable` y aborta sección IDOR/concurrencia.  
**Corrección:** Renombrar a `$prodId` / `$pid2` → `$prodId2`.  
**Verificación:** Re-ejecución TC3: 40/40 PASS incluyendo TC3-SEC-01..04, TC3-CON-01.

## FIX-002 — Lógica TC3-ASG-01

**Corrección:** `else {"PASS"}` → `else {"FAIL"}` cuando hay overlap de mesas entre meseros.

## FIX-003 — Lógica TC3-CON-01

**Corrección:** Concurrencia ahora falla si `ok < 1` en lugar de siempre PASS.

## Componentes nuevos (sesión certificación)

| Archivo | Propósito |
|---------|-----------|
| `Services/ThreeCompaniesCertSeeder.cs` | Seed Costa, Norte, Sur |
| `Controllers/SeedController.cs` | Endpoint `SeedThreeCompaniesCertification` |
| `FUNCTIONAL_CERTIFICATION_3_COMPANIES/scripts/Run-ThreeCompaniesCertification.ps1` | Suite automatizada |

## Correcciones previas reutilizadas (Order/Enterprise cert)

- `GetActiveOrder` excluye ítems cancelados (órdenes fantasma)
- `ApplyDiscount` restricción rol admin/manager/supervisor
- Reset SQL `Cert-Common.ps1` entre suites
- EF snake_case column mapping

## Backup

`FUNCTIONAL_CERTIFICATION_3_COMPANIES/backups/RestBar_pre_tc3_*.dump` — PostgreSQL antes de seed.
