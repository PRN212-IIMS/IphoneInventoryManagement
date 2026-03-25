using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Product
{
    public int ProductId { get; set; }

    public int StaffId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? Model { get; set; }

    public string? Color { get; set; }

    public string? StorageCapacity { get; set; }

    public string? UrlImages { get; set; }

    public bool Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Staff Staff { get; set; } = null!;

    public virtual ICollection<StockInDetail> StockInDetails { get; set; } = new List<StockInDetail>();
}
