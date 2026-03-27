using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Models;

namespace Services.Interfaces
{
    public interface IAdminAccountService
    {
        List<UserAccount> GetAllAccounts();
        List<UserAccount> SearchAndFilter(string? keyword, string? role, string? status);
        bool ChangeAccountStatus(int id, string role, string newStatus, out string message);
    }
}
