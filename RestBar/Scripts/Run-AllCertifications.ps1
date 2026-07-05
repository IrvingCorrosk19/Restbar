# Run all RestBar certification suites in sequence
param([string]$BaseUrl = "http://localhost:5001")

$scripts = @(
    "FUNCTIONAL_CERTIFICATION\scripts\Run-FullCertification.ps1",
    "ORDER_FUNCTIONAL_CERTIFICATION\scripts\Run-OrderCertification.ps1",
    "ORDER_ROUTING_CERTIFICATION\scripts\Run-RoutingCertification.ps1",
    "ENTERPRISE_OPERATION_CERTIFICATION\scripts\Run-EnterpriseCertification.ps1",
    "FUNCTIONAL_CERTIFICATION_3_COMPANIES\scripts\Run-ThreeCompaniesCertification.ps1"
)

$root = Split-Path $PSScriptRoot -Parent
$common = Join-Path $root "FUNCTIONAL_CERTIFICATION\scripts\Cert-Common.ps1"
$failed = 0

Write-Host "`n========== RESTBAR FULL CERTIFICATION RUN ==========" -ForegroundColor Cyan
foreach ($rel in $scripts) {
    $path = Join-Path $root $rel
    Write-Host "`n>>> $rel" -ForegroundColor Yellow
    & powershell -ExecutionPolicy Bypass -File $path -BaseUrl $BaseUrl
    if ($LASTEXITCODE -ne 0) { $failed++ }
    if (Test-Path $common) {
        . $common
        $admin = Get-CertSession $BaseUrl "admin@restbar.com"
        if ($admin) {
            Write-Host ">>> Cert reset between suites" -ForegroundColor DarkGray
            Reset-CertAllTables $BaseUrl $admin
        }
    }
}

Write-Host "`n========== FULL RUN COMPLETE ==========" -ForegroundColor Cyan
if ($failed -eq 0) { Write-Host "ALL SUITES: PASS" -ForegroundColor Green }
else { Write-Host "SOME SUITES FAILED: $failed" -ForegroundColor Red; exit 1 }
