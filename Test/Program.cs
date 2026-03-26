using BusinessObjects;
using DataAccessLayer;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CustomerDAO dao = new CustomerDAO();
            List<Customer> cus = dao.getALlCustomer();

            foreach (Customer s in cus)
            {
                Console.WriteLine(s);
            }
        }
    }
}
