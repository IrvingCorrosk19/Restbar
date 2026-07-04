-- Apply SecurityHardening migration manually for certification
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_enum
    WHERE enumtypid = 'user_role_enum'::regtype
      AND enumlabel = 'inventarista'
  ) THEN
    ALTER TYPE user_role_enum ADD VALUE 'inventarista';
  END IF;
END $$;

ALTER TABLE payments ADD COLUMN IF NOT EXISTS idempotency_key character varying(100);

CREATE UNIQUE INDEX IF NOT EXISTS idx_payments_idempotency_key
  ON payments (idempotency_key)
  WHERE idempotency_key IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS idx_unique_active_order_per_table
  ON orders (table_id)
  WHERE table_id IS NOT NULL
    AND status::text NOT IN ('Completed', 'Cancelled');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260227000000_SecurityHardening', '9.0.5'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260227000000_SecurityHardening'
);
