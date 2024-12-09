using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PrintWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Th�m c?u h�nh cho Session
builder.Services.AddDistributedMemoryCache(); // D?ch v? b? nh? l?u tr? session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Th?i gian session h?t h?n
    options.Cookie.HttpOnly = true; // ??m b?o cookie kh�ng b? truy c?p t? JavaScript
    options.Cookie.IsEssential = true; // ?�nh d?u cookie l� c?n thi?t
});

// Th�m Authentication v?i Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

// Th�m DbContext
builder.Services.AddDbContext<PrintwebContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("PRINTWEB"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Th�m Middleware cho Session
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
