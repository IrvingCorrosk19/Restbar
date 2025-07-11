namespace RestBar.ViewModel
{
    public class ProductCreateViewModel
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? Cost { get; set; }
        public decimal? TaxRate { get; set; }
        public string? Unit { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? StationId { get; set; }
    }

}
