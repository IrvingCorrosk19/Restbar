namespace RestBar.Infrastructure.Foundation;

/// <summary>
/// Feature flags for incomplete / future enterprise surfaces.
/// Defaults keep current behavior; incomplete UIs should check flags before rendering.
/// </summary>
public sealed class FeatureFlags
{
    public const string SectionName = "FeatureFlags";

    /// <summary>Supplier UI / SupplierAnalysis. Default false until Purchasing module exists.</summary>
    public bool EnableSupplierUi { get; set; }

    /// <summary>ExecuteBackupAsync real path. Default false while backup is stubbed.</summary>
    public bool EnableBackupExecution { get; set; }

    /// <summary>AdvancedSettings pages beyond Index/SystemSettings.</summary>
    public bool EnableAdvancedSettingsExtra { get; set; } = true;

    /// <summary>Seed HTTP endpoints. Forced false outside Development regardless.</summary>
    public bool EnableSeedEndpoints { get; set; } = true;

    /// <summary>Export PDF/Excel when implementations are real.</summary>
    public bool EnableReportExports { get; set; }

    /// <summary>Future modules — reserved, default off.</summary>
    public bool EnableCashModule { get; set; }
    public bool EnablePurchasingModule { get; set; }
    public bool EnableFoodCostModule { get; set; }
    public bool EnableCommandCenter { get; set; }
    public bool EnableCopilot { get; set; }
}
