using AeroDroxUAV.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using AeroDroxUAV.Models;
using System;
using System.Linq;
using AeroDroxUAV.Repositories;
using AeroDroxUAV.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ===================================
// 1. SERVICE CONFIGURATION
// ===================================

builder.Services.AddControllersWithViews(); 

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=AeroDroxUAV.db"));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDroneRepository, DroneRepository>();
builder.Services.AddScoped<IDroneServicesRepository, DroneServicesRepository>();
builder.Services.AddScoped<IAccessoriesRepository, AccessoriesRepository>();

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDroneService, DroneService>();
builder.Services.AddScoped<IDroneServicesService, DroneServicesService>();
builder.Services.AddScoped<IAccessoriesService, AccessoriesService>();

// Add Session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ===================================
// 2. DATA SEEDING (Using the Service Layer)
// ===================================

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
    
    dbContext.Database.EnsureCreated();

    var userService = serviceProvider.GetRequiredService<IUserService>();
    await userService.SeedDefaultUsersAsync();
}

// ===================================
// 3. MIDDLEWARE PIPELINE
// ===================================

if (app.Environment.IsDevelopment()) 
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// ===============================
// ADD BASIC AUTH MIDDLEWARE HERE
// ===============================
app.UseMiddleware<BasicAuthMiddleware>();

app.UseAuthorization();

// Map attribute-routed controllers (e.g., DronesApiController)
app.MapControllers(); 

// Map conventional MVC controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
