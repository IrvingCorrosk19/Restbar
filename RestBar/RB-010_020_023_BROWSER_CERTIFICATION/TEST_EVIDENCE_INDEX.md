# TEST_EVIDENCE_INDEX.md

| Artefacto | Ubicación |
|-----------|-----------|
| Playwright HTML report | `playwright-report/` |
| JSON results | `playwright-results.json` |
| Desktop log | `playwright-run-desktop-final.log` |
| Responsive log | `playwright-run-responsive2.log` |
| Screenshots / videos / traces | `evidence/test-output/` + `evidence/*.png` |
| Suite source | `../tests/Browser/` |

Reproducir:

```powershell
cd RestBar
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --urls http://localhost:5001 --no-launch-profile
# otra terminal:
cd tests/Browser
npm test
```
