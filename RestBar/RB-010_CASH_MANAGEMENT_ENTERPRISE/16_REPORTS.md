# 16 — REPORTS

---

# Por rol

| Reporte | Cajero | Supervisor | Gerente | Contador | Auditor | CEO/Holding |
|---------|--------|------------|---------|----------|---------|-------------|
| X-Report (interim) | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| Z-Report (cierre) | ✅ own | ✅ branch | ✅ | ✅ | ✅ | ✅ aggregate |
| Movements detail | ✅ own | ✅ | ✅ | ✅ | ✅ | ✅ |
| Paid-in/out log | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Voids/refunds | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Variance summary | own | ✅ | ✅ | ✅ | ✅ | ✅ |
| Cashier ranking | ❌ | ✅ | ✅ | ❌ | ✅ | ✅ |
| Incident log | ❌ | ✅ | ✅ | ❌ | ✅ | ✅ |
| Override log | ❌ | ✅ | ✅ | ❌ | ✅ | ✅ |
| Fraud risk flags | ❌ | ⚠️ | ✅ | ❌ | ✅ | ✅ |
| Multi-branch heatmap | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ |

---

# Z-Report contents (industry standard + RestBar)

```
RestBar Z-Report
Company / Branch / Register / Session #
Business Date / Opened / Closed
Cashier / Supervisor / Closer

OPENING FLOAT:        $XXX
+ Cash Sales:         $XXX
+ Paid-In:            $XXX
- Paid-Out:           $XXX
- Cash Refunds:       $XXX
- Voids (cash):       $XXX
= EXPECTED CASH:      $XXX
COUNTED CASH:         $XXX
VARIANCE:             $XXX

NON-CASH:
  Card:               $XXX
  Yappy:              $XXX
  ACH/Transfer:       $XXX

TIPS:
  Cash tips:          $XXX
  Card tips:          $XXX

TOTAL SALES (all):    $XXX
Transaction count:    N
Void count / %:       N / X%
Refund count / %:     N / X%

Approvals:            [list]
Incidents:            [list]

Hash:                 SHA256...
Signed:               RestBar Cash Integrity v1
```

---

# Export

- PDF (QuestPDF or similar — implement real, no stub)  
- Excel OpenXML  
- JSON API for ERP future  

Reuse `SalesReportService` patterns; **new** `CashReportService` — no duplicar AdvancedReports God object initially.

---

# Scheduling

Email Z to accountant on close (optional branch setting) via existing EmailService.

---

# Franchise / Holding

Aggregate Z by Company children; variance heatmap by branch.
