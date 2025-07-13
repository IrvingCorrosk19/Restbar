using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using RestBar.Models;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:assignment_type_enum.assignment_type", "station,area,tables")
                .Annotation("Npgsql:Enum:audit_action_enum.audit_action", "create,update,delete,login,logout,error,system,security,data_export,data_import,backup,restore,configuration_change")
                .Annotation("Npgsql:Enum:audit_log_level_enum.audit_log_level", "info,warning,error,critical")
                .Annotation("Npgsql:Enum:audit_module_enum.audit_module", "user,order,product,payment,accounting,report,system,security,backup,configuration")
                .Annotation("Npgsql:Enum:order_item_status_enum.order_item_status", "pending,preparing,ready,served,cancelled")
                .Annotation("Npgsql:Enum:order_status_enum.order_status", "pending,sent_to_kitchen,preparing,ready,ready_to_pay,served,cancelled,completed")
                .Annotation("Npgsql:Enum:order_type_enum.order_type", "dine_in,take_out,delivery")
                .Annotation("Npgsql:Enum:table_status_enum.table_status", "disponible,ocupada,reservada,en_espera,atendida,en_preparacion,servida,para_pago,pagada,bloqueada")
                .Annotation("Npgsql:Enum:user_role_enum", "accountant,admin,bartender,cashier,chef,manager,superadmin,supervisor,support,waiter")
                .Annotation("Npgsql:Enum:user_role_enum.user_role", "superadmin,admin,manager,supervisor,waiter,cashier,chef,bartender,accountant,support");

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    legal_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tax_id = table.Column<string>(type: "text", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "text", maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "text", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "text", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("companies_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_categories_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "BackupSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BackupType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BackupPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RetentionDays = table.Column<int>(type: "integer", nullable: true),
                    Schedule = table.Column<string>(type: "text", nullable: true),
                    LastBackupDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackupSettings_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("branches_pkey", x => x.id);
                    table.ForeignKey(
                        name: "branches_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Currencies_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "DiscountPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    MaximumDiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MinimumOrderAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountPolicies_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "email_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    placeholders = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_email_templates_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "NotificationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Configuration = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationSettings_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "OperatingHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatingHours_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSettings_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TaxCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxRates_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "areas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("areas_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_areas_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "areas_branch_id_fkey",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("categories_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_categories_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    loyalty_points = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    notes = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("customers_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_customers_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_customers_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "modifiers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    extra_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("modifiers_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_modifiers_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_modifiers_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<UserRole>(type: "user_role_enum", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                    table.ForeignKey(
                        name: "users_branch_id_fkey",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "stations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stations", x => x.id);
                    table.ForeignKey(
                        name: "FK_stations_areas_area_id",
                        column: x => x.area_id,
                        principalTable: "areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_stations_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stations_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tables",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    table_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'Disponible'::character varying"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tables_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_tables_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tables_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "tables_area_id_fkey",
                        column: x => x.area_id,
                        principalTable: "areas",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "text", nullable: false),
                    table_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LogLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    ErrorDetails = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsError = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ErrorCode = table.Column<int>(type: "integer", nullable: true),
                    ExceptionType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_logs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_logs_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_audit_logs_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "audit_logs_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 10, scale: 2, nullable: false),
                    cost = table.Column<decimal>(type: "numeric(18,2)", precision: 10, scale: 2, nullable: true),
                    tax_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true, defaultValueSql: "0.07"),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValueSql: "'unit'::character varying"),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    station_id = table.Column<Guid>(type: "uuid", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductCategoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("products_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_products_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_products_product_categories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "product_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "products_category_id_fkey",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "products_station_id_fkey",
                        column: x => x.station_id,
                        principalTable: "stations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    station_id = table.Column<Guid>(type: "uuid", nullable: true),
                    area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_table_ids = table.Column<List<Guid>>(type: "jsonb", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    unassigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_assignments_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_user_assignments_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_assignments_area_id_fkey",
                        column: x => x.area_id,
                        principalTable: "areas",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_assignments_station_id_fkey",
                        column: x => x.station_id,
                        principalTable: "stations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_assignments_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    OrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    table_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    opened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("orders_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_orders_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "orders_customer_id_fkey",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "orders_table_id_fkey",
                        column: x => x.table_id,
                        principalTable: "tables",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "orders_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "product_modifiers",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modifier_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_modifiers_pkey", x => new { x.product_id, x.modifier_id });
                    table.ForeignKey(
                        name: "product_modifiers_modifier_id_fkey",
                        column: x => x.modifier_id,
                        principalTable: "modifiers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "product_modifiers_product_id_fkey",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    invoice_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("invoices_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoices_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoices_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "invoices_customer_id_fkey",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "invoices_order_id_fkey",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("notifications_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_notifications_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "notifications_order_id_fkey",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_cancellation_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    supervisor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    products = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_cancellation_logs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "order_cancellation_logs_order_id_fkey",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "order_cancellation_logs_supervisor_id_fkey",
                        column: x => x.supervisor_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "order_cancellation_logs_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_voided = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_shared = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    payer_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "COMPLETED"),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("payments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_payments_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "payments_order_id_fkey",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("persons_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_persons_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_persons_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_persons_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "split_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    person_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("split_payments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_split_payments_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_split_payments_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "split_payments_payment_id_fkey",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    prepared_by_station_id = table.Column<Guid>(type: "uuid", nullable: true),
                    prepared_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    kitchen_status = table.Column<string>(type: "text", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    assigned_to_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_to_person_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_shared = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_items_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_order_items_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_order_items_persons_assigned_to_person_id",
                        column: x => x.assigned_to_person_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_order_items_stations_StationId",
                        column: x => x.StationId,
                        principalTable: "stations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "order_items_order_id_fkey",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "order_items_prepared_by_station_id_fkey",
                        column: x => x.prepared_by_station_id,
                        principalTable: "stations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "order_items_product_id_fkey",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_areas_branch_id",
                table: "areas",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_areas_company_id",
                table: "areas",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_BranchId",
                table: "audit_logs",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CompanyId",
                table: "audit_logs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_user_id",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_BackupSettings_CompanyId",
                table: "BackupSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_branches_company_id",
                table: "branches",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_branch_id",
                table: "categories",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_CompanyId",
                table: "categories",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "companies_legal_id_key",
                table: "companies",
                column: "legal_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CompanyId",
                table: "Currencies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_BranchId",
                table: "customers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_CompanyId",
                table: "customers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountPolicies_CompanyId",
                table: "DiscountPolicies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_email_templates_company_id",
                table: "email_templates",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_BranchId",
                table: "invoices",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_CompanyId",
                table: "invoices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_customer_id",
                table: "invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_order_id",
                table: "invoices",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_modifiers_BranchId",
                table: "modifiers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_modifiers_CompanyId",
                table: "modifiers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_BranchId",
                table: "notifications",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_CompanyId",
                table: "notifications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_order_id",
                table: "notifications",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_CompanyId",
                table: "NotificationSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatingHours_CompanyId",
                table: "OperatingHours",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_order_cancellation_logs_order_id",
                table: "order_cancellation_logs",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_cancellation_logs_supervisor_id",
                table: "order_cancellation_logs",
                column: "supervisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_cancellation_logs_user_id",
                table: "order_cancellation_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_assigned_to_person_id",
                table: "order_items",
                column: "assigned_to_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_BranchId",
                table: "order_items",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_CompanyId",
                table: "order_items",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_prepared_by_station_id",
                table: "order_items",
                column: "prepared_by_station_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_StationId",
                table: "order_items",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_BranchId",
                table: "orders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_CompanyId",
                table: "orders",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_customer_id",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_table_id",
                table: "orders",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_BranchId",
                table: "payments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_CompanyId",
                table: "payments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_order_id",
                table: "payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_persons_branch_id",
                table: "persons",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_persons_company_id",
                table: "persons",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_persons_order_id",
                table: "persons",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_modifiers_modifier_id",
                table: "product_modifiers",
                column: "modifier_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_branch_id",
                table: "products",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_company_id",
                table: "products",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_ProductCategoryId",
                table: "products",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_products_station_id",
                table: "products",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_split_payments_BranchId",
                table: "split_payments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_split_payments_CompanyId",
                table: "split_payments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_split_payments_payment_id",
                table: "split_payments",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_stations_area_id",
                table: "stations",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "IX_stations_branch_id",
                table: "stations",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_stations_company_id",
                table: "stations",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_CompanyId",
                table: "SystemSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_tables_area_id",
                table: "tables",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "IX_tables_branch_id",
                table: "tables",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_tables_company_id",
                table: "tables",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_CompanyId",
                table: "TaxRates",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_user_assignments_area_id",
                table: "user_assignments",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_assignments_BranchId",
                table: "user_assignments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_user_assignments_CompanyId",
                table: "user_assignments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_user_assignments_station_id",
                table: "user_assignments",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_assignments_user_id",
                table: "user_assignments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_branch_id",
                table: "users",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "users_email_key",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "BackupSettings");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "DiscountPolicies");

            migrationBuilder.DropTable(
                name: "email_templates");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "OperatingHours");

            migrationBuilder.DropTable(
                name: "order_cancellation_logs");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "product_modifiers");

            migrationBuilder.DropTable(
                name: "split_payments");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "TaxRates");

            migrationBuilder.DropTable(
                name: "user_assignments");

            migrationBuilder.DropTable(
                name: "persons");

            migrationBuilder.DropTable(
                name: "modifiers");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "product_categories");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "stations");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "tables");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "areas");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "companies");
        }
    }
}
