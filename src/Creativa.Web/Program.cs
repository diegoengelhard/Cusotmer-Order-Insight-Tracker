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

// Register SQL Server DbContext (will be used if connection succeeds)
builder.Services.AddDbContext<NorthwindContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NorthwindConnection")));

// Register CSV fallback repository
builder.Services.AddSingleton<NorthwindCsvRepository>();

// Register SQL service (will fallback to CSV if DB unavailable)
builder.Services.AddScoped<NorthwindSqlService>();

// Register singleton services
builder.Services.AddSingleton<WebTrackerService>();
builder.Services.AddSingleton<DatabaseHealthChecker>();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

var app = builder.Build();

// CHECK DATABASE CONNECTION AT STARTUP
using (var scope = app.Services.CreateScope())
{
    var healthChecker = scope.ServiceProvider.GetRequiredService<DatabaseHealthChecker>();
    var isSqlServerAvailable = await healthChecker.CheckConnectionAsync();

    // Store the result in application state for runtime checks
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
