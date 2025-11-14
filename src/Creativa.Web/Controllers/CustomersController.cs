using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Creativa.Web.Models;
using Creativa.Web.Services;

namespace Creativa.Web.Controllers
{
    public class CustomersController : Controller
    {
        private readonly NorthwindCsvRepository _repository;

        public CustomersController(NorthwindCsvRepository repository)
        {
            _repository = repository;
        }

        // Main View: Customers by Country
        // URL: /Customers/CustomersByCountry?country=Germany
        [HttpGet]
        public IActionResult CustomersByCountry(string? country)
        {
            country ??= string.Empty;

            var customers = string.IsNullOrWhiteSpace(country)
                ? Enumerable.Empty<Customer>()
                : _repository.GetCustomersByCountry(country);

            ViewBag.Country = country;
            return View(customers);
        }

        // Orders Info View: Orders by Customer
        // URL: /Customers/CustomerOrdersInformation?id=someID
        [HttpGet]
        public IActionResult CustomerOrdersInformation(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(nameof(CustomersByCountry));
            }

            var orders = _repository
                .GetOrdersForCustomer(id)
                .OrderBy(o => o.ShippedDate);

            // To get Customer Name, we find the customer
            var customer = _repository
                .GetCustomersByCountry(string.Empty)
                .FirstOrDefault(c => c.CustomerID == id);

            ViewBag.CustomerID = id;
            ViewBag.CustomerName = customer?.CompanyName ?? id;

            return View(orders);
        }

        // ENDPOINT JSON for Kendo Grid (Customers by Country)
        // URL: /Customers/CustomersByCountryData?country=Germany
        [HttpGet]
        public IActionResult CustomersByCountryData(string? country)
        {
            country ??= string.Empty;

            var customers = string.IsNullOrWhiteSpace(country)
                ? Enumerable.Empty<Customer>()
                : _repository.GetCustomersByCountry(country);

            var result = customers.Select(c => new
            {
                c.CustomerID,
                c.CompanyName,
                c.ContactName,
                c.Phone,
                c.Fax
            });

            return Json(result);
        }

        // ENDPOINT JSON for Kendo Grid (Orders by Customer)
        // URL: /Customers/CustomerOrdersData?customerId=MORGK
        [HttpGet]
        public IActionResult CustomerOrdersData(string? customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return Json(Enumerable.Empty<object>());
            }

            var orders = _repository
                .GetOrdersForCustomer(customerId)
                .OrderBy(o => o.ShippedDate)
                .Select(o => new
                {
                    o.OrderID,
                    o.CustomerID,
                    OrderDate = o.OrderDate,
                    ShippedDate = o.ShippedDate
                });

            return Json(orders);
        }
    }
}
