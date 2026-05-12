using System.Security.Claims;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Hubs; // <- add
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services;
using MakeForYou.BusinessLogic.Services.Implement;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.Repositories.Interfaces;
using MakeForYou.Repositories.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.ConfigureFilter(
            new IgnoreAntiforgeryTokenAttribute());
    });
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// Repository & service registrations
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Notification DI
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// SignalR
builder.Services.AddSignalR();

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Configuration.GetSection("Email"); // Ensure email section read

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
        opts.LoginPath = "/Auth/Login";
        opts.LogoutPath = "/Auth/Logout";
        opts.AccessDeniedPath = "/Auth/AccessDenied";
        opts.ExpireTimeSpan = TimeSpan.FromHours(8);
        opts.SlidingExpiration = true;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Apply pending EF Core migrations at startup (creates database if it doesn't exist)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Applying database migrations (if any)...");
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

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

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

// Map the notifications hub
app.MapHub<NotificationHub>("/hubs/notifications");

// Minimal API endpoints for notifications (used by layout dropdown; require authentication)
app.MapGet("/api/notifications", async (HttpContext http, INotificationService notificationService) =>
{
    var user = http.User;
    var idClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(idClaim) || !long.TryParse(idClaim, out var userId))
        return Results.Unauthorized();

    var notes = await notificationService.GetUserNotificationsAsync(userId);
    var payload = notes.Select(n => new
    {
        n.NotificationId,
        n.Title,
        n.Message,
        n.OrderId,
        n.IsRead,
        CreatedAt = n.CreatedAt
    });

    return Results.Ok(payload);
}).RequireAuthorization();

app.MapPost("/api/notifications/markreadall", async (HttpContext http, INotificationService notificationService) =>
{
    var user = http.User;
    var idClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(idClaim) || !long.TryParse(idClaim, out var userId))
        return Results.Unauthorized();

    var notes = await notificationService.GetUserNotificationsAsync(userId);
    var unread = notes?.Where(n => !n.IsRead).ToList() ?? new List<MakeForYou.BusinessLogic.Entities.Notification>();

    foreach (var n in unread)
    {
        await notificationService.MarkAsReadAsync(n.NotificationId);
    }

    return Results.Ok();
}).RequireAuthorization();

app.Run();
