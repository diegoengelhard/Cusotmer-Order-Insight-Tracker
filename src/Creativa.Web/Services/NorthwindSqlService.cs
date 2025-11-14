using System.Collections.Generic;
using System.Linq;
using CoreWCF;
using Creativa.Web.Data;
using Creativa.Web.Models;

namespace Creativa.Web.Services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class NorthwindSqlService : INorthwindService
    {
        private readonly NorthwindContext _context;

        public NorthwindSqlService(NorthwindContext context)
        {
            _context = context;
        }

        public List<Customer> GetCustomersByCountry(string country)
        {
            return _context.Customers
                .Where(c => c.Country == country)
                .ToList();
        }

        public List<Order> GetOrdersByCustomer(string customerId)
        {
            return _context.Orders
                .Where(o => o.CustomerID == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }
    }
}
