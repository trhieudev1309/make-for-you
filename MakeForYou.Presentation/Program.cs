using System.Security.Claims;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Hubs;
using MakeForYou.BusinessLogic.Implement;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services;
using MakeForYou.BusinessLogic.Services.Implement;
using MakeForYou.BusinessLogic.Services.Implementations;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.Repositories.Interfaces;
using MakeForYou.Repositories.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Razor Pages ───────────────────────────────────────────────────────────────
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
    });

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// ── Repository & service registrations ───────────────────────────────────────
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<ISellerRepository, SellerRepository>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<PortfolioService, PortfolioService>();

builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
builder.Services.AddScoped<IQuotationService, QuotationService>();


builder.Services.AddHttpClient<IGhnLocationService, GhnLocationService>();

builder.Services.AddScoped<ISellerService, SellerService>();
// SignalR
builder.Services.AddSignalR();

builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<ICustomizationRepository, CustomizationRepository>();
builder.Services.AddScoped<ICustomizationService, CustomizationService>();
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPayoutService, PayoutService>();

builder.Services.AddScoped<ISellerPostRepository, SellerPostRepository>();
builder.Services.AddScoped<ISellerPostService, SellerPostService>();

// ── GHN ───────────────────────────────────────────────────────────────────────
// 1. Bind GhnOptions from appsettings.json → "Ghn" section
builder.Services.Configure<GhnOptions>(
    builder.Configuration.GetSection(GhnOptions.SectionName));

// 2. Register typed HttpClient for IGhnApiClient
//    GhnApiClient sets BaseAddress, Token, and ShopId headers in its constructor.
builder.Services.AddHttpClient<IGhnApiClient, GhnApiClient>();

// 3. Keep the named "GHN" client for GhnStoreService (existing code — no headers
//    set here; GhnStoreService reads Token from IConfiguration itself).
builder.Services.AddHttpClient("GHN", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// 4. GhnStoreService (existing store-management service)
builder.Services.AddScoped<IGhnStoreService, GhnStoreService>();

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── Auth ──────────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
        opts.LoginPath = "/Auth/Login";
        opts.LogoutPath = "/Auth/Logout";
        opts.AccessDeniedPath = "/Auth/AccessDenied";
        opts.ExpireTimeSpan = TimeSpan.FromHours(8);
        opts.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ─────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

// Apply pending EF Core migrations at startup
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<MakeForYou.BusinessLogic.Hubs.ChatHub>("/hubs/chat");

// ── Notification minimal APIs ─────────────────────────────────────────────────
app.MapGet("/api/notifications", async (HttpContext http, INotificationService notificationService) =>
{
    var idClaim = http.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
    var idClaim = http.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(idClaim) || !long.TryParse(idClaim, out var userId))
        return Results.Unauthorized();

    var notes = await notificationService.GetUserNotificationsAsync(userId);
    var unread = notes?.Where(n => !n.IsRead).ToList()
                 ?? new List<MakeForYou.BusinessLogic.Entities.Notification>();

    foreach (var n in unread)
        await notificationService.MarkAsReadAsync(n.NotificationId);

    return Results.Ok();
}).RequireAuthorization();


// ── TEMP: SCRUM-41 smoke test — REMOVE before merge ──────────────────────────
app.MapGet("/dev/ghn-ping", async (IGhnApiClient ghn) =>
{
    try
    {
        var doc = await ghn.GetAsync("/shiip/public-api/master-data/province");
        var first = doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("ProvinceName")
            .GetString();
        return Results.Ok(new { status = "OK", firstProvince = first });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();
