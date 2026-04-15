using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services;
using MakeForYou.Repositories.Repository;
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



//builder.Services.AddScoped<AuthService>();

// Repository & service registrations
builder.Services.AddScoped<IProductRepository, ProductRepository>();
// Đăng ký thêm Service của bạn ở đây
builder.Services.AddScoped<IHomeService, HomeService>();

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

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
