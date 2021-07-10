using System;
using System.Linq;
using System.Reflection;
using System.Text;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.GraphQL;
using jahndigital.studentbank.server.GraphQL.Mutations;
using jahndigital.studentbank.server.GraphQL.ObjectTypes;
using jahndigital.studentbank.server.GraphQL.Queries;
using jahndigital.studentbank.server.GraphQL.Types;
using jahndigital.studentbank.server.Jobs;
using jahndigital.studentbank.server.Permissions;
using jahndigital.studentbank.services;
using jahndigital.studentbank.services.Interfaces;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using Path = System.IO.Path;

namespace jahndigital.studentbank.server
{
    public class Startup
    {
        public static readonly ILoggerFactory Factory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appConfig = Configuration.GetSection("AppConfig");

            if (string.IsNullOrEmpty(appConfig.Get<AppConfig>().Secret)) {
                throw new ArgumentNullException(nameof(appConfig),
                    "AppConfig__Secret must be provided as an environment variable.");
            }

            services.Configure<AppConfig>(appConfig);

            var tokenKey = Encoding.ASCII.GetBytes(appConfig.Get<AppConfig>().Secret);

            if (appConfig.Get<AppConfig>().DbDriver == "sqlite") {
                services.AddDbContext<AppDbContext, SqliteDbContext>(options => {
                    options.UseSqlite(Configuration.GetConnectionString("Default"));
                    options.UseLoggerFactory(Factory);
                });
            } else {
                string connectionString = Configuration.GetConnectionString("Default");
                SqlAuthenticationProvider.SetProvider(SqlAuthenticationMethod.ActiveDirectoryInteractive,
                    new SqlAppAuthenticationProvider());
                services.AddDbContext<AppDbContext>(options => {
                    options.UseSqlServer(connectionString);
                    options.UseLoggerFactory(Factory);
                });
            }

            services.AddHttpContextAccessor();

            services.AddScoped<IDbInitializerService, DbInitializerService>();

            services.AddGraphQL(
                SchemaBuilder.New()
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
                    .AddType<StudentStockQueries>()
                    .AddType<TransactionQueries>()
                    .AddType<UserQueries>()
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
                    .AddAuthorizeDirectiveType()
                    .AddType<GroupType>()
                    .AddType<InstanceType>()
                    .AddType<ProductType>()
                    .AddType<PurchaseType>()
                    .AddType<ShareType>()
                    .AddType<ShareTypeType>()
                    .AddType<StockHistoryType>()
                    .AddType<StockType>()
                    .AddType<StudentType>()
                    .AddType<TransactionTypes>()
                    .AddType<UserTypes>()
                    .AddType(new PaginationAmountType(100))
                    .BindClrType<int, IntType>()
                    .BindClrType<Money, MoneyType>()
                    .BindClrType<Rate, RateType>()
                    .Create(),
                new QueryExecutionOptions {ForceSerialExecution = true}
            );

            services.AddQuartz(q => {
                q.SchedulerId = "jahndigital.studentbank.server";
                q.UseMicrosoftDependencyInjectionJobFactory();

                var dailyJobKey = new JobKey("DailyJob");
                q.AddJob<DailyJob>(opts => opts.WithIdentity(dailyJobKey));
                q.AddTrigger(opts => {
                    opts.ForJob(dailyJobKey)
                        .WithIdentity("DailyJob-trigger")
                        .WithCronSchedule("0 0 0 1/1 * ? *");
                });

                q.ScheduleJob<DailyJob>(trigger => trigger.StartNow());
            });

            services.AddQuartzServer(options => { options.WaitForJobsToComplete = true; });

            services.AddControllers();

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config => {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                config.TokenValidationParameters = new TokenValidationParameters {
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
            services.AddAuthorization(options => {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(
                        context => !PreauthorizationHandler.AssertPreauthenticated(context)
                    )
                    .Build();
            });

            services
                .AddScoped<IUserService>(provider => new UserService(provider.GetService<AppDbContext>()!,
                    appConfig.Get<AppConfig>().Secret, appConfig.Get<AppConfig>().TokenLifetime))
                .AddScoped<IStudentService>(provider => new StudentService(provider.GetService<AppDbContext>()!,
                    appConfig.Get<AppConfig>().Secret, appConfig.Get<AppConfig>().TokenLifetime))
                .AddScoped<IRoleService, RoleService>()
                .AddScoped<ITransactionService, TransactionService>()
                .AddScoped<IShareTypeService, ShareTypeService>();

            services.AddErrorFilter<ErrorFilter>();

            #if DEBUG
            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "Student Bank API",
                    Description = "API for Student Bank",
                    Version = "v1"
                });

                OpenApiSecurityScheme securityDefinition = new() {
                    Name = "Bearer",
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Description = "Specify the authorization token.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http
                };

                options.AddSecurityDefinition("jwt_auth", securityDefinition);

                OpenApiSecurityScheme securityScheme = new() {
                    Reference = new OpenApiReference {
                        Id = "jwt_auth",
                        Type = ReferenceType.SecurityScheme
                    }
                };

                OpenApiSecurityRequirement securityRequirements = new() {
                    {securityScheme, new string[] { }}
                };

                options.AddSecurityRequirement(securityRequirements);

                var fileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                options.IncludeXmlComments(filePath);
            });
            #endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializerService dbInitializer)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            var config = Configuration
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

            #if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Bank API"); });
            #endif

            app.UseAuthorization();

            app.UseGraphQL("/graphql");

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            dbInitializer.Initialize();
            dbInitializer.SeedData();
        }
    }
}