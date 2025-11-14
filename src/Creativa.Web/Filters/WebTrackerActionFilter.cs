using Microsoft.AspNetCore.Mvc.Filters;
using Creativa.Web.Services;

namespace Creativa.Web.Filters
{
    public class WebTrackerActionFilter : IActionFilter
    {
        private readonly WebTrackerService _tracker;

        public WebTrackerActionFilter(WebTrackerService tracker)
        {
            _tracker = tracker;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var urlRequest = request.Path + request.QueryString;
            var sourceIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            _tracker.Track(urlRequest, sourceIp);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }
    }
}