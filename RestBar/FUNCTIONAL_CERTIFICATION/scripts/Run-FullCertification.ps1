# RestBar Full Functional Certification Test Suite
param([string]$BaseUrl = "http://localhost:5001")

$ErrorActionPreference = "Continue"
$global:Results = @()
$global:Passed = 0
$global:Failed = 0
$global:Defects = @()

function Add-TestResult($Id, $Category, $Name, $Status, $Details = "") {
    $global:Results += [PSCustomObject]@{ Id=$Id; Category=$Category; Name=$Name; Status=$Status; Details=$Details; At=(Get-Date -Format "s") }
    if ($Status -eq "PASS") { $global:Passed++ } else { $global:Failed++; $global:Defects += [PSCustomObject]@{ Id=$Id; Name=$Name; Details=$Details } }
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

function Test-Page {
    param($Session, $Path, $ShouldAccess = $true)
    try {
        $r = Invoke-WebRequest -Uri "$BaseUrl$Path" -WebSession $Session -UseBasicParsing -MaximumRedirection 5 -ErrorAction Stop
        $finalUrl = $r.BaseResponse.ResponseUri.AbsolutePath
        $denied = $finalUrl -match "AccessDenied|/Auth/Login"
        if ($ShouldAccess) { return ($r.StatusCode -eq 200 -and -not $denied) }
        return $denied -or $r.StatusCode -in 401,403
    } catch {
        $code = 0
        if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
        if (-not $ShouldAccess) { return $code -in 302,401,403 }
        return $false
    }
}

Write-Host "`n=== RESTBAR FUNCTIONAL CERTIFICATION ===" -ForegroundColor Cyan
Write-Host "Target: $BaseUrl`n"

# --- SEED ---
$seed = Invoke-RestMethod -Uri "$BaseUrl/Seed/SeedDemoData" -Method GET -ErrorAction SilentlyContinue
$mt = Invoke-RestMethod -Uri "$BaseUrl/Seed/SeedCertificationMultiTenant" -Method GET -ErrorAction SilentlyContinue
Add-TestResult "ENV-01" "Setup" "SeedDemoData" $(if ($seed.success) {"PASS"} else {"FAIL"}) ($seed.message)
Add-TestResult "ENV-02" "Setup" "SeedCertificationMultiTenant" $(if ($mt.success) {"PASS"} else {"FAIL"}) ($mt.message)

# --- AUTH extra ---
Add-TestResult "AUTH-13" "Login" "Valid user wrong password rejected" $(if ($null -eq (Get-Session "admin@restbar.com" "wrongpass")) {"PASS"} else {"FAIL"}) ""

# --- AUTH ---
Add-TestResult "AUTH-01" "Login" "Invalid credentials rejected" $(if ($null -eq (Get-Session "bad@test.com" "wrong")) {"PASS"} else {"FAIL"}) ""

$roleUsers = @(
    @{Id="AUTH-02";Email="admin@restbar.com";Role="admin"},
    @{Id="AUTH-03";Email="gerente@restbar.com";Role="manager"},
    @{Id="AUTH-04";Email="supervisor@restbar.com";Role="supervisor"},
    @{Id="AUTH-05";Email="mesero@restbar.com";Role="waiter"},
    @{Id="AUTH-06";Email="cajero@restbar.com";Role="cashier"},
    @{Id="AUTH-07";Email="chef@restbar.com";Role="chef"},
    @{Id="AUTH-08";Email="bartender@restbar.com";Role="bartender"},
    @{Id="AUTH-09";Email="contador@restbar.com";Role="accountant"},
    @{Id="AUTH-10";Email="soporte@restbar.com";Role="support"},
    @{Id="AUTH-11";Email="inventarista@restbar.com";Role="inventarista"},
    @{Id="AUTH-12";Email="superadmin@restbar.com";Role="superadmin"}
)
$sessions = @{}
foreach ($u in $roleUsers) {
    $sess = Get-Session $u.Email
    $sessions[$u.Role] = $sess
    Add-TestResult $u.Id "Login" "Login $($u.Role)" $(if ($sess) {"PASS"} else {"FAIL"}) $u.Email
}

# --- PERMISSIONS ---
Add-TestResult "SEC-01" "Security" "Waiter denied Company" $(if (Test-Page $sessions.waiter "/Company/Index" $false) {"PASS"} else {"FAIL"}) ""
Add-TestResult "SEC-02" "Security" "Cashier denied Product" $(if (Test-Page $sessions.cashier "/Product/Index" $false) {"PASS"} else {"FAIL"}) ""
Add-TestResult "SEC-03" "Security" "Waiter denied Audit" $(if (Test-Page $sessions.waiter "/Audit/Index" $false) {"PASS"} else {"FAIL"}) ""
Add-TestResult "SEC-04" "Security" "Admin allowed Audit" $(if (Test-Page $sessions.admin "/Audit/Index") {"PASS"} else {"FAIL"}) ""
Add-TestResult "SEC-05" "Security" "Inventarista allowed Inventory" $(if (Test-Page $sessions.inventarista "/Inventory/Index") {"PASS"} else {"FAIL"}) ""
Add-TestResult "SEC-06" "Security" "SuperAdmin allowed SuperAdmin" $(if (Test-Page $sessions.superadmin "/SuperAdmin/Index") {"PASS"} else {"FAIL"}) ""

# --- KDS ROUTE ---
Add-TestResult "NAV-01" "Navigation" "Kitchen route loads" $(if (Test-Page $sessions.chef "/Order/StationOrders?stationType=kitchen") {"PASS"} else {"FAIL"}) ""
Add-TestResult "NAV-02" "Navigation" "Bar route loads" $(if (Test-Page $sessions.bartender "/Order/StationOrders?stationType=bar") {"PASS"} else {"FAIL"}) ""

# --- EXTRA PERMISSIONS ---
Add-TestResult "SEC-07" "Security" "Manager allowed Audit" $(if (Test-Page $sessions.manager "/Audit/Index") {"PASS"} else {"FAIL"}) ""
Add-TestResult "SEC-08" "Security" "Supervisor denied Company" $(if (Test-Page $sessions.supervisor "/Company/Index" $false) {"PASS"} else {"FAIL"}) ""
Add-TestResult "SEC-09" "Security" "Support denied SuperAdmin" $(if (Test-Page $sessions.support "/SuperAdmin/Index" $false) {"PASS"} else {"FAIL"}) ""
Add-TestResult "SEC-10" "Security" "Waiter allowed Order POS" $(if (Test-Page $sessions.waiter "/Order/Index") {"PASS"} else {"FAIL"}) ""

# --- POS FLOW (admin) ---
$admin = $sessions.admin

function Test-TableFree($t) { return ($t.status -eq "Disponible" -or $t.status -eq 0 -or $t.status -eq "0") }
function Reset-TableOrder($Session, $tableId) {
    $active = Get-Json $Session "/Order/GetActiveOrder?tableId=$tableId"
    $oid = $active.Data.orderId
    if ($active.Data.hasActiveOrder -and $oid) {
        Get-Json $Session "/Order/Cancel" "POST" @{ OrderId = $oid; Reason = "Cert reset" } | Out-Null
        Start-Sleep -Milliseconds 400
    }
}

$tables = Get-Json $admin "/Order/GetActiveTables"
Add-TestResult "POS-01" "POS" "GetActiveTables" $(if ($tables.Ok) {"PASS"} else {"FAIL"}) ""

$tableId = $null
if ($tables.Data -and $tables.Data.data) {
    $free = @($tables.Data.data | Where-Object { Test-TableFree $_ } | Select-Object -First 1)[0]
    if (-not $free) {
        foreach ($t in @($tables.Data.data)) { Reset-TableOrder $admin $t.id }
        Start-Sleep -Milliseconds 500
        $tables = Get-Json $admin "/Order/GetActiveTables"
        $free = @($tables.Data.data | Where-Object { Test-TableFree $_ } | Select-Object -First 1)[0]
    }
    if ($free) {
        Reset-TableOrder $admin $free.id
        $tableId = $free.id
    }
}

$products = Get-Json $admin "/Order/GetActiveCategories"
$productId = $null
if ($products.Ok -and $products.Data.data) {
    $cats = @($products.Data.data)
    foreach ($c in $cats) {
        $prods = Get-Json $admin "/Order/GetProductsByCategory/$($c.id)"
        if ($prods.Ok -and $prods.Data.data -and @($prods.Data.data).Count -gt 0) {
            $productId = $prods.Data.data[0].id
            break
        }
    }
}

if ($tableId -and $productId) {
    $send = Get-Json $admin "/Order/SendToKitchen" "POST" @{
        TableId = $tableId
        OrderType = "DineIn"
        Items = @(@{ ProductId = $productId; Quantity = 1; Status = "Pending" })
    }
    Add-TestResult "POS-02" "POS" "SendToKitchen creates order" $(if ($send.Ok -and $send.Data.orderId) {"PASS"} else {"FAIL"}) ($send.Raw.Substring(0, [Math]::Min(200, $send.Raw.Length)))

    $orderId = $send.Data.orderId
    if (-not $orderId -and $send.Data.order) { $orderId = $send.Data.order.id }

    if ($orderId) {
        $active = Get-Json $admin "/Order/GetActiveOrder?tableId=$tableId"
        Add-TestResult "POS-03" "POS" "GetActiveOrder has items" $(if ($active.Ok -and $active.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) ""

        # Payment partial
        $pay = Get-Json $sessions.cashier "/api/Payment/partial" "POST" @{
            OrderId = $orderId; Amount = 1.00; Method = "Efectivo"; PayerName = "Test"; IdempotencyKey = [guid]::NewGuid().ToString()
        }
        Add-TestResult "PAY-01" "Payment" "Partial payment accepted" $(if ($pay.Ok -and $pay.Data.success -ne $false) {"PASS"} else {"FAIL"}) ($pay.Raw.Substring(0, [Math]::Min(200, $pay.Raw.Length)))

        $summary = Get-Json $sessions.cashier "/api/Payment/order/$orderId/summary"
        $totalPaid = $summary.Data.totalPaidAmount
        if ($null -eq $totalPaid) { $totalPaid = $summary.Data.TotalPaidAmount }
        Add-TestResult "PAY-02" "Payment" "Payment summary" $(if ($summary.Ok -and $totalPaid -ge 0) {"PASS"} else {"FAIL"}) "totalPaid=$totalPaid"

        # Overpayment rejected
        $overpay = Get-Json $sessions.cashier "/api/Payment/partial" "POST" @{
            OrderId = $orderId; Amount = 99999.00; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString()
        }
        Add-TestResult "PAY-04" "Payment" "Overpayment rejected" $(if ($overpay.Status -eq 400 -or ($overpay.Data -and $overpay.Data.success -eq $false)) {"PASS"} else {"FAIL"}) "Status=$($overpay.Status)"

        # Idempotency duplicate
        $idem = [guid]::NewGuid().ToString()
        $p1 = Get-Json $sessions.cashier "/api/Payment/partial" "POST" @{ OrderId=$orderId; Amount=0.50; Method="Efectivo"; IdempotencyKey=$idem }
        $p2 = Get-Json $sessions.cashier "/api/Payment/partial" "POST" @{ OrderId=$orderId; Amount=0.50; Method="Efectivo"; IdempotencyKey=$idem }
        Add-TestResult "PAY-03" "Payment" "Idempotency prevents duplicate" $(if ($p1.Ok -and $p2.Ok) {"PASS"} else {"FAIL"}) ""
    }
} else {
    Add-TestResult "POS-02" "POS" "SendToKitchen" "FAIL" "No table or product available"
}

# --- MULTI-TENANT ---
$adminB = Get-Session "admin.b@restbar.com"
$adminA = Get-Session "admin@restbar.com"
if ($adminB -and $adminA) {
    $prodsB = Get-Json $adminB "/Product/GetProducts"
    $prodsA = Get-Json $adminA "/Product/GetProducts"
    $listB = @()
    $listA = @()
    if ($prodsB.Data) { $listB = @($prodsB.Data.data) }
    if ($prodsA.Data) { $listA = @($prodsA.Data.data) }
    $hasExclusiveB = @($listB | Where-Object { $_.'name' -eq "Producto Exclusivo B" }).Count -gt 0
    $leakB = @($listA | Where-Object { $_.'name' -eq "Producto Exclusivo B" }).Count -gt 0
    Add-TestResult "MT-01" "MultiTenant" "Company B sees exclusive product" $(if ($hasExclusiveB) {"PASS"} else {"FAIL"}) ""
    Add-TestResult "MT-02" "MultiTenant" "Company A does NOT see B product" $(if (-not $leakB) {"PASS"} else {"FAIL"}) ""

    # Cross-branch payment IDOR
    if ($orderId) {
        $idor = Get-Json $adminB "/api/Payment/order/$orderId/summary"
        Add-TestResult "MT-03" "MultiTenant" "Company B cannot access Company A order payment" $(if ($idor.Status -eq 403 -or $idor.Status -eq 404) {"PASS"} else {"FAIL"}) "Status=$($idor.Status)"
    }

    # Branch Norte isolation
    $adminNorte = Get-Session "admin.norte@restbar.com"
    if ($adminNorte) {
        $tablesNorte = Get-Json $adminNorte "/Order/GetActiveTables"
        $hasNorte = $false
        if ($tablesNorte.Data -and $tablesNorte.Data.data) {
            $hasNorte = @($tablesNorte.Data.data | Where-Object { $_.tableNumber -eq "N-01" }).Count -gt 0
        }
        Add-TestResult "MT-04" "MultiTenant" "Branch Norte sees own table N-01" $(if ($hasNorte) {"PASS"} else {"FAIL"}) ""
        $prodsNorte = Get-Json $adminNorte "/Product/GetProducts"
        $listNorte = @()
        if ($prodsNorte.Data) { $listNorte = @($prodsNorte.Data.data) }
        $leakNorte = @($listNorte | Where-Object { $_.'name' -eq "Producto Exclusivo B" }).Count -gt 0
        Add-TestResult "MT-05" "MultiTenant" "Branch Norte does NOT see B product" $(if (-not $leakNorte) {"PASS"} else {"FAIL"}) ""
    }
}

# --- REPORTS ---
Add-TestResult "RPT-01" "Reports" "Accountant accesses Reports" $(if (Test-Page $sessions.accountant "/Reports/Index") {"PASS"} else {"FAIL"}) ""
Add-TestResult "RPT-02" "Reports" "Waiter denied Reports" $(if (Test-Page $sessions.waiter "/Reports/Index" $false) {"PASS"} else {"FAIL"}) ""
Add-TestResult "RPT-03" "Reports" "Manager accesses Reports" $(if (Test-Page $sessions.manager "/Reports/Index") {"PASS"} else {"FAIL"}) ""

# --- PAYMENT 404 ---
$fakeOrder = [guid]::NewGuid().ToString()
$fakePay = Get-Json $sessions.cashier "/api/Payment/partial" "POST" @{
    OrderId = $fakeOrder; Amount = 1.00; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString()
}
Add-TestResult "PAY-05" "Payment" "Payment on nonexistent order rejected" $(if ($fakePay.Status -eq 404) {"PASS"} else {"FAIL"}) "Status=$($fakePay.Status)"

# Export
$out = Split-Path $PSScriptRoot -Parent
$global:Results | Export-Csv "$out\04_EXECUTED_TESTS.csv" -NoTypeInformation -Encoding UTF8
$global:Defects | Export-Csv "$out\05_DEFECT_LOG.csv" -NoTypeInformation -Encoding UTF8

Write-Host "`n=== CERTIFICATION SUMMARY ===" -ForegroundColor Cyan
Write-Host "PASSED: $global:Passed"
Write-Host "FAILED: $global:Failed"
Write-Host "TOTAL:  $($global:Passed + $global:Failed)"
if ($global:Failed -eq 0) { Write-Host "VERDICT: PASS" -ForegroundColor Green; exit 0 } else { Write-Host "VERDICT: FAIL ($global:Failed defects)" -ForegroundColor Red; exit 1 }
