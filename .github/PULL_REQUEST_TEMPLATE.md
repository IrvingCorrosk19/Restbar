## Pull Request — RestBar Quality Gate (RB-027)

### Summary
<!-- What / why (1–3 bullets). No incomplete features. -->

-

### Modules touched
- [ ] POS / Orders
- [ ] Cash
- [ ] Inventory
- [ ] Procurement / Purchases
- [ ] Food Cost / Recipe
- [ ] BI / Analytics
- [ ] Reports / Exports
- [ ] Auth / RBAC / Security
- [ ] Multitenancy
- [ ] Other: ___

### Mandatory deliverables (incomplete = DO NOT MERGE)
- [ ] Code compiles (`dotnet build -c Release`)
- [ ] Unit tests added/updated for new domain logic
- [ ] Browser / API tests for user-facing or API changes
- [ ] Permissions / RBAC reviewed
- [ ] Audit trail considered (if money/stock/config)
- [ ] Error handling (no 500 on validation)
- [ ] Docs updated under the relevant `RB-*` / `RB-027` folder when behavior changes
- [ ] No secrets committed

### Local gates run
```powershell
pwsh RestBar/Com/quality/run-quality-gates.ps1 -BaseUrl http://localhost:5001
# or against VPS:
# $env:RESTBAR_BASE_URL='http://164.68.99.83:8084'; pwsh RestBar/Com/quality/run-quality-gates.ps1
```

### Risk / rollback
- Risk: 
- Rollback: 
