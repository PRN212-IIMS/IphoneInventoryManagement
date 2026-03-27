using BusinessObjects;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class StockInRepository : IStockInRepository
{
    public List<StockIn> GetAllStockIns()
    {
        using var context = new IPhoneInventoryDbContext();
        return context.StockIns
            .Include(x => x.CreatedByStaff)
            .Include(x => x.StockInDetails)
            .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.StockInDate)
            .ToList();
    }

    public StockIn? GetStockInById(int id)
    {
        using var context = new IPhoneInventoryDbContext();
        return context.StockIns
            .Include(x => x.CreatedByStaff)
            .Include(x => x.StockInDetails)
            .ThenInclude(x => x.Product)
            .FirstOrDefault(x => x.StockInId == id);
    }

    public List<StockIn> SearchStockIns(string keyword)
    {
        var normalizedKeyword = keyword.Trim().ToLowerInvariant();
        using var context = new IPhoneInventoryDbContext();
        return context.StockIns
            .Include(x => x.CreatedByStaff)
            .Include(x => x.StockInDetails)
            .Where(x =>
                (x.Note != null && x.Note.ToLower().Contains(normalizedKeyword))
                || x.StockInId.ToString().Contains(normalizedKeyword)
                || x.CreatedByStaff.FullName.ToLower().Contains(normalizedKeyword))
            .OrderByDescending(x => x.StockInDate)
            .ToList();
    }

    public void AddStockIn(StockIn stockIn)
    {
        using var context = new IPhoneInventoryDbContext();
        context.StockIns.Add(stockIn);
        context.SaveChanges();
    }

    public void AddStockInDetails(List<StockInDetail> stockInDetails)
    {
        using var context = new IPhoneInventoryDbContext();
        context.StockInDetails.AddRange(stockInDetails);
        context.SaveChanges();
    }
}
