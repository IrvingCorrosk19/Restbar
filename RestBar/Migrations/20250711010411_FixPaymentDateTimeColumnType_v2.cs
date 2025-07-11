using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class FixPaymentDateTimeColumnType_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Asegurar que la columna paid_at sea de tipo timestamp with time zone
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'payments' 
                        AND column_name = 'paid_at' 
                        AND data_type = 'timestamp without time zone'
                    ) THEN
                        ALTER TABLE payments ALTER COLUMN paid_at TYPE timestamp with time zone;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir el cambio si es necesario
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'payments' 
                        AND column_name = 'paid_at' 
                        AND data_type = 'timestamp with time zone'
                    ) THEN
                        ALTER TABLE payments ALTER COLUMN paid_at TYPE timestamp without time zone;
                    END IF;
                END $$;
            ");
        }
    }
}
