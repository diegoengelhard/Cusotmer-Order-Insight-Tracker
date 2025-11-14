using System.Collections.Generic;
using System.Linq;
using CoreWCF;
using Creativa.Web.Data;
using Creativa.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Creativa.Web.Services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class NorthwindSqlService : INorthwindService
    {
        private readonly NorthwindContext? _context;
        private readonly NorthwindCsvRepository _csvRepository;
        private readonly ILogger<NorthwindSqlService> _logger;
        private readonly bool _useSqlServer;

        public NorthwindSqlService(
            ILogger<NorthwindSqlService> logger,
            IConfiguration configuration,
            NorthwindContext? context,
            NorthwindCsvRepository csvRepository)
        {
            _logger = logger;
            _context = context;
            _csvRepository = csvRepository;

            // Check if SQL Server is available
            _useSqlServer = bool.TryParse(configuration["DatabaseAvailable"], out var available) && available;

            if (!_useSqlServer)
            {
                _logger.LogWarning("⚠️ Using CSV data source instead of SQL Server");
            }
        }

        public List<Customer> GetCustomersByCountry(string country)
        {
            if (_useSqlServer && _context != null)
            {
                try
                {
                    return _context.Customers
                        .Where(c => c.Country == country)
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error querying SQL Server, falling back to CSV");
                }
            }

            // Fallback to CSV
            return _csvRepository
                .GetCustomersByCountry(country)
                .ToList();
        }

        public List<Order> GetOrdersByCustomer(string customerId)
        {
            if (_useSqlServer && _context != null)
            {
                try
                {
                    return _context.Orders
                        .Where(o => o.CustomerID == customerId)
                        .OrderByDescending(o => o.OrderDate)
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error querying SQL Server, falling back to CSV");
                }
            }

            // Fallback to CSV
            return _csvRepository
                .GetOrdersForCustomer(customerId)
                .ToList();
        }
    }
}
