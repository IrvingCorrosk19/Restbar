# RB-025 completion checklist (post-implementation)

| Item | Status |
|------|--------|
| Analytics schema + SPs | DONE (26 fns on VPS) |
| Executive Analytics Center | DONE + smoke live/period |
| Exports CSV/XLSX/Print-HTML | DONE |
| SalesReport BranchId filter + claim default | DONE |
| Unit tests Analytics | DONE (4/4) |
| Migrate on startup | DONE (sync migration + history marked) |
| Commit / push / VPS deploy | DONE (`80a57b3`) |
| Browser smoke post-deploy | DONE (login → Centro Ejecutivo → live KPIs → ReportData 200) |
| Full Playwright analytics suite | DEFERRED |
| 1M-row perf lab | DEFERRED (not on prod) |
