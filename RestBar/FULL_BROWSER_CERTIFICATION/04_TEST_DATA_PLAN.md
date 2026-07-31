# 04 — TEST DATA PLAN

| Entity | How obtained |
|--------|----------------|
| Admin | `admin@restbar.com` / `123456` |
| Roles | waiter/cashier/chef/cajero emails if seeded; else BLOCKED |
| Company/Branch | Claims after login; SuperAdmin for multi-company |
| Tables | First Available on POS |
| Stations | kitchen / bar query params |
| Products | First product card on POS |
| Cash register | Open wizard if none |
| Suppliers/PO | List pages; skip if empty |
| Recipes | Recipe index links |
| Analytics period | `last_30` / `today` |

**Cleanup:** prefer read-only + idempotent creates with unique suffix timestamps; no destructive wipe on VPS.
