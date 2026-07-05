-- Enterprise Features 100% — idempotent schema patch
ALTER TABLE tables ADD COLUMN IF NOT EXISTS parent_table_id uuid;

ALTER TABLE products ADD COLUMN IF NOT EXISTS is_shareable boolean NOT NULL DEFAULT false;
ALTER TABLE products ADD COLUMN IF NOT EXISTS share_portions integer;

ALTER TABLE "DiscountPolicies" ADD COLUMN IF NOT EXISTS "ValidFromTime" interval;
ALTER TABLE "DiscountPolicies" ADD COLUMN IF NOT EXISTS "ValidUntilTime" interval;

CREATE TABLE IF NOT EXISTS table_merge_links (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    primary_table_id uuid NOT NULL REFERENCES tables(id) ON DELETE CASCADE,
    secondary_table_id uuid NOT NULL REFERENCES tables(id) ON DELETE CASCADE,
    secondary_capacity_snapshot integer NOT NULL,
    is_active boolean NOT NULL DEFAULT true,
    merged_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    company_id uuid,
    branch_id uuid
);

CREATE TABLE IF NOT EXISTS product_preparation_steps (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id uuid NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    station_id uuid NOT NULL REFERENCES stations(id) ON DELETE CASCADE,
    step_order integer NOT NULL,
    is_active boolean NOT NULL DEFAULT true,
    company_id uuid,
    branch_id uuid
);

CREATE TABLE IF NOT EXISTS ingredient_alternatives (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    ingredient_product_id uuid NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    alternative_product_id uuid NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    priority integer NOT NULL DEFAULT 10,
    is_active boolean NOT NULL DEFAULT true,
    company_id uuid,
    branch_id uuid
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260705132939_EnterpriseFeatures100', '9.0.0'
WHERE NOT EXISTS (
    SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260705132939_EnterpriseFeatures100'
);
