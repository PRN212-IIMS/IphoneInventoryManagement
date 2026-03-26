using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        List<Customer> GetAllCustomers();
        Customer? GetCustomerById(int id);
        Customer? GetCustomerByEmail(string email);
        Customer? GetCustomerByEmailAndPassword(string email, string password);
        List<Customer> SearchCustomers(string keyword);
        void AddCustomer(Customer customer);
        void UpdateCustomer(Customer customer);
    }
}
