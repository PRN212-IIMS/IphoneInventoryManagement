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
        List<Staff> FilterStaff(string? keyword, string? status);
        bool CreateStaff(Staff staff, out string message);
        bool UpdateStaff(Staff staff, out string message);
        bool ChangeStaffStatus(int staffId, string status, out string message);
    }
}
