using Microsoft.EntityFrameworkCore;

namespace RestBar.Models;

public partial class RestBarContext
{
    public virtual DbSet<Supplier> Suppliers { get; set; }
    public virtual DbSet<SupplierContact> SupplierContacts { get; set; }
    public virtual DbSet<SupplierProduct> SupplierProducts { get; set; }
    public virtual DbSet<PurchaseRequest> PurchaseRequests { get; set; }
    public virtual DbSet<PurchaseRequestLine> PurchaseRequestLines { get; set; }
    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public virtual DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
    public virtual DbSet<GoodsReceipt> GoodsReceipts { get; set; }
    public virtual DbSet<GoodsReceiptLine> GoodsReceiptLines { get; set; }
    public virtual DbSet<PurchaseApproval> PurchaseApprovals { get; set; }
    public virtual DbSet<SupplierScore> SupplierScores { get; set; }
    public virtual DbSet<PriceHistory> PriceHistories { get; set; }
    public virtual DbSet<ProcurementAuditEvent> ProcurementAuditEvents { get; set; }

    partial void ConfigureProcurementModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>(e =>
        {
            e.ToTable("suppliers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.Code).HasMaxLength(30).HasColumnName("code");
            e.Property(x => x.Name).HasMaxLength(200).HasColumnName("name");
            e.Property(x => x.TaxId).HasMaxLength(50).HasColumnName("tax_id");
            e.Property(x => x.Email).HasMaxLength(200).HasColumnName("email");
            e.Property(x => x.Phone).HasMaxLength(50).HasColumnName("phone");
            e.Property(x => x.PaymentTermsDays).HasColumnName("payment_terms_days").HasDefaultValue(30);
            e.Property(x => x.LeadTimeDays).HasColumnName("lead_time_days").HasDefaultValue(2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasColumnName("status");
            e.Property(x => x.IsPreferred).HasColumnName("is_preferred");
            e.Property(x => x.ScoreOverall).HasPrecision(5, 2).HasColumnName("score_overall");
            e.Property(x => x.Notes).HasMaxLength(1000).HasColumnName("notes");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.Property(x => x.CreatedBy).HasMaxLength(256).HasColumnName("created_by");
            e.Property(x => x.UpdatedBy).HasMaxLength(256).HasColumnName("updated_by");
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasDatabaseName("UX_suppliers_company_code");
            e.HasIndex(x => new { x.CompanyId, x.Status }).HasDatabaseName("IX_suppliers_company_status");
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
        });

        modelBuilder.Entity<SupplierContact>(e =>
        {
            e.ToTable("supplier_contacts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.SupplierId).HasColumnName("supplier_id");
            e.Property(x => x.Name).HasMaxLength(120).HasColumnName("name");
            e.Property(x => x.Role).HasMaxLength(80).HasColumnName("role");
            e.Property(x => x.Email).HasMaxLength(200).HasColumnName("email");
            e.Property(x => x.Phone).HasMaxLength(50).HasColumnName("phone");
            e.Property(x => x.IsPrimary).HasColumnName("is_primary");
            e.HasOne(x => x.Supplier).WithMany(s => s.Contacts).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupplierProduct>(e =>
        {
            e.ToTable("supplier_products");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.SupplierId).HasColumnName("supplier_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.SupplierSku).HasMaxLength(60).HasColumnName("supplier_sku");
            e.Property(x => x.PackSize).HasPrecision(18, 4).HasColumnName("pack_size");
            e.Property(x => x.UnitOfMeasure).HasMaxLength(20).HasColumnName("unit_of_measure");
            e.Property(x => x.AgreedUnitPrice).HasPrecision(18, 4).HasColumnName("agreed_unit_price");
            e.Property(x => x.CurrencyCode).HasMaxLength(3).HasColumnName("currency_code");
            e.Property(x => x.MinOrderQty).HasPrecision(18, 4).HasColumnName("min_order_qty");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.LeadTimeOverrideDays).HasColumnName("lead_time_override_days");
            e.HasIndex(x => new { x.SupplierId, x.ProductId }).IsUnique().HasDatabaseName("UX_supplier_products");
            e.HasOne(x => x.Supplier).WithMany(s => s.Products).HasForeignKey(x => x.SupplierId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        modelBuilder.Entity<PurchaseRequest>(e =>
        {
            e.ToTable("purchase_requests");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.RequestNumber).HasMaxLength(40).HasColumnName("request_number");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasColumnName("status");
            e.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id");
            e.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id");
            e.Property(x => x.Notes).HasMaxLength(1000).HasColumnName("notes");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
            e.Property(x => x.ResolvedAt).HasColumnName("resolved_at");
            e.HasIndex(x => new { x.CompanyId, x.RequestNumber }).IsUnique().HasDatabaseName("UX_purchase_requests_number");
            e.HasIndex(x => new { x.BranchId, x.Status }).HasDatabaseName("IX_purchase_requests_branch_status");
        });

        modelBuilder.Entity<PurchaseRequestLine>(e =>
        {
            e.ToTable("purchase_request_lines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.PurchaseRequestId).HasColumnName("purchase_request_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.Quantity).HasPrecision(18, 4).HasColumnName("quantity");
            e.Property(x => x.UnitOfMeasure).HasMaxLength(20).HasColumnName("unit_of_measure");
            e.Property(x => x.PreferredSupplierId).HasColumnName("preferred_supplier_id");
            e.Property(x => x.EstimatedUnitCost).HasPrecision(18, 4).HasColumnName("estimated_unit_cost");
            e.Property(x => x.StationId).HasColumnName("station_id");
            e.Property(x => x.Notes).HasMaxLength(500).HasColumnName("notes");
            e.HasOne(x => x.PurchaseRequest).WithMany(r => r.Lines).HasForeignKey(x => x.PurchaseRequestId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        modelBuilder.Entity<PurchaseOrder>(e =>
        {
            e.ToTable("purchase_orders");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.SupplierId).HasColumnName("supplier_id");
            e.Property(x => x.PurchaseRequestId).HasColumnName("purchase_request_id");
            e.Property(x => x.PoNumber).HasMaxLength(40).HasColumnName("po_number");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).HasColumnName("status");
            e.Property(x => x.OrderDate).HasColumnName("order_date");
            e.Property(x => x.ExpectedDelivery).HasColumnName("expected_delivery");
            e.Property(x => x.Subtotal).HasPrecision(18, 2).HasColumnName("subtotal");
            e.Property(x => x.Tax).HasPrecision(18, 2).HasColumnName("tax");
            e.Property(x => x.Total).HasPrecision(18, 2).HasColumnName("total");
            e.Property(x => x.CurrencyCode).HasMaxLength(3).HasColumnName("currency_code");
            e.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id");
            e.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id");
            e.Property(x => x.SentAt).HasColumnName("sent_at");
            e.Property(x => x.ClosedAt).HasColumnName("closed_at");
            e.Property(x => x.Notes).HasMaxLength(1000).HasColumnName("notes");
            e.Property(x => x.RowVersion).IsRowVersion().HasColumnName("row_version");
            e.HasIndex(x => new { x.CompanyId, x.PoNumber }).IsUnique().HasDatabaseName("UX_purchase_orders_number");
            e.HasIndex(x => new { x.BranchId, x.Status }).HasDatabaseName("IX_purchase_orders_branch_status");
            e.HasIndex(x => new { x.SupplierId, x.Status }).HasDatabaseName("IX_purchase_orders_supplier_status");
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId);
        });

        modelBuilder.Entity<PurchaseOrderLine>(e =>
        {
            e.ToTable("purchase_order_lines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.PurchaseOrderId).HasColumnName("purchase_order_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.SupplierProductId).HasColumnName("supplier_product_id");
            e.Property(x => x.QuantityOrdered).HasPrecision(18, 4).HasColumnName("quantity_ordered");
            e.Property(x => x.QuantityReceived).HasPrecision(18, 4).HasColumnName("quantity_received");
            e.Property(x => x.UnitPrice).HasPrecision(18, 4).HasColumnName("unit_price");
            e.Property(x => x.LineTotal).HasPrecision(18, 2).HasColumnName("line_total");
            e.Property(x => x.UnitOfMeasure).HasMaxLength(20).HasColumnName("unit_of_measure");
            e.Property(x => x.StationId).HasColumnName("station_id");
            e.Property(x => x.Notes).HasMaxLength(500).HasColumnName("notes");
            e.HasOne(x => x.PurchaseOrder).WithMany(o => o.Lines).HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        modelBuilder.Entity<GoodsReceipt>(e =>
        {
            e.ToTable("goods_receipts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.PurchaseOrderId).HasColumnName("purchase_order_id");
            e.Property(x => x.ReceiptNumber).HasMaxLength(40).HasColumnName("receipt_number");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasColumnName("status");
            e.Property(x => x.ReceivedAt).HasColumnName("received_at");
            e.Property(x => x.ReceivedByUserId).HasColumnName("received_by_user_id");
            e.Property(x => x.SupervisedByUserId).HasColumnName("supervised_by_user_id");
            e.Property(x => x.TemperatureOk).HasColumnName("temperature_ok");
            e.Property(x => x.Notes).HasMaxLength(1000).HasColumnName("notes");
            e.HasIndex(x => new { x.CompanyId, x.ReceiptNumber }).IsUnique().HasDatabaseName("UX_goods_receipts_number");
            e.HasOne(x => x.PurchaseOrder).WithMany(o => o.Receipts).HasForeignKey(x => x.PurchaseOrderId);
        });

        modelBuilder.Entity<GoodsReceiptLine>(e =>
        {
            e.ToTable("goods_receipt_lines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.GoodsReceiptId).HasColumnName("goods_receipt_id");
            e.Property(x => x.PurchaseOrderLineId).HasColumnName("purchase_order_line_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.QtyOrdered).HasPrecision(18, 4).HasColumnName("qty_ordered");
            e.Property(x => x.QtyReceived).HasPrecision(18, 4).HasColumnName("qty_received");
            e.Property(x => x.QtyAccepted).HasPrecision(18, 4).HasColumnName("qty_accepted");
            e.Property(x => x.QtyRejected).HasPrecision(18, 4).HasColumnName("qty_rejected");
            e.Property(x => x.Disposition).HasConversion<string>().HasMaxLength(20).HasColumnName("disposition");
            e.Property(x => x.UnitCost).HasPrecision(18, 4).HasColumnName("unit_cost");
            e.Property(x => x.LotNumber).HasMaxLength(60).HasColumnName("lot_number");
            e.Property(x => x.ExpiryDate).HasColumnName("expiry_date");
            e.Property(x => x.Notes).HasMaxLength(500).HasColumnName("notes");
            e.HasOne(x => x.GoodsReceipt).WithMany(r => r.Lines).HasForeignKey(x => x.GoodsReceiptId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.PurchaseOrderLine).WithMany().HasForeignKey(x => x.PurchaseOrderLineId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        modelBuilder.Entity<PurchaseApproval>(e =>
        {
            e.ToTable("purchase_approvals");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.EntityType).HasMaxLength(30).HasColumnName("entity_type");
            e.Property(x => x.EntityId).HasColumnName("entity_id");
            e.Property(x => x.ApprovalType).HasConversion<string>().HasMaxLength(20).HasColumnName("approval_type");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasColumnName("status");
            e.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id");
            e.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id");
            e.Property(x => x.ThresholdAmount).HasPrecision(18, 2).HasColumnName("threshold_amount");
            e.Property(x => x.ActualAmount).HasPrecision(18, 2).HasColumnName("actual_amount");
            e.Property(x => x.Reason).HasMaxLength(500).HasColumnName("reason");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.ResolvedAt).HasColumnName("resolved_at");
            e.HasIndex(x => new { x.EntityType, x.EntityId }).HasDatabaseName("IX_purchase_approvals_entity");
        });

        modelBuilder.Entity<SupplierScore>(e =>
        {
            e.ToTable("supplier_scores");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.SupplierId).HasColumnName("supplier_id");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.PeriodStart).HasColumnName("period_start");
            e.Property(x => x.PeriodEnd).HasColumnName("period_end");
            e.Property(x => x.PriceScore).HasPrecision(5, 2).HasColumnName("price_score");
            e.Property(x => x.OtifScore).HasPrecision(5, 2).HasColumnName("otif_score");
            e.Property(x => x.QualityScore).HasPrecision(5, 2).HasColumnName("quality_score");
            e.Property(x => x.ReliabilityScore).HasPrecision(5, 2).HasColumnName("reliability_score");
            e.Property(x => x.OverallScore).HasPrecision(5, 2).HasColumnName("overall_score");
            e.Property(x => x.ComputedAt).HasColumnName("computed_at");
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId);
        });

        modelBuilder.Entity<PriceHistory>(e =>
        {
            e.ToTable("price_history");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.SupplierId).HasColumnName("supplier_id");
            e.Property(x => x.UnitCost).HasPrecision(18, 4).HasColumnName("unit_cost");
            e.Property(x => x.Source).HasConversion<string>().HasMaxLength(20).HasColumnName("source");
            e.Property(x => x.GoodsReceiptId).HasColumnName("goods_receipt_id");
            e.Property(x => x.RecordedAt).HasColumnName("recorded_at");
            e.HasIndex(x => new { x.ProductId, x.RecordedAt }).HasDatabaseName("IX_price_history_product_date");
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        modelBuilder.Entity<ProcurementAuditEvent>(e =>
        {
            e.ToTable("procurement_audit_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.EntityType).HasMaxLength(40).HasColumnName("entity_type");
            e.Property(x => x.EntityId).HasColumnName("entity_id");
            e.Property(x => x.EventType).HasMaxLength(80).HasColumnName("event_type");
            e.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
            e.Property(x => x.ActorRole).HasMaxLength(50).HasColumnName("actor_role");
            e.Property(x => x.BeforeJson).HasColumnName("before_json");
            e.Property(x => x.AfterJson).HasColumnName("after_json");
            e.Property(x => x.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
            e.Property(x => x.DeviceId).HasMaxLength(100).HasColumnName("device_id");
            e.Property(x => x.PreviousEventHash).HasMaxLength(64).HasColumnName("previous_event_hash");
            e.Property(x => x.EventHash).HasMaxLength(64).HasColumnName("event_hash");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.HasIndex(x => new { x.CompanyId, x.CreatedAtUtc }).HasDatabaseName("IX_proc_audit_company_created");
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(x => x.LastPurchaseCost).HasPrecision(18, 4).HasColumnName("last_purchase_cost");
            e.Property(x => x.AverageCost).HasPrecision(18, 4).HasColumnName("average_cost");
            e.Property(x => x.LastPurchaseAt).HasColumnName("last_purchase_at");
        });

        modelBuilder.Entity<InventoryMovement>(e =>
        {
            e.Property(x => x.GoodsReceiptId).HasColumnName("goods_receipt_id");
            e.Property(x => x.PurchaseOrderId).HasColumnName("purchase_order_id");
            e.Property(x => x.SupplierId).HasColumnName("supplier_id");
            e.Property(x => x.UnitCost).HasPrecision(18, 4).HasColumnName("unit_cost");
            e.HasIndex(x => x.GoodsReceiptId).HasDatabaseName("IX_inv_mov_goods_receipt");
        });
    }
}
