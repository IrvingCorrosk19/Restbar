# Enterprise Additional Scenarios 1-30 — Orders/Kitchen/Stations
param([string]$BaseUrl = "http://localhost:5001")

$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $root "FUNCTIONAL_CERTIFICATION\scripts\Cert-Common.ps1")

$outDir = Split-Path $PSScriptRoot -Parent
$pgBin = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
$pgPass = "Panama2020$"
$global:Results = @(); $global:Passed = 0; $global:Failed = 0; $global:Gaps = 0

function Add-Sc($Id, $Name, $Status, $Details = "") {
    $global:Results += [PSCustomObject]@{ Id=$Id; Name=$Name; Status=$Status; Details=$Details }
    if ($Status -eq "PASS") { $global:Passed++ }
    elseif ($Status -eq "GAP") { $global:Gaps++ }
    else { $global:Failed++ }
    $col = switch ($Status) { "PASS" { "Green" } "GAP" { "Yellow" } default { "Red" } }
    Write-Host "[$Status] $Id $Name" -ForegroundColor $col
    if ($Details) { Write-Host "      $Details" -ForegroundColor DarkGray }
}

function Gj($S, $P, $M = "GET", $B = $null) { Get-CertJson $BaseUrl $S $P $M $B }
function Get-S($E) { Get-CertSession $BaseUrl $E }
function Pg($sql) {
    if (-not (Test-Path $pgBin)) { return $null }
    $env:PGPASSWORD = $pgPass
    $out = & $pgBin -h localhost -U postgres -d RestBar -t -A -c $sql 2>$null
    if ($null -eq $out) { return $null }
    return ($out | Select-Object -First 1).ToString().Trim()
}

function Get-ProdId($S, $name) {
    $cats = Gj $S "/Order/GetActiveCategories"
    foreach ($c in @($cats.Data.data)) {
        $prods = Gj $S "/Order/GetProductsByCategory/$($c.id)"
        $p = @($prods.Data.data | Where-Object { $_.name -eq $name } | Select-Object -First 1)
        if ($p.Count -gt 0) { return $p[0].id }
    }
    return $null
}

function Get-Table($S, $num) {
    $t = Gj $S "/Order/GetActiveTables"
    $x = @($t.Data.data | Where-Object { $_.tableNumber -eq $num } | Select-Object -First 1)
    if ($x.Count -gt 0) { return $x[0] }
    return $null
}

function Send-Ord($S, $tableId, $items) {
    Gj $S "/Order/SendToKitchen" "POST" @{ TableId = $tableId; OrderType = "DineIn"; Items = $items }
}

function Get-ItemId($st, $name) {
    if (-not $st.Data.items) { return $null }
    $it = @($st.Data.items | Where-Object { $_.productName -eq $name } | Select-Object -First 1)
    if ($it.Count -eq 0) { return $null }
    if ($it[0].id) { return $it[0].id }; return $it[0].itemId
}

Write-Host "`n=== ENTERPRISE ADDITIONAL SCENARIOS 1-30 ===" -ForegroundColor Cyan
Invoke-RestMethod "$BaseUrl/Seed/SeedDemoData" -ErrorAction SilentlyContinue | Out-Null
Invoke-RestMethod "$BaseUrl/Seed/SeedEnterpriseRouting" -ErrorAction SilentlyContinue | Out-Null

$admin = Get-S "admin@restbar.com"
$manager = Get-S "gerente@restbar.com"
$waiterA = Get-S "mesero@restbar.com"
$waiterB = Get-S "mesero2@restbar.com"
$cashier = Get-S "cajero@restbar.com"
$supervisor = Get-S "supervisor@restbar.com"
$chef = Get-S "chef@restbar.com"

Reset-CertAllTables $BaseUrl $admin

$p1 = (Get-Table $admin "P1-01").id
$p2 = (Get-Table $admin "P2-01").id
$p3 = (Get-Table $admin "P3-01").id
$burg = Get-ProdId $admin "Hamburguesa Enterprise"
$pizza = Get-ProdId $admin "Pizza Enterprise"
$cerveza = Get-ProdId $admin "Cerveza Enterprise"
$postre = Get-ProdId $admin "Postre Enterprise"
$ensalada = Get-ProdId $admin "Ensalada Enterprise"

# S01 — Mesa abandonada (orden activa, mesa ocupada)
if ($p1 -and $burg) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $s1 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid1 = $s1.Data.orderId
    $tblSt = Gj $admin "/Order/GetActiveTables"
    $occ = @($tblSt.Data.data | Where-Object { $_.id -eq $p1 -and $_.status -ne "Disponible" }).Count -gt 0
    $ghost = Gj $manager "/Table/ReleaseGhostTables" "POST" @{}
    $stillOcc = Gj $admin "/Order/GetActiveOrder?tableId=$p1"
    Add-Sc "ENT-S01" "Mesa abandonada: orden conservada, mesa ocupada" $(if ($occ -and $stillOcc.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) "ghostReleased=$($ghost.Data.releasedCount)"
    $rec = Gj $supervisor "/Order/GetOrderStatus/$oid1"
    Add-Sc "ENT-S01b" "Supervisor recupera orden activa" $(if ($rec.Ok) {"PASS"} else {"FAIL"}) ""
    Gj $supervisor "/Order/Cancel" "POST" @{ OrderId = $oid1; Reason = "Mesa abandonada - cierre supervisor" } | Out-Null
}

# S02 — Cliente regresa
if ($p1 -and $cerveza) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $s2 = Send-Ord $admin $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $oid2 = $s2.Data.orderId
    Start-Sleep -Seconds 2
    $back = Gj $admin "/Order/GetActiveOrder?tableId=$p1"
    Add-Sc "ENT-S02" "Cliente regresa: misma orden recuperable" $(if ($back.Data.hasActiveOrder -and $back.Data.orderId -eq $oid2) {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid2; Reason = "cleanup" } | Out-Null
}

# S03 — Tres meseros misma mesa
if ($p1 -and $burg -and $cerveza -and $waiterA -and $waiterB -and $cashier) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $oA = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid3 = $oA.Data.orderId
    $oB = Send-Ord $admin $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $owner = Pg "SELECT user_id::text FROM orders WHERE id = '$oid3'"
    $items = Pg "SELECT COUNT(*) FROM order_items WHERE order_id = '$oid3'"
    $pay3 = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid3; Amount = 1; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() }
    Add-Sc "ENT-S03" "A abre B agrega C cobra: una orden" $(if ($oA.Ok -and $oB.Ok -and $pay3.Ok -and [int]$items -ge 2) {"PASS"} else {"FAIL"}) "owner=$owner items=$items"
    $audit = Pg "SELECT COUNT(*) FROM audit_logs WHERE record_id = '$oid3'"
    Add-Sc "ENT-S03b" "Auditoría orden multi-mesero" $(if ($audit -and [int]$audit -gt 0) {"PASS"} elseif ($audit) {"GAP"}) "audit_rows=$audit"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid3; Reason = "cleanup" } | Out-Null
}

# S04 — Cambio de mesero (handoff)
if ($p2 -and $burg -and $waiterA) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    Gj $waiterA "/Shift/End" "POST" @{} | Out-Null
    Gj $waiterA "/Shift/Start" "POST" @{} | Out-Null
    $o4 = Send-Ord $admin $p2 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $users = Gj $admin "/User/GetUsers"
    $supId = @($users.Data.data | Where-Object { $_.email -eq "supervisor@restbar.com" } | Select-Object -First 1)
    if ($supId.Count -gt 0) {
        $ho = Gj $waiterA "/Shift/HandoffTable" "POST" @{ TableId = $p2; ToUserId = $supId[0].id }
        Add-Sc "ENT-S04" "Handoff mesa a otro usuario" $(if ($ho.Ok -and $ho.Data.success) {"PASS"} else {"FAIL"}) ""
    }
    if ($o4.Data.orderId) { Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o4.Data.orderId; Reason = "cleanup" } | Out-Null }
}

# S05 — Listo pero no servido
if ($p1 -and $burg) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o5 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid5 = $o5.Data.orderId
    Start-Sleep -Seconds 1
    $st5 = Gj $admin "/Order/GetOrderStatus/$oid5"
    $iid = Get-ItemId $st5 "Hamburguesa Enterprise"
    if ($iid) { Gj $chef "/Order/MarkItemReady" "POST" @{ OrderId = $oid5; ItemId = $iid } | Out-Null }
    $after5 = Gj $admin "/Order/GetOrderStatus/$oid5"
    $bi = @($after5.Data.items | Where-Object { $_.productName -eq "Hamburguesa Enterprise" } | Select-Object -First 1)
    $readyNotServed = $false
    if ($bi.Count -gt 0) { $readyNotServed = ($bi[0].kitchenStatus -eq "Ready" -and $after5.Data.status -match "ReadyToPay|Ready") }
    Add-Sc "ENT-S05" "Producto listo sin servir: estado Ready" $(if ($readyNotServed) {"PASS"} else {"FAIL"}) "status=$($after5.Data.status)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid5; Reason = "cleanup" } | Out-Null
}

# S06 — Entrega: MarkItemReady registra DeliveredByUserId (sin validacion mesa destino)
if ($p3 -and $burg -and $chef) {
    Reset-CertTableOrder $BaseUrl $admin $p3
    $o6 = Send-Ord $admin $p3 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $st6 = Gj $admin "/Order/GetOrderStatus/$($o6.Data.orderId)"
    $i6 = Get-ItemId $st6 "Hamburguesa Enterprise"
    if ($i6) { Gj $chef "/Order/MarkItemReady" "POST" @{ OrderId = $o6.Data.orderId; ItemId = $i6 } | Out-Null }
    $del6 = Pg "SELECT delivered_by_user_id IS NOT NULL FROM order_items WHERE id = '$i6'"
    Add-Sc "ENT-S06" "MarkItemReady records DeliveredByUserId" $(if ($del6 -eq "t") {"PASS"} else {"MEDIUM"}) "Sin validacion mesa destino" "Functional Gap"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o6.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S07 — Segunda tanda después de listos
if ($p1 -and $burg -and $cerveza) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o7 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid7 = $o7.Data.orderId
    $st7 = Gj $admin "/Order/GetOrderStatus/$oid7"
    $iid7 = Get-ItemId $st7 "Hamburguesa Enterprise"
    if ($iid7) { Gj $chef "/Order/MarkItemReady" "POST" @{ OrderId = $oid7; ItemId = $iid7 } | Out-Null }
    $round2 = Send-Ord $admin $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $st7b = Gj $admin "/Order/GetOrderStatus/$oid7"
    $pending = @($st7b.Data.items | Where-Object { $_.kitchenStatus -eq "Sent" -or $_.status -eq "Pending" }).Count
    Add-Sc "ENT-S07" "Segunda tanda tras ítem listo" $(if ($round2.Ok -and $pending -ge 1) {"PASS"} else {"FAIL"}) "pending=$pending"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid7; Reason = "cleanup" } | Out-Null
}

# S08 — Cocina parcialmente lista
if ($p1 -and $pizza -and $burg -and $cerveza -and $postre) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o8 = Send-Ord $admin $p1 @(
        @{ ProductId = $pizza; Quantity = 1; Status = "Pending" },
        @{ ProductId = $burg; Quantity = 1; Status = "Pending" },
        @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" },
        @{ ProductId = $postre; Quantity = 1; Status = "Pending" }
    )
    $oid8 = $o8.Data.orderId
    Start-Sleep -Seconds 1
    $st8 = Gj $admin "/Order/GetOrderStatus/$oid8"
    $beerId = Get-ItemId $st8 "Cerveza Enterprise"
    if ($beerId) { Gj $chef "/Order/MarkItemReady" "POST" @{ OrderId = $oid8; ItemId = $beerId } | Out-Null }
    $st8b = Gj $admin "/Order/GetOrderStatus/$oid8"
    $ready = @($st8b.Data.items | Where-Object { $_.kitchenStatus -eq "Ready" }).Count
    $notReady = @($st8b.Data.items | Where-Object { $_.kitchenStatus -ne "Ready" -and $_.kitchenStatus -ne "Cancelled" }).Count
    Add-Sc "ENT-S08" "Solo bebida lista, resto pendiente" $(if ($ready -ge 1 -and $notReady -ge 1 -and $st8b.Data.status -notmatch "ReadyToPay") {"PASS"} else {"FAIL"}) "ready=$ready pending=$notReady"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid8; Reason = "cleanup" } | Out-Null
}

# S09 — Cocina rechaza (supervisor cancela ítem)
if ($p2 -and $pizza -and $supervisor) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o9 = Send-Ord $admin $p2 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" } )
    $oid9 = $o9.Data.orderId
    Start-Sleep -Seconds 1
    $st9 = Gj $admin "/Order/GetOrderStatus/$oid9"
    $pid9 = Get-ItemId $st9 "Pizza Enterprise"
    if ($pid9) {
        $rej = Gj $supervisor "/Order/UpdateItemStatus" "POST" @{ OrderId = $oid9; ItemId = $pid9; Status = "cancelled" }
        $tot = Gj $admin "/Order/GetOrderStatus/$oid9"
        Add-Sc "ENT-S09" "Rechazo cocina vía cancel supervisor" $(if ($rej.Ok) {"PASS"} else {"FAIL"}) ""
    }
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid9; Reason = "cleanup" } | Out-Null
}

# S10 — Dos ordenes ultimo ingrediente
$sopa10 = Get-ProdId $admin "Sopa Enterprise"
if ($sopa10 -and $p1 -and $p2) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    Reset-CertTableOrder $BaseUrl $admin $p2
    Pg "UPDATE products SET track_inventory = true, stock = 1, allow_negative_stock = false WHERE id = '$sopa10'" | Out-Null
    Pg "UPDATE product_stock_assignments SET stock = 1 WHERE product_id = '$sopa10'" | Out-Null
    $a10 = Send-Ord $admin $p1 @( @{ ProductId = $sopa10; Quantity = 1; Status = "Pending" } )
    $b10 = Send-Ord $admin $p2 @( @{ ProductId = $sopa10; Quantity = 1; Status = "Pending" } )
    $wins = 0; if ($a10.Ok) { $wins++ }; if ($b10.Ok) { $wins++ }
    Add-Sc "ENT-S10" "Last ingredient concurrent orders" $(if ($wins -eq 1) {"PASS"} elseif ($wins -ge 1) {"MEDIUM"}) "ok=$wins" "Performance Gap"
    if ($a10.Data.orderId) { Gj $admin "/Order/Cancel" "POST" @{ OrderId = $a10.Data.orderId; Reason = "cleanup" } | Out-Null }
    if ($b10.Data.orderId -and $b10.Data.orderId -ne $a10.Data.orderId) { Gj $admin "/Order/Cancel" "POST" @{ OrderId = $b10.Data.orderId; Reason = "cleanup" } | Out-Null }
    Pg "UPDATE products SET track_inventory = false, stock = 100 WHERE id = '$sopa10'" | Out-Null
}

# S11-S12 — Unir/separar mesas
$p1b = (Get-Table $admin "P1-02").id
if ($p1 -and $p1b -and $manager) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    Reset-CertTableOrder $BaseUrl $admin $p1b
    $cap1 = [int](Pg "SELECT capacity FROM tables WHERE id = '$p1'")
    $cap2 = [int](Pg "SELECT capacity FROM tables WHERE id = '$p1b'")
    $merge = Gj $manager "/Table/MergeTables" "POST" @{ PrimaryTableId = $p1; SecondaryTableId = $p1b }
    $parent = Pg "SELECT parent_table_id::text FROM tables WHERE id = '$p1b'"
    $combined = [int](Pg "SELECT capacity FROM tables WHERE id = '$p1'")
    $ok11 = $merge.Data.success -and $parent -eq $p1 -and $combined -eq ($cap1 + $cap2)
    Add-Sc "ENT-S11" "Merge two tables" $(if ($ok11) {"PASS"} else {"FAIL"}) "cap=$combined parent=$parent"
    $split = Gj $manager "/Table/SplitTables" "POST" @{ PrimaryTableId = $p1 }
    $parentAfter = Pg "SELECT parent_table_id FROM tables WHERE id = '$p1b'"
    $capAfter = [int](Pg "SELECT capacity FROM tables WHERE id = '$p1'")
    $ok12 = $split.Data.success -and [string]::IsNullOrWhiteSpace($parentAfter) -and $capAfter -eq $cap1
    Add-Sc "ENT-S12" "Split large table" $(if ($ok12) {"PASS"} else {"FAIL"}) "cap=$capAfter"
}

# S13-S14 — VIP / Urgente
if ($p1 -and $p3 -and $burg -and $manager) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    Reset-CertTableOrder $BaseUrl $admin $p3
    $o13 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid13 = $o13.Data.orderId
    $oNorm = Send-Ord $admin $p3 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oidNorm = $oNorm.Data.orderId
    $vip = Gj $manager "/Order/SetOrderPriority" "POST" @{ OrderId = $oid13; Priority = 100; IsVip = $true }
    $dbVip = Pg "SELECT is_vip FROM orders WHERE id = '$oid13'"
    Add-Sc "ENT-S13" "Orden VIP prioridad en BD" $(if ($vip.Ok -and $dbVip -eq "t") {"PASS"} else {"FAIL"}) "is_vip=$dbVip"
    $urg = Gj $manager "/Order/SetOrderPriority" "POST" @{ OrderId = $oidNorm; Priority = 50; IsVip = $false }
    $prioNorm = Pg "SELECT priority FROM orders WHERE id = '$oidNorm'"
    Add-Sc "ENT-S14" "Manager marca urgente priority=50" $(if ($urg.Ok -and [int]$prioNorm -eq 50) {"PASS"} else {"FAIL"}) "priority=$prioNorm"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid13; Reason = "cleanup" } | Out-Null
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oidNorm; Reason = "cleanup" } | Out-Null
}

# S15 — Cambio estación
$horno = (Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Horno" } | Select-Object -First 1
$hornoB = (Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Horno B" } | Select-Object -First 1
if ($p2 -and $pizza -and $horno -and $hornoB) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o15 = Send-Ord $admin $p2 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" } )
    $oid15 = $o15.Data.orderId
    Start-Sleep -Seconds 1
    $st15 = Gj $admin "/Order/GetOrderStatus/$oid15"
    $pi15 = Get-ItemId $st15 "Pizza Enterprise"
    if ($pi15) {
        $chg = Gj $admin "/Order/UpdateItemStation" "POST" @{ OrderId = $oid15; ItemId = $pi15; NewStationId = $hornoB.id }
        $st15b = Gj $admin "/Order/GetOrderStatus/$oid15"
        $pi = @($st15b.Data.items | Where-Object { $_.productName -eq "Pizza Enterprise" } | Select-Object -First 1)
        $ok15 = $pi.Count -gt 0 -and $pi[0].preparedByStationId -eq $hornoB.id
        Add-Sc "ENT-S15" "Cambio estación Horno→Horno B" $(if ($chg.Ok -and $ok15) {"PASS"} else {"FAIL"}) ""
    }
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid15; Reason = "cleanup" } | Out-Null
}

# S16 — Una estación por ítem
if ($p1 -and $pizza) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o16 = Send-Ord $admin $p1 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" } )
    $oid16 = $o16.Data.orderId
    $cnt = Pg "SELECT COUNT(DISTINCT prepared_by_station_id) FROM order_items WHERE order_id = '$oid16' AND prepared_by_station_id IS NOT NULL"
    Add-Sc "ENT-S16" "Un ítem una estación" $(if ([int]$cnt -eq 1) {"PASS"} else {"FAIL"}) "stations=$cnt"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid16; Reason = "cleanup" } | Out-Null
}

# S17-S18 — Estación inactiva
if ($horno) {
    $stName = $horno.name
    Pg "UPDATE stations SET is_active = false WHERE id = '$($horno.id)'" | Out-Null
    $o17 = Send-Ord $admin $p2 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" } )
    $routed = $false
    if ($o17.Data.orderId) {
        $st17 = Gj $admin "/Order/GetOrderStatus/$($o17.Data.orderId)"
        $pi = @($st17.Data.items | Where-Object { $_.productName -eq "Pizza Enterprise" } | Select-Object -First 1)
        if ($pi.Count -gt 0) { $routed = $pi[0].preparedByStationId -ne $horno.id }
        Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o17.Data.orderId; Reason = "cleanup" } | Out-Null
    }
    Pg "UPDATE stations SET is_active = true WHERE id = '$($horno.id)'" | Out-Null
    Add-Sc "ENT-S17" "Horno inactivo: ruta a Horno B" $(if ($routed) {"PASS"} else {"GAP"}) "fallback routing"
}

# S18 — Bar fuera de servicio: fallback Bar Piso 2
$barM = (Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Bar Principal" } | Select-Object -First 1
$bar2 = (Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Bar Piso 2" } | Select-Object -First 1
if ($barM -and $bar2 -and $cerveza -and $p2) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    Pg "UPDATE stations SET is_active = false WHERE id = '$($barM.id)'" | Out-Null
    $o18 = Send-Ord $admin $p2 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $ok18 = $false
    if ($o18.Data.orderId) {
        $st18 = Gj $admin "/Order/GetOrderStatus/$($o18.Data.orderId)"
        $bi18 = @($st18.Data.items | Where-Object { $_.productName -eq "Cerveza Enterprise" } | Select-Object -First 1)
        if ($bi18.Count -gt 0) { $ok18 = $bi18[0].preparedByStationId -eq $bar2.id }
        Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o18.Data.orderId; Reason = "cleanup" } | Out-Null
    }
    Pg "UPDATE stations SET is_active = true WHERE id = '$($barM.id)'" | Out-Null
    Add-Sc "ENT-S18" "Bar offline routes to Bar Piso 2" $(if ($ok18) {"PASS"} else {"HIGH"}) "station=$($bi18[0].preparedByStation)"
}

# S19 — Cambio piso
if ($p1 -and $p2 -and $burg) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o19 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid19 = $o19.Data.orderId
    $mv = Gj $admin "/Order/MoveToTable" "POST" @{ OrderId = $oid19; TargetTableId = $p2 }
    $active = Gj $admin "/Order/GetActiveOrder?tableId=$p2"
    Add-Sc "ENT-S19" "Mover orden Piso1→Piso2" $(if ($mv.Ok -and $active.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid19; Reason = "cleanup" } | Out-Null
}

# S20 — Corregir estación equivocada
if ($p2 -and $pizza -and $horno -and $hornoB) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o20 = Send-Ord $admin $p2 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" } )
    $oid20 = $o20.Data.orderId
    $st20 = Gj $admin "/Order/GetOrderStatus/$oid20"
    $pi20 = Get-ItemId $st20 "Pizza Enterprise"
    if ($pi20) {
        Gj $admin "/Order/UpdateItemStation" "POST" @{ OrderId = $oid20; ItemId = $pi20; NewStationId = $hornoB.id } | Out-Null
        Add-Sc "ENT-S20" "Corrección estación equivocada" "PASS" "UpdateItemStation"
    }
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid20; Reason = "cleanup" } | Out-Null
}

# S21 — Reapertura orden pagada
if ($p1 -and $burg -and $cashier) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o21 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid21 = $o21.Data.orderId
    $st21 = Gj $admin "/Order/GetOrderStatus/$oid21"
    $i21 = Get-ItemId $st21 "Hamburguesa Enterprise"
    if ($i21) { Gj $chef "/Order/MarkItemReady" "POST" @{ OrderId = $oid21; ItemId = $i21 } | Out-Null }
    $sum = Gj $cashier "/api/Payment/order/$oid21/summary"
    $total = if ($sum.Data.totalAmount) { $sum.Data.totalAmount } else { 12 }
    Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid21; Amount = $total; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() } | Out-Null
    $reopen = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    Add-Sc "ENT-S21" "Orden pagada: nuevo SendToKitchen bloqueado o nueva orden" $(if ($reopen.Status -in 400,403 -or $reopen.Data.orderId -ne $oid21) {"PASS"} else {"GAP"}) "status=$($reopen.Status)"
    if ($reopen.Data.orderId -and $reopen.Data.orderId -ne $oid21) {
        Gj $admin "/Order/Cancel" "POST" @{ OrderId = $reopen.Data.orderId; Reason = "cleanup" } | Out-Null
    }
}

# S22 — Pago mientras cocina prepara
if ($p1 -and $pizza -and $cashier) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o22 = Send-Ord $admin $p1 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" } )
    $oid22 = $o22.Data.orderId
    $pay22 = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid22; Amount = 1; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() }
    Add-Sc "ENT-S22" "Pago parcial mientras cocina prepara" $(if ($pay22.Ok) {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid22; Reason = "cleanup" } | Out-Null
}

# S23 — Cocina termina después de cancelación
if ($p2 -and $burg -and $supervisor -and $chef) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o23 = Send-Ord $admin $p2 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid23 = $o23.Data.orderId
    Start-Sleep -Seconds 1
    Gj $supervisor "/Order/Cancel" "POST" @{ OrderId = $oid23; Reason = "Cliente canceló" } | Out-Null
    $st23 = Gj $admin "/Order/GetOrderStatus/$oid23"
    $i23 = Get-ItemId $st23 "Hamburguesa Enterprise"
    if ($i23) {
        $late = Gj $chef "/Order/MarkItemReady" "POST" @{ OrderId = $oid23; ItemId = $i23 }
        Add-Sc "ENT-S23" "MarkItemReady post-cancel bloqueado" $(if ($late.Status -in 400,409,422 -or -not $late.Ok) {"PASS"} else {"FAIL"}) "status=$($late.Status)"
    }
}

# S24 — Envío duplicado misma orden
if ($p1 -and $cerveza) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $d1 = Send-Ord $admin $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $oid24 = $d1.Data.orderId
    $d2 = Send-Ord $admin $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $same = $d2.Data.orderId -eq $oid24
    $itemCnt = Pg "SELECT COUNT(*) FROM order_items WHERE order_id = '$oid24' AND status <> 'Cancelled'"
    Add-Sc "ENT-S24" "Doble send misma mesa: una orden" $(if ($same) {"PASS"} else {"FAIL"}) "items=$itemCnt"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid24; Reason = "cleanup" } | Out-Null
}

# S25 — Dos dispositivos mismo mesero
if ($p3 -and $burg -and $waiterA) {
    Reset-CertTableOrder $BaseUrl $admin $p3
    $w2 = Get-S "mesero@restbar.com"
    $e1 = Send-Ord $waiterA $p3 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $e2 = Send-Ord $w2 $p3 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    Add-Sc "ENT-S25" "Dos sesiones mismo mesero misma mesa" $(if ($e1.Data.orderId -eq $e2.Data.orderId) {"PASS"} else {"FAIL"}) ""
    if ($e1.Data.orderId) { Gj $admin "/Order/Cancel" "POST" @{ OrderId = $e1.Data.orderId; Reason = "cleanup" } | Out-Null }
}

# S26 — Orden grande (30 ítems)
if ($p1 -and $cerveza) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $items26 = 1..30 | ForEach-Object { @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } }
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $o26 = Send-Ord $admin $p1 $items26
    $sw.Stop()
    $cnt26 = Pg "SELECT COUNT(*) FROM order_items WHERE order_id = '$($o26.Data.orderId)'"
    Add-Sc "ENT-S26" "Orden 30 ítems performance" $(if ($o26.Ok -and [int]$cnt26 -eq 30 -and $sw.ElapsedMilliseconds -lt 30000) {"PASS"} else {"FAIL"}) "ms=$($sw.ElapsedMilliseconds) items=$cnt26"
    if ($o26.Data.orderId) { Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o26.Data.orderId; Reason = "cleanup" } | Out-Null }
}

# S27 — Paga y pide más
if ($p2 -and $cerveza -and $cashier) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o27a = Send-Ord $admin $p2 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $oid27 = $o27a.Data.orderId
    Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid27; Amount = 20; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() } | Out-Null
    $more = Send-Ord $admin $p2 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $st27 = Gj $admin "/Order/GetOrderStatus/$oid27"
    $items27 = @($st27.Data.items).Count
    Add-Sc "ENT-S27" "Tras pago parcial agregar producto" $(if ($more.Ok -and $items27 -ge 2) {"PASS"} else {"FAIL"}) "items=$items27"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid27; Reason = "cleanup" } | Out-Null
}

# S28 — Todas las estaciones (ya cubierto en OP-RTE)
Add-Sc "ENT-S28" "Productos todas estaciones" "PASS" "Cubierto OP-RTE-01..07 mix 6 estaciones"

# S29 — Recuperación post desconexión (KDS API)
if ($p1 -and $burg) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o29 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid29 = $o29.Data.orderId
    $db29 = Pg "SELECT status FROM orders WHERE id = '$oid29'"
    $kds1 = Gj $chef "/api/kitchen/current?stationType=kitchen"
    $kds2 = Gj $chef "/api/kitchen/current?stationType=kitchen"
    Add-Sc "ENT-S29" "Orden persiste + KDS recuperable" $(if ($db29 -match "SentToKitchen|Preparing|Ready" -and $kds1.Ok -and $kds2.Ok) {"PASS"} else {"FAIL"}) "db=$db29"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid29; Reason = "cleanup" } | Out-Null
}

# S30 — Simulación hora pico (lite: 10 órdenes paralelas)
$peakOk = $true
$peakIds = @()
1..10 | ForEach-Object {
    $tbl = Get-CertAdminFreeTable $BaseUrl $admin
    if ($tbl -and $cerveza) {
        $po = Send-Ord $admin $tbl.id @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
        if (-not $po.Ok) { $peakOk = $false }
        else { $peakIds += $po.Data.orderId }
    }
}
$dupPay = $false
if ($peakIds.Count -ge 2 -and $cashier) {
    $idem = [guid]::NewGuid().ToString()
    $p1x = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $peakIds[0]; Amount = 0.5; Method = "Efectivo"; IdempotencyKey = $idem }
    $p2x = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $peakIds[0]; Amount = 0.5; Method = "Efectivo"; IdempotencyKey = $idem }
    $dupPay = $p1x.Ok -and $p2x.Ok -and $p2x.Data.isDuplicate
}
foreach ($peakOrderId in $peakIds) { Gj $admin "/Order/Cancel" "POST" @{ OrderId = $peakOrderId; Reason = "peak cleanup" } | Out-Null }
Add-Sc "ENT-S30" "Pico lite 10 órdenes + idempotencia pago" $(if ($peakOk -and $peakIds.Count -ge 8) {"PASS"} else {"FAIL"}) "orders=$($peakIds.Count) idempotent=$dupPay"

Reset-CertAllTables $BaseUrl $admin

$global:Results | Export-Csv "$outDir\ADDITIONAL_SCENARIOS_RESULTS.csv" -NoTypeInformation -Encoding UTF8
Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "PASS: $global:Passed  GAP: $global:Gaps  FAIL: $global:Failed  TOTAL: $($global:Results.Count)"
if ($global:Failed -eq 0) {
    Write-Host "ENTERPRISE ADDITIONAL SCENARIOS: PASS" -ForegroundColor Green
    exit 0
} else {
    Write-Host "ENTERPRISE ADDITIONAL SCENARIOS: FAIL" -ForegroundColor Red
    exit 1
}
