using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
namespace Services.Interfaces
{
    public interface IStaffService
    {
        List<Staff> GetAllStaff();
        Staff? GetStaffById(int id);
        List<Staff> SearchStaff(string keyword);
        void CreateStaff(Staff staff);
        void UpdateStaff(Staff staff);
        void ChangeStaffStatus(int staffId, string status);
    }
}
