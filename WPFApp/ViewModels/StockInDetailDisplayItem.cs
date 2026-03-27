namespace WPFApp.ViewModels
{
    public class StockInDetailDisplayItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }

        public decimal Total => Quantity * ImportPrice;
    }
}