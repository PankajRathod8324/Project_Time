using System.Threading.Tasks.Dataflow;
using Microsoft.EntityFrameworkCore;
using Entities.ViewModel;
using Entities.Models;
using DAL.Interfaces;
using BLL.Interfaces;
using BLL.Services;
using DAL.Repository;
using BLL.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Hosting.Server;
using DinkToPdf.Contracts;
using DinkToPdf;
using Entities.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var conn = builder.Configuration.GetConnectionString("PizzaShopDbConnection");
builder.Services.AddDbContext<PizzaShopContext>(q => q.UseNpgsql(conn));
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>(); // Registering the UserService here
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ITableAndSectionRepository, TableAndSectionRepository>();
builder.Services.AddScoped<ITableAndSectionService, TableAndSectionService>();
builder.Services.AddScoped<ITaxRepository, TaxRepository>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAccountManagerOrderAppRepository, AccountManagerOrderAppRepository>();
builder.Services.AddScoped<IAccountManagerOrderAppService, AccountManagerOrderAppService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// builder.Services.ConfigureApplicationCookie(options => options.LoginPath = "/LoginPage");
// RotativaConfiguration.Setup("wwwroot\\"); // Path for wkhtmltopdf binaries

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));



// Enable Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30); // 30 min session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// // Add Identity
// builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//     .AddEntityFrameworkStores<ApplicationDbContext>()
//     .AddDefaultTokenProviders();


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // RoleClaimType = ClaimTypes.Role
            RoleClaimType = "role"

        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["JwtToken"];
                return Task.CompletedTask;
            },
        };
    });


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/AccessDenied";
        options.AccessDeniedPath = "/AccessDenied";

    });


builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// 🔥 Configure Role-Based Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminPolicy", policy => policy.RequireClaim("role", "Super Admin"));
    options.AddPolicy("AccountManagerPolicy", policy => policy.RequireClaim("role", "Account Manager"));
    options.AddPolicy("ChefPolicy", policy => policy.RequireClaim("role", "Chef"));
    options.AddPolicy("ChefOrAccountManagerPolicy", policy => policy.RequireClaim("role", "Chef", "Account Manager"));
});

using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PizzaShopContext>();
    var requiredPermissions = context.Permissions.Select(p => p.PermissionName).Distinct().ToList(); // Fetch unique permissions

    builder.Services.AddAuthorization(options =>
    {
        foreach (var permission in requiredPermissions)
        {
            options.AddPolicy($"{permission}ViewPolicy", policy => policy.Requirements.Add(new PermissionRequirement($"{permission}_View")));
            options.AddPolicy($"{permission}EditPolicy", policy => policy.Requirements.Add(new PermissionRequirement($"{permission}_Edit")));
            options.AddPolicy($"{permission}DeletePolicy", policy => policy.Requirements.Add(new PermissionRequirement($"{permission}_Delete")));
        }
    });
}



var app = builder.Build();


app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStaticFiles();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404)
    {
        context.Response.Redirect("/NotFound");
    }
});
// var pizzaShopContext = _context.Users.Include(u => u.Role);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


