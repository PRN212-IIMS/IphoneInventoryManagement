namespace WPFApp
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Color { get; set; }
        public string? StorageCapacity { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}