# 18 — TEST PLAN

**Objetivo certificación RB-010:** 100% PASS antes de producción.

---

# Suites

| Suite | Tool | Count est. |
|-------|------|------------|
| Unit | xUnit RestBar.Tests | 80+ |
| Integration | WebApplicationFactory + Testcontainers PG | 40+ |
| Business | Cash workflows E2E API | 30+ |
| Negative | Invalid transitions, bad amounts | 25+ |
| Fraud | Override abuse, IDOR attempts | 20+ |
| Permission | Each role matrix | 35+ |
| Multitenant | 51+ pattern extended | 25+ |
| Performance | k6 / NBomber close storm | 5 scenarios |
| Regression | ORDER 119 + Payment idempotency | existing |
| Browser | Playwright open→pay→close | 10 flows |

---

# Unit tests (samples)

- `CashReconciliationService_ExpectedCash_OpeningPlusSalesMinusOut`  
- `CashHashChainBuilder_ChainValid_After100Movements`  
- `CashSessionStateMachine_InvalidTransition_Throws`  
- `PaymentMethodMapper_MapsYappy_ToSaleYappy`  
- `TenantScope_CashSessionCrossCompany_Denied`  

---

# Integration tests

- Open session → cash payment → movement created → payment.cash_session_id set  
- Close with zero variance → Z-report row exists  
- Close with variance > threshold → approval required → finalize after approve  
- Void payment → reversal movement  
- Refund cash → RefundCash out  
- Concurrent close → one wins RowVersion  

---

# Multitenant (mandatory)

```
Company A cashier → Company B sessionId → 403
Company A payment → assign Company B session → fail
SuperAdmin read holding → OK read-only
```

Extend `Run-MultitenantFunctionalCases.ps1` → `Run-CashMultitenantCases.ps1`

---

# Fraud scenarios

- Cajero attempts paid-out $500 without approval → blocked  
- Cajero approves own variance → denied  
- Replay same Idempotency-Key movement → single movement  
- Tamper hash chain → integrity job fails  

---

# Performance

- 100 concurrent payments + hook < 100ms p95 added latency  
- Close session with 5000 movements < 2s  

---

# Certification artifact

`RB-010_CASH_CERTIFICATION/` (post-impl):
- 01_TEST_RESULTS.csv  
- 02_DEFECT_LOG.md  
- 03_EXECUTIVE_SUMMARY.md  

Gate: 0 FAIL to merge `EnableCashModule=true` default.

---

# Regression gate CI

```yaml
- dotnet test RestBar.Tests --filter Category=Cash
- dotnet test RestBar.Tests --filter Category=Regression
- pwsh scripts/Run-CashCertification.ps1
```
