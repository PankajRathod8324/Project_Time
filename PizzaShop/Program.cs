using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using DinkToPdf;
using DinkToPdf.Contracts;
using Entities.Middleware;
using DAL.Repository;
using DAL.Interfaces;
using BLL.Interfaces;
using BLL.Services;
using Entities.Models;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// -------------------- DB Context --------------------
var conn = builder.Configuration.GetConnectionString("PizzaShopDbConnection");
builder.Services.AddDbContext<PizzaShopContext>(q => q.UseNpgsql(conn));

// -------------------- Dependency Injection --------------------
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ITableAndSectionRepository, TableAndSectionRepository>();
builder.Services.AddScoped<ITableAndSectionService, TableAndSectionService>();
builder.Services.AddScoped<ITaxRepository, TaxRepository>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAccountManagerOrderAppRepository, AccountManagerOrderAppRepository>();
builder.Services.AddScoped<IAccountManagerOrderAppService, AccountManagerOrderAppService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// -------------------- JWT Authentication --------------------
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
            RoleClaimType =  ClaimTypes.Role,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["AuthToken"];
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Redirect("/Loginpage");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.Redirect("/Loginpage");
                }
                context.HandleResponse(); // prevents default 401 response
                return Task.CompletedTask;
            }
        };
    });


// builder.Services.AddControllersWithViews(options =>
// {
// var policy = new AuthorizationPolicyBuilder()
//     .RequireAuthenticatedUser()
//     .Build();
// options.Filters.Add(new AuthorizeFilter(policy));
// });


// 🔐 Optional: Cookie auth fallback (if needed for compatibility with [Authorize])
// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options =>
//     {
//         options.LoginPath = "/Loginpage";
//         options.AccessDeniedPath = "/AccessDenied";
//     });

// -------------------- Role/Permission Policies --------------------
builder.Services.AddAuthorization(options =>
{
    // Role-based
    options.AddPolicy("SuperAdminPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Super Admin"));
    options.AddPolicy("AccountManagerPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Account Manager"));
    options.AddPolicy("ChefPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Chef"));
    options.AddPolicy("ChefOrAccountManagerPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Chef", "Account Manager"));

    // Permission-based
    using (var scope = builder.Services.BuildServiceProvider().CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<PizzaShopContext>();
        var requiredPermissions = context.Permissions
            .Select(p => p.PermissionName)
            .Distinct()
            .ToList();

        foreach (var permission in requiredPermissions)
        {
            options.AddPolicy($"{permission}ViewPolicy", policy =>
                policy.Requirements.Add(new PermissionRequirement($"{permission}_View")));
            options.AddPolicy($"{permission}EditPolicy", policy =>
                policy.Requirements.Add(new PermissionRequirement($"{permission}_Edit")));
            options.AddPolicy($"{permission}DeletePolicy", policy =>
                policy.Requirements.Add(new PermissionRequirement($"{permission}_Delete")));
        }
    }
});


var app = builder.Build();

// -------------------- Middleware Pipeline --------------------
app.UseMiddleware<GlobalExceptionMiddleware>();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();  // ⬅️ Make sure this is before UseAuthorization
app.UseAuthorization();

// WebSocket support (optional)
app.UseWebSockets();

// -------------------- Routes --------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
