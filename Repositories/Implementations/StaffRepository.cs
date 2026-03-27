using BusinessObjects;
using DataAccessLayer;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class StaffRepository : IStaffRepository
{
    public List<Staff> GetAllStaff()
    {
        using var context = new IPhoneInventoryDbContext();
        return context.Staff
            .OrderBy(x => x.StaffId)
            .ToList();
    }

    public Staff? GetStaffById(int id)
    {
        using var context = new IPhoneInventoryDbContext();
        return context.Staff.FirstOrDefault(x => x.StaffId == id);
    }

    public Staff? GetStaffByEmail(string email)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();

        using var context = new IPhoneInventoryDbContext();
        return context.Staff.FirstOrDefault(x =>
            x.Email != null && x.Email.ToLower() == normalizedEmail);
    }

    public Staff? GetStaffByEmailAndPassword(string email, string password)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();

        using var context = new IPhoneInventoryDbContext();
        return context.Staff.FirstOrDefault(x =>
            x.Email != null &&
            x.Email.ToLower() == normalizedEmail &&
            x.Password == password);
    }

    public List<Staff> SearchStaff(string keyword)
    {
        string normalizedKeyword = keyword.Trim().ToLowerInvariant();

        using var context = new IPhoneInventoryDbContext();
        return context.Staff
            .Where(x =>
                x.FullName.ToLower().Contains(normalizedKeyword) ||
                (x.Email != null && x.Email.ToLower().Contains(normalizedKeyword)) ||
                (x.Phone != null && x.Phone.Contains(keyword.Trim())))
            .OrderBy(x => x.StaffId)
            .ToList();
    }

    public List<Staff> FilterStaff(string? keyword, string? status)
    {
        using var context = new IPhoneInventoryDbContext();
        var query = context.Staff.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string normalizedKeyword = keyword.Trim().ToLowerInvariant();

            query = query.Where(x =>
                x.FullName.ToLower().Contains(normalizedKeyword) ||
                (x.Email != null && x.Email.ToLower().Contains(normalizedKeyword)) ||
                (x.Phone != null && x.Phone.Contains(keyword.Trim())));
        }

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x =>
                x.Status != null &&
                x.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderBy(x => x.StaffId).ToList();
    }

    public void AddStaff(Staff staff)
    {
        using var context = new IPhoneInventoryDbContext();
        context.Staff.Add(staff);
        context.SaveChanges();
    }

    public void UpdateStaff(Staff staff)
    {
        using var context = new IPhoneInventoryDbContext();
        context.Staff.Update(staff);
        context.SaveChanges();
    }

    public void UpdateStatus(int staffId, string status)
    {
        using var context = new IPhoneInventoryDbContext();
        var staff = context.Staff.FirstOrDefault(x => x.StaffId == staffId);

        if (staff == null) return;

        staff.Status = status;
        context.SaveChanges();
    }
}

