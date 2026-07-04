CREATE INDEX IF NOT EXISTS IX_payments_processed_by_user_id ON payments(processed_by_user_id);
CREATE INDEX IF NOT EXISTS IX_order_items_added_by_user_id ON order_items(added_by_user_id);
CREATE INDEX IF NOT EXISTS IX_order_items_delivered_by_user_id ON order_items(delivered_by_user_id);

DO $$ BEGIN
  ALTER TABLE payments ADD CONSTRAINT payments_processed_by_user_id_fkey
    FOREIGN KEY (processed_by_user_id) REFERENCES users(id) ON DELETE SET NULL;
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
  ALTER TABLE order_items ADD CONSTRAINT order_items_added_by_user_id_fkey
    FOREIGN KEY (added_by_user_id) REFERENCES users(id) ON DELETE SET NULL;
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
  ALTER TABLE order_items ADD CONSTRAINT order_items_delivered_by_user_id_fkey
    FOREIGN KEY (delivered_by_user_id) REFERENCES users(id) ON DELETE SET NULL;
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

UPDATE order_items oi
SET added_by_user_id = o.user_id
FROM orders o
WHERE oi.order_id = o.id
  AND oi.added_by_user_id IS NULL
  AND o.user_id IS NOT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260704193336_EnterpriseOperationP0', '8.0.0')
ON CONFLICT DO NOTHING;
