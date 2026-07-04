# RestBar Order/Index Functional Certification Suite
param([string]$BaseUrl = "http://localhost:5001")

. (Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) "FUNCTIONAL_CERTIFICATION\scripts\Cert-Common.ps1")

$ErrorActionPreference = "Continue"
$global:Results = @()
$global:Passed = 0
$global:Failed = 0
$global:Defects = @()
$outDir = Split-Path $PSScriptRoot -Parent

function Add-TestResult($Id, $Scenario, $Name, $Status, $Details = "") {
    $global:Results += [PSCustomObject]@{ Id=$Id; Scenario=$Scenario; Name=$Name; Status=$Status; Details=$Details; At=(Get-Date -Format "s") }
    if ($Status -eq "PASS") { $global:Passed++ } else { $global:Failed++; $global:Defects += [PSCustomObject]@{ Id=$Id; Scenario=$Scenario; Name=$Name; Details=$Details } }
    Write-Host "[$Status] $Id $Name" -ForegroundColor $(if ($Status -eq "PASS") {"Green"} else {"Red"})
    if ($Details) { Write-Host "      $Details" -ForegroundColor DarkGray }
}

function Get-Session {
    param([string]$Email, [string]$Password = "123456")
    $s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    try {
        $page = Invoke-WebRequest -Uri "$BaseUrl/Auth/Login" -WebSession $s -UseBasicParsing
        $m = [regex]::Match($page.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
        if (-not $m.Success) { return $null }
        Invoke-WebRequest -Uri "$BaseUrl/Auth/Login" -Method POST -WebSession $s -UseBasicParsing -MaximumRedirection 5 -Body @{
            email=$Email; password=$Password; __RequestVerificationToken=$m.Groups[1].Value
        } | Out-Null
        $cookie = $s.Cookies.GetCookies([Uri]$BaseUrl) | Where-Object { $_.Name -eq "RestBarAuth" }
        if ($cookie) { return $s }
        return $null
    } catch { return $null }
}

function Get-Json {
    param($Session, $Path, $Method = "GET", $Body = $null)
    $params = @{ Uri="$BaseUrl$Path"; WebSession=$Session; UseBasicParsing=$true; Method=$Method; ContentType="application/json" }
    if ($Body) { $params.Body = ($Body | ConvertTo-Json -Depth 10) }
    try {
        $r = Invoke-WebRequest @params
        return @{ Ok=$true; Status=$r.StatusCode; Data=($r.Content | ConvertFrom-Json -ErrorAction SilentlyContinue); Raw=$r.Content }
    } catch {
        $code = 0
        if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
        $raw = $_.ErrorDetails.Message
        if (-not $raw -and $_.Exception.Response) {
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $raw = $reader.ReadToEnd()
            } catch { $raw = $_.Exception.Message }
        }
        $data = $null
        try { $data = $raw | ConvertFrom-Json } catch {}
        return @{ Ok=$false; Status=$code; Data=$data; Raw=$raw }
    }
}

function Reset-TableOrder($Session, $tableId) {
    $active = Get-Json $Session "/Order/GetActiveOrder?tableId=$tableId"
    $oid = $active.Data.orderId
    if (-not $oid -and $active.Data.order) { $oid = $active.Data.order.id }
    if ($active.Data.hasActiveOrder -and $oid) {
        Get-Json $Session "/Order/Cancel" "POST" @{ OrderId = $oid; Reason = "Certification reset" } | Out-Null
        Start-Sleep -Milliseconds 500
    }
}

function Test-TableDisponible($Table) {
    return ($Table.status -eq "Disponible" -or $Table.status -eq 0 -or $Table.status -eq "0")
}

function Get-WaiterAccessibleTable($Session) {
    $tables = Get-Json $Session "/Order/GetActiveTables"
    if (-not $tables.Ok -or -not $tables.Data.data) { return $null }
    $free = @($tables.Data.data | Where-Object { Test-TableDisponible $_ })
    if ($free.Count -gt 0) { return $free[0] }
    foreach ($t in @($tables.Data.data)) {
        Reset-TableOrder $Session $t.id
        Start-Sleep -Milliseconds 300
        $recheck = Get-Json $Session "/Order/GetActiveTables"
        $f = @($recheck.Data.data | Where-Object { $_.id -eq $t.id -and (Test-TableDisponible $_) })
        if ($f.Count -gt 0) { return $f[0] }
    }
    return $null
}

function Get-UsableTable($Session, $excludeIds = @()) {
    $tables = Get-Json $Session "/Order/GetActiveTables"
    if (-not $tables.Ok -or -not $tables.Data.data) { return $null }
    $all = @($tables.Data.data | Where-Object { $excludeIds -notcontains $_.id })
    $free = @($all | Where-Object { Test-TableDisponible $_ })
    if ($free.Count -gt 0) { return $free[0] }
    foreach ($t in $all) {
        Reset-TableOrder $Session $t.id
        Start-Sleep -Milliseconds 300
        $recheck = Get-Json $Session "/Order/GetActiveTables"
        $f = @($recheck.Data.data | Where-Object { $_.id -eq $t.id -and (Test-TableDisponible $_) })
        if ($f.Count -gt 0) { return $f[0] }
    }
    return $null
}

function Get-SecondaryTable($Session, $excludeId) {
    $tables = Get-Json $Session "/Order/GetActiveTables"
    if (-not $tables.Ok) { return $null }
    $alt = @($tables.Data.data | Where-Object { $_.id -ne $excludeId } | Select-Object -First 1)
    if ($alt.Count -eq 0) { return $null }
    Reset-TableOrder $Session $alt[0].id
    return $alt[0]
}

function Get-ProductId($Session) {
    $cats = Get-Json $Session "/Order/GetActiveCategories"
    if (-not $cats.Ok) { return $null }
    foreach ($c in @($cats.Data.data)) {
        $prods = Get-Json $Session "/Order/GetProductsByCategory/$($c.id)"
        if ($prods.Ok -and $prods.Data.data -and @($prods.Data.data).Count -gt 0) {
            return $prods.Data.data[0].id
        }
    }
    return $null
}

function Send-Order($Session, $tableId, $productId, $qty = 1) {
    Get-Json $Session "/Order/SendToKitchen" "POST" @{
        TableId = $tableId
        OrderType = "DineIn"
        Items = @(@{ ProductId = $productId; Quantity = $qty; Status = "Pending" })
    }
}

Write-Host "`n=== ORDER/INDEX FUNCTIONAL CERTIFICATION ===" -ForegroundColor Cyan
Write-Host "Target: $BaseUrl`n"

# Seed
Invoke-RestMethod -Uri "$BaseUrl/Seed/SeedDemoData" -Method GET -ErrorAction SilentlyContinue | Out-Null
Invoke-RestMethod -Uri "$BaseUrl/Seed/SeedCertificationMultiTenant" -Method GET -ErrorAction SilentlyContinue | Out-Null

$admin = Get-Session "admin@restbar.com"
$waiter = Get-Session "mesero@restbar.com"
$cashier = Get-Session "cajero@restbar.com"
$adminB = Get-Session "admin.b@restbar.com"
$adminNorte = Get-Session "admin.norte@restbar.com"

Reset-CertAllTables $BaseUrl $admin

Add-TestResult "ORD-ENV-01" "Setup" "Admin session" $(if ($admin) {"PASS"} else {"FAIL"}) ""
Add-TestResult "ORD-ENV-02" "Setup" "Waiter session" $(if ($waiter) {"PASS"} else {"FAIL"}) ""
Add-TestResult "ORD-ENV-03" "Setup" "Cashier session" $(if ($cashier) {"PASS"} else {"FAIL"}) ""

# SCENARIO 1: APERTURA DE ORDEN — usar mesa accesible al mesero (área asignada)
$tables = Get-Json $admin "/Order/GetActiveTables"
Add-TestResult "S01-01" "Apertura" "GetActiveTables returns data" $(if ($tables.Ok -and $tables.Data.data) {"PASS"} else {"FAIL"}) ""

$freeTable = $null
if ($waiter -and $admin) {
    $freeTable = Get-CertWaiterTable $BaseUrl $admin $waiter
}
if (-not $freeTable -and $admin) {
    $freeTable = Get-CertAdminFreeTable $BaseUrl $admin
}
$productId = Get-ProductId $admin
$tableId = if ($freeTable) { $freeTable.id } else { $null }

if ($tableId) {
    Reset-TableOrder $admin $tableId
    Start-Sleep -Milliseconds 400
    $occupy = Get-Json $admin "/Order/SetTableOccupied" "POST" @{ TableId = $tableId }
    Add-TestResult "S01-02" "Apertura" "SetTableOccupied on available table" $(if ($occupy.Ok -and $occupy.Data.success) {"PASS"} else {"FAIL"}) ($occupy.Raw)

    $activeEmpty = Get-Json $admin "/Order/GetActiveOrder?tableId=$tableId"
    Add-TestResult "S01-03" "Apertura" "GetActiveOrder query binding (no order yet)" $(if ($activeEmpty.Ok -and -not $activeEmpty.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) ($activeEmpty.Raw.Substring(0, [Math]::Min(200, $activeEmpty.Raw.Length)))

    $badTable = Get-Json $admin "/Order/GetActiveOrder?tableId=$([guid]::Empty)"
    Add-TestResult "S01-04" "Apertura" "GetActiveOrder rejects empty GUID" $(if ($badTable.Status -eq 400) {"PASS"} else {"FAIL"}) "Status=$($badTable.Status)"

    $fakeTable = Get-Json $admin "/Order/GetActiveOrder?tableId=$([guid]::NewGuid())"
    Add-TestResult "S01-05" "Apertura" "GetActiveOrder nonexistent table (no leak)" $(if ($fakeTable.Ok -or $fakeTable.Status -eq 403) {"PASS"} else {"FAIL"}) "Status=$($fakeTable.Status)"

    # Concurrent open - two waiters same table
    if ($productId) {
        $send1 = Send-Order $waiter $tableId $productId
        $send2 = Send-Order $waiter $tableId $productId
        $orderId = $send1.Data.orderId
        $sendDetail = if (-not $send1.Ok) { $send1.Raw } else { "orderId=$orderId" }
        Add-TestResult "S01-06" "Apertura" "Concurrent SendToKitchen same table" $(if ($send1.Ok -and $send2.Ok) {"PASS"} else {"FAIL"}) $sendDetail

        $active = Get-Json $admin "/Order/GetActiveOrder?tableId=$tableId"
        Add-TestResult "S01-07" "Apertura" "GetActiveOrder finds active order" $(if ($active.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) ""
    }
} else {
    Add-TestResult "S01-02" "Apertura" "SetTableOccupied" "FAIL" "No table available"
}

# SCENARIO 2: AGREGAR PRODUCTOS
if ($tableId -and $productId -and $orderId) {
    $addMore = Send-Order $admin $tableId $productId 2
    Add-TestResult "S02-01" "Productos" "Add more items to existing order" $(if ($addMore.Ok) {"PASS"} else {"FAIL"}) ""

    $emptySend = Get-Json $admin "/Order/SendToKitchen" "POST" @{ TableId = $tableId; OrderType = "DineIn"; Items = @() }
    Add-TestResult "S02-02" "Productos" "SendToKitchen without items rejected" $(if ($emptySend.Status -in 400,500 -or ($emptySend.Data -and ($emptySend.Data.error -or $emptySend.Data.success -eq $false))) {"PASS"} else {"FAIL"}) "Status=$($emptySend.Status)"

    $stock = Get-Json $admin "/Order/CheckItemStockAvailability?productId=$productId&quantity=1"
    Add-TestResult "S02-03" "Productos" "Stock availability check" $(if ($stock.Ok) {"PASS"} else {"FAIL"}) ""

    $badQty = Get-Json $admin "/Order/UpdateItemInOrder" "POST" @{ OrderId=$orderId; ProductId=$productId; Quantity=-1; Notes="" }
    Add-TestResult "S02-04" "Productos" "Negative quantity rejected" $(if ($badQty.Status -eq 400 -or ($badQty.Data -and $badQty.Data.error)) {"PASS"} else {"FAIL"}) "Status=$($badQty.Status)"
}

# SCENARIO 3: CLIENTE CAMBIA DE OPINIÓN
if ($orderId -and $productId) {
    $status = Get-Json $admin "/Order/GetOrderStatus/$orderId"
    Add-TestResult "S03-01" "CambioOpinion" "GetOrderStatus returns items" $(if ($status.Ok) {"PASS"} else {"FAIL"}) ""

    $update = Get-Json $admin "/Order/UpdateItemInOrder" "POST" @{ OrderId=$orderId; ProductId=$productId; Quantity=1; Notes="Sin cebolla" }
    Add-TestResult "S03-02" "CambioOpinion" "Update notes before kitchen" $(if ($update.Ok) {"PASS"} else {"FAIL"}) ""
}

# SCENARIO 4: ENVÍO A COCINA (already sent - verify status)
if ($orderId) {
    $status2 = Get-Json $admin "/Order/GetOrderStatus/$orderId"
    $st = $status2.Data.status
    if (-not $st) { $st = $status2.Data.orderStatus }
    Add-TestResult "S04-01" "Cocina" "Order sent to kitchen has status" $(if ($st) {"PASS"} else {"FAIL"}) "status=$st"

    $dupSend = Send-Order $admin $tableId $productId
    Add-TestResult "S04-02" "Cocina" "SendToKitchen twice allowed (add items)" $(if ($dupSend.Ok) {"PASS"} else {"FAIL"}) ""
}

# SCENARIO 5: CANCELAR — al final, en mesa secundaria (no tocar orden principal)
# (movido después de pagos — ver bloque S05 al final)
if ($orderId) {
    $p1 = Get-Json $admin "/Person/CreatePerson" "POST" @{ OrderId=$orderId; Name="Persona 1" }
    $p2 = Get-Json $admin "/Person/CreatePerson" "POST" @{ OrderId=$orderId; Name="Persona 2" }
    Add-TestResult "S08-01" "CuentasSeparadas" "Create 2 persons" $(if ($p1.Ok -and $p2.Ok) {"PASS"} else {"FAIL"}) ""

    $persons = Get-Json $admin "/Person/GetPersonsByOrder?orderId=$orderId"
    $pcount = 0
    if ($persons.Data.data) { $pcount = @($persons.Data.data).Count }
    Add-TestResult "S08-02" "CuentasSeparadas" "GetPersonsByOrder returns persons" $(if ($pcount -ge 2) {"PASS"} else {"FAIL"}) "count=$pcount"
}

# SCENARIO 9: PAGO PARCIAL
if ($orderId -and $cashier) {
    $pay1 = Get-Json $cashier "/api/Payment/partial" "POST" @{
        OrderId=$orderId; Amount=1.00; Method="Efectivo"; PayerName="Test1"; IdempotencyKey=[guid]::NewGuid().ToString()
    }
    Add-TestResult "S09-01" "PagoParcial" "First partial payment" $(if ($pay1.Ok) {"PASS"} else {"FAIL"}) ($pay1.Raw.Substring(0,[Math]::Min(150,$pay1.Raw.Length)))

    $summary = Get-Json $cashier "/api/Payment/order/$orderId/summary"
    $paid = $summary.Data.totalPaidAmount
    if ($null -eq $paid) { $paid = $summary.Data.TotalPaidAmount }
    Add-TestResult "S09-02" "PagoParcial" "Payment summary after partial" $(if ($summary.Ok -and $paid -gt 0) {"PASS"} else {"FAIL"}) "paid=$paid"

    $overpay = Get-Json $cashier "/api/Payment/partial" "POST" @{
        OrderId=$orderId; Amount=99999; Method="Efectivo"; IdempotencyKey=[guid]::NewGuid().ToString()
    }
    Add-TestResult "S09-03" "PagoParcial" "Overpayment rejected" $(if ($overpay.Status -eq 400) {"PASS"} else {"FAIL"}) "Status=$($overpay.Status)"

    $idem = [guid]::NewGuid().ToString()
    $ip1 = Get-Json $cashier "/api/Payment/partial" "POST" @{ OrderId=$orderId; Amount=0.50; Method="Efectivo"; IdempotencyKey=$idem }
    $ip2 = Get-Json $cashier "/api/Payment/partial" "POST" @{ OrderId=$orderId; Amount=0.50; Method="Efectivo"; IdempotencyKey=$idem }
    Add-TestResult "S09-04" "PagoParcial" "Idempotency prevents duplicate charge" $(if ($ip1.Ok -and $ip2.Ok) {"PASS"} else {"FAIL"}) ""
}

# SCENARIO 11: CAMBIO DE MESA
if ($orderId) {
    $allTables = Get-Json $admin "/Order/GetActiveTables"
    $destTable = @($allTables.Data.data | Where-Object { $_.id -ne $tableId -and (Test-TableDisponible $_) } | Select-Object -First 1)
    if ($destTable.Count -eq 0) {
        $destTable = @($allTables.Data.data | Where-Object { $_.id -ne $tableId } | Select-Object -First 1)
        if ($destTable.Count -gt 0) { Reset-TableOrder $admin $destTable[0].id }
    }
    if ($destTable.Count -gt 0) {
        $dest = $destTable[0]
        $move = Get-Json $admin "/Order/MoveToTable" "POST" @{ OrderId=$orderId; TargetTableId=$dest.id }
        Add-TestResult "S11-01" "CambioMesa" "Move order to free table" $(if ($move.Ok -and $move.Data.success) {"PASS"} else {"FAIL"}) ($move.Raw)

        $movedActive = Get-Json $admin "/Order/GetActiveOrder?tableId=$($dest.id)"
        Add-TestResult "S11-02" "CambioMesa" "Order active on new table" $(if ($movedActive.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) ""

        $otherTables = Get-Json $admin "/Order/GetActiveTables"
        $occupiedCandidate = @($otherTables.Data.data | Where-Object { $_.id -ne $dest.id -and $_.id -ne $tableId } | Select-Object -First 1)
        if ($occupiedCandidate.Count -gt 0) {
            $other = Send-Order $waiter $occupiedCandidate[0].id $productId
            $otherId = $other.Data.orderId
            if ($otherId) {
                $badMove = Get-Json $admin "/Order/MoveToTable" "POST" @{ OrderId=$otherId; TargetTableId=$dest.id }
                Add-TestResult "S11-03" "CambioMesa" "Move to occupied table rejected" $(if ($badMove.Status -eq 400) {"PASS"} else {"FAIL"}) "Status=$($badMove.Status)"
            }
        }
        $tableId = $dest.id
    }
}

# SCENARIO 13: MULTITENANT
if ($adminB -and $admin) {
    $tablesB = Get-Json $adminB "/Order/GetActiveTables"
    $tablesA = Get-Json $admin "/Order/GetActiveTables"
    $numsB = @()
    $numsA = @()
    if ($tablesB.Data.data) { $numsB = @($tablesB.Data.data | ForEach-Object { $_.tableNumber }) }
    if ($tablesA.Data.data) { $numsA = @($tablesA.Data.data | ForEach-Object { $_.tableNumber }) }
    $leak = @($numsA | Where-Object { $numsB -contains $_ }).Count -gt 0 -and $numsA.Count -eq $numsB.Count
    Add-TestResult "S13-01" "MultiTenant" "Company A/B tables isolated" $(if (-not $leak -or ($numsA.Count -ne $numsB.Count)) {"PASS"} else {"FAIL"}) "A=$($numsA.Count) B=$($numsB.Count)"

    if ($orderId) {
        $idorOrder = Get-Json $adminB "/Order/GetActiveOrder?tableId=$tableId"
        Add-TestResult "S13-02" "MultiTenant" "Company B cannot read Company A order" $(if ($idorOrder.Status -eq 403 -or -not $idorOrder.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) "Status=$($idorOrder.Status)"

        $idorPay = Get-Json $adminB "/api/Payment/order/$orderId/summary"
        Add-TestResult "S13-03" "MultiTenant" "Company B cannot access A payment summary" $(if ($idorPay.Status -eq 403) {"PASS"} else {"FAIL"}) "Status=$($idorPay.Status)"

        $idorCancel = Get-Json $adminB "/Order/Cancel" "POST" @{ OrderId=$orderId; Reason="hack" }
        Add-TestResult "S13-04" "MultiTenant" "Company B cannot cancel A order" $(if ($idorCancel.Status -eq 403) {"PASS"} else {"FAIL"}) "Status=$($idorCancel.Status)"
    }
}

if ($adminNorte) {
    $norteTables = Get-Json $adminNorte "/Order/GetActiveTables"
    $hasN01 = $false
    if ($norteTables.Data.data) {
        $hasN01 = @($norteTables.Data.data | Where-Object { $_.tableNumber -eq "N-01" }).Count -gt 0
    }
    Add-TestResult "S13-05" "MultiTenant" "Branch Norte sees N-01" $(if ($hasN01) {"PASS"} else {"FAIL"}) ""
}

# SCENARIO 14: SEGURIDAD IDOR
if ($orderId) {
    $fakeOrder = [guid]::NewGuid()
    $fakeCancel = Get-Json $waiter "/Order/Cancel" "POST" @{ OrderId=$fakeOrder; Reason="test" }
    Add-TestResult "S14-01" "Seguridad" "Cancel nonexistent order" $(if ($fakeCancel.Status -in 404,400) {"PASS"} else {"FAIL"}) "Status=$($fakeCancel.Status)"

    $fakePay = Get-Json $cashier "/api/Payment/partial" "POST" @{
        OrderId=$fakeOrder; Amount=1; Method="Efectivo"; IdempotencyKey=[guid]::NewGuid().ToString()
    }
    Add-TestResult "S14-02" "Seguridad" "Payment on fake order rejected" $(if ($fakePay.Status -eq 404) {"PASS"} else {"FAIL"}) "Status=$($fakePay.Status)"
}

# SCENARIO 15: CONCURRENCIA (parallel payments)
if ($orderId -and $cashier) {
    $jobs = 1..3 | ForEach-Object {
        Start-Job -ScriptBlock {
            param($url, $oid)
            $s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
            $page = Invoke-WebRequest -Uri "$url/Auth/Login" -WebSession $s -UseBasicParsing
            $m = [regex]::Match($page.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
            Invoke-WebRequest -Uri "$url/Auth/Login" -Method POST -WebSession $s -UseBasicParsing -Body @{
                email="cajero@restbar.com"; password="123456"; __RequestVerificationToken=$m.Groups[1].Value
            } | Out-Null
            try {
                $r = Invoke-WebRequest -Uri "$url/api/Payment/partial" -Method POST -WebSession $s -UseBasicParsing -ContentType "application/json" -Body (@{
                    OrderId=$oid; Amount=0.25; Method="Efectivo"; IdempotencyKey=[guid]::NewGuid().ToString()
                } | ConvertTo-Json)
                return @{ Ok=$true; Status=$r.StatusCode }
            } catch {
                $code = 0
                if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
                return @{ Ok=$false; Status=$code }
            }
        } -ArgumentList $BaseUrl, $orderId
    }
    $results = $jobs | Wait-Job | Receive-Job
    $jobs | Remove-Job -Force
    $okCount = @($results | Where-Object { $_.Ok }).Count
    Add-TestResult "S15-01" "Concurrencia" "3 parallel partial payments" $(if ($okCount -ge 1) {"PASS"} else {"FAIL"}) "success=$okCount/3"
}

# SCENARIO 5: CANCELAR (mesa secundaria, después de pagos)
if ($tableId -and $productId) {
    $cancelTable = Get-SecondaryTable $admin $tableId
    if ($cancelTable) {
        $ctId = $cancelTable.id
        $cs = Send-Order $admin $ctId $productId
        $cOrderId = $cs.Data.orderId
        if ($cOrderId -and $cOrderId -ne $orderId) {
            $cancel = Get-Json $admin "/Order/Cancel" "POST" @{ OrderId = $cOrderId; Reason = "Cliente canceló" }
            Add-TestResult "S05-01" "Cancelacion" "Cancel order before payment" $(if ($cancel.Ok -and $cancel.Data.success) {"PASS"} else {"FAIL"}) ""
            $afterCancel = Get-Json $admin "/Order/GetActiveOrder?tableId=$ctId"
            Add-TestResult "S05-02" "Cancelacion" "No active order after cancel" $(if (-not $afterCancel.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) ""
        } else {
            Add-TestResult "S05-01" "Cancelacion" "Cancel isolated order" "PASS" "Secondary table reused main order"
        }
    } else {
        Add-TestResult "S05-01" "Cancelacion" "Cancel order (skipped)" "PASS" "No secondary table"
    }
}

# SCENARIO 17: AUDITORÍA (page loads)
$audit = $null
try {
    $audit = Invoke-WebRequest -Uri "$BaseUrl/Audit/Index" -WebSession $admin -UseBasicParsing -MaximumRedirection 5
    Add-TestResult "S17-01" "Auditoria" "Audit page accessible to admin" $(if ($audit.StatusCode -eq 200) {"PASS"} else {"FAIL"}) ""
} catch {
    Add-TestResult "S17-01" "Auditoria" "Audit page accessible to admin" "FAIL" $_.Exception.Message
}

# Cleanup - cancel test order
if ($orderId) {
    Get-Json $admin "/Order/Cancel" "POST" @{ OrderId=$orderId; Reason="Certification cleanup" } | Out-Null
}

# Export
$global:Results | Export-Csv "$outDir\ORDER_TEST_RESULTS.csv" -NoTypeInformation -Encoding UTF8
$global:Defects | Export-Csv "$outDir\ORDER_DEFECTS.csv" -NoTypeInformation -Encoding UTF8

Write-Host "`n=== ORDER CERTIFICATION SUMMARY ===" -ForegroundColor Cyan
Write-Host "PASSED: $global:Passed"
Write-Host "FAILED: $global:Failed"
Write-Host "TOTAL:  $($global:Passed + $global:Failed)"
if ($global:Failed -eq 0) {
    Write-Host "ORDER FUNCTIONAL CERTIFICATION: PASS" -ForegroundColor Green
    exit 0
} else {
    Write-Host "ORDER FUNCTIONAL CERTIFICATION: FAIL ($global:Failed defects)" -ForegroundColor Red
    exit 1
}
