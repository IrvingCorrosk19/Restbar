# RestBar — Certificación funcional 3 empresas multitenant
param([string]$BaseUrl = "http://localhost:5001")

$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $root "FUNCTIONAL_CERTIFICATION\scripts\Cert-Common.ps1")

$ErrorActionPreference = "Continue"
$outDir = Split-Path $PSScriptRoot -Parent
$global:Results = @(); $global:Passed = 0; $global:Failed = 0

function Add-Tc($Id, $Cat, $Name, $Status, $Details = "") {
    $global:Results += [PSCustomObject]@{ Id=$Id; Category=$Cat; Name=$Name; Status=$Status; Details=$Details }
    if ($Status -eq "PASS") { $global:Passed++ } else { $global:Failed++ }
    Write-Host "[$Status] $Id $Name" -ForegroundColor $(if ($Status -eq "PASS") {"Green"} else {"Red"})
    if ($Details) { Write-Host "      $Details" -ForegroundColor DarkGray }
}

function Get-S($Email) { Get-CertSession $BaseUrl $Email }
function Gj($S, $Path, $Method = "GET", $Body = $null) { Get-CertJson $BaseUrl $S $Path $Method $Body }
function Test-Page($S, $Path, $Allow = $true) {
    try {
        $r = Invoke-WebRequest -Uri "$BaseUrl$Path" -WebSession $S -UseBasicParsing -MaximumRedirection 5
        $denied = $r.BaseResponse.ResponseUri.AbsolutePath -match "AccessDenied|/Auth/Login"
        if ($Allow) { return ($r.StatusCode -eq 200 -and -not $denied) }
        return $denied -or $r.StatusCode -in 401,403
    } catch {
        $code = 0; if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
        return (-not $Allow) -and ($code -in 302,401,403)
    }
}

Write-Host "`n=== 3-COMPANIES MULTITENANT CERTIFICATION ===" -ForegroundColor Cyan

$seed = Invoke-RestMethod "$BaseUrl/Seed/SeedThreeCompaniesCertification" -ErrorAction Stop
Add-Tc "TC3-ENV-01" "Setup" "SeedThreeCompaniesCertification" $(if ($seed.success) {"PASS"} else {"FAIL"}) $seed.message

# Sessions
$costaAdmin = Get-S "admin@costa.restbar.com"
$costaWaiter = Get-S "mesero1@costa.restbar.com"
$costaWaiter2 = Get-S "mesero2@costa.restbar.com"
$costaCashier = Get-S "cajero@costa.restbar.com"
$costaChef = Get-S "chef@costa.restbar.com"
$costaManager = Get-S "manager@costa.restbar.com"
$norteAdmin = Get-S "admin@norte.restbar.com"
$norteWaiter = Get-S "mesero1@norte.restbar.com"
$surAdmin = Get-S "admin@sur.restbar.com"
$surWaiter = Get-S "mesero1@sur.restbar.com"
$superadmin = Get-S "superadmin@restbar.com"

Add-Tc "TC3-ENV-02" "Setup" "Costa users login" $(if ($costaAdmin -and $costaWaiter -and $costaCashier) {"PASS"} else {"FAIL"}) ""
Add-Tc "TC3-ENV-03" "Setup" "Norte users login" $(if ($norteAdmin -and $norteWaiter) {"PASS"} else {"FAIL"}) ""
Add-Tc "TC3-ENV-04" "Setup" "Sur users login" $(if ($surAdmin -and $surWaiter) {"PASS"} else {"FAIL"}) ""
Add-Tc "TC3-ENV-05" "Setup" "SuperAdmin login" $(if ($superadmin) {"PASS"} else {"FAIL"}) ""

Reset-CertAllTables $BaseUrl $costaAdmin

# --- MULTITENANT ISOLATION ---
$tablesCosta = Gj $costaAdmin "/Order/GetActiveTables"
$tablesNorte = Gj $norteAdmin "/Order/GetActiveTables"
$tablesSur = Gj $surAdmin "/Order/GetActiveTables"
$numsCosta = @($tablesCosta.Data.data | ForEach-Object { $_.tableNumber })
$numsNorte = @($tablesNorte.Data.data | ForEach-Object { $_.tableNumber })
$numsSur = @($tablesSur.Data.data | ForEach-Object { $_.tableNumber })
$leakCN = @($numsCosta | Where-Object { $numsNorte -contains $_ }).Count -gt 0
$leakCS = @($numsCosta | Where-Object { $numsSur -contains $_ }).Count -gt 0
Add-Tc "TC3-MT-01" "MultiTenant" "Costa tables isolated from Norte" $(if (-not $leakCN) {"PASS"} else {"FAIL"}) "costa=$($numsCosta.Count) norte=$($numsNorte.Count)"
Add-Tc "TC3-MT-02" "MultiTenant" "Costa tables isolated from Sur" $(if (-not $leakCS) {"PASS"} else {"FAIL"}) "sur=$($numsSur.Count)"

function Get-ExclusiveVisible($Session, $exclusiveName) {
    $cats = Gj $Session "/Order/GetActiveCategories"
    foreach ($c in @($cats.Data.data)) {
        $prods = Gj $Session "/Order/GetProductsByCategory/$($c.id)"
        if (@($prods.Data.data | Where-Object { $_.name -eq $exclusiveName }).Count -gt 0) { return $true }
    }
    return $false
}

Add-Tc "TC3-MT-03" "MultiTenant" "Costa sees exclusive Costa only" $(if (Get-ExclusiveVisible $costaAdmin "Producto Exclusivo Costa") {"PASS"} else {"FAIL"}) ""
Add-Tc "TC3-MT-04" "MultiTenant" "Norte does NOT see Costa exclusive" $(if (-not (Get-ExclusiveVisible $norteAdmin "Producto Exclusivo Costa")) {"PASS"} else {"FAIL"}) ""
Add-Tc "TC3-MT-05" "MultiTenant" "Sur does NOT see Norte exclusive" $(if (-not (Get-ExclusiveVisible $surAdmin "Producto Exclusivo Norte")) {"PASS"} else {"FAIL"}) ""

# --- WAITER ASSIGNMENTS ---
$w1Tables = Gj $costaWaiter "/Order/GetActiveTables"
$w2Tables = Gj $costaWaiter2 "/Order/GetActiveTables"
$w1Nums = @($w1Tables.Data.data | ForEach-Object { $_.tableNumber })
$w2Nums = @($w2Tables.Data.data | ForEach-Object { $_.tableNumber })
$overlap = @($w1Nums | Where-Object { $w2Nums -contains $_ }).Count
Add-Tc "TC3-ASG-01" "Assignments" "Mesero1 and Mesero2 different areas" $(if ($w1Tables.Ok -and $w2Tables.Ok -and $overlap -eq 0) {"PASS"} else {"FAIL"}) "w1=$($w1Nums.Count) w2=$($w2Nums.Count) overlap=$overlap"

# --- ROLE PERMISSIONS ---
Add-Tc "TC3-ROL-01" "Roles" "Waiter denied Company admin" $(if (Test-Page $costaWaiter "/Company/Index" $false) {"PASS"} else {"FAIL"}) ""
Add-Tc "TC3-ROL-02" "Roles" "Chef denied Reports" $(if (Test-Page $costaChef "/Reports/Index" $false) {"PASS"} else {"FAIL"}) ""
Add-Tc "TC3-ROL-03" "Roles" "Waiter allowed Order POS" $(if (Test-Page $costaWaiter "/Order/Index") {"PASS"} else {"FAIL"}) ""
Add-Tc "TC3-ROL-04" "Roles" "Manager allowed Reports" $(if (Test-Page $costaManager "/Reports/Index") {"PASS"} else {"FAIL"}) ""
Add-Tc "TC3-ROL-05" "Roles" "SuperAdmin allowed SuperAdmin" $(if (Test-Page $superadmin "/SuperAdmin/Index") {"PASS"} else {"FAIL"}) ""

# --- ORDER FLOW per company ---
function Test-CompanyOrderFlow($Label, $Admin, $Waiter, $Cashier, $Prefix) {
    Reset-CertAllTables $BaseUrl $Admin
    $table = Get-CertWaiterTable $BaseUrl $Admin $Waiter
    if (-not $table) {
        $table = Get-CertAdminFreeTable $BaseUrl $Admin
    }
    $productId = $null
    $cats = Gj $Admin "/Order/GetActiveCategories"
    foreach ($c in @($cats.Data.data)) {
        $prods = Gj $Admin "/Order/GetProductsByCategory/$($c.id)"
        $p = @($prods.Data.data | Where-Object { $_.name -like "Hamburguesa*" } | Select-Object -First 1)
        if ($p.Count -gt 0) { $productId = $p[0].id; break }
    }
    if (-not $table -or -not $productId) {
        Add-Tc "TC3-ORD-$Label" "OrderFlow" "$Prefix full order flow" "FAIL" "No table/product"
        return $null
    }
    $send = Gj $Waiter "/Order/SendToKitchen" "POST" @{
        TableId = $table.id; OrderType = "DineIn"
        Items = @(@{ ProductId = $productId; Quantity = 1; Status = "Pending"; Notes = "Sin cebolla" })
    }
    $oid = $send.Data.orderId
    Add-Tc "TC3-ORD-$Label-01" "OrderFlow" "$Prefix send to kitchen" $(if ($send.Ok -and $oid) {"PASS"} else {"FAIL"}) ""

    if ($oid) {
        $st = Gj $Admin "/Order/GetOrderStatus/$oid"
        $stationId = $null
        if ($st.Data.items -and $st.Data.items.Count -gt 0) { $stationId = $st.Data.items[0].preparedByStationId }
        Add-Tc "TC3-RTE-$Label-01" "Routing" "$Prefix item has station" $(if ($stationId) {"PASS"} else {"FAIL"}) "station=$stationId"

        $p1 = Gj $Admin "/Person/CreatePerson" "POST" @{ OrderId = $oid; Name = "Cliente 1" }
        $p2 = Gj $Admin "/Person/CreatePerson" "POST" @{ OrderId = $oid; Name = "Cliente 2" }
        Add-Tc "TC3-SPL-$Label-01" "SplitAccount" "$Prefix create 2 persons" $(if ($p1.Ok -and $p2.Ok) {"PASS"} else {"FAIL"}) ""

        $pay = Gj $Cashier "/api/Payment/partial" "POST" @{
            OrderId = $oid; Amount = 1.00; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString()
        }
        Add-Tc "TC3-PAY-$Label-01" "Payment" "$Prefix partial payment" $(if ($pay.Ok) {"PASS"} else {"FAIL"}) ""

        $allTables = Gj $Admin "/Order/GetActiveTables"
        $dest = @($allTables.Data.data | Where-Object { $_.id -ne $table.id -and (Test-CertTableFree $_) } | Select-Object -First 1)
        if ($dest.Count -gt 0) {
            $move = Gj $Admin "/Order/MoveToTable" "POST" @{ OrderId = $oid; TargetTableId = $dest[0].id }
            Add-Tc "TC3-MOV-$Label-01" "TableChange" "$Prefix move order" $(if ($move.Ok -and $move.Data.success) {"PASS"} else {"FAIL"}) ""
        }

        $cancelTable = Get-CertAdminFreeTable $BaseUrl $Admin
        if ($cancelTable) {
            $cs = Gj $Waiter "/Order/SendToKitchen" "POST" @{
                TableId = $cancelTable.id; OrderType = "DineIn"
                Items = @(@{ ProductId = $productId; Quantity = 1; Status = "Pending" })
            }
            $cOid = $cs.Data.orderId
            if ($cOid -and $cOid -ne $oid) {
                $cancel = Gj $Admin "/Order/Cancel" "POST" @{ OrderId = $cOid; Reason = "Cliente canceló" }
                Add-Tc "TC3-CAN-$Label-01" "Cancellation" "$Prefix cancel before pay" $(if ($cancel.Ok) {"PASS"} else {"FAIL"}) ""
            }
        }

        Gj $Admin "/Order/Cancel" "POST" @{ OrderId = $oid; Reason = "Cert cleanup" } | Out-Null
    }
    return $oid
}

$costaOrderId = Test-CompanyOrderFlow "C" $costaAdmin $costaWaiter $costaCashier "Costa"
$norteCashier = Get-S "cajero@norte.restbar.com"
$surCashier = Get-S "cajero@sur.restbar.com"
Test-CompanyOrderFlow "N" $norteAdmin $norteWaiter $norteCashier "Norte" | Out-Null
Test-CompanyOrderFlow "S" $surAdmin $surWaiter $surCashier "Sur" | Out-Null

# --- IDOR cross-company (use fresh Costa order) ---
Reset-CertAllTables $BaseUrl $costaAdmin
$ct = Get-CertWaiterTable $BaseUrl $costaAdmin $costaWaiter
$cid = $null
if ($ct) {
    $cats = Gj $costaAdmin "/Order/GetActiveCategories"
    $prodId = $null
    foreach ($c in @($cats.Data.data)) {
        $prods = Gj $costaAdmin "/Order/GetProductsByCategory/$($c.id)"
        if ($prods.Data.data) { $prodId = $prods.Data.data[0].id; break }
    }
    if ($prodId) {
        $s = Gj $costaWaiter "/Order/SendToKitchen" "POST" @{
            TableId = $ct.id; OrderType = "DineIn"
            Items = @(@{ ProductId = $prodId; Quantity = 1; Status = "Pending" })
        }
        $cid = $s.Data.orderId
    }
}
if ($cid) {
    $idorPay = Gj $norteAdmin "/api/Payment/order/$cid/summary"
    Add-Tc "TC3-SEC-01" "Security" "Norte cannot read Costa payment" $(if ($idorPay.Status -eq 403) {"PASS"} else {"FAIL"}) "status=$($idorPay.Status)"
    $idorCancel = Gj $norteAdmin "/Order/Cancel" "POST" @{ OrderId = $cid; Reason = "hack" }
    Add-Tc "TC3-SEC-02" "Security" "Norte cannot cancel Costa order" $(if ($idorCancel.Status -eq 403) {"PASS"} else {"FAIL"}) "status=$($idorCancel.Status)"
    $idorTable = Gj $norteAdmin "/Order/GetActiveOrder?tableId=$($ct.id)"
    Add-Tc "TC3-SEC-03" "Security" "Norte cannot read Costa table order" $(if ($idorTable.Status -eq 403 -or -not $idorTable.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) ""
    $fake = [guid]::NewGuid()
    Add-Tc "TC3-SEC-04" "Security" "Fake order payment 404" $(if ((Gj $costaCashier "/api/Payment/partial" "POST" @{ OrderId=$fake; Amount=1; Method="Efectivo"; IdempotencyKey=[guid]::NewGuid().ToString() }).Status -eq 404) {"PASS"} else {"FAIL"}) ""
    Gj $costaAdmin "/Order/Cancel" "POST" @{ OrderId = $cid; Reason = "cleanup" } | Out-Null
}

# Cross-company move table
if ($ct -and $tablesNorte.Data.data) {
    $norteTable = $tablesNorte.Data.data[0]
    if ($cid) {
        $badMove = Gj $costaAdmin "/Order/MoveToTable" "POST" @{ OrderId = $cid; TargetTableId = $norteTable.id }
        Add-Tc "TC3-MOV-SEC-01" "TableChange" "Cannot move order to other company table" $(if ($badMove.Status -in 400,403,404) {"PASS"} else {"FAIL"}) "status=$($badMove.Status)"
    }
}

# --- AUDIT & REPORTS ---
try {
    $audit = Invoke-WebRequest -Uri "$BaseUrl/Audit/Index" -WebSession $costaAdmin -UseBasicParsing
    Add-Tc "TC3-AUD-01" "Audit" "Costa admin audit page" $(if ($audit.StatusCode -eq 200) {"PASS"} else {"FAIL"}) ""
} catch { Add-Tc "TC3-AUD-01" "Audit" "Costa admin audit page" "FAIL" $_.Exception.Message }

Add-Tc "TC3-RPT-01" "Reports" "Costa manager reports" $(if (Test-Page $costaManager "/Reports/Index") {"PASS"} else {"FAIL"}) ""

# Concurrent payments Costa
if ($cid -and $costaCashier) {
    Reset-CertAllTables $BaseUrl $costaAdmin
    $ct2 = Get-CertWaiterTable $BaseUrl $costaAdmin $costaWaiter
    $pid2 = $null
    $cats2 = Gj $costaAdmin "/Order/GetActiveCategories"
    foreach ($c in @($cats2.Data.data)) {
        $prods = Gj $costaAdmin "/Order/GetProductsByCategory/$($c.id)"
        if ($prods.Data.data) { $pid2 = $prods.Data.data[0].id; break }
    }
    if ($ct2 -and $pid2) {
        $s2 = Gj $costaWaiter "/Order/SendToKitchen" "POST" @{
            TableId = $ct2.id; OrderType = "DineIn"
            Items = @(@{ ProductId = $pid2; Quantity = 1; Status = "Pending" })
        }
        $oid2 = $s2.Data.orderId
        if ($oid2) {
            $jobs = 1..2 | ForEach-Object {
                Start-Job -ScriptBlock {
                    param($url, $oid)
                    $s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
                    $page = Invoke-WebRequest -Uri "$url/Auth/Login" -WebSession $s -UseBasicParsing
                    $m = [regex]::Match($page.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
                    Invoke-WebRequest -Uri "$url/Auth/Login" -Method POST -WebSession $s -UseBasicParsing -Body @{
                        email="cajero@costa.restbar.com"; password="123456"; __RequestVerificationToken=$m.Groups[1].Value
                    } | Out-Null
                    try {
                        $r = Invoke-WebRequest -Uri "$url/api/Payment/partial" -Method POST -WebSession $s -UseBasicParsing -ContentType "application/json" -Body (@{
                            OrderId=$oid; Amount=0.50; Method="Efectivo"; IdempotencyKey=[guid]::NewGuid().ToString()
                        } | ConvertTo-Json)
                        return @{ Ok=$true }
                    } catch { return @{ Ok=$false } }
                } -ArgumentList $BaseUrl, $oid2
            }
            $pr = $jobs | Wait-Job | Receive-Job
            $jobs | Remove-Job -Force
            $ok = @($pr | Where-Object { $_.Ok }).Count
            Add-Tc "TC3-CON-01" "Concurrency" "2 parallel payments Costa" $(if ($ok -ge 1) {"PASS"} else {"FAIL"}) "ok=$ok/2"
            Gj $costaAdmin "/Order/Cancel" "POST" @{ OrderId = $oid2; Reason = "cleanup" } | Out-Null
        }
    }
}

Reset-CertAllTables $BaseUrl $costaAdmin

$global:Results | Export-Csv "$outDir\TC3_TEST_RESULTS.csv" -NoTypeInformation -Encoding UTF8
Write-Host "`n=== 3-COMPANIES SUMMARY ===" -ForegroundColor Cyan
Write-Host "PASSED: $global:Passed  FAILED: $global:Failed  TOTAL: $($global:Passed + $global:Failed)"
if ($global:Failed -eq 0) {
    Write-Host "FUNCTIONAL CERTIFICATION 3 COMPANIES: PASS" -ForegroundColor Green
    exit 0
} else {
    Write-Host "FUNCTIONAL CERTIFICATION 3 COMPANIES: FAIL ($global:Failed)" -ForegroundColor Red
    exit 1
}
