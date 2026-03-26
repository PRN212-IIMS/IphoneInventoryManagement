using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface IStockInRepository
    {
        List<StockIn> GetAllStockIns();
        StockIn? GetStockInById(int id);
        List<StockIn> SearchStockIns(string keyword);
        void AddStockIn(StockIn stockIn);
        void AddStockInDetails(List<StockInDetail> stockInDetails);
    }
}
