using AeroDroxUAV.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using AeroDroxUAV.Models;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// ===================================
// 1. SERVICE CONFIGURATION
// ===================================

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=AeroDroxUAV.db"));

// Add Session services (Essential for your current authentication logic)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register IHttpContextAccessor (needed for accessing session in views/controllers)
builder.Services.AddHttpContextAccessor();

// NEW: Configure Swagger/OpenAPI services (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ===================================
// 2. DATA SEEDING
// ===================================

// Seed users upon application startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if(!db.Users.Any())
    {
        db.Users.Add(new User{
            Username="admin",
            Password="admin123",
            Role="Admin"
        });

        db.Users.Add(new User{
            Username="user",
            Password="user123",
            Role="User"
        });

        db.SaveChanges();
    }
}

// ===================================
// 3. MIDDLEWARE PIPELINE
// ===================================

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // NEW: Enable Swagger UI and JSON generation only in Development
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days.
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Uncomment if you enable HTTPS
app.UseStaticFiles();
app.UseRouting();

// UseSession must be called before UseAuthorization
app.UseSession();
app.UseAuthorization();

// Set the default route to Account/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();