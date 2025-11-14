using Microsoft.AspNetCore.Mvc;
using Creativa.Web.Services;

namespace Creativa.Web.Controllers
{
    public class CustomersController : Controller
    {
        private readonly NorthwindSqlService _service;

        public CustomersController(NorthwindSqlService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult CustomersByCountry(string? country)
        {
            ViewBag.Country = country ?? "";
            return View();
        }

        [HttpGet]
        public IActionResult CustomersByCountryData(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return Json(new List<object>());
            }

            var customers = _service.GetCustomersByCountry(country);
            return Json(customers);
        }

        [HttpGet]
        public IActionResult CustomerOrdersInformation(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var orders = _service.GetOrdersByCustomer(id);
            ViewBag.CustomerId = id;
            return View(orders);
        }
    }
}
