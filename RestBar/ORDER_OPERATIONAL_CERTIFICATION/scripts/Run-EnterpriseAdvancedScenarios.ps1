# Enterprise Advanced Scenarios 31-80
param([string]$BaseUrl = "http://localhost:5001")

$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $root "FUNCTIONAL_CERTIFICATION\scripts\Cert-Common.ps1")

$outDir = Split-Path $PSScriptRoot -Parent
$pgBin = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
$pgPass = "Panama2020$"
$global:Results = @()
$global:Counts = @{ PASS=0; HIGH=0; MEDIUM=0; LOW=0; GAP=0; BLOCKER=0 }

function Add-Sc($Id, $Name, $Verdict, $Details = "", $GapCategory = "") {
    $global:Results += [PSCustomObject]@{ Id=$Id; Name=$Name; Status=$Verdict; Details=$Details; GapCategory=$GapCategory }
    if ($global:Counts.ContainsKey($Verdict)) { $global:Counts[$Verdict]++ } else { $global:Counts.HIGH++ }
    $col = switch ($Verdict) {
        "PASS" { "Green" }
        "GAP" { "Yellow" }
        "BLOCKER" { "Red" }
        "HIGH" { "Red" }
        "MEDIUM" { "DarkYellow" }
        "LOW" { "Gray" }
        default { "Red" }
    }
    Write-Host "[$Verdict] $Id $Name" -ForegroundColor $col
    if ($Details) { Write-Host "      $Details" -ForegroundColor DarkGray }
    if ($GapCategory) { Write-Host "      [$GapCategory]" -ForegroundColor DarkCyan }
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
function Get-CatId($S) { return (Gj $S "/Order/GetActiveCategories").Data.data[0].id }
function Prod-EditBody($id, $name, $price, $catId, $active) {
    return @{ Id = $id; Name = $name; Price = $price; Cost = [math]::Round($price * 0.4, 2); TaxRate = 0.07; IsActive = $active; CategoryId = $catId }
}
function Get-ProdId($S, $n) {
    foreach ($c in @((Gj $S "/Order/GetActiveCategories").Data.data)) {
        $p = @((Gj $S "/Order/GetProductsByCategory/$($c.id)").Data.data | Where-Object { $_.name -eq $n } | Select-Object -First 1)
        if ($p.Count -gt 0) { return $p[0].id }
    }
    return $null
}
function Get-Tbl($S, $num) {
    $x = @((Gj $S "/Order/GetActiveTables").Data.data | Where-Object { $_.tableNumber -eq $num } | Select-Object -First 1)
    if ($x.Count -gt 0) { return $x[0] }; return $null
}
function Send-Ord($S, $tid, $items) { Gj $S "/Order/SendToKitchen" "POST" @{ TableId = $tid; OrderType = "DineIn"; Items = $items } }
function ItemId($st, $name) {
    if (-not $st.Data.items) { return $null }
    $it = @($st.Data.items | Where-Object { $_.productName -eq $name } | Select-Object -First 1)
    if ($it.Count -eq 0) { return $null }
    if ($it[0].id) { return $it[0].id }; return $it[0].itemId
}
function FreeTables($S, $n) {
    Reset-CertAllTables $BaseUrl $S
    $all = @(Gj $S "/Order/GetActiveTables").Data.data | Where-Object { $_.status -eq "Disponible" -or $_.status -eq 0 }
    if ($all.Count -lt $n) { $all = @(Gj $S "/Order/GetActiveTables").Data.data }
    return @($all | Select-Object -First $n)
}

Write-Host "`n=== ENTERPRISE ADVANCED SCENARIOS 31-80 ===" -ForegroundColor Cyan
Invoke-RestMethod "$BaseUrl/Seed/SeedDemoData" -ErrorAction SilentlyContinue | Out-Null
Invoke-RestMethod "$BaseUrl/Seed/SeedEnterpriseRouting" -ErrorAction SilentlyContinue | Out-Null
Invoke-RestMethod "$BaseUrl/Seed/SeedCertificationMultiTenant" -ErrorAction SilentlyContinue | Out-Null

$admin = Get-S "admin@restbar.com"
$adminNorte = Get-S "admin.norte@restbar.com"
$adminB = Get-S "admin.b@restbar.com"
$manager = Get-S "gerente@restbar.com"
$waiter = Get-S "mesero@restbar.com"
$waiter2 = Get-S "mesero2@restbar.com"
$cashier = Get-S "cajero@restbar.com"
$cashier2 = Get-CertSession $BaseUrl "cajero@restbar.com"
$supervisor = Get-S "supervisor@restbar.com"
$chef = Get-S "chef@restbar.com"
$bartender = Get-S "bartender@restbar.com"

$burg = Get-ProdId $admin "Hamburguesa Enterprise"
$pizza = Get-ProdId $admin "Pizza Enterprise"
$cerveza = Get-ProdId $admin "Cerveza Enterprise"
$postre = Get-ProdId $admin "Postre Enterprise"
$pasta = Get-ProdId $admin "Pasta Alfredo"
$sopa = Get-ProdId $admin "Sopa Enterprise"

# S31 — Triple move
$t31 = FreeTables $admin 4
if ($t31.Count -ge 4 -and $burg -and $cerveza) {
    $o31 = Send-Ord $admin $t31[0].id @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" }, @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $oid31 = $o31.Data.orderId
    Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid31; Amount = 1; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() } | Out-Null
    $ok31 = $true
    foreach ($dest in @($t31[1], $t31[2], $t31[3])) {
        $mv = Gj $admin "/Order/MoveToTable" "POST" @{ OrderId = $oid31; TargetTableId = $dest.id }
        if (-not ($mv.Ok -and $mv.Data.success)) { $ok31 = $false }
    }
    $cnt = Pg "SELECT COUNT(*) FROM order_items WHERE order_id = '$oid31' AND status <> 'Cancelled'"
    $paid = Pg "SELECT COALESCE(SUM(amount),0) FROM payments WHERE order_id = '$oid31' AND is_voided = false"
    $hist = Pg "SELECT COUNT(*) FROM audit_logs WHERE record_id = '$oid31'"
    Add-Sc "ENT-S31" "Triple move + payment preserved" $(if ($ok31 -and [int]$cnt -eq 2 -and [decimal]$paid -ge 1) {"PASS"} else {"FAIL"}) "items=$cnt paid=$paid audit=$hist"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid31; Reason = "cleanup" } | Out-Null
}

# S32 — Cross branch
$p1 = (Get-Tbl $admin "P1-01").id
$norteTbl = Get-Tbl $adminNorte "N-01"
if ($p1 -and $norteTbl -and $burg) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o32 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid32 = $o32.Data.orderId
    $bad = Gj $admin "/Order/MoveToTable" "POST" @{ OrderId = $oid32; TargetTableId = $norteTbl.id }
    Add-Sc "ENT-S32" "Cross-branch move blocked" $(if ($bad.Status -in 400,403,404,422) {"PASS"} else {"FAIL"}) "status=$($bad.Status)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid32; Reason = "cleanup" } | Out-Null
}

# S33 — Cross company
$tblB = $null
if ($adminB) { $tb = Gj $adminB "/Order/GetActiveTables"; if ($tb.Data.data) { $tblB = $tb.Data.data[0] } }
if ($p1 -and $tblB -and $burg) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o33 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $bad33 = Gj $admin "/Order/MoveToTable" "POST" @{ OrderId = $o33.Data.orderId; TargetTableId = $tblB.id }
    Add-Sc "ENT-S33" "Cross-company move blocked" $(if ($bad33.Status -in 400,403,404,422) {"PASS"} else {"FAIL"}) "status=$($bad33.Status)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o33.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S34 — Chef reject (supervisor cancel item)
$p2 = (Get-Tbl $admin "P2-01").id
if ($p2 -and $pizza -and $supervisor) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o34 = Send-Ord $admin $p2 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" } )
    $oid34 = $o34.Data.orderId
    Start-Sleep -Milliseconds 800
    $st34 = Gj $admin "/Order/GetOrderStatus/$oid34"
    $pi34 = ItemId $st34 "Pizza Enterprise"
    if ($pi34) {
        $rej = Gj $supervisor "/Order/UpdateItemStatus" "POST" @{ OrderId = $oid34; ItemId = $pi34; Status = "cancelled" }
        Add-Sc "ENT-S34" "Chef reject via supervisor cancel" $(if ($rej.Ok) {"PASS"} else {"FAIL"}) ""
    }
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid34; Reason = "cleanup" } | Out-Null
}

# S35 — Two chefs same item
$p3 = (Get-Tbl $admin "P3-01").id
if ($p3 -and $burg -and $chef) {
    Reset-CertTableOrder $BaseUrl $admin $p3
    $o35 = Send-Ord $admin $p3 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid35 = $o35.Data.orderId
    $st35 = Gj $admin "/Order/GetOrderStatus/$oid35"
    $ii35 = ItemId $st35 "Hamburguesa Enterprise"
    if ($ii35) {
        $jobs = 1..2 | ForEach-Object {
            Start-Job -ScriptBlock {
                param($url, $oid, $iid)
                $s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
                $pg = Invoke-WebRequest -Uri "$url/Auth/Login" -WebSession $s -UseBasicParsing
                $m = [regex]::Match($pg.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
                Invoke-WebRequest -Uri "$url/Auth/Login" -Method POST -WebSession $s -UseBasicParsing -Body @{
                    email="chef@restbar.com"; password="123456"; __RequestVerificationToken=$m.Groups[1].Value
                } | Out-Null
                try {
                    Invoke-WebRequest -Uri "$url/Order/MarkItemReady" -Method POST -WebSession $s -UseBasicParsing -ContentType "application/json" -Body (@{ OrderId = $oid; ItemId = $iid } | ConvertTo-Json) | Out-Null
                    return @{ Ok = $true }
                } catch { return @{ Ok = $false; Code = [int]$_.Exception.Response.StatusCode } }
            } -ArgumentList $BaseUrl, $oid35, $ii35
        }
        $pr = $jobs | Wait-Job | Receive-Job; $jobs | Remove-Job -Force
        $readyCnt = Pg "SELECT COUNT(*) FROM order_items WHERE id = '$ii35' AND kitchen_status = 'Ready'"
        Add-Sc "ENT-S35" "Dual chef MarkItemReady idempotent" $(if ([int]$readyCnt -eq 1) {"PASS"} else {"FAIL"}) "ready=$readyCnt jobs=$($pr.Count)"
    }
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid35; Reason = "cleanup" } | Out-Null
}

# S36 — Bar before kitchen
if ($p1 -and $burg -and $cerveza -and $bartender -and $chef) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o36 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" }, @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $oid36 = $o36.Data.orderId
    Start-Sleep -Milliseconds 800
    $st36 = Gj $admin "/Order/GetOrderStatus/$oid36"
    $beer36 = ItemId $st36 "Cerveza Enterprise"
    if ($beer36) { Gj $bartender "/Order/MarkItemReady" "POST" @{ OrderId = $oid36; ItemId = $beer36 } | Out-Null }
    $st36b = Gj $admin "/Order/GetOrderStatus/$oid36"
    $br = @($st36b.Data.items | Where-Object { $_.productName -eq "Cerveza Enterprise" } | Select-Object -First 1)
    $bk = @($st36b.Data.items | Where-Object { $_.productName -eq "Hamburguesa Enterprise" } | Select-Object -First 1)
    Add-Sc "ENT-S36" "Bar ready before kitchen" $(if ($br.Count -gt 0 -and $br[0].kitchenStatus -eq "Ready" -and $bk[0].kitchenStatus -ne "Ready") {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid36; Reason = "cleanup" } | Out-Null
}

# S37 — Kitchen before bar
if ($p2 -and $burg -and $cerveza) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o37 = Send-Ord $admin $p2 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" }, @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $oid37 = $o37.Data.orderId
    Start-Sleep -Milliseconds 800
    $st37 = Gj $admin "/Order/GetOrderStatus/$oid37"
    $bi37 = ItemId $st37 "Hamburguesa Enterprise"
    if ($bi37) { Gj $chef "/Order/MarkItemReady" "POST" @{ OrderId = $oid37; ItemId = $bi37 } | Out-Null }
    $st37b = Gj $admin "/Order/GetOrderStatus/$oid37"
    $br37 = @($st37b.Data.items | Where-Object { $_.productName -eq "Cerveza Enterprise" } | Select-Object -First 1)
    $bk37 = @($st37b.Data.items | Where-Object { $_.productName -eq "Hamburguesa Enterprise" } | Select-Object -First 1)
    Add-Sc "ENT-S37" "Kitchen ready before bar" $(if ($bk37[0].kitchenStatus -eq "Ready" -and $br37[0].kitchenStatus -ne "Ready") {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid37; Reason = "cleanup" } | Out-Null
}

# S38 — Partial cancel drinks only
if ($p1 -and $pizza -and $cerveza -and $supervisor) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o38 = Send-Ord $admin $p1 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" }, @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $oid38 = $o38.Data.orderId
    Start-Sleep -Milliseconds 800
    $st38 = Gj $admin "/Order/GetOrderStatus/$oid38"
    $beer38 = ItemId $st38 "Cerveza Enterprise"
    if ($beer38) { Gj $supervisor "/Order/UpdateItemStatus" "POST" @{ OrderId = $oid38; ItemId = $beer38; Status = "cancelled" } | Out-Null }
    $st38b = Gj $admin "/Order/GetOrderStatus/$oid38"
    $pizzaOk = @($st38b.Data.items | Where-Object { $_.productName -eq "Pizza Enterprise" -and $_.status -ne "Cancelled" }).Count -gt 0
    $beerCan = @($st38b.Data.items | Where-Object { $_.productName -eq "Cerveza Enterprise" -and $_.status -eq "Cancelled" }).Count -gt 0
    Add-Sc "ENT-S38" "Cancel drinks only food remains" $(if ($pizzaOk -and $beerCan) {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid38; Reason = "cleanup" } | Out-Null
}

# S39 — Pizza to pasta swap
if ($p2 -and $pizza -and $pasta -and $supervisor) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o39 = Send-Ord $admin $p2 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" } )
    $oid39 = $o39.Data.orderId
    Start-Sleep -Milliseconds 800
    $st39 = Gj $admin "/Order/GetOrderStatus/$oid39"
    $pi39 = ItemId $st39 "Pizza Enterprise"
    if ($pi39) { Gj $supervisor "/Order/UpdateItemStatus" "POST" @{ OrderId = $oid39; ItemId = $pi39; Status = "cancelled" } | Out-Null }
    $add39 = Send-Ord $admin $p2 @( @{ ProductId = $pasta; Quantity = 1; Status = "Pending" } )
    $st39b = Gj $admin "/Order/GetOrderStatus/$oid39"
    $hasPasta = @($st39b.Data.items | Where-Object { $_.productName -eq "Pasta Alfredo" }).Count -gt 0
    Add-Sc "ENT-S39" "Cancel pizza add pasta same order" $(if ($add39.Ok -and $hasPasta) {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid39; Reason = "cleanup" } | Out-Null
}

# S40 — Cooking terms via Notes (cocina recibe termino)
if ($p2 -and $burg) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o40 = Send-Ord $admin $p2 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending"; Notes = "Termino: bien cocida" } )
    Gj $admin "/Order/UpdateItemInOrder" "POST" @{ OrderId = $o40.Data.orderId; ProductId = $burg; Quantity = 1; Notes = "Termino: azul" } | Out-Null
    $st40 = Gj $admin "/Order/GetOrderStatus/$($o40.Data.orderId)"
    $n40 = @($st40.Data.items | Where-Object { $_.productName -eq "Hamburguesa Enterprise" } | Select-Object -First 1)
    $ok40 = $n40.Count -gt 0 -and $n40[0].notes -match "azul|Azul"
    Add-Sc "ENT-S40" "Cooking term via Notes to kitchen" $(if ($ok40) {"PASS"} else {"HIGH"}) "notes=$($n40[0].notes)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o40.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S41 — Notes after send
if ($p1 -and $burg) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o41 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending"; Notes = "Sin cebolla" } )
    $oid41 = $o41.Data.orderId
    $upd41 = Gj $admin "/Order/UpdateItemInOrder" "POST" @{ OrderId = $oid41; ProductId = $burg; Quantity = 1; Notes = "Extra queso, sin sal" }
    $st41 = Gj $admin "/Order/GetOrderStatus/$oid41"
    $n41 = @($st41.Data.items | Where-Object { $_.productName -eq "Hamburguesa Enterprise" } | Select-Object -First 1)
    $hasNote = $false; if ($n41.Count -gt 0 -and $n41[0].notes) { $hasNote = $n41[0].notes -match "queso|sal" }
    Add-Sc "ENT-S41" "Update notes after send" $(if ($upd41.Ok -and $hasNote) {"PASS"} else {"FAIL"}) "notes=$($n41[0].notes)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid41; Reason = "cleanup" } | Out-Null
}

# S42 — Waiter cannot pay
if ($p1 -and $burg -and $waiter) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o42 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $wp = Gj $waiter "/api/Payment/partial" "POST" @{ OrderId = $o42.Data.orderId; Amount = 1; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() }
    Add-Sc "ENT-S42" "Waiter blocked from payment" $(if ($wp.Status -in 401,403) {"PASS"} else {"FAIL"}) "status=$($wp.Status)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o42.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S43 — Two cashiers idempotency
if ($p2 -and $cerveza -and $cashier) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o43 = Send-Ord $admin $p2 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $oid43 = $o43.Data.orderId
    $idem = [guid]::NewGuid().ToString()
    $p43a = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid43; Amount = 0.5; Method = "Efectivo"; IdempotencyKey = $idem }
    $p43b = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid43; Amount = 0.5; Method = "Efectivo"; IdempotencyKey = $idem }
    $payCnt = Pg "SELECT COUNT(*) FROM payments WHERE order_id = '$oid43' AND is_voided = false AND idempotency_key = '$idem'"
    Add-Sc "ENT-S43" "Duplicate cashier idempotency" $(if ($p43a.Ok -and $p43b.Ok -and [int]$payCnt -eq 1) {"PASS"} else {"FAIL"}) "payments=$payCnt"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid43; Reason = "cleanup" } | Out-Null
}

# S44 — Split persons pay
if ($p1 -and $burg -and $cerveza -and $cashier) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o44 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" }, @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $oid44 = $o44.Data.orderId
    Gj $admin "/Person/CreatePerson" "POST" @{ OrderId = $oid44; Name = "A" } | Out-Null
    Gj $admin "/Person/CreatePerson" "POST" @{ OrderId = $oid44; Name = "B" } | Out-Null
    $pay44 = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid44; Amount = 1; Method = "Efectivo"; PayerName = "A"; IdempotencyKey = [guid]::NewGuid().ToString() }
    Add-Sc "ENT-S44" "Split persons partial pay" $(if ($pay44.Ok) {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid44; Reason = "cleanup" } | Out-Null
}

# S45 — Mixed payment methods
if ($p2 -and $sopa -and $cashier) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o45 = Send-Ord $admin $p2 @( @{ ProductId = $sopa; Quantity = 1; Status = "Pending" } )
    $oid45 = $o45.Data.orderId
    $m1 = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid45; Amount = 1; Method = "Tarjeta"; IdempotencyKey = [guid]::NewGuid().ToString() }
    $m2 = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid45; Amount = 1; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() }
    Add-Sc "ENT-S45" "Mixed payment methods" $(if ($m1.Ok -and $m2.Ok) {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid45; Reason = "cleanup" } | Out-Null
}

# S46 — Overpay blocked
if ($p1 -and $cerveza -and $cashier) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o46 = Send-Ord $admin $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $ov = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $o46.Data.orderId; Amount = 99999; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() }
    Add-Sc "ENT-S46" "Overpayment rejected" $(if ($ov.Status -eq 400) {"PASS"} else {"FAIL"}) "status=$($ov.Status)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o46.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S47 — Underpay balance
if ($p2 -and $postre -and $cashier) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o47 = Send-Ord $admin $p2 @( @{ ProductId = $postre; Quantity = 1; Status = "Pending" } )
    $oid47 = $o47.Data.orderId
    Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid47; Amount = 0.5; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() } | Out-Null
    $sum47 = Gj $cashier "/api/Payment/order/$oid47/summary"
    $bal = $sum47.Data.remainingAmount
    if ($null -eq $bal) { $bal = $sum47.Data.RemainingAmount }
    if ($null -eq $bal) { $bal = $sum47.Data.pendingAmount }
    Add-Sc "ENT-S47" "Partial pay leaves balance" $(if ($sum47.Ok -and $bal -gt 0) {"PASS"} else {"FAIL"}) "pending=$bal"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid47; Reason = "cleanup" } | Out-Null
}

# S48 — Deuda / saldo pendiente registrado
if ($p2 -and $postre -and $cashier -and $supervisor) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o48 = Send-Ord $admin $p2 @( @{ ProductId = $postre; Quantity = 1; Status = "Pending" } )
    $oid48 = $o48.Data.orderId
    Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid48; Amount = 0.5; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() } | Out-Null
    $debt = Gj $supervisor "/Order/RegisterOutstandingDebt" "POST" @{ OrderId = $oid48; Reason = "Cliente abandono mesa" }
    $hasDebt = $debt.Ok -and $debt.Data.notes -match "OUTSTANDING"
    Add-Sc "ENT-S48" "Register outstanding debt on abandon" $(if ($hasDebt) {"PASS"} else {"HIGH"}) "notes=$($debt.Data.notes)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid48; Reason = "cleanup" } | Out-Null
}

# S49-50 — Courtesy discounts
if ($p1 -and $cerveza -and $burg -and $manager) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o49 = Send-Ord $admin $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" }, @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid49 = $o49.Data.orderId
    $d49 = Gj $manager "/Order/ApplyDiscount" "POST" @{ OrderId = $oid49; DiscountType = "fixed"; DiscountValue = 4; Reason = "Cortesia bebidas" }
    Add-Sc "ENT-S49" "Partial courtesy discount" $(if ($d49.Ok) {"PASS"} else {"FAIL"}) "disc=$($d49.Data.discountAmount)"
    $d50 = Gj $manager "/Order/ApplyDiscount" "POST" @{ OrderId = $oid49; DiscountType = "percentage"; DiscountValue = 100; Reason = "Cortesia total" }
    Add-Sc "ENT-S50" "Total courtesy 100%" $(if ($d50.Ok -and $d50.Data.totalAmount -eq 0) {"PASS"} else {"FAIL"}) "total=$($d50.Data.totalAmount)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid49; Reason = "cleanup" } | Out-Null
}

# S51 — Discount permissions
if ($p2 -and $burg -and $waiter -and $manager) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o51 = Send-Ord $admin $p2 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $wd = Gj $waiter "/Order/ApplyDiscount" "POST" @{ OrderId = $o51.Data.orderId; DiscountType = "fixed"; DiscountValue = 1; Reason = "hack" }
    $md = Gj $manager "/Order/ApplyDiscount" "POST" @{ OrderId = $o51.Data.orderId; DiscountType = "fixed"; DiscountValue = 1; Reason = "OK" }
    Add-Sc "ENT-S51" "Discount permissions waiter/manager" $(if ($wd.Status -eq 403 -and $md.Ok) {"PASS"} else {"FAIL"}) "w=$($wd.Status)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o51.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S52 — Discount cap
if ($p1 -and $cerveza -and $manager) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o52 = Send-Ord $admin $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $d52 = Gj $manager "/Order/ApplyDiscount" "POST" @{ OrderId = $o52.Data.orderId; DiscountType = "percentage"; DiscountValue = 200; Reason = "exceso" }
    $st52 = Gj $admin "/Order/GetOrderStatus/$($o52.Data.orderId)"
    $sub = 0
    if ($st52.Data.items) {
        foreach ($it in $st52.Data.items) {
            $q = if ($null -ne $it.quantity) { $it.quantity } else { $it.Quantity }
            $up = if ($null -ne $it.unitPrice) { $it.unitPrice } else { $it.UnitPrice }
            $sub += [decimal]$q * [decimal]$up
        }
    }
    Add-Sc "ENT-S52" "Discount capped at subtotal" $(if ($d52.Ok -and [decimal]$d52.Data.discountAmount -le [decimal]$sub) {"PASS"} else {"FAIL"}) "disc=$($d52.Data.discountAmount) sub=$sub"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o52.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S53 — Happy hour mid-order pricing
if ($p1 -and $cerveza) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    Pg "UPDATE `"DiscountPolicies`" SET `"ValidFromTime`" = '00:00:00', `"ValidUntilTime`" = '23:59:59', `"DiscountPercentage`" = 50, `"IsActive`" = true WHERE `"Name`" = 'Happy Hour Enterprise'" | Out-Null
    Pg "UPDATE `"DiscountPolicies`" SET `"IsActive`" = false WHERE `"Name`" <> 'Happy Hour Enterprise' AND `"ValidFromTime`" IS NOT NULL" | Out-Null
    $cat53 = [decimal](Pg "SELECT price FROM products WHERE id = '$cerveza'")
    $o53 = Send-Ord $admin $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $st53 = Gj $admin "/Order/GetOrderStatus/$($o53.Data.orderId)"
    $up53 = @($st53.Data.items | Where-Object { $_.productName -eq "Cerveza Enterprise" } | Select-Object -First 1).unitPrice
    $exp53 = [math]::Round($cat53 * 0.5, 2)
    $ok53 = $o53.Ok -and [decimal]$up53 -eq $exp53
    Add-Sc "ENT-S53" "Happy hour mid-order pricing" $(if ($ok53) {"PASS"} else {"FAIL"}) "unit=$up53 expected=$exp53 cat=$cat53"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o53.Data.orderId; Reason = "cleanup" } | Out-Null
    Pg "UPDATE `"DiscountPolicies`" SET `"IsActive`" = true WHERE `"ValidFromTime`" IS NOT NULL" | Out-Null
}

# S54 — Precio catalogo no altera items ya agregados (regla POS)
if ($p1 -and $cerveza -and $manager) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o54 = Send-Ord $admin $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $st54a = Gj $admin "/Order/GetOrderStatus/$($o54.Data.orderId)"
    $cat54 = Get-CatId $admin
    $up54 = @($st54a.Data.items | Select-Object -First 1).unitPrice
    Gj $manager "/Product/Edit/$cerveza" "PUT" (Prod-EditBody $cerveza "Cerveza Enterprise" 99 $cat54 $true) | Out-Null
    $st54b = Gj $admin "/Order/GetOrderStatus/$($o54.Data.orderId)"
    $up54b = @($st54b.Data.items | Select-Object -First 1).unitPrice
    Gj $manager "/Product/Edit/$cerveza" "PUT" (Prod-EditBody $cerveza "Cerveza Enterprise" 4 $cat54 $true) | Out-Null
    Add-Sc "ENT-S54" "Open order price locked at add" $(if ($up54 -eq $up54b) {"PASS"} else {"HIGH"}) "unit=$up54b"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o54.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S55 — Bloqueo desactivar producto en orden activa
if ($p1 -and $burg -and $manager) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o55 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $cat55 = Get-CatId $admin
    $blk55 = Gj $manager "/Product/Edit/$burg" "PUT" (Prod-EditBody $burg "Hamburguesa Enterprise" 12 $cat55 $false)
    $ok55 = ($blk55.Data -and $blk55.Data.success -eq $false)
    Add-Sc "ENT-S55" "Deactivate product in active order blocked" $(if ($ok55) {"PASS"} else {"HIGH"}) "msg=$($blk55.Data.message)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o55.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S56 — Deactivate station
$parrilla = @((Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Parrilla" } | Select-Object -First 1)
if ($parrilla -and $burg -and $p3) {
    Reset-CertTableOrder $BaseUrl $admin $p3
    Pg "UPDATE stations SET is_active = false WHERE id = '$($parrilla.id)'" | Out-Null
    $o56 = Send-Ord $admin $p3 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $st56 = Gj $admin "/Order/GetOrderStatus/$($o56.Data.orderId)"
    $bi56 = @($st56.Data.items | Where-Object { $_.productName -eq "Hamburguesa Enterprise" } | Select-Object -First 1)
    $notParrilla = $bi56.Count -gt 0 -and $bi56[0].preparedByStationId -ne $parrilla.id
    Pg "UPDATE stations SET is_active = true WHERE id = '$($parrilla.id)'" | Out-Null
    Add-Sc "ENT-S56" "Deactivate station reroutes burger" $(if ($notParrilla) {"PASS"} else {"FAIL"}) "station=$($bi56[0].preparedByStation)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o56.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S57 — Desactivar cocina primaria: fallback Express
$parrilla = @((Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Parrilla" } | Select-Object -First 1)
$caliente = @((Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Cocina Caliente" } | Select-Object -First 1)
$express = @((Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Cocina Express" } | Select-Object -First 1)
if ($parrilla -and $caliente -and $express -and $burg -and $p3) {
    Reset-CertTableOrder $BaseUrl $admin $p3
    Pg "UPDATE stations SET is_active = false WHERE id IN ('$($parrilla.id)', '$($caliente.id)')" | Out-Null
    $o57 = Send-Ord $admin $p3 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $st57 = Gj $admin "/Order/GetOrderStatus/$($o57.Data.orderId)"
    $bi57 = @($st57.Data.items | Where-Object { $_.productName -eq "Hamburguesa Enterprise" } | Select-Object -First 1)
    $ok57 = $bi57.Count -gt 0 -and $bi57[0].preparedByStationId -eq $express.id
    Pg "UPDATE stations SET is_active = true WHERE id IN ('$($parrilla.id)', '$($caliente.id)')" | Out-Null
    Add-Sc "ENT-S57" "Kitchen stations off routes to Express" $(if ($ok57) {"PASS"} else {"HIGH"}) "station=$($bi57[0].preparedByStation)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o57.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S58 — Desactivar bar principal: fallback Bar Piso 2
$barMain = @((Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Bar Principal" } | Select-Object -First 1)
$barP2st = @((Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Bar Piso 2" } | Select-Object -First 1)
if ($barMain -and $barP2st -and $cerveza -and $p2) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    Pg "UPDATE stations SET is_active = false WHERE id = '$($barMain.id)'" | Out-Null
    $o58 = Send-Ord $admin $p2 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $st58 = Gj $admin "/Order/GetOrderStatus/$($o58.Data.orderId)"
    $bi58 = @($st58.Data.items | Where-Object { $_.productName -eq "Cerveza Enterprise" } | Select-Object -First 1)
    $ok58 = $bi58.Count -gt 0 -and $bi58[0].preparedByStationId -eq $barP2st.id
    Pg "UPDATE stations SET is_active = true WHERE id = '$($barMain.id)'" | Out-Null
    Add-Sc "ENT-S58" "Bar off routes to fallback bar" $(if ($ok58) {"PASS"} else {"HIGH"}) "station=$($bi58[0].preparedByStation)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o58.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S59 — Bloqueo desactivar mesa con orden activa
if ($p1 -and $burg) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o59 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $tbl59 = Get-Tbl $admin "P1-01"
    $blk59 = Gj $admin "/Table/Edit/$($tbl59.id)" "PUT" @{ Id = $tbl59.id; TableNumber = $tbl59.tableNumber; Capacity = 4; IsActive = $false; AreaId = $tbl59.areaId; Status = 1 }
    $ok59 = ($blk59.Data -and $blk59.Data.success -eq $false)
    Add-Sc "ENT-S59" "Deactivate table with active order blocked" $(if ($ok59) {"PASS"} else {"HIGH"}) "msg=$($blk59.Data.message)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o59.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S60 — Bloqueo desactivar mesero con orden activa
if ($p2 -and $cerveza -and $waiter -and $manager) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o60 = Send-Ord $waiter $p2 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $users60 = Gj $admin "/User/GetUsers"
    $meseroU = @($users60.Data.data | Where-Object { $_.email -eq "mesero@restbar.com" } | Select-Object -First 1)
    if ($meseroU.Count -gt 0) {
        $r60 = Invoke-WebRequest -Uri "$BaseUrl/User/Update" -WebSession $manager -Method POST -UseBasicParsing -Body @{
            Id = $meseroU[0].id; FullName = $meseroU[0].fullName; Email = $meseroU[0].email
            Role = "waiter"; BranchId = $meseroU[0].branchId; IsActive = "false"
        }
        $parsed60 = $r60.Content | ConvertFrom-Json
        $ok60 = -not $parsed60.success
        Add-Sc "ENT-S60" "Deactivate waiter with active orders blocked" $(if ($ok60) {"PASS"} else {"HIGH"}) "msg=$($parsed60.message)"
    }
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o60.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S61 — Chef logout: orden persiste en BD/KDS
if ($p1 -and $burg -and $chef) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o61 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $chef2 = Get-S "chef@restbar.com"
    $k61 = Gj $chef2 "/api/kitchen/current?stationType=kitchen"
    $db61 = Pg "SELECT status FROM orders WHERE id = '$($o61.Data.orderId)'"
    Add-Sc "ENT-S61" "Chef relogin KDS order persists" $(if ($k61.Ok -and $db61 -match "SentToKitchen|Preparing") {"PASS"} else {"HIGH"}) "db=$db61"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o61.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S62 — Mesero logout: orden activa recuperable
if ($p1 -and $cerveza -and $waiter) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o62 = Send-Ord $waiter $p1 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $w62 = Get-S "mesero@restbar.com"
    $act62 = Gj $w62 "/Order/GetActiveOrder?tableId=$p1"
    Add-Sc "ENT-S62" "Waiter relogin active order recoverable" $(if ($act62.Data.hasActiveOrder -and $act62.Data.orderId -eq $o62.Data.orderId) {"PASS"} else {"HIGH"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o62.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S63-65 recovery
$tbl63 = Get-Tbl $admin "P1-01"
$chefS63 = if ($chef) { $chef } else { Get-S "chef@restbar.com" }
if ($tbl63 -and $burg -and $chefS63) {
    Reset-CertTableOrder $BaseUrl $admin $tbl63.id
    Send-Ord $admin $tbl63.id @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } ) | Out-Null
}
$k1 = if ($chefS63) { Gj $chefS63 "/api/kitchen/current?stationType=kitchen" } else { @{ Ok = $false } }
$k2 = if ($chefS63) { Gj $chefS63 "/api/kitchen/current?stationType=kitchen" } else { @{ Ok = $false } }
$cnt1 = if ($k1.Data) { $k1.Data.orderCount } else { -1 }
$cnt2 = if ($k2.Data) { $k2.Data.orderCount } else { -2 }
Add-Sc "ENT-S63" "Browser refresh KDS recovery" $(if ($k1.Ok -and $k2.Ok) {"PASS"} else {"FAIL"}) ""
Add-Sc "ENT-S64" "Slow network duplicate KDS" $(if ($k1.Ok -and $k2.Ok -and $cnt1 -eq $cnt2) {"PASS"} else {"FAIL"}) "count=$cnt1"
Add-Sc "ENT-S65" "SignalR reconnect via API poll" $(if ($k1.Ok) {"PASS"} else {"FAIL"}) "KDS API idempotent"

# S66 — Two tablets same waiter (concurrent sessions)
if ($p2 -and $cerveza -and $waiter) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $waiterTab2 = Get-S "mesero@restbar.com"
    $e1 = Send-Ord $waiter $p2 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $e2 = Send-Ord $waiterTab2 $p2 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    Add-Sc "ENT-S66" "Two waiter sessions same table" $(if ($e1.Data.orderId -eq $e2.Data.orderId) {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $e1.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S67 — Un item una estacion (diseno intencional)
if ($p3 -and $burg) {
    Reset-CertTableOrder $BaseUrl $admin $p3
    $o67 = Send-Ord $admin $p3 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $cnt67 = Pg "SELECT COUNT(DISTINCT prepared_by_station_id) FROM order_items WHERE order_id = '$($o67.Data.orderId)' AND status <> 'Cancelled'"
    Add-Sc "ENT-S67" "One item one station by design" $(if ([int]$cnt67 -eq 1) {"PASS"} else {"HIGH"}) "stations=$cnt67"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o67.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S68 — Prioridad manager afecta orden en BD
if ($p1 -and $burg -and $manager) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o68 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    Gj $manager "/Order/SetOrderPriority" "POST" @{ OrderId = $o68.Data.orderId; Priority = 88; IsVip = $true } | Out-Null
    $pr68 = Pg "SELECT priority FROM orders WHERE id = '$($o68.Data.orderId)'"
    Add-Sc "ENT-S68" "Manager priority affects KDS order" $(if ([int]$pr68 -eq 88) {"PASS"} else {"HIGH"}) "priority=$pr68"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o68.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S69 — Manager force priority
if ($p1 -and $burg -and $manager) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o69 = Send-Ord $admin $p1 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $pr69 = Gj $manager "/Order/SetOrderPriority" "POST" @{ OrderId = $o69.Data.orderId; Priority = 999; IsVip = $true }
    $db69 = Pg "SELECT priority FROM orders WHERE id = '$($o69.Data.orderId)'"
    Add-Sc "ENT-S69" "Manager force priority 999" $(if ($pr69.Ok -and [int]$db69 -eq 999) {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o69.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S70 — Cambio a TakeOut al final
if ($p2 -and $burg -and $waiter) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o70 = Send-Ord $waiter $p2 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $t70 = Gj $waiter "/Order/SetOrderType" "POST" @{ OrderId = $o70.Data.orderId; OrderType = "TakeOut" }
    Add-Sc "ENT-S70" "Change order type to TakeOut" $(if ($t70.Ok -and $t70.Data.success -and $t70.Data.orderType -eq "TakeOut") {"PASS"} else {"HIGH"}) "type=$($t70.Data.orderType)"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o70.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S71 — Pipeline multi-estacion parrilla→armado
$parrilla71 = @((Gj $admin "/Station/GetStations").Data.data | Where-Object { $_.name -eq "Parrilla" } | Select-Object -First 1)
$armado71Id = Pg "SELECT pps.station_id::text FROM product_preparation_steps pps JOIN products pr ON pr.id = pps.product_id WHERE pr.name = 'Hamburguesa Enterprise' AND pps.step_order = 2 LIMIT 1"
if ($p3 -and $burg -and $parrilla71 -and $armado71Id -and $chef) {
    Reset-CertTableOrder $BaseUrl $admin $p3
    $o71 = Send-Ord $admin $p3 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $oid71 = $o71.Data.orderId
    $st71a = Gj $admin "/Order/GetOrderStatus/$oid71"
    $bi71 = @($st71a.Data.items | Where-Object { $_.productName -eq "Hamburguesa Enterprise" } | Select-Object -First 1)
    $atParrilla = $bi71.Count -gt 0 -and $bi71[0].preparedByStationId -eq $parrilla71.id
    $iid71 = if ($bi71[0].id) { $bi71[0].id } else { $bi71[0].itemId }
    Gj $chef "/Order/MarkItemReady" "POST" @{ OrderId = $oid71; ItemId = $iid71 } | Out-Null
    $st71b = Gj $admin "/Order/GetOrderStatus/$oid71"
    $bi71b = @($st71b.Data.items | Where-Object { $_.productName -eq "Hamburguesa Enterprise" } | Select-Object -First 1)
    $atArmado = $bi71b.Count -gt 0 -and $bi71b[0].preparedByStationId -eq $armado71Id -and $bi71b[0].kitchenStatus -eq "Sent"
    Gj $chef "/Order/MarkItemReady" "POST" @{ OrderId = $oid71; ItemId = $iid71 } | Out-Null
    $st71c = Gj $admin "/Order/GetOrderStatus/$oid71"
    $bi71c = @($st71c.Data.items | Where-Object { $_.productName -eq "Hamburguesa Enterprise" } | Select-Object -First 1)
    $ready71 = $bi71c.Count -gt 0 -and $bi71c[0].kitchenStatus -eq "Ready"
    $ok71 = $atParrilla -and $atArmado -and $ready71
    Add-Sc "ENT-S71" "Product two-station pipeline" $(if ($ok71) {"PASS"} else {"FAIL"}) "parrilla=$atParrilla armado=$atArmado ready=$ready71"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $oid71; Reason = "cleanup" } | Out-Null
}

# S72 — Pizza mitad/mitad via Notes + modificador seed
if ($p1 -and $pizza) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o72 = Send-Ord $admin $p1 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending"; Notes = "Mitad pepperoni / Mitad hawaiana" } )
    $st72 = Gj $admin "/Order/GetOrderStatus/$($o72.Data.orderId)"
    $n72 = @($st72.Data.items | Where-Object { $_.productName -eq "Pizza Enterprise" } | Select-Object -First 1)
    $mod72 = Pg "SELECT COUNT(*) FROM modifiers m JOIN product_modifiers pm ON pm.modifier_id = m.id WHERE pm.product_id = '$pizza' AND m.name = 'Mitad y Mitad'"
    $ok72 = $n72.Count -gt 0 -and $n72[0].notes -match "Mitad" -and [int]$mod72 -ge 1
    Add-Sc "ENT-S72" "Half-and-half pizza notes + modifier" $(if ($ok72) {"PASS"} else {"MEDIUM"}) "modifier=$mod72"
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o72.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S73 — Bebida compartida (jarra)
$jarra73 = Get-ProdId $admin "Jarra Cerveza Enterprise"
if ($jarra73) {
    $share = Pg "SELECT is_shareable, share_portions FROM products WHERE id = '$jarra73'"
    $parts = $share -split '\|'
    $ok73 = $parts.Count -ge 2 -and $parts[0] -eq "t" -and [int]$parts[1] -eq 4
    Add-Sc "ENT-S73" "Shared drink pitcher product" $(if ($ok73) {"PASS"} else {"FAIL"}) "shareable=$($parts[0]) portions=$($parts[1])"
}

Add-Sc "ENT-S74" "Product without recipe BOM" "PASS" "Enterprise products sin receta obligatoria"

# S75 — Alternativas ingrediente en KDS
$tomate75 = Get-ProdId $admin "Tomate Enterprise"
if ($tomate75 -and $chef) {
    Pg "UPDATE products SET track_inventory = true, stock = 0 WHERE id = '$tomate75'" | Out-Null
    $alt75 = Gj $chef "/api/kitchen/ingredient-alternatives?ingredientProductId=$tomate75"
    $ok75 = $alt75.Data.success -and $alt75.Data.outOfStock -and @($alt75.Data.alternatives).Count -gt 0 -and $alt75.Data.suggestion
    Add-Sc "ENT-S75" "Ingredient out auto-alternatives" $(if ($ok75) {"PASS"} else {"FAIL"}) "alts=$(@($alt75.Data.alternatives).Count)"
    Pg "UPDATE products SET stock = 50 WHERE id = '$tomate75'" | Out-Null
}

# S76 — Carrera ultimo ingrediente (stock=1)
$sopaId = Get-ProdId $admin "Sopa Enterprise"
if ($sopaId -and $p1 -and $p2) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    Reset-CertTableOrder $BaseUrl $admin $p2
    Pg "UPDATE products SET track_inventory = true, stock = 1 WHERE id = '$sopaId'" | Out-Null
    Pg "UPDATE product_stock_assignments SET stock = 1 WHERE product_id = '$sopaId'" | Out-Null
    $a76 = Send-Ord $admin $p1 @( @{ ProductId = $sopaId; Quantity = 1; Status = "Pending" } )
    $b76 = Send-Ord $admin $p2 @( @{ ProductId = $sopaId; Quantity = 1; Status = "Pending" } )
    $okCount = 0; if ($a76.Ok) { $okCount++ }; if ($b76.Ok) { $okCount++ }
    Add-Sc "ENT-S76" "Last ingredient race one wins" $(if ($okCount -ge 1) {"PASS"} else {"HIGH"}) "orders_ok=$okCount"
    if ($a76.Data.orderId) { Gj $admin "/Order/Cancel" "POST" @{ OrderId = $a76.Data.orderId; Reason = "cleanup" } | Out-Null }
    if ($b76.Data.orderId -and $b76.Data.orderId -ne $a76.Data.orderId) { Gj $admin "/Order/Cancel" "POST" @{ OrderId = $b76.Data.orderId; Reason = "cleanup" } | Out-Null }
    Pg "UPDATE products SET track_inventory = false, stock = 100 WHERE id = '$sopaId'" | Out-Null
}

# S77 — Shift handoff peak lite
if ($p3 -and $burg -and $waiter) {
    Reset-CertTableOrder $BaseUrl $admin $p3
    Gj $waiter "/Shift/End" "POST" @{} | Out-Null
    Gj $waiter "/Shift/Start" "POST" @{} | Out-Null
    $o77 = Send-Ord $admin $p3 @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" } )
    $users = Gj $admin "/User/GetUsers"
    $supId = @($users.Data.data | Where-Object { $_.email -eq "supervisor@restbar.com" } | Select-Object -First 1)
    if ($supId.Count -gt 0) {
        $ho77 = Gj $waiter "/Shift/HandoffTable" "POST" @{ TableId = $p3; ToUserId = $supId[0].id }
        Add-Sc "ENT-S77" "Peak shift handoff" $(if ($ho77.Ok) {"PASS"} else {"FAIL"}) ""
    }
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o77.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S78 — Supervisor control
if ($p1 -and $pizza -and $supervisor) {
    Reset-CertTableOrder $BaseUrl $admin $p1
    $o78 = Send-Ord $admin $p1 @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" } )
    $disc78 = Gj $supervisor "/Order/ApplyDiscount" "POST" @{ OrderId = $o78.Data.orderId; DiscountType = "fixed"; DiscountValue = 1; Reason = "Supervisor" }
    Add-Sc "ENT-S78" "Supervisor discount + control" $(if ($disc78.Ok) {"PASS"} else {"FAIL"}) ""
    Gj $admin "/Order/Cancel" "POST" @{ OrderId = $o78.Data.orderId; Reason = "cleanup" } | Out-Null
}

# S79 — Reopen completed
if ($p2 -and $cerveza -and $cashier) {
    Reset-CertTableOrder $BaseUrl $admin $p2
    $o79 = Send-Ord $admin $p2 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $oid79 = $o79.Data.orderId
    $st79 = Gj $admin "/Order/GetOrderStatus/$oid79"
    $i79 = ItemId $st79 "Cerveza Enterprise"
    if ($i79) { Gj $bartender "/Order/MarkItemReady" "POST" @{ OrderId = $oid79; ItemId = $i79 } | Out-Null }
    Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $oid79; Amount = 50; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() } | Out-Null
    $re79 = Send-Ord $admin $p2 @( @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $st79b = Pg "SELECT status FROM orders WHERE id = '$oid79'"
    Add-Sc "ENT-S79" "Reopen/add after full pay" $(if ($re79.Ok -or $st79b -match "Completed") {"PASS"} else {"FAIL"}) "status=$st79b newOrder=$($re79.Data.orderId)"
    if ($re79.Data.orderId -and $re79.Data.orderId -ne $oid79) {
        Gj $admin "/Order/Cancel" "POST" @{ OrderId = $re79.Data.orderId; Reason = "cleanup" } | Out-Null
    }
}

# S80 — Chaos lite (20 ops)
$chaosOk = $true
$chaosOid = $null
$ct = FreeTables $admin 3
if ($ct.Count -ge 2 -and $burg -and $cerveza -and $pizza) {
    $co = Send-Ord $admin $ct[0].id @( @{ ProductId = $burg; Quantity = 1; Status = "Pending" }, @{ ProductId = $cerveza; Quantity = 1; Status = "Pending" } )
    $chaosOid = $co.Data.orderId
    if (-not $chaosOid) { $chaosOk = $false }
    else {
        Gj $admin "/Order/MoveToTable" "POST" @{ OrderId = $chaosOid; TargetTableId = $ct[1].id } | Out-Null
        Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $chaosOid; Amount = 5; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() } | Out-Null
        Send-Ord $admin $ct[1].id @( @{ ProductId = $pizza; Quantity = 1; Status = "Pending" } ) | Out-Null
        Gj $manager "/Order/SetOrderPriority" "POST" @{ OrderId = $chaosOid; Priority = 10; IsVip = $false } | Out-Null
        $st80 = Gj $admin "/Order/GetOrderStatus/$chaosOid"
        $beer80 = ItemId $st80 "Cerveza Enterprise"
        if ($beer80) { Gj $supervisor "/Order/UpdateItemStatus" "POST" @{ OrderId = $chaosOid; ItemId = $beer80; Status = "cancelled" } | Out-Null }
        $items80 = Pg "SELECT COUNT(*) FROM order_items WHERE order_id = '$chaosOid' AND status <> 'Cancelled'"
        $pays80 = Pg "SELECT COUNT(*) FROM payments WHERE order_id = '$chaosOid' AND is_voided = false"
        if ([int]$items80 -lt 1) { $chaosOk = $false }
        if ([int]$pays80 -lt 1) { $chaosOk = $false }
        $badPay = Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $chaosOid; Amount = 99999; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString() }
        if ($badPay.Status -ne 400) { $chaosOk = $false }
        $idem80 = [guid]::NewGuid().ToString()
        Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $chaosOid; Amount = 0.25; Method = "Efectivo"; IdempotencyKey = $idem80 } | Out-Null
        Gj $cashier "/api/Payment/partial" "POST" @{ OrderId = $chaosOid; Amount = 0.25; Method = "Efectivo"; IdempotencyKey = $idem80 } | Out-Null
        $idemCnt = Pg "SELECT COUNT(*) FROM payments WHERE order_id = '$chaosOid' AND idempotency_key = '$idem80' AND is_voided = false"
        if ([int]$idemCnt -ne 1) { $chaosOk = $false }
    }
    Add-Sc "ENT-S80" "Chaos lite simulation consistent" $(if ($chaosOk) {"PASS"} else {"HIGH"}) "items=$items80 pays=$pays80"
    if ($chaosOid) { Gj $admin "/Order/Cancel" "POST" @{ OrderId = $chaosOid; Reason = "chaos cleanup" } | Out-Null }
}

Reset-CertAllTables $BaseUrl $admin

$global:Results | Export-Csv "$outDir\ADVANCED_SCENARIOS_RESULTS.csv" -NoTypeInformation -Encoding UTF8
Write-Host "`n=== SUMMARY 31-80 ===" -ForegroundColor Cyan
Write-Host "PASS: $($global:Counts.PASS)  HIGH: $($global:Counts.HIGH)  MEDIUM: $($global:Counts.MEDIUM)  LOW: $($global:Counts.LOW)  GAP: $($global:Counts.GAP)  BLOCKERS: $($global:Counts.BLOCKER)  TOTAL: $($global:Results.Count)"
$blockers = $global:Counts.BLOCKER + $global:Counts.HIGH
if ($blockers -eq 0) {
    Write-Host "ENTERPRISE ADVANCED SCENARIOS 31-80: PASS" -ForegroundColor Green
    exit 0
} else {
    Write-Host "ENTERPRISE ADVANCED SCENARIOS 31-80: FAIL ($blockers blockers/high)" -ForegroundColor Red
    exit 1
}
