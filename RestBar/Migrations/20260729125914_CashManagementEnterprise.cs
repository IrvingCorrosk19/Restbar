using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class CashManagementEnterprise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "cash_session_id",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cash_session_id",
                table: "payment_refunds",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cash_audit_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cash_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cash_movement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    before_json = table.Column<string>(type: "text", nullable: true),
                    after_json = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    device_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    previous_event_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    event_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_audit_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cash_registers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    register_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    default_opening_float = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    requires_blind_close = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    variance_threshold_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    variance_threshold_percent = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    max_paid_out_without_approval = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    station_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    business_day_cutoff_hour = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_registers", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_registers_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cash_registers_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cash_registers_stations_station_id",
                        column: x => x.station_id,
                        principalTable: "stations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "cash_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cash_register_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shift_id = table.Column<Guid>(type: "uuid", nullable: true),
                    session_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    opened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    opened_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    closed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    supervisor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    manager_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    opening_float_declared = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    expected_cash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    counted_cash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    variance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    expected_card = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    expected_digital = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_sales = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_refunds = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_tips = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_paid_in = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_paid_out = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    blind_close_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    close_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reopened_from_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_sessions_cash_registers_cash_register_id",
                        column: x => x.cash_register_id,
                        principalTable: "cash_registers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cash_sessions_shifts_shift_id",
                        column: x => x.shift_id,
                        principalTable: "shifts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cash_sessions_users_opened_by_user_id",
                        column: x => x.opened_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cash_approvals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cash_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cash_movement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approval_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    threshold_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    actual_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_approvals", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_approvals_cash_sessions_cash_session_id",
                        column: x => x.cash_session_id,
                        principalTable: "cash_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cash_counts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cash_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    count_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    counted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    counted_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    witness_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_counted = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_blind = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_counts", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_counts_cash_sessions_cash_session_id",
                        column: x => x.cash_session_id,
                        principalTable: "cash_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cash_incidents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cash_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    resolved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolution_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_incidents", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_incidents_cash_sessions_cash_session_id",
                        column: x => x.cash_session_id,
                        principalTable: "cash_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cash_movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cash_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    movement_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_refund_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_movement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    comments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    performed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    authorized_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    previous_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    record_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    device_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    affects_cash_drawer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_movements", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_movements_cash_sessions_cash_session_id",
                        column: x => x.cash_session_id,
                        principalTable: "cash_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cash_movements_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "cash_z_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cash_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_json = table.Column<string>(type: "text", nullable: false),
                    generated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    generated_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    integrity_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_z_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_z_reports_cash_sessions_cash_session_id",
                        column: x => x.cash_session_id,
                        principalTable: "cash_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cash_count_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cash_count_id = table.Column<Guid>(type: "uuid", nullable: false),
                    denomination_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_count_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_count_lines_cash_counts_cash_count_id",
                        column: x => x.cash_count_id,
                        principalTable: "cash_counts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payments_cash_session",
                table: "payments",
                column: "cash_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_refunds_cash_session_id",
                table: "payment_refunds",
                column: "cash_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_approvals_cash_session_id",
                table: "cash_approvals",
                column: "cash_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_audit_session_created",
                table: "cash_audit_events",
                columns: new[] { "cash_session_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_cash_count_lines_cash_count_id",
                table: "cash_count_lines",
                column: "cash_count_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_counts_cash_session_id",
                table: "cash_counts",
                column: "cash_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_incidents_cash_session_id",
                table: "cash_incidents",
                column: "cash_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_movements_payment_id",
                table: "cash_movements",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_movements_session_created",
                table: "cash_movements",
                columns: new[] { "cash_session_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "UX_cash_movements_idempotency",
                table: "cash_movements",
                column: "idempotency_key",
                unique: true,
                filter: "\"idempotency_key\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_cash_movements_session_seq",
                table: "cash_movements",
                columns: new[] { "cash_session_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cash_registers_company_id",
                table: "cash_registers",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_registers_station_id",
                table: "cash_registers",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "UX_cash_registers_branch_code",
                table: "cash_registers",
                columns: new[] { "branch_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cash_sessions_branch_opened",
                table: "cash_sessions",
                columns: new[] { "branch_id", "opened_at" });

            migrationBuilder.CreateIndex(
                name: "IX_cash_sessions_opened_by_user_id",
                table: "cash_sessions",
                column: "opened_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_sessions_register_status",
                table: "cash_sessions",
                columns: new[] { "cash_register_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_cash_sessions_shift_id",
                table: "cash_sessions",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "UX_cash_z_reports_session",
                table: "cash_z_reports",
                column: "cash_session_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_refunds_cash_sessions_cash_session_id",
                table: "payment_refunds",
                column: "cash_session_id",
                principalTable: "cash_sessions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_cash_sessions_cash_session_id",
                table: "payments",
                column: "cash_session_id",
                principalTable: "cash_sessions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payment_refunds_cash_sessions_cash_session_id",
                table: "payment_refunds");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_cash_sessions_cash_session_id",
                table: "payments");

            migrationBuilder.DropTable(
                name: "cash_approvals");

            migrationBuilder.DropTable(
                name: "cash_audit_events");

            migrationBuilder.DropTable(
                name: "cash_count_lines");

            migrationBuilder.DropTable(
                name: "cash_incidents");

            migrationBuilder.DropTable(
                name: "cash_movements");

            migrationBuilder.DropTable(
                name: "cash_z_reports");

            migrationBuilder.DropTable(
                name: "cash_counts");

            migrationBuilder.DropTable(
                name: "cash_sessions");

            migrationBuilder.DropTable(
                name: "cash_registers");

            migrationBuilder.DropIndex(
                name: "IX_payments_cash_session",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payment_refunds_cash_session_id",
                table: "payment_refunds");

            migrationBuilder.DropColumn(
                name: "cash_session_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "cash_session_id",
                table: "payment_refunds");
        }
    }
}
