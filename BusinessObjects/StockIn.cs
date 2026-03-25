using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class StockIn
{
    public int StockInId { get; set; }

    public int CreatedByStaffId { get; set; }

    public DateTime StockInDate { get; set; }

    public string? Note { get; set; }

    public virtual Staff CreatedByStaff { get; set; } = null!;

    public virtual ICollection<StockInDetail> StockInDetails { get; set; } = new List<StockInDetail>();
}
