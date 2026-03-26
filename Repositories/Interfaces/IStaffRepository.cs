using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IStaffRepository
    {
        List<Staff> GetAllStaff();
        Staff? GetStaffById(int id);
        Staff? GetStaffByEmail(string email);
        Staff? GetStaffByEmailAndPassword(string email, string password);
        List<Staff> SearchStaff(string keyword);
        void AddStaff(Staff staff);
        void UpdateStaff(Staff staff);
        void UpdateStatus(int staffId, string status);
    }
}
