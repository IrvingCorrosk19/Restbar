using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations;

public partial class RbUltimateUniqueOrderNumber : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
-- Deduplicate OrderNumber within company before unique index (keep newest by OpenedAt).
WITH ranked AS (
  SELECT id,
         ROW_NUMBER() OVER (
           PARTITION BY ""CompanyId"", ""OrderNumber""
           ORDER BY opened_at DESC NULLS LAST, id DESC
         ) AS rn
  FROM public.orders
  WHERE ""OrderNumber"" IS NOT NULL AND ""CompanyId"" IS NOT NULL
)
UPDATE public.orders o
SET ""OrderNumber"" = o.""OrderNumber"" || '-' || SUBSTRING(o.id::text, 1, 8)
FROM ranked r
WHERE o.id = r.id AND r.rn > 1;

DROP INDEX IF EXISTS IX_orders_company_order_number;

CREATE UNIQUE INDEX IF NOT EXISTS UX_orders_company_ordernumber
    ON public.orders (""CompanyId"", ""OrderNumber"")
    WHERE ""CompanyId"" IS NOT NULL AND ""OrderNumber"" IS NOT NULL;

ANALYZE public.orders;
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DROP INDEX IF EXISTS UX_orders_company_ordernumber;
CREATE INDEX IF NOT EXISTS IX_orders_company_order_number
    ON public.orders (""CompanyId"", ""OrderNumber"");
");
    }
}
