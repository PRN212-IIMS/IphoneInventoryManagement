using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface IStockInRepository
    {
        List<StockIn> GetAllStockIns();
        StockIn? GetStockInById(int id);
        List<StockIn> SearchStockIns(string keyword);
        void AddStockIn(StockIn stockIn);
        void AddStockInDetails(List<StockInDetail> details);
        void CreateStockInWithDetails(StockIn stockIn, List<StockInDetail> details);
    }
}