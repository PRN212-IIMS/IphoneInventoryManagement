using BusinessObjects;

namespace Services.Interfaces
{
    public interface IStockInService
    {
        List<StockIn> GetAllStockIns();
        StockIn? GetStockInById(int id);
        List<StockIn> SearchStockIns(string keyword);
        void CreateStockIn(StockIn stockIn, List<StockInDetail> details);
    }
}