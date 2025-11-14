using System.Collections.Generic;
using System.Linq;
using CoreWCF;
using Creativa.Web.Models;

namespace Creativa.Web.Services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class NorthwindService : INorthwindService
    {
        private readonly NorthwindCsvRepository _repository;

        public NorthwindService(NorthwindCsvRepository repository)
        {
            _repository = repository;
        }

        public List<Customer> GetCustomersByCountry(string country)
        {
            return _repository
                .GetCustomersByCountry(country)
                .ToList();
        }

        public List<Order> GetOrdersByCustomer(string customerId)
        {
            return _repository
                .GetOrdersForCustomer(customerId)
                .ToList();
        }
    }
}
