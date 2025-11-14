using Microsoft.AspNetCore.Mvc.Filters;
using Creativa.Web.Services;

namespace Creativa.Web.Filters
{
    public class WebTrackerActionFilter : IAsyncActionFilter
    {
        private readonly WebTrackerService _trackerService;

        public WebTrackerActionFilter(WebTrackerService trackerService)
        {
            _trackerService = trackerService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            var url = $"{request.Path}{request.QueryString}";
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            await _trackerService.LogRequestAsync(url, ip);

            await next();
        }
    }
}
