using BusinessObjects;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implementations
{
    public class StockInService : IStockInService
    {
        private readonly IStockInRepository _stockInRepository;

        public StockInService()
        {
            _stockInRepository = new StockInRepository();
        }

        public List<StockIn> GetAllStockIns()
        {
            return _stockInRepository.GetAllStockIns();
        }

        public StockIn? GetStockInById(int id)
        {
            if (id <= 0)
                throw new Exception("ID phiếu nhập không hợp lệ.");

            return _stockInRepository.GetStockInById(id);
        }

        public List<StockIn> SearchStockIns(string keyword)
        {
            return _stockInRepository.SearchStockIns(keyword ?? string.Empty);
        }

        public void CreateStockIn(StockIn stockIn, List<StockInDetail> details)
        {
            if (stockIn == null)
                throw new ArgumentNullException(nameof(stockIn));

            if (stockIn.CreatedByStaffId <= 0)
                throw new Exception("Nhân viên tạo phiếu nhập không hợp lệ.");

            if (details == null || details.Count == 0)
                throw new Exception("Phiếu nhập phải có ít nhất 1 sản phẩm.");

            foreach (var detail in details)
            {
                if (detail.ProductId <= 0)
                    throw new Exception("Sản phẩm trong phiếu nhập không hợp lệ.");

                if (detail.Quantity <= 0)
                    throw new Exception("Số lượng nhập phải lớn hơn 0.");

                if (detail.ImportPrice < 0)
                    throw new Exception("Đơn giá nhập không hợp lệ.");
            }

            _stockInRepository.CreateStockInWithDetails(stockIn, details);
        }
    }
}
