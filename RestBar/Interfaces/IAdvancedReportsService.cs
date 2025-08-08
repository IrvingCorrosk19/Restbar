using RestBar.ViewModels;

namespace RestBar.Interfaces
{
    public interface IAdvancedReportsService
    {
        // Análisis de Inventario
        Task<InventoryAnalysisReport> GetInventoryAnalysisAsync(ReportFilters filters);
        Task<List<LowStockAlert>> GetLowStockAlertsAsync();
        Task<List<InventoryTurnoverData>> GetInventoryTurnoverAsync(ReportFilters filters);
        Task<List<InventoryValueReport>> GetInventoryValueReportAsync();
        
        // Análisis de Proveedores
        Task<SupplierAnalysisReport> GetSupplierAnalysisAsync(ReportFilters filters);
        Task<List<SupplierPerformanceData>> GetSupplierPerformanceAsync(ReportFilters filters);
        Task<List<PurchaseOrderAnalysis>> GetPurchaseOrderAnalysisAsync(ReportFilters filters);
        
        // Análisis de Rentabilidad
        Task<ProfitabilityAnalysis> GetProfitabilityAnalysisAsync(ReportFilters filters);
        Task<List<ProductProfitabilityData>> GetProductProfitabilityAsync(ReportFilters filters);
        Task<List<CategoryProfitabilityData>> GetCategoryProfitabilityAsync(ReportFilters filters);
        
        // Análisis de Tendencias
        Task<TrendAnalysisReport> GetTrendAnalysisAsync(ReportFilters filters);
        Task<List<SeasonalTrendData>> GetSeasonalTrendsAsync(ReportFilters filters);
        Task<List<GrowthAnalysisData>> GetGrowthAnalysisAsync(ReportFilters filters);
        
        // Reportes de Auditoría
        Task<AuditReport> GetAuditReportAsync(ReportFilters filters);
        Task<List<UserActivityData>> GetUserActivityReportAsync(ReportFilters filters);
        Task<List<SystemLogData>> GetSystemLogReportAsync(ReportFilters filters);
        
        // Reportes de Transferencias
        Task<TransferAnalysisReport> GetTransferAnalysisAsync(ReportFilters filters);
        Task<List<TransferEfficiencyData>> GetTransferEfficiencyAsync(ReportFilters filters);
        
        // Reportes de Configuración Avanzada
        Task<SystemHealthReport> GetSystemHealthReportAsync();
        Task<List<PerformanceMetricsData>> GetPerformanceMetricsAsync(ReportFilters filters);
        
        // Exportación
        Task<byte[]> ExportAdvancedReportToPdfAsync(string reportType, ReportFilters filters);
        Task<byte[]> ExportAdvancedReportToExcelAsync(string reportType, ReportFilters filters);
    }
} 