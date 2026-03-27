using BusinessObjects;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService()
        {
            _customerRepository = new CustomerRepository();
        }

        public List<Customer> GetAllCustomers()
        {
            return _customerRepository.GetAllCustomers();
        }

        public Customer? GetCustomerById(int id)
        {
            if (id <= 0)
                return null;

            return _customerRepository.GetCustomerById(id);
        }

        public List<Customer> SearchCustomers(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _customerRepository.GetAllCustomers();

            return _customerRepository.SearchCustomers(keyword);
        }

        public void Register(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (string.IsNullOrWhiteSpace(customer.FullName))
                throw new Exception("Họ tên không được để trống.");

            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new Exception("Email không được để trống.");

            if (string.IsNullOrWhiteSpace(customer.Password))
                throw new Exception("Mật khẩu không được để trống.");

            var existingCustomer = _customerRepository.GetCustomerByEmail(customer.Email);
            if (existingCustomer != null)
                throw new Exception("Email đã tồn tại.");

            _customerRepository.AddCustomer(customer);
        }

        public void UpdateProfile(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (customer.CustomerId <= 0)
                throw new Exception("Customer ID không hợp lệ.");

            if (string.IsNullOrWhiteSpace(customer.FullName))
                throw new Exception("Họ tên không được để trống.");

            _customerRepository.UpdateCustomer(customer);
        }

        public void ChangePassword(int customerId, string oldPassword, string newPassword)
        {
            if (customerId <= 0)
                throw new Exception("Customer ID không hợp lệ.");

            if (string.IsNullOrWhiteSpace(oldPassword))
                throw new Exception("Mật khẩu cũ không được để trống.");

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new Exception("Mật khẩu mới không được để trống.");

            var customer = _customerRepository.GetCustomerById(customerId);
            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng.");

            if (customer.Password != oldPassword)
                throw new Exception("Mật khẩu cũ không đúng.");

            customer.Password = newPassword;
            _customerRepository.UpdateCustomer(customer);
        }
    }
}