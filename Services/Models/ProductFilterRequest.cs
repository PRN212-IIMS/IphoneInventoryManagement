namespace Services.Models
{
    public class ProductFilterRequest
    {
        public string? Keyword { get; set; }
        public string? Color { get; set; }
        public string? StorageCapacity { get; set; }
        public bool? Status { get; set; }
    }
}