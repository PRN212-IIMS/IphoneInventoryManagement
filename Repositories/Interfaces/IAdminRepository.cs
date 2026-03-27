using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface IAdminRepository
    {
        List<Admin> GetAllAdmins();
        Admin? GetAdminById(int id);
        Admin? GetAdminByEmail(string email);
    }
}
