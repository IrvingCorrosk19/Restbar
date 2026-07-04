-- Enterprise closure migration 2026-07-04
ALTER TABLE orders ADD COLUMN IF NOT EXISTS discount_amount numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS discount_type varchar(20) NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS discount_reason varchar(500) NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS is_vip boolean NOT NULL DEFAULT false;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS priority int NOT NULL DEFAULT 0;
ALTER TABLE stations ADD COLUMN IF NOT EXISTS printer_name varchar(200) NULL;

CREATE TABLE IF NOT EXISTS recipes (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id uuid NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    name varchar(200) NOT NULL,
    is_active boolean NOT NULL DEFAULT true,
    "CompanyId" uuid NULL,
    "BranchId" uuid NULL,
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS recipe_lines (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    recipe_id uuid NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    ingredient_product_id uuid NOT NULL REFERENCES products(id),
    quantity numeric(18,4) NOT NULL,
    station_id uuid NULL REFERENCES stations(id)
);

CREATE TABLE IF NOT EXISTS inventory_movements (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id uuid NOT NULL REFERENCES products(id),
    station_id uuid NULL REFERENCES stations(id),
    "BranchId" uuid NULL,
    "CompanyId" uuid NULL,
    movement_type varchar(30) NOT NULL,
    quantity numeric(18,4) NOT NULL,
    stock_before numeric(18,4) NOT NULL,
    stock_after numeric(18,4) NOT NULL,
    reason varchar(500) NULL,
    reference varchar(100) NULL,
    user_id uuid NULL REFERENCES users(id),
    order_id uuid NULL REFERENCES orders(id),
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS stock_transfers (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id uuid NOT NULL REFERENCES products(id),
    from_station_id uuid NOT NULL REFERENCES stations(id),
    to_station_id uuid NOT NULL REFERENCES stations(id),
    "BranchId" uuid NULL,
    "CompanyId" uuid NULL,
    quantity numeric(18,4) NOT NULL,
    status varchar(20) NOT NULL DEFAULT 'Pending',
    requested_by_user_id uuid NULL REFERENCES users(id),
    approved_by_user_id uuid NULL REFERENCES users(id),
    notes varchar(500) NULL,
    requested_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at timestamptz NULL
);

CREATE TABLE IF NOT EXISTS shifts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES users(id),
    "BranchId" uuid NULL,
    "CompanyId" uuid NULL,
    started_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ended_at timestamptz NULL,
    is_active boolean NOT NULL DEFAULT true,
    notes varchar(500) NULL
);

CREATE TABLE IF NOT EXISTS shift_table_handoffs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    shift_id uuid NOT NULL REFERENCES shifts(id) ON DELETE CASCADE,
    table_id uuid NOT NULL REFERENCES tables(id),
    from_user_id uuid NOT NULL REFERENCES users(id),
    to_user_id uuid NOT NULL REFERENCES users(id),
    handed_off_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS payment_refunds (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id uuid NOT NULL REFERENCES payments(id),
    order_id uuid NOT NULL REFERENCES orders(id),
    amount numeric(18,2) NOT NULL,
    tip_amount numeric(18,2) NOT NULL DEFAULT 0,
    reason varchar(500) NULL,
    status varchar(20) NOT NULL DEFAULT 'Completed',
    processed_by_user_id uuid NULL REFERENCES users(id),
    approved_by_user_id uuid NULL REFERENCES users(id),
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS commission_rules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "CompanyId" uuid NULL,
    "BranchId" uuid NULL,
    role varchar(30) NULL,
    station_id uuid NULL REFERENCES stations(id),
    rate numeric(8,4) NOT NULL DEFAULT 0.05,
    is_active boolean NOT NULL DEFAULT true,
    description varchar(200) NULL
);

CREATE TABLE IF NOT EXISTS tip_allocations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id uuid NOT NULL REFERENCES payments(id),
    order_id uuid NOT NULL REFERENCES orders(id),
    user_id uuid NOT NULL REFERENCES users(id),
    amount numeric(18,2) NOT NULL,
    percentage numeric(8,4) NOT NULL,
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO commission_rules (id, rate, is_active, description)
SELECT gen_random_uuid(), 0.05, true, 'Comisión default meseros'
WHERE NOT EXISTS (SELECT 1 FROM commission_rules);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260704194500_EnterpriseClosure', '8.0.0')
ON CONFLICT DO NOTHING;
