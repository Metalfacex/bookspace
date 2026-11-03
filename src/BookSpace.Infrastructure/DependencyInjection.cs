using BookSpace.Core.Abstractions;
using BookSpace.Core.Rules;
using BookSpace.Core.Services;
using BookSpace.Infrastructure.Data;
using BookSpace.Infrastructure.Repositories;
using BookSpace.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookSpace.Infrastructure;

public static class DependencyInjection
{
    /// <summary>არეგისტრირებს ბაზას, რეპოზიტორიებს და დომენის სერვისებს.</summary>
    public static IServiceCollection AddBookSpaceInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection არ არის მითითებული.");
        var provider = configuration["Database:Provider"] ?? "SqlServer";

        services.AddDbContext<BookSpaceDbContext>(options =>
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        var bookingRules = configuration.GetSection("BookSpace:Booking").Get<BookingRules>() ?? new BookingRules();
        var accountRules = configuration.GetSection("BookSpace:Account").Get<AccountRules>() ?? new AccountRules();
        services.AddSingleton(bookingRules);
        services.AddSingleton(accountRules);

        services.AddSingleton<IClock>(new SystemClock(configuration["BookSpace:TimeZone"] ?? "Asia/Tbilisi"));
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddScoped<AuthService>();
        services.AddScoped<RoomService>();
        services.AddScoped<AvailabilityService>();
        services.AddScoped<BookingService>();
        services.AddScoped<ReminderService>();
        services.AddScoped<StatisticsService>();

        return services;
    }
}
