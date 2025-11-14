using Microsoft.EntityFrameworkCore;
using Creativa.Web.Data;

namespace Creativa.Web.Services
{
    public class DatabaseHealthChecker
    {
        private readonly ILogger<DatabaseHealthChecker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private const int MaxRetries = 3;
        private const int DelayBetweenRetriesMs = 2000;

        public DatabaseHealthChecker(
            ILogger<DatabaseHealthChecker> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            _logger.LogInformation(" Checking SQL Server connection...");

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<NorthwindContext>();

                    _logger.LogInformation($"Attempt {attempt}/{MaxRetries}: Connecting to SQL Server...");

                    await context.Database.CanConnectAsync();

                    _logger.LogInformation("Successfully connected to SQL Server (Northwind database)");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Connection attempt {attempt}/{MaxRetries} failed: {ex.Message}");

                    if (attempt < MaxRetries)
                    {
                        _logger.LogInformation($"Retrying in {DelayBetweenRetriesMs}ms...");
                        await Task.Delay(DelayBetweenRetriesMs);
                    }
                }
            }

            _logger.LogError("Could not connect to SQL Server after {MaxRetries} attempts", MaxRetries);
            _logger.LogWarning("Falling back to CSV data source (data/customers.csv, data/orders.csv)");
            return false;
        }
    }
}
