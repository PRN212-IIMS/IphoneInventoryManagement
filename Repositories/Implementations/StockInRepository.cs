using BusinessObjects;
using DataAccessLayer;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class StockInRepository : IStockInRepository
    {
        private readonly StockInDAO _stockInDAO;

        public StockInRepository()
        {
            _stockInDAO = new StockInDAO();
        }

        public List<StockIn> GetAllStockIns()
        {
            return _stockInDAO.GetAllStockIns();
        }

        public StockIn? GetStockInById(int id)
        {
            return _stockInDAO.GetStockInById(id);
        }

        public List<StockIn> SearchStockIns(string keyword)
        {
            return _stockInDAO.SearchStockIns(keyword);
        }

        public void AddStockIn(StockIn stockIn)
        {
            _stockInDAO.AddStockIn(stockIn);
        }

        public void AddStockInDetails(List<StockInDetail> details)
        {
            _stockInDAO.AddStockInDetails(details);
        }

        public void CreateStockInWithDetails(StockIn stockIn, List<StockInDetail> details)
        {
            _stockInDAO.CreateStockInWithDetails(stockIn, details);
        }
    }
}
