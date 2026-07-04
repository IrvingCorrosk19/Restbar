-- Data repair: keep only the newest active order per table, cancel duplicates
WITH ranked AS (
  SELECT id, table_id,
         ROW_NUMBER() OVER (PARTITION BY table_id ORDER BY opened_at DESC NULLS LAST, id DESC) AS rn
  FROM orders
  WHERE table_id IS NOT NULL
    AND status::text NOT IN ('Completed', 'Cancelled')
)
UPDATE orders o
SET status = 'Cancelled', "UpdatedAt" = NOW()
FROM ranked r
WHERE o.id = r.id AND r.rn > 1;

-- Now create the unique index
CREATE UNIQUE INDEX IF NOT EXISTS idx_unique_active_order_per_table
  ON orders (table_id)
  WHERE table_id IS NOT NULL
    AND status::text NOT IN ('Completed', 'Cancelled');

-- Release ghost tables
UPDATE tables t
SET status = 'Disponible'
WHERE t.status::text = 'Ocupada'
  AND NOT EXISTS (
    SELECT 1 FROM orders o
    WHERE o.table_id = t.id
      AND o.status::text NOT IN ('Completed', 'Cancelled')
  );
