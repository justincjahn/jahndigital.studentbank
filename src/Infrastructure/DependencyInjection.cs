using JahnDigital.StudentBank.Application;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Application.Roles.Services;
using JahnDigital.StudentBank.Application.Transactions.Services;
using JahnDigital.StudentBank.Infrastructure.Authentication.Services;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.Infrastructure.Roles.Services;
using JahnDigital.StudentBank.Infrastructure.Transactions.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JahnDigital.StudentBank.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        AppConfig appConfig = configuration.GetRequiredSection("AppConfig").Get<AppConfig>();

        #if !DEBUG
        if (string.IsNullOrEmpty(appConfig.Secret))
        {
            throw new ArgumentNullException(nameof(appConfig),
                "AppConfig__Secret must be provided as an environment variable.");
        }
        #endif

        SqlAuthenticationProvider.SetProvider(
            SqlAuthenticationMethod.ActiveDirectoryInteractive,
            new SqlAppAuthenticationProvider()
        );

        services.AddPooledDbContextFactory<AppDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            );

            options.UseLoggerFactory(loggerFactory);
        });

        // NOTE: HotChocolate may run queries concurrently and AddScoped will give mediatr only one context per request instead of one per thread.
        services.AddTransient<IAppDbContext>(provider =>
        {
            var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            return factory.CreateDbContext();
        });

        services
            .AddScoped<IPasswordHasher, MsIdentityPasswordHasher>()
            .AddScoped<IDbInitializerService, DbInitializerService>()
            .AddScoped<IJwtTokenGenerator>(_ => new JwtTokenService(appConfig.Secret, appConfig.TokenLifetime))
            .AddScoped<IRoleService, RoleService>()
            .AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}
