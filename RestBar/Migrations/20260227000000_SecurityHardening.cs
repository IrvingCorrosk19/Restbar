using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <summary>
    /// Migración de seguridad — 2026-02-27
    /// Cambios:
    /// 1. Agrega columna idempotency_key a payments (unique where not null)
    /// 2. Agrega índice parcial único para una orden activa por mesa
    /// 3. Agrega valor 'inventarista' al enum user_role_enum de PostgreSQL
    /// </summary>
    public partial class SecurityHardening : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. IdempotencyKey en payments
            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Índice único parcial: solo claves no nulas (permite nulls múltiples)
            migrationBuilder.Sql(
                @"CREATE UNIQUE INDEX idx_payments_idempotency_key
                  ON payments (idempotency_key)
                  WHERE idempotency_key IS NOT NULL");

            // 2. Índice parcial único: una sola orden activa por mesa
            // Status NOT IN ('Completed', 'Cancelled') — PostgreSQL enum cast
            migrationBuilder.Sql(
                @"CREATE UNIQUE INDEX idx_unique_active_order_per_table
                  ON orders (table_id)
                  WHERE table_id IS NOT NULL
                    AND status::text NOT IN ('Completed', 'Cancelled')");

            // 3. Agregar 'inventarista' al enum de PostgreSQL
            // ALTER TYPE ADD VALUE no puede ejecutarse en una transacción — usar COMMIT previo
            migrationBuilder.Sql(
                @"DO $$
                  BEGIN
                    IF NOT EXISTS (
                      SELECT 1 FROM pg_enum
                      WHERE enumtypid = 'user_role_enum'::regtype
                        AND enumlabel = 'inventarista'
                    ) THEN
                      ALTER TYPE user_role_enum ADD VALUE 'inventarista';
                    END IF;
                  END
                  $$;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar índices
            migrationBuilder.DropIndex(
                name: "idx_payments_idempotency_key",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "idx_unique_active_order_per_table",
                table: "orders");

            // Eliminar columna idempotency_key
            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "payments");

            // NOTA: PostgreSQL no soporta eliminar valores de un enum (ALTER TYPE DROP VALUE).
            // El valor 'inventarista' permanece en el enum aunque no tenga usuarios asignados.
        }
    }
}
