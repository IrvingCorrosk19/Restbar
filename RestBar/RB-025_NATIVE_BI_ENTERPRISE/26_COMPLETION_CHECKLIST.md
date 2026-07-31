# RB-025 completion checklist (post-implementation)

| Item | Status |
|------|--------|
| Analytics schema + SPs | DONE (26 fns on VPS) |
| Executive Analytics Center | DONE + smoke live/period |
| Exports CSV/XLSX/Print-HTML | DONE (Analytics + AdvancedReports ClosedXML/HTML) |
| SalesReport BranchId filter + claim default | DONE |
| Unit tests Analytics | DONE (4/4) |
| Migrate on startup | DONE (sync migration + history marked) |
| Commit / push / VPS deploy | DONE |
| Browser smoke post-deploy | DONE |
| Playwright analytics suite | DONE (6/6 PASS vs VPS) |
| EXPLAIN ANALYZE key SPs (real data) | DONE (&lt;10 ms) |
| 1M-row synthetic perf lab | DEFERRED (not on prod) |
| Native binary PDF (QuestPDF/Skia) | DEFERRED — HTML print path shipped |
| KPIs NO DISPONIBLE (tax/covers/WMS/…) | DEFERRED — require model change |
