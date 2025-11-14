using CoreWCF;
using CoreWCF.Configuration;
using Creativa.Web.Data;
using Creativa.Web.Filters;
using Creativa.Web.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<WebTrackerActionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Register SQL Server DbContext
builder.Services.AddDbContext<NorthwindContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NorthwindConnection")));

// Register CSV fallback repository
builder.Services.AddSingleton<NorthwindCsvRepository>();

// Register SQL service
builder.Services.AddScoped<NorthwindSqlService>();

// Register WebTrackerService (now uses SQL Server)
builder.Services.AddScoped<WebTrackerService>();

// Register DatabaseHealthChecker
builder.Services.AddSingleton<DatabaseHealthChecker>();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

var app = builder.Build();

// Check database connection at startup
using (var scope = app.Services.CreateScope())
{
    var healthChecker = scope.ServiceProvider.GetRequiredService<DatabaseHealthChecker>();
    var isSqlServerAvailable = await healthChecker.CheckConnectionAsync();

    app.Services.GetRequiredService<IConfiguration>()["DatabaseAvailable"] = isSqlServerAvailable.ToString();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customers}/{action=CustomersByCountry}/{id?}");

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<NorthwindSqlService>(options =>
    {
        options.BaseAddresses.Add(new Uri("http://localhost/NorthwindService"));
    });
    serviceBuilder.AddServiceEndpoint<NorthwindSqlService, INorthwindService>(
        new BasicHttpBinding(),
        "/basichttp");
});

app.Run();
