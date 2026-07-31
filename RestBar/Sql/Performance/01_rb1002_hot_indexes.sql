-- RB-1002 hot-path indexes (idempotent). Apply on VPS if Migrate delayed.
CREATE INDEX IF NOT EXISTS IX_audit_logs_company_timestamp
    ON public.audit_logs ("CompanyId", "timestamp" DESC);
CREATE INDEX IF NOT EXISTS IX_audit_logs_timestamp
    ON public.audit_logs ("timestamp" DESC);
CREATE INDEX IF NOT EXISTS IX_audit_logs_company_module_timestamp
    ON public.audit_logs ("CompanyId", "Module", "timestamp" DESC);
CREATE INDEX IF NOT EXISTS IX_audit_logs_company_error_timestamp
    ON public.audit_logs ("CompanyId", "IsError", "timestamp" DESC)
    WHERE "IsError" = true;
CREATE INDEX IF NOT EXISTS IX_orders_status_opened
    ON public.orders (status, opened_at);
CREATE INDEX IF NOT EXISTS IX_orders_company_status_opened
    ON public.orders ("CompanyId", status, opened_at);
CREATE INDEX IF NOT EXISTS IX_orders_branch_closed
    ON public.orders ("BranchId", closed_at)
    WHERE closed_at IS NOT NULL;
CREATE INDEX IF NOT EXISTS IX_orders_company_closed
    ON public.orders ("CompanyId", closed_at)
    WHERE closed_at IS NOT NULL;
CREATE INDEX IF NOT EXISTS IX_order_items_kitchen_status_sent
    ON public.order_items (prepared_by_station_id, kitchen_status, sent_at)
    WHERE kitchen_status IN ('Pending', 'Sent');
CREATE INDEX IF NOT EXISTS IX_customers_company_name
    ON public.customers ("CompanyId", full_name);
CREATE INDEX IF NOT EXISTS IX_customers_company_email
    ON public.customers ("CompanyId", email);
CREATE INDEX IF NOT EXISTS IX_products_company_branch_active
    ON public.products (company_id, branch_id, is_active);
ANALYZE public.audit_logs;
ANALYZE public.orders;
ANALYZE public.order_items;
ANALYZE public.customers;
ANALYZE public.products;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260731120000_Rb1002PerformanceIndexes', '9.0.5')
ON CONFLICT DO NOTHING;
