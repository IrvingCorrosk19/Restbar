using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations;

public partial class RbUltimateMfaAndOrderNumberIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
ALTER TABLE users ADD COLUMN IF NOT EXISTS mfa_enabled boolean NOT NULL DEFAULT false;
ALTER TABLE users ADD COLUMN IF NOT EXISTS mfa_secret text NULL;

CREATE INDEX IF NOT EXISTS IX_orders_company_order_number
    ON public.orders (""CompanyId"", ""OrderNumber"");

ANALYZE public.orders;
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DROP INDEX IF EXISTS IX_orders_company_order_number;
ALTER TABLE users DROP COLUMN IF EXISTS mfa_secret;
ALTER TABLE users DROP COLUMN IF EXISTS mfa_enabled;
");
    }
}
