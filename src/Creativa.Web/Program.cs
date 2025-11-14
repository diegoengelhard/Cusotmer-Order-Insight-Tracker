using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Creativa.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// CoreWCF config to allow synchronous IO
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.AllowSynchronousIO = true;
});

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserva PascalCase
    });

// Shared repository for Northwind data
builder.Services.AddSingleton<NorthwindCsvRepository>();

// WCF service
builder.Services.AddSingleton<NorthwindService>();

// CoreWCF: base services
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

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

// MVC default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customers}/{action=CustomersByCountry}/{id?}");

// CoreWCF: map the service
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<NorthwindService>(options =>
    {
        options.BaseAddresses.Add(new Uri("http://localhost/NorthwindService"));
    });

    serviceBuilder.AddServiceEndpoint<NorthwindService, INorthwindService>(
        new BasicHttpBinding(),
        "/basichttp");
});

app.Run();
