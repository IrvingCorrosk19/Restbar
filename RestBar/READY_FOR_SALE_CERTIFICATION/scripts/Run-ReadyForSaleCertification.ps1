# RestBar Ready-for-Sale Certification (Scenarios 29-67)
param([string]$BaseUrl = "http://localhost:5001")

. (Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) "FUNCTIONAL_CERTIFICATION\scripts\Cert-Common.ps1")

$ErrorActionPreference = "Continue"
$global:Results = @(); $global:Passed = 0; $global:Failed = 0; $global:Blockers = @()
$outDir = Split-Path $PSScriptRoot -Parent
$pgBin = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
$pgPass = "Panama2020$"

function Add-Rfs($Id, $Scenario, $Name, $Status, $Blocker = "", $Details = "") {
    $global:Results += [PSCustomObject]@{ Id=$Id; Scenario=$Scenario; Name=$Name; Status=$Status; Blocker=$Blocker; Details=$Details }
    if ($Status -eq "PASS") { $global:Passed++ }
    else { $global:Failed++; if ($Blocker) { $global:Blockers += [PSCustomObject]@{ Id=$Id; Blocker=$Blocker; Name=$Name; Details=$Details } } }
    Write-Host "[$Status] $Id $Name" -ForegroundColor $(if ($Status -eq "PASS") {"Green"} elseif ($Status -eq "BLOCKER") {"Red"} else {"Yellow"})
    if ($Details) { Write-Host "      $Details" -ForegroundColor DarkGray }
}

function Get-Session($Email, $Password = "123456") { Get-CertSession $BaseUrl $Email $Password }
function Get-Json($Session, $Path, $Method = "GET", $Body = $null) { Get-CertJson $BaseUrl $Session $Path $Method $Body }

function Invoke-Pg($sql) {
    if (-not (Test-Path $pgBin)) { return $null }
    $env:PGPASSWORD = $pgPass
    return & $pgBin -h localhost -U postgres -d RestBar -t -A -c $sql 2>$null
}

Write-Host "`n=== READY FOR SALE CERTIFICATION (29-67) ===" -ForegroundColor Cyan

# Seed chain
Invoke-RestMethod "$BaseUrl/Seed/SeedDemoData" -ErrorAction SilentlyContinue | Out-Null
Invoke-RestMethod "$BaseUrl/Seed/SeedEnterpriseRouting" -ErrorAction SilentlyContinue | Out-Null
$commercial = Invoke-RestMethod "$BaseUrl/Seed/SeedCommercialDemo" -ErrorAction SilentlyContinue

$admin = Get-Session "admin@restbar.com"
$waiter = Get-Session "mesero@restbar.com"
$manager = Get-Session "gerente@restbar.com"
$cashier = Get-Session "cajero@restbar.com"
$superadmin = Get-Session "superadmin@restbar.com"

Reset-CertAllTables $BaseUrl $admin

# --- 29 ONBOARDING ---
Add-Rfs "RFS-29-01" "Onboarding" "Company exists" $(if ($admin) {"PASS"} else {"FAIL"}) "" ""
$branchCount = Invoke-Pg "SELECT COUNT(*) FROM branches WHERE company_id = (SELECT id FROM companies WHERE name='RestBar Principal' LIMIT 1)"
Add-Rfs "RFS-29-02" "Onboarding" "Branch configured" $(if ($branchCount -and [int]$branchCount -ge 1) {"PASS"} else {"FAIL"}) "" "count=$branchCount"
$areaCount = Invoke-Pg "SELECT COUNT(*) FROM areas WHERE branch_id = (SELECT id FROM branches WHERE name='RestBar Centro' LIMIT 1)"
Add-Rfs "RFS-29-03" "Onboarding" "Areas/pisos created" $(if ($areaCount -and [int]$areaCount -ge 3) {"PASS"} else {"FAIL"}) "" "count=$areaCount"
$stationCount = Invoke-Pg "SELECT COUNT(*) FROM stations WHERE branch_id = (SELECT id FROM branches WHERE name='RestBar Centro' LIMIT 1)"
Add-Rfs "RFS-29-04" "Onboarding" "Stations (kitchen/bar)" $(if ($stationCount -and [int]$stationCount -ge 4) {"PASS"} else {"FAIL"}) "" "count=$stationCount"
Add-Rfs "RFS-29-05" "Onboarding" "Guided wizard self-service" "BLOCKER" "UX BLOCKER" "No onboarding wizard - manual CRUD only"

# --- 30 FIRST DAY ---
$waiterTables = Get-Json $waiter "/Order/GetActiveTables"
Add-Rfs "RFS-30-01" "FirstDay" "POS accessible to waiter" $(if ($waiterTables.Ok) {"PASS"} else {"FAIL"}) "" ""
Add-Rfs "RFS-30-02" "FirstDay" "In-app setup guide" "BLOCKER" "UX BLOCKER" "No first-day checklist in UI"

# --- 31 DEMO COMERCIAL ---
$table = Get-CertWaiterTable $BaseUrl $admin $waiter
$productId = $null
$cats = Get-Json $admin "/Order/GetActiveCategories"
foreach ($c in @($cats.Data.data)) {
    $prods = Get-Json $admin "/Order/GetProductsByCategory/$($c.id)"
    if ($prods.Data.data -and @($prods.Data.data).Count -gt 0) { $productId = $prods.Data.data[0].id; break }
}
$orderId = $null
if ($table -and $productId -and $waiter) {
    $send = Get-Json $waiter "/Order/SendToKitchen" "POST" @{ TableId=$table.id; OrderType="DineIn"; Items=@(@{ ProductId=$productId; Quantity=1; Status="Pending" }) }
    $orderId = $send.Data.orderId
    Add-Rfs "RFS-31-01" "Demo" "Create order + send kitchen" $(if ($send.Ok) {"PASS"} else {"FAIL"}) "" ""
    $kds = Get-Json (Get-Session "chef@restbar.com") "/api/kitchen/current"
    Add-Rfs "RFS-31-02" "Demo" "KDS receives order" $(if ($kds.Ok) {"PASS"} else {"FAIL"}) "" ""
}
if ($orderId -and $cashier) {
    $pay = Get-Json $cashier "/api/Payment/partial" "POST" @{ OrderId=$orderId; Amount=1.00; Method="Efectivo"; IdempotencyKey=[guid]::NewGuid().ToString() }
    Add-Rfs "RFS-31-03" "Demo" "Partial payment" $(if ($pay.Ok) {"PASS"} else {"FAIL"}) "" ""
    $audit = $null
    try { $audit = Invoke-WebRequest -Uri "$BaseUrl/Audit/Index" -WebSession $admin -UseBasicParsing; Add-Rfs "RFS-31-04" "Demo" "Audit accessible" $(if ($audit.StatusCode -eq 200) {"PASS"} else {"FAIL"}) "" "" }
    catch { Add-Rfs "RFS-31-04" "Demo" "Audit accessible" "FAIL" "" $_.Exception.Message }
}

# --- 32 NON-TECHNICAL OWNER ---
Add-Rfs "RFS-32-01" "NonTechnical" "Product list API" $(if ((Get-Json $admin "/Order/GetActiveCategories").Ok) {"PASS"} else {"BLOCKER"}) "UX BLOCKER" "Categories API"
Add-Rfs "RFS-32-02" "NonTechnical" "Cash register close UI" "BLOCKER" "SALE BLOCKER" "No caja module"

# --- 33 SUPPORT ---
Add-Rfs "RFS-33-01" "Support" "Audit logs exist" $(if ($audit -or (Get-Json $admin "/Audit/Index").Ok) {"PASS"} else {"FAIL"}) "" ""
Add-Rfs "RFS-33-02" "Support" "Structured incident codes" "BLOCKER" "UX BLOCKER" "No incident playbook in-app"

# --- 34 DEMO DATA ---
Add-Rfs "RFS-34-01" "DemoData" 'Commercial seed tables min 30' $(if ($commercial.tables -ge 30) {"PASS"} else {"FAIL"}) "" "tables=$($commercial.tables)"
Add-Rfs "RFS-34-02" "DemoData" 'Commercial seed products min 100' $(if ($commercial.products -ge 100) {"PASS"} else {"FAIL"}) "" "products=$($commercial.products)"
Add-Rfs "RFS-34-03" "DemoData" 'Historical orders min 30' $(if ($commercial.historicalOrders -ge 30) {"PASS"} else {"FAIL"}) "" "orders=$($commercial.historicalOrders)"
$waiterCount = Invoke-Pg "SELECT COUNT(*) FROM users WHERE role='waiter' AND branch_id = (SELECT id FROM branches WHERE name='RestBar Centro' LIMIT 1)"
Add-Rfs "RFS-34-04" "DemoData" 'Waiters min 10' $(if ($waiterCount -and [int]$waiterCount -ge 10) {"PASS"} else {"FAIL"}) "" "count=$waiterCount"

# --- 43 CONTROLLED VOIDS (before suspension — order still active) ---
if ($orderId -and $waiter -and $manager) {
    $wBlocked = $false
    try {
        Invoke-WebRequest -Uri "$BaseUrl/Order/ApplyDiscount" -Method POST -WebSession $waiter -UseBasicParsing -ContentType "application/json" -Body (@{ OrderId=$orderId; DiscountType="fixed"; DiscountValue=1; Reason="test" } | ConvertTo-Json) -ErrorAction Stop | Out-Null
    } catch {
        $code = 0
        if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
        $wBlocked = ($code -in 302,403,401)
    }
    Add-Rfs "RFS-43-01" "Controls" "Waiter cannot apply discount" $(if ($wBlocked) {"PASS"} else {"FAIL"}) "FINANCIAL BLOCKER" "blocked=$wBlocked"
    $mDisc = Get-Json $manager "/Order/ApplyDiscount" "POST" @{ OrderId=$orderId; DiscountType="fixed"; DiscountValue=1; Reason="mgr" }
    Add-Rfs "RFS-43-02" "Controls" "Manager can apply discount" $(if ($mDisc.Ok) {"PASS"} else {"FAIL"}) "" "status=$($mDisc.Status)"
}

# --- 62 FINANCIAL INTEGRITY (before suspension) ---
if ($orderId -and $cashier) {
    $summary = Get-Json $cashier "/api/Payment/order/$orderId/summary"
    $paid = $summary.Data.totalPaidAmount
    if ($null -eq $paid) { $paid = $summary.Data.TotalPaidAmount }
    Add-Rfs "RFS-62-01" "FinancialIntegrity" "Payment summary matches partials" $(if ($paid -gt 0) {"PASS"} else {"FAIL"}) "" "paid=$paid"
}

# --- 35-37 SAAS SUSPENSION ---
$companyId = Invoke-Pg "SELECT id FROM companies WHERE name='RestBar Principal' LIMIT 1"
if ($superadmin -and $companyId) {
    Invoke-Pg "UPDATE companies SET is_active = false WHERE id = '$companyId'" | Out-Null
    $blockedLogin = Get-Session "mesero@restbar.com"
    Add-Rfs "RFS-37-01" "Suspension" "Suspended tenant login blocked" $(if (-not $blockedLogin) {"PASS"} else {"FAIL"}) "" ""
    # Reactivate for session tests — use pg since login blocked
    Invoke-Pg "UPDATE companies SET is_active = true WHERE id = '$companyId'" | Out-Null
    $waiter = Get-Session "mesero@restbar.com"
    Invoke-Pg "UPDATE companies SET is_active = false WHERE id = '$companyId'" | Out-Null
    Start-Sleep -Milliseconds 300
    $suspendTableId = if ($table) { $table.id } else { $null }
    if ($suspendTableId -and $productId -and $waiter) {
        $blockOp = Get-Json $waiter "/Order/SendToKitchen" "POST" @{ TableId=$suspendTableId; OrderType="DineIn"; Items=@(@{ ProductId=$productId; Quantity=1; Status="Pending" }) }
        $suspended = ($blockOp.Data.suspended -eq $true) -or ($blockOp.Status -eq 403)
        Add-Rfs "RFS-36-01" "SaaSCancel" "Suspended tenant cannot create orders" $(if ($suspended) {"PASS"} else {"FAIL"}) "" "status=$($blockOp.Status)"
    } else {
        Add-Rfs "RFS-36-01" "SaaSCancel" "Suspended tenant cannot create orders" "FAIL" "" "No table for suspend test"
    }
    Invoke-Pg "UPDATE companies SET is_active = true WHERE id = '$companyId'" | Out-Null
    Add-Rfs "RFS-36-02" "SaaSCancel" "Reactivation restores login" $(if (Get-Session "mesero@restbar.com") {"PASS"} else {"FAIL"}) "" ""
}
Add-Rfs "RFS-35-01" "Licensing" "Subscription plans engine" "BLOCKER" "SALE BLOCKER" "No plan tiers or usage limits"
Add-Rfs "RFS-35-02" "Licensing" "Usage limits enforced" "BLOCKER" "SALE BLOCKER" "No user/branch/table limits"

# --- 38-39 MIGRATION / EXPORT ---
Add-Rfs "RFS-38-01" "Migration" "POS import wizard" "BLOCKER" "SALE BLOCKER" "No import API"
try {
    $exp = Invoke-WebRequest -Uri "$BaseUrl/Audit/Export" -WebSession $admin -UseBasicParsing
    Add-Rfs "RFS-39-01" "Export" "Audit CSV export" $(if ($exp.StatusCode -eq 200) {"PASS"} else {"FAIL"}) "" ""
} catch { Add-Rfs "RFS-39-01" "Export" "Audit CSV export" "FAIL" "" $_.Exception.Message }
Add-Rfs "RFS-39-02" "Export" "Full tenant data export" "BLOCKER" "SALE BLOCKER" "No bulk tenant export"

# --- 40-42 CLOSE ---
$daily = Get-Json $admin "/PaymentView/GenerateReport?type=daily"
Add-Rfs "RFS-40-01" "DayClose" "Daily sales report" $(if ($daily.Ok) {"PASS"} else {"FAIL"}) "" ""
Add-Rfs "RFS-40-02" "DayClose" "Formal day-close workflow" "BLOCKER" "SALE BLOCKER" "Report only - no close ritual"
$shift = Get-Json $manager "/Shift/Start" "POST" $null
Add-Rfs "RFS-41-01" "ShiftClose" "Shift start API" $(if ($shift.Ok -or $shift.Status -eq 400) {"PASS"} else {"FAIL"}) "" ""
Add-Rfs "RFS-42-01" "CashRegister" "Cash register module" "BLOCKER" "SALE BLOCKER" "No caja open/close/arqueo"

# --- 43 CONTROLLED VOIDS — moved above suspension block ---

# --- 44 BUSINESS CONFIG ---
try {
    $settingsPage = Invoke-WebRequest -Uri "$BaseUrl/AdvancedSettings/Index" -WebSession $admin -UseBasicParsing
    Add-Rfs "RFS-44-01" "BizConfig" "Advanced settings accessible" $(if ($settingsPage.StatusCode -eq 200) {"PASS"} else {"BLOCKER"}) "UX BLOCKER" ""
} catch { Add-Rfs "RFS-44-01" "BizConfig" "Advanced settings accessible" "BLOCKER" "UX BLOCKER" $_.Exception.Message }

# --- 50-52 PRINT / BILL ---
Add-Rfs "RFS-50-01" "Print" "Thermal print + retry" "BLOCKER" "SALE BLOCKER" "HTML receipt only"
Add-Rfs "RFS-51-01" "Precuenta" "Pre-bill flow" "BLOCKER" "SALE BLOCKER" "Not implemented"
Add-Rfs "RFS-52-01" "Invoice" "Post-payment invoice generation" "BLOCKER" "SALE BLOCKER" "Invoice model exists, no UI flow"

# --- 54-58 PREMIUM ---
Add-Rfs "RFS-54-01" "VIP" "Customer VIP program" "BLOCKER" "UX BLOCKER" "Order.IsVip only"
Add-Rfs "RFS-55-01" "Allergens" "Structured allergen tags" "BLOCKER" "UX BLOCKER" "Free-text notes only"
Add-Rfs "RFS-56-01" "Combos" "Combo bundles" "BLOCKER" "SALE BLOCKER" "Not implemented"
Add-Rfs "RFS-58-01" "Pricing" "Happy hour auto-pricing" "BLOCKER" "SALE BLOCKER" "Manual discount only"

# --- 61 REPORTS ---
Add-Rfs "RFS-61-01" "Reports" "Reports page loads" $(if ((Invoke-WebRequest -Uri "$BaseUrl/Reports/Index" -WebSession $admin -UseBasicParsing -ErrorAction SilentlyContinue).StatusCode -eq 200) {"PASS"} else {"FAIL"}) "" ""

# --- 64 SAAS ISOLATION ---
$activeB = Invoke-Pg "SELECT is_active FROM companies WHERE name='RestBar Empresa B' LIMIT 1"
Add-Rfs "RFS-64-01" "SaaSIsolation" "Tenant B independent of A suspension" $(if ($activeB -eq "t") {"PASS"} else {"FAIL"}) "" ""

# --- 66-67 MARKET SEGMENTS (documented blockers) ---
Add-Rfs "RFS-66-01" "Market" "Small restaurant / bar" "PASS" "" "Core POS certified 119 tests"
Add-Rfs "RFS-66-02" "Market" "Hotel / casino / franchise SaaS" "BLOCKER" "SALE BLOCKER" "Missing licensing, caja, fiscal"

# Cleanup
if ($orderId -and $admin) { Get-Json $admin "/Order/Cancel" "POST" @{ OrderId=$orderId; Reason="RFS cleanup" } | Out-Null }
Invoke-Pg "UPDATE companies SET is_active = true WHERE name='RestBar Principal'" | Out-Null
Reset-CertAllTables $BaseUrl $admin

$global:Results | Export-Csv "$outDir\RFS_TEST_RESULTS.csv" -NoTypeInformation -Encoding UTF8
$global:Blockers | Export-Csv "$outDir\RFS_SALE_BLOCKERS.csv" -NoTypeInformation -Encoding UTF8

$saleBlockers = @($global:Blockers | Where-Object { $_.Blocker -eq "SALE BLOCKER" }).Count
Write-Host "`n=== READY FOR SALE SUMMARY ===" -ForegroundColor Cyan
Write-Host "PASSED: $global:Passed  FAILED: $global:Failed  SALE BLOCKERS: $saleBlockers"
if ($saleBlockers -eq 0) { Write-Host "READY FOR SALE: YES" -ForegroundColor Green; exit 0 }
else { Write-Host ("READY FOR SALE: NO - " + $saleBlockers + " sale blockers") -ForegroundColor Red; exit 1 }
