using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountingTablesWithTimezone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Accounts_ParentAccountId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_orders_OrderId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_payments_PaymentId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntryDetails_Accounts_AccountId",
                table: "JournalEntryDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntryDetails_JournalEntries_JournalEntryId",
                table: "JournalEntryDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JournalEntryDetails",
                table: "JournalEntryDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JournalEntries",
                table: "JournalEntries");

            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "accounts");

            migrationBuilder.RenameTable(
                name: "JournalEntryDetails",
                newName: "journal_entry_details");

            migrationBuilder.RenameTable(
                name: "JournalEntries",
                newName: "journal_entries");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "accounts",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Nature",
                table: "accounts",
                newName: "nature");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "accounts",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "accounts",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "accounts",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "accounts",
                newName: "category");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "accounts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "accounts",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "accounts",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "ParentAccountId",
                table: "accounts",
                newName: "parent_account_id");

            migrationBuilder.RenameColumn(
                name: "IsSystem",
                table: "accounts",
                newName: "is_system");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "accounts",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "accounts",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "accounts",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_ParentAccountId",
                table: "accounts",
                newName: "IX_accounts_parent_account_id");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "journal_entry_details",
                newName: "reference");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "journal_entry_details",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "journal_entry_details",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "journal_entry_details",
                newName: "journal_entry_id");

            migrationBuilder.RenameColumn(
                name: "DebitAmount",
                table: "journal_entry_details",
                newName: "debit_amount");

            migrationBuilder.RenameColumn(
                name: "CreditAmount",
                table: "journal_entry_details",
                newName: "credit_amount");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "journal_entry_details",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "IX_JournalEntryDetails_JournalEntryId",
                table: "journal_entry_details",
                newName: "IX_journal_entry_details_journal_entry_id");

            migrationBuilder.RenameIndex(
                name: "IX_JournalEntryDetails_AccountId",
                table: "journal_entry_details",
                newName: "IX_journal_entry_details_account_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "journal_entries",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "journal_entries",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "journal_entries",
                newName: "reference");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "journal_entries",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "journal_entries",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "journal_entries",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "journal_entries",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TotalDebit",
                table: "journal_entries",
                newName: "total_debit");

            migrationBuilder.RenameColumn(
                name: "TotalCredit",
                table: "journal_entries",
                newName: "total_credit");

            migrationBuilder.RenameColumn(
                name: "PostedBy",
                table: "journal_entries",
                newName: "posted_by");

            migrationBuilder.RenameColumn(
                name: "PostedAt",
                table: "journal_entries",
                newName: "posted_at");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "journal_entries",
                newName: "payment_id");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "journal_entries",
                newName: "order_id");

            migrationBuilder.RenameColumn(
                name: "EntryNumber",
                table: "journal_entries",
                newName: "entry_number");

            migrationBuilder.RenameColumn(
                name: "EntryDate",
                table: "journal_entries",
                newName: "entry_date");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "journal_entries",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "journal_entries",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_JournalEntries_PaymentId",
                table: "journal_entries",
                newName: "IX_journal_entries_payment_id");

            migrationBuilder.RenameIndex(
                name: "IX_JournalEntries_OrderId",
                table: "journal_entries",
                newName: "IX_journal_entries_order_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "accounts",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                table: "accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_system",
                table: "accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "accounts",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "journal_entry_details",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<decimal>(
                name: "debit_amount",
                table: "journal_entry_details",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "credit_amount",
                table: "journal_entry_details",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "journal_entries",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "journal_entries",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                table: "journal_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "total_debit",
                table: "journal_entries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_credit",
                table: "journal_entries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "posted_by",
                table: "journal_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "journal_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "journal_entries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_accounts",
                table: "accounts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journal_entry_details",
                table: "journal_entry_details",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journal_entries",
                table: "journal_entries",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_accounts_parent_account_id",
                table: "accounts",
                column: "parent_account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entries_orders_order_id",
                table: "journal_entries",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entries_payments_payment_id",
                table: "journal_entries",
                column: "payment_id",
                principalTable: "payments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entry_details_accounts_account_id",
                table: "journal_entry_details",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entry_details_journal_entries_journal_entry_id",
                table: "journal_entry_details",
                column: "journal_entry_id",
                principalTable: "journal_entries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accounts_accounts_parent_account_id",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entries_orders_order_id",
                table: "journal_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entries_payments_payment_id",
                table: "journal_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entry_details_accounts_account_id",
                table: "journal_entry_details");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entry_details_journal_entries_journal_entry_id",
                table: "journal_entry_details");

            migrationBuilder.DropPrimaryKey(
                name: "PK_accounts",
                table: "accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journal_entry_details",
                table: "journal_entry_details");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journal_entries",
                table: "journal_entries");

            migrationBuilder.RenameTable(
                name: "accounts",
                newName: "Accounts");

            migrationBuilder.RenameTable(
                name: "journal_entry_details",
                newName: "JournalEntryDetails");

            migrationBuilder.RenameTable(
                name: "journal_entries",
                newName: "JournalEntries");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Accounts",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "nature",
                table: "Accounts",
                newName: "Nature");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Accounts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Accounts",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "Accounts",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "Accounts",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Accounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "Accounts",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Accounts",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "parent_account_id",
                table: "Accounts",
                newName: "ParentAccountId");

            migrationBuilder.RenameColumn(
                name: "is_system",
                table: "Accounts",
                newName: "IsSystem");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Accounts",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "Accounts",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Accounts",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_parent_account_id",
                table: "Accounts",
                newName: "IX_Accounts_ParentAccountId");

            migrationBuilder.RenameColumn(
                name: "reference",
                table: "JournalEntryDetails",
                newName: "Reference");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "JournalEntryDetails",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "JournalEntryDetails",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "journal_entry_id",
                table: "JournalEntryDetails",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "debit_amount",
                table: "JournalEntryDetails",
                newName: "DebitAmount");

            migrationBuilder.RenameColumn(
                name: "credit_amount",
                table: "JournalEntryDetails",
                newName: "CreditAmount");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "JournalEntryDetails",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entry_details_journal_entry_id",
                table: "JournalEntryDetails",
                newName: "IX_JournalEntryDetails_JournalEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entry_details_account_id",
                table: "JournalEntryDetails",
                newName: "IX_JournalEntryDetails_AccountId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "JournalEntries",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "JournalEntries",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "reference",
                table: "JournalEntries",
                newName: "Reference");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "JournalEntries",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "JournalEntries",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "JournalEntries",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "JournalEntries",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "total_debit",
                table: "JournalEntries",
                newName: "TotalDebit");

            migrationBuilder.RenameColumn(
                name: "total_credit",
                table: "JournalEntries",
                newName: "TotalCredit");

            migrationBuilder.RenameColumn(
                name: "posted_by",
                table: "JournalEntries",
                newName: "PostedBy");

            migrationBuilder.RenameColumn(
                name: "posted_at",
                table: "JournalEntries",
                newName: "PostedAt");

            migrationBuilder.RenameColumn(
                name: "payment_id",
                table: "JournalEntries",
                newName: "PaymentId");

            migrationBuilder.RenameColumn(
                name: "order_id",
                table: "JournalEntries",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "entry_number",
                table: "JournalEntries",
                newName: "EntryNumber");

            migrationBuilder.RenameColumn(
                name: "entry_date",
                table: "JournalEntries",
                newName: "EntryDate");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "JournalEntries",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "JournalEntries",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entries_payment_id",
                table: "JournalEntries",
                newName: "IX_JournalEntries_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entries_order_id",
                table: "JournalEntries",
                newName: "IX_JournalEntries_OrderId");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Accounts",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Accounts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Accounts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "JournalEntryDetails",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<decimal>(
                name: "DebitAmount",
                table: "JournalEntryDetails",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "CreditAmount",
                table: "JournalEntryDetails",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "JournalEntries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "JournalEntries",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "JournalEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalDebit",
                table: "JournalEntries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalCredit",
                table: "JournalEntries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "PostedBy",
                table: "JournalEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "JournalEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "JournalEntries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JournalEntryDetails",
                table: "JournalEntryDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JournalEntries",
                table: "JournalEntries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Accounts_ParentAccountId",
                table: "Accounts",
                column: "ParentAccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_orders_OrderId",
                table: "JournalEntries",
                column: "OrderId",
                principalTable: "orders",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_payments_PaymentId",
                table: "JournalEntries",
                column: "PaymentId",
                principalTable: "payments",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntryDetails_Accounts_AccountId",
                table: "JournalEntryDetails",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntryDetails_JournalEntries_JournalEntryId",
                table: "JournalEntryDetails",
                column: "JournalEntryId",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
