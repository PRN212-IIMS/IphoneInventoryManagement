using BusinessObjects;
using DataAccessLayer;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class CustomerRepository : ICustomerRepository
{
    public List<Customer> GetAllCustomers()
    {
        using var context = new IPhoneInventoryDbContext();
        return context.Customers.OrderBy(x => x.CustomerId).ToList();
    }

    public Customer? GetCustomerById(int id)
    {
        using var context = new IPhoneInventoryDbContext();
        return context.Customers.FirstOrDefault(x => x.CustomerId == id);
    }

    public Customer? GetCustomerByEmail(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        using var context = new IPhoneInventoryDbContext();
        return context.Customers.FirstOrDefault(x => x.Email != null && x.Email.ToLower() == normalizedEmail);
    }

    public Customer? GetCustomerByEmailAndPassword(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        using var context = new IPhoneInventoryDbContext();
        return context.Customers.FirstOrDefault(x => x.Email != null && x.Email.ToLower() == normalizedEmail && x.Password == password);
    }

    public List<Customer> SearchCustomers(string keyword)
    {
        var normalizedKeyword = keyword.Trim().ToLowerInvariant();
        using var context = new IPhoneInventoryDbContext();
        return context.Customers
            .Where(x => x.FullName.ToLower().Contains(normalizedKeyword)
                || (x.Email != null && x.Email.ToLower().Contains(normalizedKeyword))
                || (x.Phone != null && x.Phone.Contains(keyword.Trim())))
            .OrderBy(x => x.CustomerId)
            .ToList();
    }

    public void AddCustomer(Customer customer)
    {
        using var context = new IPhoneInventoryDbContext();
        context.Customers.Add(customer);
        context.SaveChanges();
    }

    public void UpdateCustomer(Customer customer)
    {
        using var context = new IPhoneInventoryDbContext();
        context.Customers.Update(customer);
        context.SaveChanges();
    }

    public void UpdateCustomerStatus(int customerId, string status)
{
    using var context = new IPhoneInventoryDbContext();
    var customer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

    if (customer == null) return;

    customer.Status = status;
    context.SaveChanges();
}
}
