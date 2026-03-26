using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICustomerService
    {
        List<Customer> GetAllCustomers();
        Customer? GetCustomerById(int id);
        List<Customer> SearchCustomers(string keyword);
        void Register(Customer customer);
        void UpdateProfile(Customer customer);
        void ChangePassword(int customerId, string oldPassword, string newPassword);
    }
}
