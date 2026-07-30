# 15 — API DESIGN

**Base:** `/api/v1/cash` · JSON · Cookie auth + CSRF MVC forms where needed

---

# CashRegister

| Method | Route | Permission |
|--------|-------|------------|
| GET | `/registers` | cash.register.view |
| GET | `/registers/{id}` | cash.register.view |
| POST | `/registers` | cash.register.manage |
| PUT | `/registers/{id}` | cash.register.manage |
| DELETE | `/registers/{id}` | cash.register.manage (soft) |

---

# CashSession

| Method | Route | Body | Response |
|--------|-------|------|----------|
| POST | `/sessions/open` | registerId, openingFloat, denominations?, shiftId? | sessionId, number |
| GET | `/sessions/active` | ?registerId | current session user/register |
| GET | `/sessions/{id}` | — | session detail + totals |
| POST | `/sessions/{id}/suspend` | reason | ok |
| POST | `/sessions/{id}/resume` | — | ok |
| POST | `/sessions/{id}/close/start` | — | counting mode |
| POST | `/sessions/{id}/close/count` | denominations | counted total |
| POST | `/sessions/{id}/close/finalize` | notes | zReportId or approvalId |
| POST | `/sessions/{id}/reopen` | reason, managerPin? | newSessionId |
| GET | `/sessions` | dateFrom, dateTo, registerId | paged list |

---

# CashMovement

| Method | Route | Body |
|--------|-------|------|
| GET | `/sessions/{id}/movements` | paging |
| POST | `/movements/paid-in` | sessionId, amount, reasonCode, comments |
| POST | `/movements/paid-out` | sessionId, amount, reasonCode, comments |

Auto movements: internal via `ICashPaymentHook`, no HTTP público.

---

# CashApproval

| POST | `/approvals/{id}/approve` | comments |
| POST | `/approvals/{id}/reject` | comments |
| GET | `/approvals/pending` | branch scope |

---

# Reports

| GET | `/reports/z/{sessionId}` | JSON + ?format=pdf\|xlsx |
| GET | `/reports/x/{sessionId}` | interim snapshot |
| GET | `/reports/daily` | branchId, businessDate |
| GET | `/reports/cashier/{userId}` | period |

---

# Command Center snapshot

| GET | `/snapshot/branch` | widgets data <500ms cached |

---

# SignalR (OrderHub extend)

Client methods:
- `JoinCashRegister(registerId)` — tenant validated  
- `LeaveCashRegister(registerId)`  

Server events:
- `CashSessionOpened`, `CashMovementRecorded`, `CashSessionClosed`, `CashApprovalRequired`, `CashVarianceAlert`

---

# Events (in-process v1)

`ICashEventPublisher` → NotificationService + Audit

---

# Error codes

| Code | HTTP | Meaning |
|------|------|---------|
| CASH_SESSION_REQUIRED | 409 | No open session for cash payment |
| CASH_REGISTER_BUSY | 409 | Session already open |
| CASH_SESSION_CLOSED | 400 | Mutation on closed session |
| CASH_APPROVAL_REQUIRED | 402 | Needs supervisor |
| CASH_TENANT_DENIED | 403 | IDOR |
| CASH_VARIANCE_UNAPPROVED | 400 | Cannot close |

---

# Idempotency

Header `Idempotency-Key` on paid-in/out manual (optional). Payment hook uses Payment.IdempotencyKey.
