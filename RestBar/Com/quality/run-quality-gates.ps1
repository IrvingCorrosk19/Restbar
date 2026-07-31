# RestBar/Com/quality/run-quality-gates.ps1
# Local / agent Quality Gate runner (RB-027). Exit 0 only if required gates pass.
param(
    [string]$BaseUrl = $env:RESTBAR_BASE_URL,
    [switch]$SkipBrowser,
    [switch]$SkipCoverage
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
$app = Join-Path $root "RestBar"
$browser = Join-Path $app "tests\Browser"
$failed = @()

function Gate([string]$name, [scriptblock]$action) {
    Write-Host ""
    Write-Host "======== $name ========" -ForegroundColor Cyan
    try {
        & $action
        Write-Host "PASS $name" -ForegroundColor Green
    } catch {
        Write-Host "FAIL $name : $_" -ForegroundColor Red
        $script:failed += $name
    }
}

Gate "G1 Build" {
    Push-Location $root
    try {
        dotnet restore RestBar/RestBar.csproj | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "restore failed" }
        dotnet build RestBar/RestBar.csproj -c Release --no-restore | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "build failed" }
    } finally { Pop-Location }
}

Gate "G2 Unit tests" {
    Push-Location $root
    try {
        $dotnetArgs = @("test", "RestBar.Tests/RestBar.Tests.csproj", "-c", "Release", "--verbosity", "minimal")
        if (-not $SkipCoverage) {
            $out = Join-Path $app "RB-027_QUALITY_GATE\evidence\unit-coverage"
            New-Item -ItemType Directory -Force -Path $out | Out-Null
            $dotnetArgs += @("--collect:XPlat Code Coverage", "--results-directory", $out)
        }
        & dotnet @dotnetArgs | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "unit tests failed" }
    } finally { Pop-Location }
}

Gate "G3 Security advisory scan" {
    Push-Location $root
    try {
        $report = Join-Path $app "RB-027_QUALITY_GATE\evidence\vuln-report.txt"
        New-Item -ItemType Directory -Force -Path (Split-Path $report) | Out-Null
        dotnet list RestBar/RestBar.csproj package --vulnerable --include-transitive 2>&1 |
            Tee-Object -FilePath $report | Out-Host
        # Advisory: known MailKit/MimeKit moderates. High/Critical => fail.
        $txt = Get-Content $report -Raw
        if ($txt -match '(?i)\b(Critical|High)\b') {
            throw "High/Critical vulnerable packages found - see $report"
        }
    } finally { Pop-Location }
}

Gate "G8 Policy assets" {
    $required = @(
        "RestBar\RB-027_QUALITY_GATE\01_Quality_Gates.md",
        "RestBar\RB-027_QUALITY_GATE\08_Development_Standards.md",
        "RestBar\RB-027_QUALITY_GATE\09_Contribution_Guide.md",
        "RestBar\tests\Browser\playwright.config.js",
        "RestBar.Tests\RestBar.Tests.csproj"
    )
    foreach ($r in $required) {
        $p = Join-Path $root $r
        if (-not (Test-Path $p)) { throw "Missing $r" }
    }
}

if (-not $SkipBrowser) {
    if (-not $BaseUrl) {
        Write-Host "SKIP G4 Browser - set RESTBAR_BASE_URL or pass -BaseUrl" -ForegroundColor Yellow
    } else {
        Gate "G4 Browser smoke" {
            Push-Location $browser
            try {
                $env:RESTBAR_BASE_URL = $BaseUrl
                npx playwright test Smoke Security Multitenant Performance --project=chromium-desktop --reporter=list --retries=0 | Out-Host
                if ($LASTEXITCODE -ne 0) { throw "browser smoke failed" }
            } finally { Pop-Location }
        }
    }
}

Write-Host ""
if ($failed.Count -gt 0) {
    Write-Host "QUALITY GATE FAILED: $($failed -join ', ')" -ForegroundColor Red
    exit 1
}
Write-Host "QUALITY GATE PASSED" -ForegroundColor Green
exit 0
