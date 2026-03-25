using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class StockInDetail
{
    public int StockInDetailId { get; set; }

    public int StockInId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal ImportPrice { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual StockIn StockIn { get; set; } = null!;
}
