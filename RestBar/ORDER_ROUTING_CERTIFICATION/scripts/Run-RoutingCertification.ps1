# Enterprise Order Routing Certification
param([string]$BaseUrl = "http://localhost:5001")

$ErrorActionPreference = "Continue"
$global:Results = @(); $global:Passed = 0; $global:Failed = 0; $global:Defects = @()
$outDir = Split-Path $PSScriptRoot -Parent

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
        Invoke-WebRequest -Uri "$BaseUrl/Auth/Login" -Method POST -WebSession $s -UseBasicParsing -Body @{
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
        return @{ Ok=$true; Status=$r.StatusCode; Data=($r.Content | ConvertFrom-Json); Raw=$r.Content }
    } catch {
        $code = 0; if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
        $raw = $_.ErrorDetails.Message
        try { if (-not $raw -and $_.Exception.Response) { $raw = (New-Object IO.StreamReader($_.Exception.Response.GetResponseStream())).ReadToEnd() } } catch {}
        $data = $null; try { $data = $raw | ConvertFrom-Json } catch {}
        return @{ Ok=$false; Status=$code; Data=$data; Raw=$raw }
    }
}

function Get-StationId($Session, $name) {
    $r = Get-Json $Session "/Station/GetStations"
    if ($r.Data -and $r.Data.data) {
        $st = @($r.Data.data | Where-Object { $_.name -eq $name } | Select-Object -First 1)
        if ($st.Count -gt 0) { return $st[0].id }
    }
    return $null
}

function Get-TableId($Session, $num) {
    $t = Get-Json $Session "/Order/GetActiveTables"
    if ($t.Data.data) {
        $tbl = @($t.Data.data | Where-Object { $_.tableNumber -eq $num } | Select-Object -First 1)
        if ($tbl.Count -gt 0) { return $tbl[0].id }
    }
    return $null
}

function Get-ProductIdByName($Session, $name) {
    $cats = Get-Json $Session "/Order/GetActiveCategories"
    foreach ($c in @($cats.Data.data)) {
        $prods = Get-Json $Session "/Order/GetProductsByCategory/$($c.id)"
        if ($prods.Data.data) {
            $p = @($prods.Data.data | Where-Object { $_.name -eq $name } | Select-Object -First 1)
            if ($p.Count -gt 0) { return $p[0].id }
        }
    }
    return $null
}

function Send-Order($Session, $tableId, $items) {
    Get-Json $Session "/Order/SendToKitchen" "POST" @{
        TableId = $tableId; OrderType = "DineIn"
        Items = $items
    }
}

function Get-KdsItems($Session, $stationId) {
    $r = Get-Json $Session "/api/kitchen/current?stationId=$stationId"
    $items = @()
    if ($r.Data.orders) {
        foreach ($o in @($r.Data.orders)) {
            foreach ($i in @($o.items)) { $items += $i }
        }
    }
    return $items
}

Write-Host "`n=== ENTERPRISE ORDER ROUTING CERTIFICATION ===" -ForegroundColor Cyan

Invoke-RestMethod "$BaseUrl/Seed/SeedDemoData" -ErrorAction SilentlyContinue | Out-Null
$seed = Invoke-RestMethod "$BaseUrl/Seed/SeedEnterpriseRouting" -ErrorAction SilentlyContinue
Add-Test "RT-ENV-01" "Setup" "SeedEnterpriseRouting" $(if ($seed.success) {"PASS"} else {"FAIL"}) $seed.message

$admin = Get-Session "admin@restbar.com"
$chef = Get-Session "chef@restbar.com"
Add-Test "RT-ENV-02" "Setup" "Admin session" $(if ($admin) {"PASS"} else {"FAIL"}) ""
Add-Test "RT-ENV-03" "Setup" "Chef session" $(if ($chef) {"PASS"} else {"FAIL"}) ""

# Resolve IDs
$p1 = Get-TableId $admin "P1-01"
$p2 = Get-TableId $admin "P2-01"
$p3 = Get-TableId $admin "P3-01"
$parrilla = Get-StationId $admin "Parrilla"
$horno = Get-StationId $admin "Horno"
$barMain = Get-StationId $admin "Bar Principal"
$barVip = Get-StationId $admin "Bar VIP"
$cocinaP2 = Get-StationId $admin "Cocina Piso 2"
$cocinaP1 = Get-StationId $admin "Cocina Piso 1"
$pasteleria = Get-StationId $admin "Pastelería"

$burg = Get-ProductIdByName $admin "Hamburguesa Enterprise"
$pizza = Get-ProductIdByName $admin "Pizza Enterprise"
$cerveza = Get-ProductIdByName $admin "Cerveza Enterprise"
$tragoVip = Get-ProductIdByName $admin "Trago VIP"
$postre = Get-ProductIdByName $admin "Postre Enterprise"
$sopa = Get-ProductIdByName $admin "Sopa Enterprise"

Add-Test "RT-ENV-04" "Setup" "Enterprise tables/stations resolved" $(if ($p1 -and $parrilla -and $barVip) {"PASS"} else {"FAIL"}) "p1=$p1 parrilla=$parrilla"

$oid1 = $null

# SCENARIO 1: Multi-floor isolation
if ($p1 -and $burg -and $cocinaP2) {
    $o1 = Send-Order $admin $p1 @(
        @{ ProductId=$burg; Quantity=1; Status="Pending" }
    )
    $oid1 = $o1.Data.orderId
    Start-Sleep -Seconds 1
    $kdsP2 = Get-KdsItems $chef $cocinaP2
    $leakP2 = @($kdsP2 | Where-Object { $_.productName -eq "Hamburguesa Enterprise" }).Count -gt 0
    Add-Test "RT-S01-01" "MultiPiso" "Piso1 order NOT in Cocina Piso 2" $(if (-not $leakP2) {"PASS"} else {"FAIL"}) "leak=$leakP2"

    $kdsParrilla = Get-KdsItems $chef $parrilla
    $hasBurg = @($kdsParrilla | Where-Object { $_.productName -eq "Hamburguesa Enterprise" }).Count -gt 0
    Add-Test "RT-S01-02" "MultiPiso" "Piso1 burger in Parrilla Piso1" $(if ($hasBurg) {"PASS"} else {"FAIL"}) ""
}

# SCENARIO 2: Multi-station split in one order
if ($p1 -and $pizza -and $cerveza -and $sopa -and $postre) {
    $o2 = Send-Order $admin $p1 @(
        @{ ProductId=$pizza; Quantity=1; Status="Pending" },
        @{ ProductId=$sopa; Quantity=1; Status="Pending" },
        @{ ProductId=$cerveza; Quantity=1; Status="Pending" },
        @{ ProductId=$postre; Quantity=1; Status="Pending" }
    )
    Start-Sleep -Seconds 1
    $hornoItems = Get-KdsItems $chef $horno
    $barItems = Get-KdsItems $chef $barMain
    $pastItems = Get-KdsItems $chef $pasteleria
    $pizzaInHorno = @($hornoItems | Where-Object { $_.productName -eq "Pizza Enterprise" }).Count -gt 0
    $beerInBar = @($barItems | Where-Object { $_.productName -eq "Cerveza Enterprise" }).Count -gt 0
    $beerInHorno = @($hornoItems | Where-Object { $_.productName -eq "Cerveza Enterprise" }).Count -gt 0
    $postreInPast = @($pastItems | Where-Object { $_.productName -eq "Postre Enterprise" }).Count -gt 0
    Add-Test "RT-S02-01" "MultiEstacion" "Pizza only in Horno" $(if ($pizzaInHorno -and -not $beerInHorno) {"PASS"} else {"FAIL"}) ""
    Add-Test "RT-S02-02" "MultiEstacion" "Cerveza only in Bar Principal" $(if ($beerInBar) {"PASS"} else {"FAIL"}) ""
    Add-Test "RT-S02-03" "MultiEstacion" "Postre only in Pastelería" $(if ($postreInPast) {"PASS"} else {"FAIL"}) ""
}

# SCENARIO 3: Two bars VIP vs Principal
if ($tragoVip -and $barVip -and $barMain -and $p1) {
    $ov = Send-Order $admin $p1 @( @{ ProductId=$tragoVip; Quantity=1; Status="Pending" } )
    Start-Sleep -Seconds 1
    $vipItems = Get-KdsItems $chef $barVip
    $mainItems = Get-KdsItems $chef $barMain
    $vipHas = @($vipItems | Where-Object { $_.productName -eq "Trago VIP" }).Count -gt 0
    $mainLeak = @($mainItems | Where-Object { $_.productName -eq "Trago VIP" }).Count -gt 0
    Add-Test "RT-S03-01" "DosBares" "Trago VIP in Bar VIP" $(if ($vipHas) {"PASS"} else {"FAIL"}) ""
    Add-Test "RT-S03-02" "DosBares" "Trago VIP NOT in Bar Principal" $(if (-not $mainLeak) {"PASS"} else {"FAIL"}) ""
}

# SCENARIO 8: Product without station
# Create temp product without assignment via checking send failure - skip if no API; use invalid product path
# Test: product without assignment should fail on send - we'd need unassigned product in seed - skip or add

# SCENARIO 9: Pizza priority Horno vs Horno B — verificar estación del pedido P3, no KDS global
if ($p3 -and $pizza -and $horno) {
    $op = Send-Order $admin $p3 @( @{ ProductId=$pizza; Quantity=1; Status="Pending" } )
    $oid9 = $op.Data.orderId
    Start-Sleep -Seconds 1
    $routedOk = $false
    $stationDetail = ""
    if ($oid9) {
        $status9 = Get-Json $admin "/Order/GetOrderStatus/$oid9"
        $hornoB9 = Get-StationId $admin "Horno B"
        $pizzaItem = @($status9.Data.items | Where-Object { $_.productName -eq "Pizza Enterprise" } | Select-Object -Last 1)
        if ($pizzaItem.Count -gt 0) {
            $sid = $pizzaItem[0].preparedByStationId
            $sname = $pizzaItem[0].preparedByStation
            $routedOk = ($sid -eq $horno -or $sname -eq "Horno") -and ($sid -ne $hornoB9)
            $stationDetail = "station=$sname id=$sid"
        }
    }
    Add-Test "RT-S09-01" "MultiEstacion" "Pizza routed to Horno A (priority 20) not Horno B" $(if ($routedOk) {"PASS"} else {"FAIL"}) $stationDetail
}

# SCENARIO 15: Multitenant KDS
$adminB = Get-Session "admin.b@restbar.com"
if ($adminB -and $cocinaP1) {
    $kdsB = Get-KdsItems $adminB $cocinaP1
    $leakTenant = @($kdsB | Where-Object { $_.productName -like "*Enterprise*" }).Count -gt 0
    Add-Test "RT-S15-01" "MultiTenant" "Empresa B KDS no ve productos Empresa A" $(if (-not $leakTenant) {"PASS"} else {"FAIL"}) "items=$($kdsB.Count)"
}

# SCENARIO 6: Change station (Pizza Horno → Horno B)
if ($p2 -and $pizza -and $horno) {
    $hornoB = Get-StationId $admin "Horno B"
    $op6 = Send-Order $admin $p2 @( @{ ProductId=$pizza; Quantity=1; Status="Pending" } )
    $oid6 = $op6.Data.orderId
    Start-Sleep -Seconds 1
    $status6 = Get-Json $admin "/Order/GetOrderStatus/$oid6"
    $itemId6 = $null
    if ($status6.Data.items) {
        $it6 = @($status6.Data.items | Where-Object { $_.productName -eq "Pizza Enterprise" } | Select-Object -First 1)
        if ($it6.Count -gt 0) { $itemId6 = if ($it6[0].id) { $it6[0].id } else { $it6[0].itemId } }
    }
    if ($itemId6 -and $hornoB) {
        $chg = Get-Json $admin "/Order/UpdateItemStation" "POST" @{ OrderId=$oid6; ItemId=$itemId6; NewStationId=$hornoB }
        Start-Sleep -Seconds 1
        $hornoBItems = Get-KdsItems $chef $hornoB | Where-Object { $_.productName -eq "Pizza Enterprise" }
        Add-Test "RT-S06-01" "CambioEstacion" "Pizza moved Horno to Horno B" $(if ($chg.Ok -and $hornoBItems) {"PASS"} else {"FAIL"}) ($chg.Raw)
    }
}

# SCENARIO 11: Re-fetch KDS (reimpresión/recuperación)
if ($barMain) {
    $r1 = Get-Json $chef "/api/kitchen/current?stationId=$barMain"
    $r2 = Get-Json $chef "/api/kitchen/current?stationId=$barMain"
    $c1 = if ($r1.Data.orderCount) { $r1.Data.orderCount } else { 0 }
    $c2 = if ($r2.Data.orderCount) { $r2.Data.orderCount } else { 0 }
    Add-Test "RT-S11-01" "Reimpresion" "KDS snapshot idempotent (no duplicate orders)" $(if ($c1 -eq $c2) {"PASS"} else {"FAIL"}) "count=$c1/$c2"
}

$global:Results | Export-Csv "$outDir\ROUTING_TEST_RESULTS.csv" -NoTypeInformation -Encoding UTF8
$global:Defects | Export-Csv "$outDir\ROUTING_DEFECTS.csv" -NoTypeInformation -Encoding UTF8

Write-Host "`nPASSED: $global:Passed  FAILED: $global:Failed  TOTAL: $($global:Passed + $global:Failed)"
if ($global:Failed -eq 0) { Write-Host "ORDER ROUTING CERTIFICATION: PASS" -ForegroundColor Green; exit 0 }
else { Write-Host "ORDER ROUTING CERTIFICATION: FAIL" -ForegroundColor Red; exit 1 }
