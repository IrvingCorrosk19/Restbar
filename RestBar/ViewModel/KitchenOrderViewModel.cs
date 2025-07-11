namespace RestBar.ViewModel
{
    public class KitchenOrderViewModel
    {
        public Guid OrderId { get; set; }
        public string TableNumber { get; set; }
        public DateTime? OpenedAt { get; set; }
        public List<KitchenOrderItemViewModel> Items { get; set; } = new();
        public string? Notes { get; set; }
        public Models.OrderType OrderType { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public int ReadyItemsCount { get; set; }
        public int TotalItemsCount { get; set; }
        
        // Nuevas propiedades para información detallada del estado
        public int TotalItems { get; set; }
        public int PendingItems { get; set; }
        public int ReadyItems { get; set; }
        public int PreparingItems { get; set; }
        
        // Propiedad calculada para mostrar resumen
        public string StatusSummary => 
            PendingItems > 0 ? $"🔄 {PendingItems} pendiente(s)" :
            PreparingItems > 0 ? $"⏳ {PreparingItems} preparando" :
            ReadyItems > 0 ? $"✅ {ReadyItems} listo(s)" : "Sin items";
    }

    public class KitchenOrderItemViewModel
    {
        public Guid ItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending";
        public string KitchenStatus { get; set; } = "Pending";
        public string StationName { get; set; } = string.Empty;
        public bool IsReady => Status == "Ready" || KitchenStatus == "Ready";
    }
} 