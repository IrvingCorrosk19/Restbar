# 06 — ROOT CAUSE ANALYSIS

## ORD-D11 / D12 — Fallos SQL 42703 (column does not exist)

**Síntoma:** Mesero `GetActiveTables` → `success: false`; `GetActiveOrder` → 500.  
**Causa raíz:** Migraciones SQL manuales (`enterprise-closure-migration.sql`) crearon columnas snake_case (`printer_name`, `discount_amount`) pero `RestBarContext` no tenía `HasColumnName`. EF generaba SQL con PascalCase.  
**Corrección:** Mapeos explícitos en `Station` y `Order` entities.

## ORD-D13 — GetActiveOrder devolvía orden sin ítems activos

**Síntoma:** S01-03 fallaba tras reset (orden con ítems cancelados).  
**Causa raíz:** `GetActiveOrderByTableAsync` incluía órdenes con todos los ítems en `Cancelled`.  
**Corrección:** Filtro `OrderItems.Any(oi => oi.Status != Cancelled)` + controller solo cuenta ítems activos.

## ORD-D15 — Recipe Save DbUpdateConcurrencyException

**Síntoma:** ENT-REC-01 falla en segunda ejecución.  
**Causa raíz:** Reasignación `existing.Lines = new List` confundía change tracker tras `RemoveRange`.  
**Corrección:** `RemoveRange` + `_context.RecipeLines.Add` individual con `RecipeId` explícito.

## RT-S09 falso negativo (histórico)

**Síntoma:** Pizza aparecía en Horno B en cert.  
**Causa raíz:** Test validaba KDS global (datos residuales RT-S06) no estación del pedido P3.  
**Corrección:** Assert sobre `preparedByStationId` del pedido específico.

## ORD-D18 — Mesas huérfanas EnPreparacion/Ocupada bloquean cert

**Síntoma:** S01-02 "No table available"; S01-06 "No autorizado para esta mesa".  
**Causa raíz:** `Invoke-CertPgReset` filtraba por `branches.name IN ('Central',...)` pero nombres reales son `RestBar Centro`, `RestBar Norte`. Mesas quedaban en `EnPreparacion` sin órdenes activas; mesero (área Salón) no veía mesas `Disponible`. Fallback admin usaba mesa fuera del área del mesero.  
**Corrección:** `Cert-Common.ps1` reset global de mesas/órdenes + `Get-CertWaiterTable` + reset inter-suite en `Run-AllCertifications.ps1`.

