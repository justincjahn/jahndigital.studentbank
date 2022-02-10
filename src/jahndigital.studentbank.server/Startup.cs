using System;
using System.Linq;
using System.Text;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;
using JahnDigital.StudentBank.Application;
using JahnDigital.StudentBank.Domain.ValueObjects;
using JahnDigital.StudentBank.Infrastructure;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using jahndigital.studentbank.server.GraphQL;
using jahndigital.studentbank.server.GraphQL.Mutations;
using jahndigital.studentbank.server.GraphQL.ObjectTypes;
using jahndigital.studentbank.server.GraphQL.Queries;
using jahndigital.studentbank.server.GraphQL.Types;
using jahndigital.studentbank.server.Jobs;
using jahndigital.studentbank.server.Permissions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Quartz;

namespace jahndigital.studentbank.server
{
    public class Startup
    {
        private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => { builder.AddConsole(); });

        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddApplication()
                .AddInfrastructure(Configuration, LoggerFactory);
            
            AppConfig appConfig = Configuration.GetRequiredSection("AppConfig").Get<AppConfig>();

            #if !DEBUG
            if (string.IsNullOrEmpty(appConfig.Secret))
            {
                throw new ArgumentNullException(nameof(appConfig),
                    "AppConfig__Secret must be provided as an environment variable.");
            }
            #endif

            byte[]? tokenKey = Encoding.ASCII.GetBytes(appConfig.Secret);

            services.AddHttpContextAccessor();

            services
                .AddGraphQLServer()
                .AddQueryType<QueryType>()
                .AddType<GroupQueries>()
                .AddType<InstanceQueries>()
                .AddType<ProductQueries>()
                .AddType<PurchaseQueries>()
                .AddType<ShareQueries>()
                .AddType<ShareTypeQueries>()
                .AddType<StockHistoryQueries>()
                .AddType<StockQueries>()
                .AddType<StockQueriesType>()
                .AddType<StudentQueries>()
                .AddType<UserQueries>()
                .AddType<StudentStockQueries>()
                .AddType<TransactionQueries>()
                .AddMutationType<MutationType>()
                .AddType<GroupMutations>()
                .AddType<InstanceMutations>()
                .AddType<ProductMutations>()
                .AddType<PurchaseMutations>()
                .AddType<ShareMutations>()
                .AddType<ShareTypeMutations>()
                .AddType<StockMutations>()
                .AddType<StudentMutations>()
                .AddType<StudentStockMutations>()
                .AddType<TransactionMutations>()
                .AddType<UserMutations>()
                .AddType<GroupType>()
                .AddType<InstanceType>()
                .AddType<ProductType>()
                .AddType<PurchaseType>()
                .AddType<ShareType>()
                .AddType<ShareTypeType>()
                .AddType<StockHistoryType>()
                .AddType<StockType>()
                .AddType<StudentType>()
                .AddType<StudentStockType>()
                .AddType<TransactionTypes>()
                .AddType<UserTypes>()
                .AddType<AuthenticateResponseType>()
                .BindRuntimeType<int, IntType>()
                .BindRuntimeType<Money, MoneyType>()
                .BindRuntimeType<Rate, RateType>()
                .AddProjections()
                .AddSorting()
                .AddFiltering()
                .AddAuthorization()
                .SetPagingOptions(new PagingOptions
                {
                    MaxPageSize = 10000, DefaultPageSize = 100, IncludeTotalCount = true
                })
                .ModifyRequestOptions(options =>
                {
                    options.ExecutionTimeout = TimeSpan.FromMinutes(10);
                });

            services.AddQuartz(q =>
            {
                q.SchedulerId = "jahndigital.studentbank.server";
                q.UseMicrosoftDependencyInjectionJobFactory();

                JobKey? dailyJobKey = new JobKey("DailyJob");
                q.AddJob<DailyJob>(opts => opts.WithIdentity(dailyJobKey));
                q.AddTrigger(opts =>
                {
                    opts.ForJob(dailyJobKey)
                        .WithIdentity("DailyJob-trigger")
                        .WithCronSchedule("0 0 0 1/1 * ? *");
                });

                q.ScheduleJob<DailyJob>(trigger => trigger.StartNow());
            });

            services.AddQuartzServer(options => { options.WaitForJobsToComplete = true; });

            services.AddControllers();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config =>
            {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

            // Dynamically register policies for built-in permissions.
            services.AddSingleton<IAuthorizationPolicyProvider, AggregatePolicyProvider>();

            // Add an authz handler that ensures users can only access their own data.
            services
                .AddScoped<IAuthorizationHandler, PreauthorizationHandler>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>()
                .AddScoped<IAuthorizationHandler, DataOwnerAuthorizationHandler>()
                .AddScoped<IAuthorizationHandler, DataOwnerAuthorizationHandlerGraphQL>();

            // By default, don't allow preauthenticated users access to protected resources
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(
                        context => !PreauthorizationHandler.AssertPreauthenticated(context)
                    )
                    .Build();
            });

            services.AddErrorFilter<ErrorFilter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializerService dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            string[]? config = Configuration
                .GetSection("AllowedOrigins")
                .GetChildren()
                .Select(x => x.Value)
                .ToArray();

            app.UseCors(o => o
                .WithHeaders("X-Requested-With", "X-HTTP-Method-Override", "Content-Type", "Accepts", "Authorization",
                    "User-Agent")
                .WithOrigins(config)
                .WithMethods("GET", "POST", "OPTIONS")
                .AllowCredentials());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(x => x.MapGraphQL());

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            dbInitializer.Initialize();
            dbInitializer.SeedData();
        }
    }
}
