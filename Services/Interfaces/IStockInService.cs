using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IStockInService
    {
        List<StockIn> GetAllStockIns();
        StockIn? GetStockInById(int id);
        List<StockIn> SearchStockIns(string keyword);
        void CreateStockIn(StockIn stockIn, List<StockInDetail> stockInDetails);
    }
}
