using System.Reflection;
using System.Threading.RateLimiting;
using BookSpace.Infrastructure;
using BookSpace.Infrastructure.Data;
using BookSpace.Web.Background;
using BookSpace.Web.Configuration;
using BookSpace.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // CSRF დაცვა ყველა MVC ფორმისთვის (API კონტროლერები ცალკე [IgnoreAntiforgeryToken]-ით).
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "BookSpace.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // API-ზე გადამისამართების ნაცვლად 401/403 ვაბრუნებთ.
        options.Events.OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

// REST API-ს დოკუმენტაცია (OpenAPI / Swagger): /swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BookSpace API",
        Version = "v1",
        Description = "სასწავლო ოთახების დაჯავშნის REST API. ავტორიზაცია — იგივე cookie, რაც ვებ-გვერდზე (/Account/Login)."
    });
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml))
    {
        options.IncludeXmlComments(xml);
    }
});

// Rate limiting: API-ზე ერთ მომხმარებელს/IP-ს ფანჯარაში მაქსიმუმ N მოთხოვნა, შემდეგ 429.
builder.Services.Configure<ApiRateLimitOptions>(builder.Configuration.GetSection(ApiRateLimitOptions.SectionName));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        await context.HttpContext.Response.WriteAsJsonAsync(
            new ErrorDto("TooManyRequests", "ძალიან ბევრი მოთხოვნა. სცადეთ ცოტა ხანში."), token);
    };
    options.AddPolicy(ApiRateLimitOptions.PolicyName, context =>
    {
        var settings = context.RequestServices.GetRequiredService<IOptions<ApiRateLimitOptions>>().Value;
        var key = context.User.Identity?.IsAuthenticated == true
            ? "user:" + context.User.Identity.Name
            : "ip:" + (context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = settings.PermitLimit,
            Window = TimeSpan.FromSeconds(settings.WindowSeconds),
            QueueLimit = 0
        });
    });
});

builder.Services.AddBookSpaceInfrastructure(builder.Configuration);

if (builder.Configuration.GetValue("BookSpace:Reminders:Enabled", true))
{
    builder.Services.AddHostedService<ReminderBackgroundService>();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BookSpace API v1");
    options.DocumentTitle = "BookSpace API";
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.Services.InitializeBookSpaceDatabaseAsync();

app.Run();

public partial class Program
{
}
