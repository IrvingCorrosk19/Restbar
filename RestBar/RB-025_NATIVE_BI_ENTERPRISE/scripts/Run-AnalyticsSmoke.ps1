# RB-025 — HTTP smoke for Executive Analytics (no Playwright required)
param([string]$BaseUrl = "http://164.68.99.83:8084")

$ErrorActionPreference = "Continue"
$script:pass = 0
$script:fail = 0
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

function Assert-Ok([string]$id, [bool]$cond, [string]$detail) {
    if ($cond) {
        $script:pass++
        Write-Host "PASS $id - $detail" -ForegroundColor Green
    } else {
        $script:fail++
        Write-Host "FAIL $id - $detail" -ForegroundColor Red
    }
}

try {
    $null = Invoke-WebRequest -Uri "$BaseUrl/Auth/Login" -WebSession $session -UseBasicParsing
    $form = @{ email = "admin@restbar.com"; password = "123456" }
    try {
        $loginPost = Invoke-WebRequest -Uri "$BaseUrl/Auth/Login" -Method POST -Body $form -WebSession $session -MaximumRedirection 5 -UseBasicParsing
        Assert-Ok "SMK-LOGIN" ($loginPost.StatusCode -lt 400) "HTTP $($loginPost.StatusCode)"
    } catch {
        Assert-Ok "SMK-LOGIN" ($session.Cookies.Count -gt 0) "cookies after login attempt"
    }

    $center = Invoke-WebRequest -Uri "$BaseUrl/ExecutiveAnalytics" -WebSession $session -UseBasicParsing
    Assert-Ok "SMK-CENTER" ($center.StatusCode -eq 200 -and $center.Content -match "Centro Ejecutivo") "HTTP $($center.StatusCode)"

    $live = Invoke-WebRequest -Uri "$BaseUrl/ExecutiveAnalytics/Live?period=today" -WebSession $session -UseBasicParsing
    Assert-Ok "SMK-LIVE" ($live.StatusCode -eq 200) "HTTP $($live.StatusCode)"

    foreach ($key in @("sales-hour","executive-summary","cash-summary","inventory-health")) {
        $rd = Invoke-WebRequest -Uri "$BaseUrl/ExecutiveAnalytics/ReportData?key=$key&period=last_30" -WebSession $session -UseBasicParsing
        Assert-Ok "SMK-RD-$key" ($rd.StatusCode -eq 200) "HTTP $($rd.StatusCode)"
    }

    $csv = Invoke-WebRequest -Uri "$BaseUrl/ExecutiveAnalytics/Export?key=sales-hour&format=csv&period=last_30" -WebSession $session -UseBasicParsing
    Assert-Ok "SMK-CSV" ($csv.StatusCode -eq 200 -and $csv.RawContentLength -gt 0) "bytes $($csv.RawContentLength)"
}
catch {
    Assert-Ok "SMK-EXCEPTION" $false $_.Exception.Message
}

Write-Host ""
Write-Host "RESULT PASS=$pass FAIL=$fail" -ForegroundColor $(if ($fail -eq 0) { "Green" } else { "Red" })
if ($fail -gt 0) { exit 1 }
