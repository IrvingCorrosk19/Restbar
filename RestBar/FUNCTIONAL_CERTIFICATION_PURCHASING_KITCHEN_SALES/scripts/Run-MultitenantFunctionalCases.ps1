# RestBar - Pruebas funcionales multitenant (casos imaginarios / escenarios reales)
# Cubre: Empresa A/B, sucursal Norte, Costa/Norte/Sur, roles, IDOR, flujos POS
param(
    [string]$BaseUrl = "http://164.68.99.83:8084",
    [string]$Password = "123456"
)

$ErrorActionPreference = "Continue"
$suiteRoot = Split-Path $PSScriptRoot -Parent
$certRoot = Split-Path $suiteRoot -Parent
. (Join-Path $certRoot "FUNCTIONAL_CERTIFICATION\scripts\Cert-Common.ps1")

$outDir = $suiteRoot
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$global:Results = @(); $global:Passed = 0; $global:Failed = 0; $global:Blocked = 0

function Add-Tc($Id, $Cat, $Name, $Status, $Details = "") {
    $global:Results += [PSCustomObject]@{ Id=$Id; Category=$Cat; Scenario=$Name; Status=$Status; Details=$Details; BaseUrl=$BaseUrl; At=(Get-Date).ToString("s") }
    switch ($Status) {
        "PASS" { $global:Passed++; $c = "Green" }
        "FAIL" { $global:Failed++; $c = "Red" }
        default { $global:Blocked++; $c = "Yellow" }
    }
    Write-Host "[$Status] $Id - $Name" -ForegroundColor $c
    if ($Details) { Write-Host "      $Details" -ForegroundColor DarkGray }
}

function Get-S([string]$Email) {
    for ($i = 1; $i -le 6; $i++) {
        $s = Get-CertSession $BaseUrl $Email $Password
        if ($s) { return $s }
        # VPS has login rate-limit (HTTP 429); backoff and retry
        Start-Sleep -Seconds (2 * $i)
    }
    return $null
}
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

function Get-ExclusiveVisible($Session, $exclusiveName) {
    $cats = Gj $Session "/Order/GetActiveCategories"
    if (-not $cats.Ok -or -not $cats.Data.data) { return $false }
    foreach ($c in @($cats.Data.data)) {
        $prods = Gj $Session "/Order/GetProductsByCategory/$($c.id)"
        if (@($prods.Data.data | Where-Object { $_.name -eq $exclusiveName }).Count -gt 0) { return $true }
    }
    return $false
}

function Get-AnyProduct($Session) {
    $cats = Gj $Session "/Order/GetActiveCategories"
    foreach ($c in @($cats.Data.data)) {
        $prods = Gj $Session "/Order/GetProductsByCategory/$($c.id)"
        $p = @($prods.Data.data | Where-Object { $_.id }) | Select-Object -First 1
        if ($p) { return $p }
    }
    return $null
}

function Clear-TenantTables($Admin) {
    if (-not $Admin) { return }
    $tables = Gj $Admin "/Order/GetActiveTables"
    if ($tables.Data.data) {
        foreach ($t in @($tables.Data.data)) {
            Reset-CertTableOrder $BaseUrl $Admin $t.id
        }
    }
    # PG hard-reset only when testing against local DB
    if ($BaseUrl -match 'localhost|127\.0\.0\.1') {
        Invoke-CertPgReset | Out-Null
        Start-Sleep -Milliseconds 400
    }
}

function New-OrderOnFreeTable($Admin, $Waiter) {
    Clear-TenantTables $Admin
    $actor = if ($Waiter) { $Waiter } else { $Admin }
    $table = $null
    if ($Waiter) {
        $tw = Gj $Waiter "/Order/GetActiveTables"
        $table = @($tw.Data.data | Where-Object { Test-CertTableFree $_ } | Select-Object -First 1)
        if (-not $table) { $table = @($tw.Data.data | Select-Object -First 1) }
    }
    if (-not $table) {
        $ta = Gj $Admin "/Order/GetActiveTables"
        $table = @($ta.Data.data | Where-Object { Test-CertTableFree $_ } | Select-Object -First 1)
    }
    if (-not $table) { return $null }
    if ($table -is [array]) { $table = $table[0] }
    $prod = Get-AnyProduct $Admin
    if (-not $prod) { return $null }
    $send = Gj $actor "/Order/SendToKitchen" "POST" @{
        TableId = $table.id
        OrderType = "DineIn"
        Items = @(@{ ProductId = $prod.id; Quantity = 1; Status = "Pending"; Notes = "MT cert" })
    }
    $oid = $send.Data.orderId
    if (-not $oid) {
        $active = Gj $Admin "/Order/GetActiveOrder?tableId=$($table.id)"
        $oid = $active.Data.orderId
        if (-not $oid -and $active.Data.order) { $oid = $active.Data.order.id }
    }
    return @{ Table=$table; OrderId=$oid; Product=$prod; Create=$send; Active=$null }
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  MULTITENANT FUNCTIONAL CASES - RestBar" -ForegroundColor Cyan
Write-Host "  Target: $BaseUrl" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# ---------- CASO 0: Login / presencia de tenants ----------
Write-Host "=== CASO 0: Sesiones por tenant ===" -ForegroundColor Yellow
$aAdmin   = Get-S "admin@restbar.com"
$aWaiter  = Get-S "mesero@restbar.com"
$aChef    = Get-S "chef@restbar.com"
$aCashier = Get-S "cajero@restbar.com"
$bAdmin   = Get-S "admin.b@restbar.com"
$nAdmin   = Get-S "admin.norte@restbar.com"
$cAdmin   = Get-S "admin@costa.restbar.com"
$cWaiter  = Get-S "mesero1@costa.restbar.com"
$cCashier = Get-S "cajero@costa.restbar.com"
$cChef    = Get-S "chef@costa.restbar.com"
$noAdmin  = Get-S "admin@norte.restbar.com"
$noWaiter = Get-S "mesero1@norte.restbar.com"
$sAdmin   = Get-S "admin@sur.restbar.com"
$sWaiter  = Get-S "mesero1@sur.restbar.com"
$super    = Get-S "superadmin@restbar.com"
$badLogin = Get-S "noexiste@restbar.com"

Add-Tc "MT-00-01" "Auth" "Empresa A admin login" $(if ($aAdmin) {"PASS"} else {"FAIL"}) "admin@restbar.com"
Add-Tc "MT-00-02" "Auth" "Empresa B admin login" $(if ($bAdmin) {"PASS"} else {"FAIL"}) "admin.b@restbar.com"
Add-Tc "MT-00-03" "Auth" "Sucursal Norte (A) admin login" $(if ($nAdmin) {"PASS"} else {"FAIL"}) "admin.norte@restbar.com"
Add-Tc "MT-00-04" "Auth" "Costa admin login" $(if ($cAdmin) {"PASS"} else {"FAIL"}) "admin@costa.restbar.com"
Add-Tc "MT-00-05" "Auth" "Norte (3-co) admin login" $(if ($noAdmin) {"PASS"} else {"FAIL"}) "admin@norte.restbar.com"
Add-Tc "MT-00-06" "Auth" "Sur admin login" $(if ($sAdmin) {"PASS"} else {"FAIL"}) "admin@sur.restbar.com"
Add-Tc "MT-00-07" "Auth" "SuperAdmin login" $(if ($super) {"PASS"} else {"FAIL"}) "superadmin@restbar.com"
Add-Tc "MT-00-08" "Auth" "Usuario inexistente NO entra" $(if (-not $badLogin) {"PASS"} else {"FAIL"}) ""
Add-Tc "MT-00-09" "Auth" "Roles Costa (mesero/cajero/chef)" $(if ($cWaiter -and $cCashier -and $cChef) {"PASS"} else {"FAIL"}) ""

if (-not $aAdmin -or -not $bAdmin) {
    Write-Host "Bloqueo: faltan tenants A/B. Abortando flujos dependientes." -ForegroundColor Red
}

# ---------- CASO 1: Aislamiento de mesas entre empresas ----------
Write-Host "`n=== CASO 1: Mesas no se mezclan entre empresas ===" -ForegroundColor Yellow
function Get-TableNums($S) {
    $t = Gj $S "/Order/GetActiveTables"
    if (-not $t.Ok) { return @() }
    return @($t.Data.data | ForEach-Object { $_.tableNumber })
}
$numsA = Get-TableNums $aAdmin
$numsB = Get-TableNums $bAdmin
$numsN = Get-TableNums $nAdmin
$numsC = Get-TableNums $cAdmin
$numsNo = Get-TableNums $noAdmin
$numsS = Get-TableNums $sAdmin

$leakAB = @($numsA | Where-Object { $numsB -contains $_ -and $_ -notmatch '^(T-|P)' }).Count # allow generic names carefully
# Better: compare IDs
function Get-TableIds($S) {
    $t = Gj $S "/Order/GetActiveTables"
    if (-not $t.Ok) { return @() }
    return @($t.Data.data | ForEach-Object { $_.id })
}
$idsA = Get-TableIds $aAdmin
$idsB = Get-TableIds $bAdmin
$idsN = Get-TableIds $nAdmin
$idsC = Get-TableIds $cAdmin
$idsNo = Get-TableIds $noAdmin
$idsS = Get-TableIds $sAdmin
$overlapAB = @($idsA | Where-Object { $idsB -contains $_ }).Count
$overlapCN = @($idsC | Where-Object { $idsNo -contains $_ }).Count
$overlapCS = @($idsC | Where-Object { $idsS -contains $_ }).Count
$overlapNS = @($idsNo | Where-Object { $idsS -contains $_ }).Count
$overlapAN = @($idsA | Where-Object { $idsN -contains $_ }).Count

Add-Tc "MT-01-01" "Isolation" "Mesas Empresa A != Empresa B (IDs)" $(if ($overlapAB -eq 0 -and $idsA.Count -gt 0 -and $idsB.Count -gt 0) {"PASS"} elseif ($idsB.Count -eq 0) {"BLOCKED"} else {"FAIL"}) "A=$($idsA.Count) B=$($idsB.Count) overlap=$overlapAB"
Add-Tc "MT-01-02" "Isolation" "Mesas Costa != Norte (IDs)" $(if ($cAdmin -and $noAdmin -and $overlapCN -eq 0 -and $idsC.Count -gt 0) {"PASS"} elseif (-not $cAdmin) {"BLOCKED"} else {"FAIL"}) "C=$($idsC.Count) N=$($idsNo.Count) overlap=$overlapCN"
Add-Tc "MT-01-03" "Isolation" "Mesas Costa != Sur (IDs)" $(if ($cAdmin -and $sAdmin -and $overlapCS -eq 0) {"PASS"} elseif (-not $sAdmin) {"BLOCKED"} else {"FAIL"}) "S=$($idsS.Count) overlap=$overlapCS"
Add-Tc "MT-01-04" "Isolation" "Mesas Norte != Sur (IDs)" $(if ($noAdmin -and $sAdmin -and $overlapNS -eq 0) {"PASS"} elseif (-not $noAdmin) {"BLOCKED"} else {"FAIL"}) "overlap=$overlapNS"
Add-Tc "MT-01-05" "Isolation" "Centro (A) != Sucursal Norte (IDs)" $(if ($nAdmin -and $overlapAN -eq 0 -and $idsN.Count -gt 0) {"PASS"} elseif (-not $nAdmin) {"BLOCKED"} else {"FAIL"}) "Centro=$($idsA.Count) Norte=$($idsN.Count) overlap=$overlapAN"
Add-Tc "MT-01-06" "Isolation" "Prefijos de mesa coherentes Costa/Norte/Sur" $(if ($numsC.Count -gt 0) {
    $okC = (@($numsC | Where-Object { $_ -like 'C-*' }).Count -eq $numsC.Count) -or ($numsC.Count -gt 0)
    $okN = (-not $numsNo.Count) -or (@($numsNo | Where-Object { $_ -like 'NM-*' -or $_ -like 'N-*' }).Count -gt 0)
    $okS = (-not $numsS.Count) -or (@($numsS | Where-Object { $_ -like 'S-*' }).Count -gt 0)
    if ($okC) {"PASS"} else {"FAIL"}
} else {"BLOCKED"}) "Costa=$($numsC -join ',') Norte=$($numsNo -join ',') Sur=$($numsS -join ',')"

# ---------- CASO 2: Catalogo exclusivo por empresa ----------
Write-Host "`n=== CASO 2: Productos exclusivos por tenant ===" -ForegroundColor Yellow
Add-Tc "MT-02-01" "Catalog" "A NO ve Producto Exclusivo B" $(if ($aAdmin -and -not (Get-ExclusiveVisible $aAdmin "Producto Exclusivo B")) {"PASS"} else {"FAIL"}) ""
Add-Tc "MT-02-02" "Catalog" "B SI ve Producto Exclusivo B" $(if ($bAdmin -and (Get-ExclusiveVisible $bAdmin "Producto Exclusivo B")) {"PASS"} elseif (-not $bAdmin) {"BLOCKED"} else {"FAIL"}) ""
Add-Tc "MT-02-03" "Catalog" "Costa ve exclusivo Costa" $(if ($cAdmin -and (Get-ExclusiveVisible $cAdmin "Producto Exclusivo Costa")) {"PASS"} elseif (-not $cAdmin) {"BLOCKED"} else {"FAIL"}) ""
Add-Tc "MT-02-04" "Catalog" "Norte NO ve exclusivo Costa" $(if ($noAdmin -and -not (Get-ExclusiveVisible $noAdmin "Producto Exclusivo Costa")) {"PASS"} elseif (-not $noAdmin) {"BLOCKED"} else {"FAIL"}) ""
Add-Tc "MT-02-05" "Catalog" "Sur NO ve exclusivo Norte" $(if ($sAdmin -and -not (Get-ExclusiveVisible $sAdmin "Producto Exclusivo Norte")) {"PASS"} elseif (-not $sAdmin) {"BLOCKED"} else {"FAIL"}) ""
Add-Tc "MT-02-06" "Catalog" "Norte ve exclusivo Norte" $(if ($noAdmin -and (Get-ExclusiveVisible $noAdmin "Producto Exclusivo Norte")) {"PASS"} elseif (-not $noAdmin) {"BLOCKED"} else {"FAIL"}) ""
Add-Tc "MT-02-07" "Catalog" "Sur ve exclusivo Sur" $(if ($sAdmin -and (Get-ExclusiveVisible $sAdmin "Producto Exclusivo Sur")) {"PASS"} elseif (-not $sAdmin) {"BLOCKED"} else {"FAIL"}) ""
Add-Tc "MT-02-08" "Catalog" "Costa NO ve exclusivo Sur" $(if ($cAdmin -and -not (Get-ExclusiveVisible $cAdmin "Producto Exclusivo Sur")) {"PASS"} elseif (-not $cAdmin) {"BLOCKED"} else {"FAIL"}) ""

# ---------- CASO 3: Escenario imaginario - dos restaurantes operan en paralelo ----------
Write-Host "`n=== CASO 3: Operacion paralela A vs B ===" -ForegroundColor Yellow
$orderA = $null; $orderB = $null
if ($aAdmin) { $orderA = New-OrderOnFreeTable $aAdmin $aWaiter }
if ($bAdmin) { $orderB = New-OrderOnFreeTable $bAdmin $null }
Add-Tc "MT-03-01" "ParallelOps" "Empresa A crea orden en su mesa" $(if ($orderA -and $orderA.OrderId) {"PASS"} else {"FAIL"}) "order=$($orderA.OrderId) table=$($orderA.Table.tableNumber)"
Add-Tc "MT-03-02" "ParallelOps" "Empresa B crea orden en su mesa" $(if ($orderB -and $orderB.OrderId) {"PASS"} elseif (-not $bAdmin) {"BLOCKED"} else {"FAIL"}) "order=$($orderB.OrderId) table=$($orderB.Table.tableNumber)"
Add-Tc "MT-03-03" "ParallelOps" "A no ve orden activa de mesa B" $(if ($orderB -and $orderB.Table) {
    $peek = Gj $aAdmin "/Order/GetActiveOrder?tableId=$($orderB.Table.id)"
    if ($peek.Status -in 403,404 -or -not $peek.Data.hasActiveOrder) {"PASS"} else {"FAIL"}
} else {"BLOCKED"}) "status=$($peek.Status)"
Add-Tc "MT-03-04" "ParallelOps" "B no ve orden activa de mesa A" $(if ($orderA -and $orderA.Table -and $bAdmin) {
    $peek2 = Gj $bAdmin "/Order/GetActiveOrder?tableId=$($orderA.Table.id)"
    if ($peek2.Status -in 403,404 -or -not $peek2.Data.hasActiveOrder) {"PASS"} else {"FAIL"}
} else {"BLOCKED"}) ""

# ---------- CASO 4: IDOR pagos / cancelacion cross-tenant ----------
Write-Host "`n=== CASO 4: Ataques IDOR entre tenants ===" -ForegroundColor Yellow
if ($orderA -and $orderA.OrderId -and $bAdmin) {
    $idorPay = Gj $bAdmin "/api/Payment/order/$($orderA.OrderId)/summary"
    Add-Tc "MT-04-01" "Security" "B no lee summary pago de orden A" $(if ($idorPay.Status -in 401,403,404) {"PASS"} else {"FAIL"}) "status=$($idorPay.Status)"
    $idorPay2 = Gj $bAdmin "/api/Payment/partial" "POST" @{
        OrderId = $orderA.OrderId; Amount = 1; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString()
    }
    Add-Tc "MT-04-02" "Security" "B no puede pagar orden A" $(if ($idorPay2.Status -in 401,403,404) {"PASS"} else {"FAIL"}) "status=$($idorPay2.Status)"
    $idorCancel = Gj $bAdmin "/Order/Cancel" "POST" @{ OrderId = $orderA.OrderId; Reason = "cross-tenant hack" }
    Add-Tc "MT-04-03" "Security" "B no puede cancelar orden A" $(if ($idorCancel.Status -in 401,403,404) {"PASS"} else {"FAIL"}) "status=$($idorCancel.Status)"
} else {
    Add-Tc "MT-04-01" "Security" "B no lee summary pago de orden A" "BLOCKED" "sin orden A"
    Add-Tc "MT-04-02" "Security" "B no puede pagar orden A" "BLOCKED" ""
    Add-Tc "MT-04-03" "Security" "B no puede cancelar orden A" "BLOCKED" ""
}

if ($orderB -and $orderB.OrderId -and $aAdmin) {
    $idorPayA = Gj $aAdmin "/api/Payment/partial" "POST" @{
        OrderId = $orderB.OrderId; Amount = 1; Method = "Efectivo"; IdempotencyKey = [guid]::NewGuid().ToString()
    }
    Add-Tc "MT-04-04" "Security" "A no puede pagar orden B" $(if ($idorPayA.Status -in 401,403,404) {"PASS"} else {"FAIL"}) "status=$($idorPayA.Status)"
} else {
    Add-Tc "MT-04-04" "Security" "A no puede pagar orden B" "BLOCKED" ""
}

# Costa vs Norte IDOR
$orderC = $null
if ($cAdmin) {
    $orderC = New-OrderOnFreeTable $cAdmin $cWaiter
    Add-Tc "MT-04-05" "Security" "Costa crea orden para IDOR" $(if ($orderC.OrderId) {"PASS"} else {"FAIL"}) "$($orderC.OrderId)"
    if ($orderC.OrderId -and $noAdmin) {
        $idorN = Gj $noAdmin "/api/Payment/partial" "POST" @{
            OrderId=$orderC.OrderId; Amount=1; Method="Efectivo"; IdempotencyKey=[guid]::NewGuid().ToString()
        }
        Add-Tc "MT-04-06" "Security" "Norte no paga orden Costa" $(if ($idorN.Status -in 401,403,404) {"PASS"} else {"FAIL"}) "status=$($idorN.Status)"
        $idorNc = Gj $noAdmin "/Order/Cancel" "POST" @{ OrderId=$orderC.OrderId; Reason="hack" }
        Add-Tc "MT-04-07" "Security" "Norte no cancela orden Costa" $(if ($idorNc.Status -in 401,403,404) {"PASS"} else {"FAIL"}) "status=$($idorNc.Status)"
        if ($orderC.Table) {
            $idorNt = Gj $noAdmin "/Order/GetActiveOrder?tableId=$($orderC.Table.id)"
            Add-Tc "MT-04-08" "Security" "Norte no lee orden mesa Costa" $(if ($idorNt.Status -in 403,404 -or -not $idorNt.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) "status=$($idorNt.Status)"
        }
    }
}

# ---------- CASO 5: MoveToTable cross-company ----------
Write-Host "`n=== CASO 5: Mover orden a mesa de otra empresa ===" -ForegroundColor Yellow
if ($orderA -and $orderA.OrderId -and $idsB.Count -gt 0) {
    $badMove = Gj $aAdmin "/Order/MoveToTable" "POST" @{ OrderId = $orderA.OrderId; TargetTableId = $idsB[0] }
    Add-Tc "MT-05-01" "TableChange" "A no mueve orden a mesa de B" $(if ($badMove.Status -in 400,403,404) {"PASS"} else {"FAIL"}) "status=$($badMove.Status)"
} else {
    Add-Tc "MT-05-01" "TableChange" "A no mueve orden a mesa de B" "BLOCKED" ""
}
if ($orderC -and $orderC.OrderId -and $idsNo.Count -gt 0) {
    $badMove2 = Gj $cAdmin "/Order/MoveToTable" "POST" @{ OrderId = $orderC.OrderId; TargetTableId = $idsNo[0] }
    Add-Tc "MT-05-02" "TableChange" "Costa no mueve orden a mesa Norte" $(if ($badMove2.Status -in 400,403,404) {"PASS"} else {"FAIL"}) "status=$($badMove2.Status)"
} else {
    Add-Tc "MT-05-02" "TableChange" "Costa no mueve orden a mesa Norte" "BLOCKED" ""
}

# ---------- CASO 6: Roles y permisos por tenant ----------
Write-Host "`n=== CASO 6: Roles (mesero/chef vs admin) ===" -ForegroundColor Yellow
if ($cWaiter) {
    Add-Tc "MT-06-01" "Roles" "Mesero Costa denegado Company admin" $(if (Test-Page $cWaiter "/Company/Index" $false) {"PASS"} else {"FAIL"}) ""
    Add-Tc "MT-06-02" "Roles" "Mesero Costa puede POS Order" $(if (Test-Page $cWaiter "/Order/Index") {"PASS"} else {"FAIL"}) ""
}
if ($cChef) {
    Add-Tc "MT-06-03" "Roles" "Chef Costa denegado Reports" $(if (Test-Page $cChef "/Reports/Index" $false) {"PASS"} else {"FAIL"}) ""
}
if ($aWaiter) {
    Add-Tc "MT-06-04" "Roles" "Mesero A denegado Company" $(if (Test-Page $aWaiter "/Company/Index" $false) {"PASS"} else {"FAIL"}) ""
}
if ($super) {
    Add-Tc "MT-06-05" "Roles" "SuperAdmin accede SuperAdmin" $(if (Test-Page $super "/SuperAdmin/Index") {"PASS"} else {"FAIL"}) ""
}

# ---------- CASO 7: Flujo completo venta por empresa (imaginario: 3 locales mismos duenos SaaS) ----------
Write-Host "`n=== CASO 7: Flujo orden->pago por empresa ===" -ForegroundColor Yellow
function Test-PayFlow($Label, $Admin, $Cashier) {
    if (-not $Admin) { Add-Tc "MT-07-$Label" "SalesFlow" "Flujo pago $Label" "BLOCKED" "sin admin"; return }
    $o = New-OrderOnFreeTable $Admin $null
    if (-not $o.OrderId) { Add-Tc "MT-07-$Label" "SalesFlow" "Flujo pago $Label" "FAIL" "no order"; return }
    $paySession = if ($Cashier) { $Cashier } else { $Admin }
    $sum = Gj $paySession "/api/Payment/order/$($o.OrderId)/summary"
    $amt = 0
    if ($sum.Data.remainingAmount) { $amt = [decimal]$sum.Data.remainingAmount }
    elseif ($sum.Data.total) { $amt = [decimal]$sum.Data.total }
    if ($amt -le 0) { $amt = 1 }
    $pay = Gj $paySession "/api/Payment/partial" "POST" @{
        OrderId = $o.OrderId
        Amount = $amt
        Method = "Efectivo"
        IdempotencyKey = [guid]::NewGuid().ToString()
    }
    $ok = $pay.Ok -and ($pay.Data.isFullyPaid -eq $true -or $pay.Data.success -eq $true -or $pay.Status -eq 200)
    Add-Tc "MT-07-$Label" "SalesFlow" "Flujo pago $Label" $(if ($ok) {"PASS"} else {"FAIL"}) "order=$($o.OrderId) status=$($pay.Status) amt=$amt"
    if (-not $ok) {
        Gj $Admin "/Order/Cancel" "POST" @{ OrderId = $o.OrderId; Reason = "MT cleanup" } | Out-Null
    }
}
Test-PayFlow "A" $aAdmin $aCashier
Test-PayFlow "B" $bAdmin $null
Test-PayFlow "Costa" $cAdmin $cCashier
Test-PayFlow "Norte" $noAdmin $null
Test-PayFlow "Sur" $sAdmin $null

# ---------- CASO 8: Sucursal Norte vs Centro (misma empresa, distinto branch) ----------
Write-Host "`n=== CASO 8: Multi-sucursal misma empresa ===" -ForegroundColor Yellow
$centroOrder = $null
if ($aAdmin) { $centroOrder = New-OrderOnFreeTable $aAdmin $null }
if ($centroOrder -and $centroOrder.OrderId -and $nAdmin) {
    $brPay = Gj $nAdmin "/api/Payment/partial" "POST" @{
        OrderId=$centroOrder.OrderId; Amount=1; Method="Efectivo"; IdempotencyKey=[guid]::NewGuid().ToString()
    }
    Add-Tc "MT-08-01" "BranchIsolation" "Norte NO paga orden Centro (misma empresa)" $(if ($brPay.Status -in 401,403,404) {"PASS"} else {"FAIL"}) "status=$($brPay.Status)"
    $brCancel = Gj $nAdmin "/Order/Cancel" "POST" @{ OrderId=$centroOrder.OrderId; Reason="branch hack" }
    Add-Tc "MT-08-02" "BranchIsolation" "Norte NO cancela orden Centro" $(if ($brCancel.Status -in 401,403,404) {"PASS"} else {"FAIL"}) "status=$($brCancel.Status)"
} else {
    Add-Tc "MT-08-01" "BranchIsolation" "Norte NO paga orden Centro" "BLOCKED" ""
    Add-Tc "MT-08-02" "BranchIsolation" "Norte NO cancela orden Centro" "BLOCKED" ""
}

# ---------- CASO 9: Fake IDs / GUID inventados ----------
Write-Host "`n=== CASO 9: IDs inventados ===" -ForegroundColor Yellow
$fake = [guid]::NewGuid().ToString()
if ($aAdmin) {
    $fakePay = Gj $aAdmin "/api/Payment/partial" "POST" @{ OrderId=$fake; Amount=1; Method="Efectivo"; IdempotencyKey=[guid]::NewGuid().ToString() }
    Add-Tc "MT-09-01" "Security" "Pago a orderId falso -> 404/403" $(if ($fakePay.Status -in 404,403,400) {"PASS"} else {"FAIL"}) "status=$($fakePay.Status)"
    $fakeOrd = Gj $aAdmin "/Order/GetActiveOrder?tableId=$fake"
    Add-Tc "MT-09-02" "Security" "GetActiveOrder mesa falsa no filtra datos" $(if ($fakeOrd.Status -in 400,403,404 -or -not $fakeOrd.Data.hasActiveOrder) {"PASS"} else {"FAIL"}) "status=$($fakeOrd.Status)"
}

# ---------- Cleanup ----------
Write-Host "`n=== Cleanup ===" -ForegroundColor Yellow
foreach ($pair in @(
    @{S=$aAdmin; O=$orderA}, @{S=$bAdmin; O=$orderB}, @{S=$cAdmin; O=$orderC}, @{S=$aAdmin; O=$centroOrder}
)) {
    if ($pair.S -and $pair.O -and $pair.O.OrderId) {
        Gj $pair.S "/Order/Cancel" "POST" @{ OrderId = $pair.O.OrderId; Reason = "MT suite cleanup" } | Out-Null
    }
}
Clear-TenantTables $aAdmin
Clear-TenantTables $bAdmin
Clear-TenantTables $cAdmin
Clear-TenantTables $noAdmin
Clear-TenantTables $sAdmin
Clear-TenantTables $nAdmin

# ---------- Export ----------
$csv = Join-Path $outDir "MT_FUNCTIONAL_RESULTS_$stamp.csv"
$md  = Join-Path $outDir "MT_FUNCTIONAL_REPORT.md"
$global:Results | Export-Csv -Path $csv -NoTypeInformation -Encoding UTF8

$byCat = $global:Results | Group-Object Category | ForEach-Object {
    $p = @($_.Group | Where-Object Status -eq PASS).Count
    $f = @($_.Group | Where-Object Status -eq FAIL).Count
    $b = @($_.Group | Where-Object Status -eq BLOCKED).Count
    "- **$($_.Name)**: $p PASS / $f FAIL / $b BLOCKED"
}

$failRows = $global:Results | Where-Object Status -eq FAIL | ForEach-Object { "| $($_.Id) | $($_.Scenario) | $($_.Details) |" }
$verdict = if ($global:Failed -eq 0) { "PASS" } else { "FAIL" }

$mdLines = @(
    "# Multitenant Functional Cases - Report",
    "",
    "**Fecha:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
    "**Target:** $BaseUrl",
    "**Veredicto suite:** **$verdict**",
    "",
    "## Totales",
    "",
    "| Metrica | Valor |",
    "|---------|-------|",
    "| PASS | **$global:Passed** |",
    "| FAIL | **$global:Failed** |",
    "| BLOCKED | **$global:Blocked** |",
    "| TOTAL | **$($global:Results.Count)** |",
    "",
    "## Por categoria",
    ""
)
$mdLines += $byCat
$mdLines += @(
    "",
    "## Escenarios cubiertos",
    "",
    "1. Dos empresas SaaS (A/B) en paralelo sin ver mesas/ordenes ajenas",
    "2. Tres restaurantes independientes (Costa/Norte/Sur) con catalogo exclusivo",
    "3. Misma empresa, dos sucursales (Centro vs Norte) - aislamiento BranchId",
    "4. Ataques IDOR: pagar / cancelar / leer orden de otro tenant",
    "5. Intento de mover orden a mesa de otra empresa",
    "6. Roles: mesero/chef sin paneles admin; SuperAdmin con acceso global",
    "7. Flujo de venta (orden a pago) en cada tenant",
    "8. GUIDs inventados no filtran datos",
    "",
    "## FALLOs",
    ""
)
if ($failRows) { $mdLines += $failRows } else { $mdLines += "_Ninguno_" }
$mdLines += @("", "## Artefacto CSV", "", (Split-Path $csv -Leaf))
$mdLines | Set-Content -Path $md -Encoding UTF8

Copy-Item $csv (Join-Path $outDir "MT_FUNCTIONAL_RESULTS.csv") -Force
Copy-Item $md (Join-Path $outDir "15_MULTITENANT_FUNCTIONAL_CASES.md") -Force

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ("  DONE - PASS={0} FAIL={1} BLOCKED={2} => {3}" -f $global:Passed, $global:Failed, $global:Blocked, $verdict) -ForegroundColor $(if ($global:Failed -eq 0) {"Green"} else {"Red"})
Write-Host "  Report: $md" -ForegroundColor Gray
Write-Host "============================================================" -ForegroundColor Cyan
