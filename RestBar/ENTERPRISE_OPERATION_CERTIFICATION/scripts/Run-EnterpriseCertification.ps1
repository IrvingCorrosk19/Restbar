# RestBar Enterprise Operations Certification Suite
param([string]$BaseUrl = "http://localhost:5001")

. (Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) "FUNCTIONAL_CERTIFICATION\scripts\Cert-Common.ps1")

$ErrorActionPreference = "Continue"
$global:Results = @(); $global:Passed = 0; $global:Failed = 0; $global:Defects = @()
$outDir = Split-Path $PSScriptRoot -Parent
$pgBin = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
$pgPass = "Panama2020$"

function Add-Test($Id, $Scenario, $Name, $Status, $Details = "") {
    $global:Results += [PSCustomObject]@{ Id=$Id; Scenario=$Scenario; Name=$Name; Status=$Status; Details=$Details }
    if ($Status -eq "PASS") { $global:Passed++ } else { $global:Failed++; $global:Defects += [PSCustomObject]@{ Id=$Id; Name=$Name; Details=$Details } }
    Write-Host "[$Status] $Id $Name" -ForegroundColor $(if ($Status -eq "PASS") {"Green"} else {"Red"})
    if ($Details) { Write-Host "      $Details" -ForegroundColor DarkGray }
}

function Get-Session($Email, $Password = "123456") {
    $s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    try {
        $page = Invoke-WebRequest -Uri "$BaseUrl/Auth/Login" -WebSession $s -UseBasicParsing
        $m = [regex]::Match($page.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
        if (-not $m.Success) { return $null }
        Invoke-WebRequest -Uri "$BaseUrl/Auth/Login" -Method POST -WebSession $s -UseBasicParsing -MaximumRedirection 5 -Body @{
            email=$Email; password=$Password; __RequestVerificationToken=$m.Groups[1].Value
        } | Out-Null
        if ($s.Cookies.GetCookies([Uri]$BaseUrl) | Where-Object { $_.Name -eq "RestBarAuth" }) { return $s }
        return $null
    } catch { return $null }
}

function Get-Json($Session, $Path, $Method = "GET", $Body = $null) {
    $p = @{ Uri="$BaseUrl$Path"; WebSession=$Session; UseBasicParsing=$true; Method=$Method; ContentType="application/json" }
    if ($Body) { $p.Body = ($Body | ConvertTo-Json -Depth 10) }
    try {
        $r = Invoke-WebRequest @p
        return @{ Ok=$true; Status=$r.StatusCode; Data=($r.Content | ConvertFrom-Json -ErrorAction SilentlyContinue); Raw=$r.Content }
    } catch {
        $code = 0; if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
        $raw = $_.ErrorDetails.Message
        if (-not $raw -and $_.Exception.Response) {
            try { $raw = (New-Object IO.StreamReader($_.Exception.Response.GetResponseStream())).ReadToEnd() } catch { $raw = $_.Exception.Message }
        }
        $data = $null; try { $data = $raw | ConvertFrom-Json } catch {}
        return @{ Ok=$false; Status=$code; Data=$data; Raw=$raw }
    }
}

function Test-TableFree($t) { return ($t.status -eq "Disponible" -or $t.status -eq 0 -or $t.status -eq "0") }

function Get-ProductByName($Session, $name) {
    $cats = Get-Json $Session "/Order/GetActiveCategories"
    foreach ($c in @($cats.Data.data)) {
        $prods = Get-Json $Session "/Order/GetProductsByCategory/$($c.id)"
        $p = @($prods.Data.data | Where-Object { $_.name -eq $name } | Select-Object -First 1)
        if ($p.Count -gt 0) { return $p[0] }
    }
    return $null
}

function Invoke-PgQuery($sql) {
    if (-not (Test-Path $pgBin)) { return $null }
    $env:PGPASSWORD = $pgPass
    $out = & $pgBin -h localhost -U postgres -d RestBar -t -A -c $sql 2>$null
    return ($out | ForEach-Object { $_.Trim() } | Where-Object { $_ } | Select-Object -First 1)
}

Write-Host "`n=== ENTERPRISE OPERATIONS CERTIFICATION ===" -ForegroundColor Cyan
Write-Host "Target: $BaseUrl`n"

Invoke-RestMethod "$BaseUrl/Seed/SeedDemoData" -ErrorAction SilentlyContinue | Out-Null
Invoke-RestMethod "$BaseUrl/Seed/SeedEnterpriseRouting" -ErrorAction SilentlyContinue | Out-Null

$admin = Get-Session "admin@restbar.com"
$manager = Get-Session "gerente@restbar.com"
$cashier = Get-Session "cajero@restbar.com"
$waiter = Get-Session "mesero@restbar.com"
$supervisor = Get-Session "supervisor@restbar.com"
$inventarista = Get-Session "inventarista@restbar.com"

Reset-CertAllTables $BaseUrl $admin

Add-Test "ENT-ENV-01" "Setup" "Admin session" $(if ($admin) {"PASS"} else {"FAIL"}) ""
Add-Test "ENT-ENV-02" "Setup" "Manager session" $(if ($manager) {"PASS"} else {"FAIL"}) ""
Add-Test "ENT-ENV-03" "Setup" "Cashier session" $(if ($cashier) {"PASS"} else {"FAIL"}) ""
Add-Test "ENT-ENV-04" "Setup" "Inventarista session" $(if ($inventarista) {"PASS"} else {"FAIL"}) ""

# Resolve table + product
$table = $null
if ($waiter -and $admin) {
    $table = Get-CertWaiterTable $BaseUrl $admin $waiter
}
if (-not $table -and $admin) {
    $table = Get-CertAdminFreeTable $BaseUrl $admin
}
$product = Get-ProductByName $admin "Café Americano"
if (-not $product) { $product = Get-ProductByName $admin "Pizza Enterprise" }
$pizza = Get-ProductByName $admin "Pizza Enterprise"

$orderId = $null
$itemId = $null
$paymentId = $null

if ($table -and $product) {
    $send = Get-Json $waiter "/Order/SendToKitchen" "POST" @{
        TableId = $table.id; OrderType = "DineIn"
        Items = @(@{ ProductId = $product.id; Quantity = 1; Status = "Pending" })
    }
    $orderId = $send.Data.orderId
    if ($orderId) {
        $st = Get-Json $admin "/Order/GetOrderStatus/$orderId"
        if ($st.Data.items) { $itemId = $st.Data.items[0].id }
    }
}

# DISCOUNTS
if ($orderId) {
    $disc = Get-Json $manager "/Order/ApplyDiscount" "POST" @{
        OrderId = $orderId; DiscountType = "fixed"; DiscountValue = 2.00; Reason = "Cert discount"
    }
    Add-Test "ENT-DISC-01" "Descuentos" "Apply fixed discount" $(if ($disc.Ok -and $disc.Data.success -and $disc.Data.discountAmount -ge 2) {"PASS"} else {"FAIL"}) "amount=$($disc.Data.discountAmount)"

    $discPct = Get-Json $manager "/Order/ApplyDiscount" "POST" @{
        OrderId = $orderId; DiscountType = "percentage"; DiscountValue = 10; Reason = "Cert 10%"
    }
    Add-Test "ENT-DISC-02" "Descuentos" "Apply percentage discount" $(if ($discPct.Ok -and $discPct.Data.success) {"PASS"} else {"FAIL"}) ""
} else {
    Add-Test "ENT-DISC-01" "Descuentos" "Apply fixed discount" "FAIL" "No order"
    Add-Test "ENT-DISC-02" "Descuentos" "Apply percentage discount" "FAIL" "No order"
}

# PAYMENT + TIPS + REFUND
if ($orderId -and $cashier) {
    $pay = Get-Json $cashier "/api/Payment/partial" "POST" @{
        OrderId = $orderId; Amount = 1.00; TipAmount = 0.50; Method = "Efectivo"
        PayerName = "Cert"; IdempotencyKey = [guid]::NewGuid().ToString()
    }
    $paymentId = $pay.Data.payment.id
    if (-not $paymentId -and $pay.Data.payment) { $paymentId = $pay.Data.payment.Id }
    Add-Test "ENT-TIP-01" "Propinas" "Payment with tip accepted" $(if ($pay.Ok -and $pay.Data.success -ne $false) {"PASS"} else {"FAIL"}) ""

    if ($paymentId) {
        $tipCount = Invoke-PgQuery "SELECT COUNT(*) FROM tip_allocations WHERE payment_id = '$paymentId'"
        Add-Test "ENT-TIP-02" "Propinas" "TipAllocation rows created" $(if ($tipCount -and [int]$tipCount -gt 0) {"PASS"} else {"FAIL"}) "count=$tipCount"

        $refund = Get-Json $cashier "/api/Payment/refund" "POST" @{
            PaymentId = $paymentId; Amount = 0.50; Reason = "Cert partial refund"
        }
        Add-Test "ENT-REF-01" "Reembolsos" "Partial refund processed" $(if ($refund.Ok -and $refund.Data.success) {"PASS"} else {"FAIL"}) ($refund.Raw.Substring(0, [Math]::Min(120, $refund.Raw.Length)))
    } else {
        Add-Test "ENT-TIP-02" "Propinas" "TipAllocation rows created" "FAIL" "No paymentId"
        Add-Test "ENT-REF-01" "Reembolsos" "Partial refund processed" "FAIL" "No paymentId"
    }
} else {
    Add-Test "ENT-TIP-01" "Propinas" "Payment with tip" "FAIL" "No order/cashier"
}

# COMMISSION RULES (seed)
$commCount = Invoke-PgQuery "SELECT COUNT(*) FROM commission_rules WHERE is_active = true"
Add-Test "ENT-COMM-01" "Comisiones" "Active commission rules exist" $(if ($commCount -and [int]$commCount -gt 0) {"PASS"} else {"FAIL"}) "count=$commCount"

# SHIFTS
if ($waiter) {
    Get-Json $waiter "/Shift/End" "POST" @{} | Out-Null
    $start = Get-Json $waiter "/Shift/Start" "POST" @{}
    Add-Test "ENT-SHIFT-01" "Turnos" "Start shift" $(if ($start.Ok -and $start.Data.success) {"PASS"} else {"FAIL"}) ""

    $dup = Get-Json $waiter "/Shift/Start" "POST" @{}
    Add-Test "ENT-SHIFT-02" "Turnos" "Duplicate start rejected" $(if ($dup.Status -eq 400 -or ($dup.Data -and $dup.Data.success -eq $false)) {"PASS"} else {"FAIL"}) "Status=$($dup.Status)"

    if ($table) {
        $users = Get-Json $admin "/User/GetUsers"
        $toUser = $null
        if ($users.Data -and $users.Data.data) {
            $u = @($users.Data.data | Where-Object { $_.email -eq "supervisor@restbar.com" } | Select-Object -First 1)
            if ($u.Count -gt 0) { $toUser = $u[0].id }
        }
        if ($toUser) {
            $hand = Get-Json $waiter "/Shift/HandoffTable" "POST" @{ TableId = $table.id; ToUserId = $toUser }
            Add-Test "ENT-SHIFT-03" "Turnos" "Table handoff recorded" $(if ($hand.Ok -and $hand.Data.success) {"PASS"} else {"FAIL"}) ""
        } else {
            Add-Test "ENT-SHIFT-03" "Turnos" "Table handoff recorded" "FAIL" "No target user"
        }
    }

    $end = Get-Json $waiter "/Shift/End" "POST" @{}
    Add-Test "ENT-SHIFT-04" "Turnos" "End shift" $(if ($end.Ok -and $end.Data.success) {"PASS"} else {"FAIL"}) ""
}

# INVENTORY MOVEMENTS
if ($inventarista -and $product) {
    $purchase = Get-Json $inventarista "/InventoryMovement/CreatePurchase" "POST" @{
        ProductId = $product.id; Quantity = 5; Reason = "Cert purchase"
    }
    Add-Test "ENT-INV-01" "Inventario" "Create purchase movement" $(if ($purchase.Ok -and $purchase.Data.success) {"PASS"} else {"FAIL"}) ($purchase.Raw.Substring(0, [Math]::Min(100, $purchase.Raw.Length)))

    $movs = Get-Json $inventarista "/InventoryMovement/GetMovementsByDateRange"
    $hasMovs = $movs.Data.movements -and @($movs.Data.movements).Count -gt 0
    Add-Test "ENT-INV-02" "Inventario" "Get movements by date range" $(if ($movs.Ok -and $hasMovs) {"PASS"} else {"FAIL"}) ""
} else {
    Add-Test "ENT-INV-01" "Inventario" "Create purchase movement" "FAIL" "No inventarista/product"
    Add-Test "ENT-INV-02" "Inventario" "Get movements" "FAIL" "No inventarista"
}

# STOCK TRANSFER
if ($inventarista -and $pizza) {
    $stations = Get-Json $admin "/Station/GetStations"
    $horno = @($stations.Data.data | Where-Object { $_.name -eq "Horno" } | Select-Object -First 1)[0]
    $hornoB = @($stations.Data.data | Where-Object { $_.name -eq "Horno B" } | Select-Object -First 1)[0]
    if ($horno -and $hornoB) {
        $req = Get-Json $inventarista "/StockTransfer/Request" "POST" @{
            ProductId = $pizza.id; FromStationId = $horno.id; ToStationId = $hornoB.id; Quantity = 1; Notes = "Cert xfer"
        }
        Add-Test "ENT-XFER-01" "Transferencias" "Request stock transfer" $(if ($req.Ok -and $req.Data.success) {"PASS"} else {"FAIL"}) ""

        if ($req.Data.transferId) {
            $appr = Get-Json $inventarista "/StockTransfer/Approve?id=$($req.Data.transferId)" "POST" @{}
            Add-Test "ENT-XFER-02" "Transferencias" "Approve stock transfer" $(if ($appr.Ok -and $appr.Data.success) {"PASS"} else {"FAIL"}) ($appr.Raw.Substring(0, [Math]::Min(100, $appr.Raw.Length)))
        } else {
            Add-Test "ENT-XFER-02" "Transferencias" "Approve stock transfer" "FAIL" "No transferId"
        }

        $xferList = Get-Json $inventarista "/StockTransfer/Index"
        Add-Test "ENT-XFER-03" "Transferencias" "List transfers" $(if ($xferList.Ok -and $xferList.Data.data) {"PASS"} else {"FAIL"}) ""
    }
}

# RECIPES
$burger = Get-ProductByName $admin "Hamburguesa Enterprise"
if ($manager -and $pizza -and $burger) {
    $save = Get-Json $manager "/Recipe/Save" "POST" @{
        ProductId = $pizza.id; Name = "Pizza Cert Recipe"
        Lines = @(@{ IngredientProductId = $burger.id; Quantity = 0.5 })
    }
    Add-Test "ENT-REC-01" "Recetas" "Save recipe BOM" $(if ($save.Ok -and $save.Data.success) {"PASS"} else {"FAIL"}) ($save.Raw.Substring(0, [Math]::Min(100, $save.Raw.Length)))

    $byProd = Get-Json $manager "/Recipe/ByProduct?productId=$($pizza.id)"
    $hasRecipe = $byProd.Data.recipe -and $byProd.Data.recipe.lines -and @($byProd.Data.recipe.lines).Count -gt 0
    Add-Test "ENT-REC-02" "Recetas" "Get recipe by product" $(if ($byProd.Ok -and $hasRecipe) {"PASS"} else {"FAIL"}) ""
} else {
    Add-Test "ENT-REC-01" "Recetas" "Save recipe BOM" "FAIL" "Need pizza+burger products"
    Add-Test "ENT-REC-02" "Recetas" "Get recipe by product" "FAIL" "Need pizza+burger products"
}

# SUPERVISOR CANCEL POST-KITCHEN
if ($orderId -and $itemId -and $waiter -and $supervisor) {
    $waiterCancel = Get-Json $waiter "/Order/UpdateItemStatus" "POST" @{
        OrderId = $orderId; ItemId = $itemId; Status = "cancelled"
    }
    Add-Test "ENT-CANC-01" "Cancelacion" "Waiter blocked cancel in-kitchen item" $(if ($waiterCancel.Status -eq 403) {"PASS"} else {"FAIL"}) "Status=$($waiterCancel.Status)"

    $supCancel = Get-Json $supervisor "/Order/UpdateItemStatus" "POST" @{
        OrderId = $orderId; ItemId = $itemId; Status = "cancelled"
    }
    Add-Test "ENT-CANC-02" "Cancelacion" "Supervisor can cancel in-kitchen item" $(if ($supCancel.Ok -and $supCancel.Data.success) {"PASS"} else {"FAIL"}) ""
} else {
    Add-Test "ENT-CANC-01" "Cancelacion" "Waiter blocked" "FAIL" "No order/item"
    Add-Test "ENT-CANC-02" "Cancelacion" "Supervisor cancel" "FAIL" "No order/item"
}

$global:Results | Export-Csv "$outDir\ENTERPRISE_TEST_RESULTS.csv" -NoTypeInformation -Encoding UTF8
$global:Defects | Export-Csv "$outDir\ENTERPRISE_DEFECTS.csv" -NoTypeInformation -Encoding UTF8

Write-Host "`n=== ENTERPRISE CERTIFICATION SUMMARY ===" -ForegroundColor Cyan
Write-Host "PASSED: $global:Passed"
Write-Host "FAILED: $global:Failed"
Write-Host "TOTAL:  $($global:Passed + $global:Failed)"
if ($global:Failed -eq 0) { Write-Host "ENTERPRISE OPERATIONS CERTIFICATION: PASS" -ForegroundColor Green; exit 0 }
else { Write-Host "ENTERPRISE OPERATIONS CERTIFICATION: FAIL ($global:Failed defects)" -ForegroundColor Red; exit 1 }
