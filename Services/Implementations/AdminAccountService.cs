using DataAccessLayer;
using Services.Interfaces;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class AdminAccountService : IAdminAccountService
    {
        public List<UserAccount> GetAllAccounts()
        {
            using var context = new IPhoneInventoryDbContext();

            var admins = context.Admins
                .Select(a => new UserAccount
                {
                    Id = a.AdminId,
                    FullName = a.FullName,
                    Email = a.Email,
                    Phone = null,
                    Role = "Admin",
                    Status = a.Status
                })
                .ToList();

            var staffs = context.Staff
                .Select(s => new UserAccount
                {
                    Id = s.StaffId,
                    FullName = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                    Role = "Staff",
                    Status = s.Status
                })
                .ToList();

            var customers = context.Customers
                .Select(c => new UserAccount
                {
                    Id = c.CustomerId,
                    FullName = c.FullName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Role = "Customer",
                    Status = c.Status
                })
                .ToList();

            return admins
                 .Concat(staffs)
                 .Concat(customers)
                 .OrderBy(x => x.Role == "Staff" ? 1 :
                  x.Role == "Customer" ? 2 : 3)
                 .ThenByDescending(x => x.Id)
                 .ToList();
        }

        public List<UserAccount> SearchAndFilter(string? keyword, string? role, string? status)
        {
            var query = GetAllAccounts().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string normalizedKeyword = keyword.Trim().ToLower();

                query = query.Where(x =>
                    x.FullName.ToLower().Contains(normalizedKeyword) ||
                    x.Email.ToLower().Contains(normalizedKeyword) ||
                    (x.Phone != null && x.Phone.Contains(keyword.Trim()))
                );
            }

            if (!string.IsNullOrWhiteSpace(role) && role != "All")
            {
                query = query.Where(x => x.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderBy(x => x.Role == "Staff" ? 1 :
                  x.Role == "Customer" ? 2 : 3)
                .ThenByDescending(x => x.Id)
                .ToList();
        }

        public bool ChangeAccountStatus(int id, string role, string newStatus, out string message)
        {
            using var context = new IPhoneInventoryDbContext();

            if (string.IsNullOrWhiteSpace(role))
            {
                message = "Role is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(newStatus))
            {
                message = "Status is required.";
                return false;
            }

            if (newStatus != "Active" && newStatus != "Inactive")
            {
                message = "Status must be Active or Inactive.";
                return false;
            }

            if (role.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                var staff = context.Staff.FirstOrDefault(x => x.StaffId == id);
                if (staff == null)
                {
                    message = "Staff account not found.";
                    return false;
                }

                staff.Status = newStatus;
                context.SaveChanges();
                message = "Staff status updated successfully.";
                return true;
            }

            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                var customer = context.Customers.FirstOrDefault(x => x.CustomerId == id);
                if (customer == null)
                {
                    message = "Customer account not found.";
                    return false;
                }

                customer.Status = newStatus;
                context.SaveChanges();
                message = "Customer status updated successfully.";
                return true;
            }

            message = "Only Staff and Customer accounts can change status.";
            return false;
        }
    }
}
