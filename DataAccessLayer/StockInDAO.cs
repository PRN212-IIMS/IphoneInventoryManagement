using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class StockInDAO
    {
        public List<StockIn> GetAllStockIns()
        {
            using var context = new IPhoneInventoryDbContext();
            return context.StockIns
                .AsNoTracking()
                .Include(s => s.CreatedByStaff)
                .Include(s => s.StockInDetails)
                .OrderByDescending(s => s.StockInDate)
                .ToList();
        }

        public StockIn? GetStockInById(int id)
        {
            using var context = new IPhoneInventoryDbContext();
            return context.StockIns
                .AsNoTracking()
                .Include(s => s.CreatedByStaff)
                .Include(s => s.StockInDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefault(s => s.StockInId == id);
        }

        public List<StockIn> SearchStockIns(string keyword)
        {
            using var context = new IPhoneInventoryDbContext();
            var query = context.StockIns
                .AsNoTracking()
                .Include(s => s.CreatedByStaff)
                .Include(s => s.StockInDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(s =>
                    s.StockInId.ToString().Contains(keyword) ||
                    (s.CreatedByStaff != null && s.CreatedByStaff.FullName.Contains(keyword)));
            }

            return query.OrderByDescending(s => s.StockInDate).ToList();
        }

        public int AddStockIn(StockIn stockIn)
        {
            using var context = new IPhoneInventoryDbContext();

            stockIn.StockInDate = stockIn.StockInDate == default ? DateTime.Now : stockIn.StockInDate;
            context.StockIns.Add(stockIn);
            context.SaveChanges();

            return stockIn.StockInId;
        }

        public void AddStockInDetails(List<StockInDetail> details)
        {
            using var context = new IPhoneInventoryDbContext();
            context.StockInDetails.AddRange(details);
            context.SaveChanges();
        }

        public void CreateStockInWithDetails(StockIn stockIn, List<StockInDetail> details)
        {
            using var context = new IPhoneInventoryDbContext();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                stockIn.StockInDate = stockIn.StockInDate == default ? DateTime.Now : stockIn.StockInDate;
                context.StockIns.Add(stockIn);
                context.SaveChanges();

                foreach (var detail in details)
                {
                    detail.StockInId = stockIn.StockInId;
                }

                context.StockInDetails.AddRange(details);

                foreach (var detail in details)
                {
                    var product = context.Products.FirstOrDefault(p => p.ProductId == detail.ProductId);
                    if (product == null)
                    {
                        throw new Exception($"Không tìm thấy sản phẩm có ID = {detail.ProductId}");
                    }

                    product.StockQuantity += detail.Quantity;
                    product.UpdatedAt = DateTime.Now;
                }

                context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}