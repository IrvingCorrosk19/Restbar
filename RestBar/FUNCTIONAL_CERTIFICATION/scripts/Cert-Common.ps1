# Shared helpers for RestBar certification suites
param()

$script:CertPgBin = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
$script:CertPgPass = "Panama2020$"

function Get-CertJson {
    param($BaseUrl, $Session, $Path, $Method = "GET", $Body = $null)
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

function Get-CertSession {
    param([string]$BaseUrl, [string]$Email, [string]$Password = "123456")
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

function Test-CertTableFree($Table) {
    return ($Table.status -eq "Disponible" -or $Table.status -eq 0 -or $Table.status -eq "0")
}

function Reset-CertTableOrder {
    param([string]$BaseUrl, $Session, $tableId)
    $active = Get-CertJson $BaseUrl $Session "/Order/GetActiveOrder?tableId=$tableId"
    $oid = $active.Data.orderId
    if (-not $oid -and $active.Data.order) { $oid = $active.Data.order.id }
    if ($active.Data.hasActiveOrder -and $oid) {
        Get-CertJson $BaseUrl $Session "/Order/Cancel" "POST" @{ OrderId = $oid; Reason = "Certification reset" } | Out-Null
        Start-Sleep -Milliseconds 350
    }
}

function Invoke-CertPgReset {
    if (-not (Test-Path $script:CertPgBin)) { return $false }
    $env:PGPASSWORD = $script:CertPgPass
    $sql = @"
UPDATE orders
SET status = 'Cancelled', closed_at = COALESCE(closed_at, NOW()), version = version + 1
WHERE status NOT IN ('Cancelled', 'Completed');

UPDATE tables
SET status = 'Disponible'
WHERE is_active = true AND status <> 'Disponible';

UPDATE payments
SET is_voided = true
WHERE is_voided = false
  AND order_id IN (SELECT id FROM orders WHERE status = 'Cancelled');
"@
    & $script:CertPgBin -h localhost -U postgres -d RestBar -c $sql 2>$null | Out-Null
    return ($LASTEXITCODE -eq 0)
}

function Reset-CertAllTables {
    param([string]$BaseUrl, $AdminSession)
    if (-not $AdminSession) { return }
    $tables = Get-CertJson $BaseUrl $AdminSession "/Order/GetActiveTables"
    if ($tables.Data.data) {
        foreach ($t in @($tables.Data.data)) {
            Reset-CertTableOrder $BaseUrl $AdminSession $t.id
        }
    }
    Invoke-CertPgReset | Out-Null
    Start-Sleep -Milliseconds 600
}

function Get-CertWaiterTable {
    param([string]$BaseUrl, $AdminSession, $WaiterSession)
    Reset-CertAllTables $BaseUrl $AdminSession
    $tables = Get-CertJson $BaseUrl $WaiterSession "/Order/GetActiveTables"
    if (-not $tables.Ok -or -not $tables.Data.data) { return $null }
    $free = @($tables.Data.data | Where-Object { Test-CertTableFree $_ })
    if ($free.Count -gt 0) { return $free[0] }
    foreach ($t in @($tables.Data.data)) {
        Reset-CertTableOrder $BaseUrl $AdminSession $t.id
    }
    Invoke-CertPgReset | Out-Null
    Start-Sleep -Milliseconds 400
    $recheck = Get-CertJson $BaseUrl $WaiterSession "/Order/GetActiveTables"
    $free2 = @($recheck.Data.data | Where-Object { Test-CertTableFree $_ })
    if ($free2.Count -gt 0) { return $free2[0] }
    return $null
}

function Get-CertAdminFreeTable {
    param([string]$BaseUrl, $AdminSession)
    Reset-CertAllTables $BaseUrl $AdminSession
    $tables = Get-CertJson $BaseUrl $AdminSession "/Order/GetActiveTables"
    if (-not $tables.Ok -or -not $tables.Data.data) { return $null }
    $free = @($tables.Data.data | Where-Object { Test-CertTableFree $_ } | Select-Object -First 1)
    if ($free.Count -gt 0) { return $free[0] }
    return $null
}
