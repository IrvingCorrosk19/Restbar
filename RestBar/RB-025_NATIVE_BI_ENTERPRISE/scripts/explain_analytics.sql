\timing on
\pset pager off

SELECT company_id AS c, id AS b FROM branches LIMIT 1 \gset

EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)
SELECT * FROM analytics.sp_executive_summary(:'c'::uuid, :'b'::uuid, now() - interval '30 days', now());

EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)
SELECT * FROM analytics.sp_sales_by_hour(:'c'::uuid, :'b'::uuid, now() - interval '30 days', now());

EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)
SELECT * FROM analytics.sp_sales_by_product(:'c'::uuid, :'b'::uuid, now() - interval '30 days', now(), 50);

EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)
SELECT * FROM analytics.sp_inventory_health(:'c'::uuid, :'b'::uuid);

EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)
SELECT * FROM analytics.sp_cash_summary(:'c'::uuid, :'b'::uuid, now() - interval '30 days', now());
