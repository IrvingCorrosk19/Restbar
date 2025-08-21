﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RestBar.Models;

public partial class RestBarContext : DbContext
{
    public RestBarContext()
    {
    }

    public RestBarContext(DbContextOptions<RestBarContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Modifier> Modifiers { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderCancellationLog> OrderCancellationLogs { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<SplitPayment> SplitPayments { get; set; }

    public virtual DbSet<Table> Tables { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Station> Stations { get; set; }
    
    public virtual DbSet<UserAssignment> UserAssignments { get; set; }
    
    // Contabilidad

    
    // ✅ NUEVO: Movimientos de inventario
    public virtual DbSet<InventoryMovement> InventoryMovements { get; set; }
    

    
    // ✅ NUEVO: Transferencias
    public virtual DbSet<Transfer> Transfers { get; set; }
    public virtual DbSet<TransferItem> TransferItems { get; set; }
    
    // ✅ NUEVO: Ajustes Avanzados
    public virtual DbSet<SystemSettings> SystemSettings { get; set; }
    public virtual DbSet<Printer> Printers { get; set; }
    public virtual DbSet<Currency> Currencies { get; set; }
    public virtual DbSet<TaxRate> TaxRates { get; set; }
    public virtual DbSet<DiscountPolicy> DiscountPolicies { get; set; }
    public virtual DbSet<OperatingHours> OperatingHours { get; set; }
    public virtual DbSet<NotificationSettings> NotificationSettings { get; set; }
    public virtual DbSet<BackupSettings> BackupSettings { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
 => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=RestBar;Username=postgres;Password=Panama2020$");


    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    {
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        optionsBuilder.UseNpgsql("Host=dpg-d1pffq3uibrs73dna8cg-a;Port=5432;Database=restbar_omzm;Username=admin;Password=05i7r62GYkoupNL6Gmc8oFQVV8OotGjc;Ssl Mode=Require;Trust Server Certificate=true");
    //    }


    // optionsBuilder.UseNpgsql("Host=dpg-d1otd649c44c7380il7g-a;Port=5432;Database=restbar_92si;Username=admin;Password=KMTp5bPR7rlkSDizV1t5RlDJTEFIjAdr;Ssl Mode=Require;Trust Server Certificate=true");

    // => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=RestBar;Username=postgres;Password=Panama2020$");
    //optionsBuilder.UseNpgsql("Host=dpg-d1o87hjipnbc73elplu0-a.oregon-postgres.render.com;Port=5432;Database=restbar;Username=admin;Password=eBX4yz9XMPYuxU30aCWrhpX8JzD10bFy;Ssl Mode=Require;Trust Server Certificate=true");
    //optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=RestBar;Username=postgres;Password=Panama2020$");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("areas_pkey");

            entity.ToTable("areas");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");

            entity.HasOne(d => d.Branch).WithMany(p => p.Areas)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("areas_branch_id_fkey");
        });

        modelBuilder.HasPostgresEnum<UserRole>("user_role_enum");
        modelBuilder.HasPostgresEnum<MovementType>("movement_type_enum");
        modelBuilder.HasPostgresEnum<OrderStatus>("order_status_enum");
        modelBuilder.HasPostgresEnum<OrderType>("order_type_enum");
        modelBuilder.HasPostgresEnum<OrderItemStatus>("order_item_status_enum");
        modelBuilder.HasPostgresEnum<TableStatus>("table_status_enum");
        modelBuilder.HasPostgresEnum<JournalEntryStatus>("journal_entry_status_enum");
        modelBuilder.HasPostgresEnum<JournalEntryType>("journal_entry_type_enum");


        modelBuilder.HasPostgresEnum<TransferStatus>("transfer_status_enum");
        modelBuilder.HasPostgresEnum<AssignmentType>("assignment_type_enum");
        modelBuilder.HasPostgresEnum<AuditLogLevel>("audit_log_level_enum");
        modelBuilder.HasPostgresEnum<AuditAction>("audit_action_enum");
        modelBuilder.HasPostgresEnum<AuditModule>("audit_module_enum");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");
            entity.ToTable("users");
            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasColumnType("user_role_enum")
                .HasColumnName("role");

            entity.HasOne(d => d.Branch).WithMany(p => p.Users)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("users_branch_id_fkey");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("audit_logs_pkey");

            entity.ToTable("audit_logs");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.TableName)
                .HasMaxLength(100)
                .HasColumnName("table_name");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("timestamp");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            // ✅ NUEVAS COLUMNAS AGREGADAS
            entity.Property(e => e.CompanyId).HasColumnName("CompanyId");
            entity.Property(e => e.BranchId).HasColumnName("BranchId");
            entity.Property(e => e.LogLevel)
                .HasMaxLength(50)
                .HasColumnName("LogLevel");
            entity.Property(e => e.Module)
                .HasMaxLength(100)
                .HasColumnName("Module");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("Description");
            entity.Property(e => e.OldValues)
                .HasColumnType("text")
                .HasColumnName("OldValues");
            entity.Property(e => e.NewValues)
                .HasColumnType("text")
                .HasColumnName("NewValues");
            entity.Property(e => e.ErrorDetails)
                .HasColumnType("text")
                .HasColumnName("ErrorDetails");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(200)
                .HasColumnName("IpAddress");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("UserAgent");
            entity.Property(e => e.SessionId)
                .HasMaxLength(100)
                .HasColumnName("SessionId");
            entity.Property(e => e.IsError)
                .HasDefaultValue(false)
                .HasColumnName("IsError");
            entity.Property(e => e.ErrorCode).HasColumnName("ErrorCode");
            entity.Property(e => e.ExceptionType)
                .HasMaxLength(200)
                .HasColumnName("ExceptionType");
            entity.Property(e => e.StackTrace)
                .HasColumnType("text")
                .HasColumnName("StackTrace");

            // ✅ RELACIONES EXISTENTES
            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("audit_logs_user_id_fkey");

            // ✅ NUEVAS RELACIONES
            entity.HasOne(d => d.Company).WithMany()
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_audit_logs_companies_CompanyId");

            entity.HasOne(d => d.Branch).WithMany()
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_audit_logs_branches_BranchId");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("branches_pkey");

            entity.ToTable("branches");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");

            entity.HasOne(d => d.Company).WithMany(p => p.Branches)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("branches_company_id_fkey");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("companies_pkey");

            entity.ToTable("companies");

            entity.HasIndex(e => e.LegalId, "companies_legal_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.LegalId)
                .HasMaxLength(50)
                .HasColumnName("legal_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            
            // ✅ NUEVOS CAMPOS AGREGADOS
            entity.Property(e => e.TaxId)
                .HasColumnType("text")
                .HasColumnName("tax_id");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.Phone)
                .HasColumnType("text")
                .HasColumnName("phone");
            entity.Property(e => e.Email)
                .HasColumnType("text")
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            
            // ✅ NUEVO: Campo UpdatedAt
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");
            
            // ✅ NUEVOS CAMPOS: Tracking de usuarios
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customers_pkey");

            entity.ToTable("customers");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.LoyaltyPoints)
                .HasDefaultValue(0)
                .HasColumnName("loyalty_points");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inventory_pkey");

            entity.ToTable("inventory");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("last_updated");
            entity.Property(e => e.MinThreshold)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("min_threshold");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");
            
            // ✅ NUEVAS PROPIEDADES para las columnas adicionales en la BD
            entity.Property(e => e.Stock)
                .HasColumnName("stock");
            entity.Property(e => e.MinStock)
                .HasColumnName("min_stock");
            entity.Property(e => e.MaxStock)
                .HasColumnName("max_stock");

            entity.HasOne(d => d.Branch).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("inventory_branch_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("inventory_product_id_fkey");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoices_pkey");

            entity.ToTable("invoices");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Tax)
                .HasPrecision(10, 2)
                .HasColumnName("tax");
            entity.Property(e => e.Total)
                .HasPrecision(10, 2)
                .HasColumnName("total");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("invoices_customer_id_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("invoices_order_id_fkey");
        });

        modelBuilder.Entity<Modifier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("modifiers_pkey");

            entity.ToTable("modifiers");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ExtraCost)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("extra_cost");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.OrderId).HasColumnName("order_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("notifications_order_id_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ClosedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("closed_at")
                .HasConversion(
                    v => v,  // to db
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v // from db
                );
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.OpenedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("opened_at")
                .HasConversion(
                    v => v,  // to db
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v // from db
                );
            entity.Property(e => e.OrderType)
                .HasMaxLength(20)
                .HasConversion<string>()
                .HasColumnName("order_type");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v)
                )
                .HasColumnName("status");
            entity.Property(e => e.TableId).HasColumnName("table_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(50)
                .HasColumnName("OrderNumber");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("orders_customer_id_fkey");

            entity.HasOne(d => d.Table).WithMany(p => p.Orders)
                .HasForeignKey(d => d.TableId)
                .HasConstraintName("orders_table_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("orders_user_id_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_items_pkey");

            entity.ToTable("order_items");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Discount)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .HasColumnName("unit_price");
            entity.Property(e => e.PreparedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("prepared_at")
                .HasConversion(
                    v => v,  // to db
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v // from db
                );
            entity.Property(e => e.PreparedByStationId)
                .HasColumnName("prepared_by_station_id");
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasColumnName("status");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_items_order_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("order_items_product_id_fkey");

            entity.HasOne(d => d.PreparedByStation).WithMany()
                .HasForeignKey(d => d.PreparedByStationId)
                .HasConstraintName("order_items_prepared_by_station_id_fkey");
        });

        modelBuilder.Entity<OrderCancellationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_cancellation_logs_pkey");

            entity.ToTable("order_cancellation_logs");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SupervisorId).HasColumnName("supervisor_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("date");
            entity.Property(e => e.Products).HasColumnName("products");

            entity.HasOne(d => d.Order).WithMany()
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_cancellation_logs_order_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("order_cancellation_logs_user_id_fkey");

            entity.HasOne(d => d.Supervisor).WithMany()
                .HasForeignKey(d => d.SupervisorId)
                .HasConstraintName("order_cancellation_logs_supervisor_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.IsVoided)
                .HasDefaultValue(false)
                .HasColumnName("is_voided");
            entity.Property(e => e.Method)
                .HasMaxLength(30)
                .HasColumnName("method");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaidAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("paid_at");
            entity.Property(e => e.IsShared)
                .HasDefaultValue(false)
                .HasColumnName("is_shared");
            entity.Property(e => e.PayerName)
                .HasMaxLength(100)
                .HasColumnName("payer_name");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("COMPLETED")
                .HasColumnName("status");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("payments_order_id_fkey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.ToTable("products");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Cost)
                .HasPrecision(10, 2)
                .HasColumnName("cost");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.TaxRate)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0.07")
                .HasColumnName("tax_rate");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasDefaultValueSql("'unit'::character varying")
                .HasColumnName("unit");
            entity.Property(e => e.StationId).HasColumnName("station_id");

            entity.HasOne(d => d.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("products_category_id_fkey");

            entity.HasOne(d => d.Station)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("products_station_id_fkey");

            entity.HasMany(d => d.Modifiers).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductModifier",
                    r => r.HasOne<Modifier>().WithMany()
                        .HasForeignKey("ModifierId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("product_modifiers_modifier_id_fkey"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("product_modifiers_product_id_fkey"),
                    j =>
                    {
                        j.HasKey("ProductId", "ModifierId").HasName("product_modifiers_pkey");
                        j.ToTable("product_modifiers");
                        j.IndexerProperty<Guid>("ProductId").HasColumnName("product_id");
                        j.IndexerProperty<Guid>("ModifierId").HasColumnName("modifier_id");
                    });
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_categories_pkey");

            entity.ToTable("product_categories");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SplitPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("split_payments_pkey");

            entity.ToTable("split_payments");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.PersonName)
                .HasMaxLength(100)
                .HasColumnName("person_name");
            entity.Property(e => e.Method)
                .HasMaxLength(30)
                .HasColumnName("method");

            entity.HasOne(d => d.Payment).WithMany(p => p.SplitPayments)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("split_payments_payment_id_fkey");
        });

        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tables_pkey");

            entity.ToTable("tables");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'AVAILABLE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TableNumber)
                .HasMaxLength(10)
                .HasColumnName("table_number");

            entity.HasOne(d => d.Area).WithMany(p => p.Tables)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("tables_area_id_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("stations");
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.AreaId)
                .HasColumnName("area_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            // Configurar la relación con Area
            entity.HasOne(s => s.Area)
                .WithMany(a => a.Stations)
                .HasForeignKey(s => s.AreaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("user_assignments");
            
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.StationId).HasColumnName("station_id");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.AssignedTableIds)
                .HasColumnType("jsonb")
                .HasColumnName("assigned_table_ids");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("assigned_at");
            entity.Property(e => e.UnassignedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("unassigned_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_assignments_user_id_fkey");
            entity.HasOne(d => d.Station).WithMany()
                .HasForeignKey(d => d.StationId)
                .HasConstraintName("user_assignments_station_id_fkey");
            entity.HasOne(d => d.Area).WithMany()
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("user_assignments_area_id_fkey");
        });

        // Configuración para entidades de contabilidad
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("accounts");
            
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasColumnName("type");
            entity.Property(e => e.Category)
                .IsRequired()
                .HasColumnName("category");
            entity.Property(e => e.Nature)
                .IsRequired()
                .HasColumnName("nature");
            entity.Property(e => e.ParentAccountId)
                .HasColumnName("parent_account_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsSystem)
                .HasDefaultValue(false)
                .HasColumnName("is_system");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");

            // Relación con cuenta padre
            entity.HasOne(d => d.ParentAccount)
                .WithMany(p => p.SubAccounts)
                .HasForeignKey(d => d.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("journal_entries");
            
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.EntryNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("entry_number");
            entity.Property(e => e.EntryDate)
                .IsRequired()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("entry_date");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasColumnName("type");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.Reference)
                .HasMaxLength(500)
                .HasColumnName("reference");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(JournalEntryStatus.Draft)
                .HasColumnName("status");
            entity.Property(e => e.PostedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("posted_at");
            entity.Property(e => e.PostedBy)
                .HasMaxLength(100)
                .HasColumnName("posted_by");
            entity.Property(e => e.TotalDebit)
                .HasPrecision(18, 2)
                .HasColumnName("total_debit");
            entity.Property(e => e.TotalCredit)
                .HasPrecision(18, 2)
                .HasColumnName("total_credit");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
            entity.Property(e => e.OrderId)
                .HasColumnName("order_id");
            entity.Property(e => e.PaymentId)
                .HasColumnName("payment_id");

            // Relaciones opcionales
            entity.HasOne(d => d.Order)
                .WithMany()
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(d => d.Payment)
                .WithMany()
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<JournalEntryDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("journal_entry_details");
            
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.JournalEntryId)
                .IsRequired()
                .HasColumnName("journal_entry_id");
            entity.Property(e => e.AccountId)
                .IsRequired()
                .HasColumnName("account_id");
            entity.Property(e => e.DebitAmount)
                .HasPrecision(18, 2)
                .HasColumnName("debit_amount");
            entity.Property(e => e.CreditAmount)
                .HasPrecision(18, 2)
                .HasColumnName("credit_amount");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Reference)
                .HasMaxLength(100)
                .HasColumnName("reference");

            // Relaciones
            entity.HasOne(d => d.JournalEntry)
                .WithMany(p => p.Details)
                .HasForeignKey(d => d.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Account)
                .WithMany()
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ✅ NUEVO: Configuración de InventoryMovement
        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("inventory_movements");
            
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.InventoryId)
                .IsRequired()
                .HasColumnName("inventory_id");
            entity.Property(e => e.ProductId)
                .IsRequired()
                .HasColumnName("product_id");
            entity.Property(e => e.BranchId)
                .IsRequired()
                .HasColumnName("branch_id");
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.Property(e => e.Type)
                .HasColumnType("movement_type_enum.movement_type")
                .HasColumnName("type");
            entity.Property(e => e.Quantity)
                .HasPrecision(18, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.PreviousStock)
                .HasPrecision(18, 2)
                .HasColumnName("previous_stock");
            entity.Property(e => e.NewStock)
                .HasPrecision(18, 2)
                .HasColumnName("new_stock");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .HasColumnName("reason");
            entity.Property(e => e.Reference)
                .HasMaxLength(100)
                .HasColumnName("reference");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");

            // Relaciones
            entity.HasOne(d => d.Inventory)
                .WithMany(p => p.Movements)
                .HasForeignKey(d => d.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Branch)
                .WithMany()
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    /// <summary>
    /// Override de SaveChanges para aplicar tracking automático
    /// </summary>
    public override int SaveChanges()
    {
        ApplyTrackingChanges();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override de SaveChangesAsync para aplicar tracking automático
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTrackingChanges();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Aplica cambios de tracking automático a las entidades
    /// </summary>
    private void ApplyTrackingChanges()
    {
        var currentTime = DateTime.UtcNow;
        var currentUser = GetCurrentUser();

        // Procesar entidades agregadas
        var addedEntries = ChangeTracker.Entries<ITrackableEntity>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in addedEntries)
        {
            var entity = entry.Entity;
            entity.CreatedAt = currentTime;
            entity.UpdatedAt = currentTime;
            entity.CreatedBy = currentUser;
            entity.UpdatedBy = currentUser;
        }

        // Procesar entidades modificadas
        var modifiedEntries = ChangeTracker.Entries<ITrackableEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in modifiedEntries)
        {
            var entity = entry.Entity;
            entity.UpdatedAt = currentTime;
            entity.UpdatedBy = currentUser;
        }
    }

    /// <summary>
    /// Obtiene el usuario actual del contexto HTTP
    /// </summary>
    private string GetCurrentUser()
    {
        try
        {
            // Intentar obtener el usuario del contexto HTTP
            var httpContext = HttpContextAccessor?.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var email = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                    return email;

                var username = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(username))
                    return username;

                var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    return userId;
            }

            return "system";
        }
        catch
        {
            return "system";
        }
    }

    /// <summary>
    /// Accesor para el contexto HTTP (se debe configurar en Program.cs)
    /// </summary>
    public IHttpContextAccessor? HttpContextAccessor { get; set; }
}
