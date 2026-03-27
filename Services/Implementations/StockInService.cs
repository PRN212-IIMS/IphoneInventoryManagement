using BusinessObjects;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implementations;

public class StockInService : IStockInService
{
    private readonly IStockInRepository _stockInRepository;
    private readonly IProductRepository _productRepository;

    public StockInService()
    {
        _stockInRepository = new StockInRepository();
        _productRepository = new ProductRepository();
    }

    public List<StockIn> GetAllStockIns()
    {
        return _stockInRepository.GetAllStockIns();
    }

    public StockIn? GetStockInById(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return _stockInRepository.GetStockInById(id);
    }

    public List<StockIn> SearchStockIns(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return _stockInRepository.GetAllStockIns();
        }

        return _stockInRepository.SearchStockIns(keyword);
    }

    public void CreateStockIn(StockIn stockIn, List<StockInDetail> stockInDetails)
    {
        if (stockIn is null)
        {
            throw new ArgumentNullException(nameof(stockIn));
        }

        if (stockIn.CreatedByStaffId <= 0)
        {
            throw new Exception("Staff ID is invalid.");
        }

        if (stockInDetails is null || stockInDetails.Count == 0)
        {
            throw new Exception("Stock-in must contain at least one product.");
        }

        foreach (var detail in stockInDetails)
        {
            if (detail.ProductId <= 0)
            {
                throw new Exception("Product ID is invalid.");
            }

            if (detail.Quantity <= 0)
            {
                throw new Exception("Quantity must be greater than 0.");
            }

            if (detail.ImportPrice < 0)
            {
                throw new Exception("Import price cannot be negative.");
            }

            var product = _productRepository.GetProductById(detail.ProductId);
            if (product is null)
            {
                throw new Exception($"Product ID {detail.ProductId} was not found.");
            }
        }

        stockIn.StockInDate = DateTime.Now;
        _stockInRepository.AddStockIn(stockIn);

        foreach (var detail in stockInDetails)
        {
            detail.StockInId = stockIn.StockInId;
        }

        _stockInRepository.AddStockInDetails(stockInDetails);

        foreach (var detail in stockInDetails)
        {
            var product = _productRepository.GetProductById(detail.ProductId);
            if (product is null)
            {
                continue;
            }

            _productRepository.UpdateStockQuantity(product.ProductId, product.StockQuantity + detail.Quantity);
        }
    }
}
