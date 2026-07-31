# 08 — Search Optimization (RB-1002)

| Domain | Query pattern | Index / EF | Status |
|--------|---------------|------------|--------|
| Customers | `FullName/Email/Phone.Contains` + CompanyId | `IX_customers_company_name`, `_email` + AsNoTracking | Done |
| Products | company/branch/active | `IX_products_company_branch_active` | Done |
| Orders | branch/status/opened, table/status | Pre-existing + new status/opened | Done |
| Users | existing CompanyId indexes | No change | OK |
| Audit | Module + Timestamp | New composites | Done |

## Partial text search

`Contains` → SQL `LIKE %term%` cannot use btree optimally. For enterprise FTS:

- Observation **O-SRCH-01**: consider `pg_trgm` GIN on `customers.full_name` / `products.name` in a later phase (requires extension + measured benefit).

No trigram installed this cycle (avoids extension surprise in prod).
