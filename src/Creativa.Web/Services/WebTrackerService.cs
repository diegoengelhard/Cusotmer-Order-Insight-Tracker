using Creativa.Web.Data;
using Creativa.Web.Models;

namespace Creativa.Web.Services
{
    public class WebTrackerService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WebTrackerService> _logger;

        public WebTrackerService(IServiceScopeFactory scopeFactory, ILogger<WebTrackerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task LogRequestAsync(string url, string sourceIp)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<NorthwindContext>();

                var entry = new WebTracker
                {
                    URLRequest = url,
                    SourceIp = sourceIp,
                    TimeOfAction = DateTime.UtcNow
                };

                await context.WebTracker.AddAsync(entry);
                await context.SaveChangesAsync();

                _logger.LogInformation("Request logged: {Url} from {Ip}", url, sourceIp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging request to database");
            }
        }
    }
}
