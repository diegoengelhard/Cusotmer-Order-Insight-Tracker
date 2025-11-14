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
    // ✅ Configurar JSON para usar camelCase
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddDbContext<NorthwindContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NorthwindConnection")));

builder.Services.AddScoped<NorthwindSqlService>();
builder.Services.AddSingleton<WebTrackerService>();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

var app = builder.Build();

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
