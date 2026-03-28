using BusinessObjects;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Services.Implementations
{
    public class StockInService : IStockInService
    {
        private const int MaxLineQuantity = 1000;
        private const int MaxDistinctProducts = 50;
        private const decimal MinImportPrice = 100000m;
        private const decimal MaxImportPrice = 100000000m;
        private const int MaxNoteLength = 500;

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
                throw new Exception("Invalid stock-in ID.");

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
                throw new Exception("Invalid staff for stock-in.");

            if (details == null || details.Count == 0)
                throw new Exception("Stock-in must have at least one product line.");
            if (details.Count > MaxDistinctProducts)
                throw new Exception($"Stock-in cannot have more than {MaxDistinctProducts} product lines.");

            if (!string.IsNullOrWhiteSpace(stockIn.Note))
            {
                stockIn.Note = stockIn.Note.Trim();
                if (stockIn.Note.Length > MaxNoteLength)
                    throw new Exception($"Note cannot exceed {MaxNoteLength} characters.");
            }

            var productIds = new HashSet<int>();

            foreach (var detail in details)
            {
                if (detail.ProductId <= 0)
                    throw new Exception("Invalid product in stock-in.");
                if (!productIds.Add(detail.ProductId))
                    throw new Exception("Duplicate product in stock-in; please merge quantities.");

                if (detail.Quantity <= 0)
                    throw new Exception("Quantity must be greater than 0.");
                if (detail.Quantity > MaxLineQuantity)
                    throw new Exception($"Each line cannot exceed {MaxLineQuantity} units.");

                if (detail.ImportPrice < MinImportPrice || detail.ImportPrice > MaxImportPrice)
                    throw new Exception($"Import unit price must be between {MinImportPrice:N0} and {MaxImportPrice:N0} VND.");

                if (decimal.Truncate(detail.ImportPrice) != detail.ImportPrice)
                    throw new Exception("Import unit price must be a whole number.");

                string importInvariant = detail.ImportPrice.ToString(CultureInfo.InvariantCulture);
                if (!Regex.IsMatch(importInvariant, @"^\d+$"))
                    throw new Exception("Import unit price may only contain digits (0-9), with no commas, dots, or other characters.");
            }

            _stockInRepository.CreateStockInWithDetails(stockIn, details);
        }
    }
}
