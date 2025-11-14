using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Creativa.Web.Models;
using Microsoft.AspNetCore.Hosting;

namespace Creativa.Web.Services
{
    public class NorthwindCsvRepository
    {
        private readonly List<Customer> _customers;
        private readonly List<Order> _orders;

        public NorthwindCsvRepository(IWebHostEnvironment env)
        {
            // ContentRootPath for the data files
            var root = Path.GetFullPath(
                Path.Combine(env.ContentRootPath, "..", "..", "data")
            );

            var customersPath = Path.Combine(root, "customers.csv");
            var ordersPath = Path.Combine(root, "orders.csv");

            _customers = File.Exists(customersPath)
                ? LoadCustomers(customersPath)
                : new List<Customer>();

            _orders = File.Exists(ordersPath)
                ? LoadOrders(ordersPath)
                : new List<Order>();
        }

        // Load the Customers
        private static List<Customer> LoadCustomers(string path)
        {
            var result = new List<Customer>();

            foreach (var line in File.ReadLines(path).Skip(1)) // skip column header
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // columns from the csv separated by comma
                var cols = line.Split(',');

                if (cols.Length < 10)
                    continue;

                result.Add(new Customer
                {
                    CustomerID = cols[0],
                    CompanyName = cols[1],
                    ContactName = cols[2],
                    Country = cols[8],
                    Phone = cols[9],
                    Fax = cols.Length > 10 ? cols[10] : null
                });
            }

            return result;
        }

        //  Load the Orders
        private static List<Order> LoadOrders(string path)
        {
            var result = new List<Order>();
            var culture = CultureInfo.InvariantCulture;

            foreach (var line in File.ReadLines(path).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var cols = line.Split(',');

                if (cols.Length < 7)
                    continue;

                // orderID,customerID,employeeID,orderDate,requiredDate,shippedDate,...
                int orderId;
                if (!int.TryParse(cols[0], out orderId))
                    continue;

                DateTime? ParseDate(string s)
                {
                    if (string.IsNullOrWhiteSpace(s) || s.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        return null;

                    if (DateTime.TryParse(s, culture, DateTimeStyles.None, out var dt))
                        return dt;

                    return null;
                }

                var order = new Order
                {
                    OrderID = orderId,
                    CustomerID = cols[1],
                    OrderDate = ParseDate(cols[3]),
                    ShippedDate = ParseDate(cols[5])
                };

                result.Add(order);
            }

            return result;
        }

        // Public API of the Repository
        public IEnumerable<Customer> GetCustomersByCountry(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
                return Enumerable.Empty<Customer>();

            country = country.Trim();

            return _customers
                .Where(c =>
                    !string.IsNullOrWhiteSpace(c.Country) &&
                    c.Country.Contains(country, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.ContactName);
        }

        public IEnumerable<Order> GetOrdersForCustomer(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return Enumerable.Empty<Order>();

            return _orders
                .Where(o => string.Equals(o.CustomerID, customerId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(o => o.ShippedDate);
        }
    }
}
