using RealEstate.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// HttpContextAccessor for accessing cookies in services
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<RealEstate.Web.Services.ApiService>();

// API Client konfigürasyonu
// Varsayılan API adresini geliştirme makinesindeki çalışan API'ye işaret edecek şekilde ayarla
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001";
builder.Services.AddHttpClient<PropertyApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddHttpClient<RealEstate.Web.Services.ApiService>(client =>
{
    // Buradaki adres senin API'nin çalıştığı port olmalı (5180)
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Auth Service
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Notification Service
builder.Services.AddHttpClient<NotificationService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Handle status codes (404, 500, etc.) by re-executing to ErrorController
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();