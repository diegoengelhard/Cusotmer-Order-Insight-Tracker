using System.Collections.Generic;
using CoreWCF;
using Creativa.Web.Models;

namespace Creativa.Web.Services
{
    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface INorthwindService
    {
        [OperationContract]
        List<Customer> GetCustomersByCountry(string country);

        [OperationContract]
        List<Order> GetOrdersByCustomer(string customerId);
    }
}
