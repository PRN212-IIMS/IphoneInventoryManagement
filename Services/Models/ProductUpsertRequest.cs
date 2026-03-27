namespace Services.Models
{
    public class ProductUpsertRequest
    {
        public int ProductId { get; set; }
        public int StaffId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string StorageCapacity { get; set; } = string.Empty;
        public string? UrlImages { get; set; }
        public bool Status { get; set; } = true;
    }
}