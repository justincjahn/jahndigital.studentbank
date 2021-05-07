using System.Reflection;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using jahndigital.studentbank.server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using jahndigital.studentbank.server.Permissions;
using jahndigital.studentbank.dal.Contexts;
using HotChocolate;
using HotChocolate.Execution.Configuration;
using HotChocolate.AspNetCore;
using jahndigital.studentbank.utils;
using jahndigital.studentbank.server.GraphQL.Types;
using HotChocolate.Types;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging;

namespace jahndigital.studentbank.server
{
    public class Startup
    {
        public static readonly ILoggerFactory factory = LoggerFactory.Create(builder => {
            builder.AddConsole();
        });

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appConfig = Configuration.GetSection("AppConfig");
            if (string.IsNullOrEmpty(appConfig.Get<AppConfig>().Secret)) {
                throw new ArgumentNullException("AppConfig__Secret must be provided as an environment variable.");
            }

            services.Configure<AppConfig>(appConfig);

            var tokenKey = Encoding.ASCII.GetBytes(appConfig.Get<AppConfig>().Secret);

            services.AddDbContext<AppDbContext, SqliteDbContext>(options => {
                options.UseSqlite(Configuration.GetConnectionString("Default"));
                options.UseLoggerFactory(factory);
            });

            services.AddHttpContextAccessor();

            services.AddScoped<IDbInitializerService, DbInitializerService>();

            services.AddGraphQL(
                SchemaBuilder.New()
                    .AddQueryType<GraphQL.ObjectTypes.QueryType>()
                        .AddType<GraphQL.Queries.GroupQueries>()
                        .AddType<GraphQL.Queries.InstanceQueries>()
                        .AddType<GraphQL.Queries.ProductQueries>()
                        .AddType<GraphQL.Queries.PurchaseQueries>()
                        .AddType<GraphQL.Queries.ShareQueries>()
                        .AddType<GraphQL.Queries.ShareTypeQueries>()
                        .AddType<GraphQL.Queries.StockHistoryQueries>()
                        .AddType<GraphQL.Queries.StockQueries>()
                        .AddType<GraphQL.ObjectTypes.StockQueriesType>()
                        .AddType<GraphQL.Queries.StudentQueries>()
                        .AddType<GraphQL.Queries.StudentStockQueries>()
                        .AddType<GraphQL.Queries.TransactionQueries>()
                        .AddType<GraphQL.Queries.UserQueries>()
                    .AddMutationType<GraphQL.ObjectTypes.MutationType>()
                        .AddType<GraphQL.Mutations.GroupMutations>()
                        .AddType<GraphQL.Mutations.InstanceMutations>()
                        .AddType<GraphQL.Mutations.ProductMutations>()
                        .AddType<GraphQL.Mutations.PurchaseMutations>()
                        .AddType<GraphQL.Mutations.ShareMutations>()
                        .AddType<GraphQL.Mutations.ShareTypeMutations>()
                        .AddType<GraphQL.Mutations.StockMutations>()
                        .AddType<GraphQL.Mutations.StudentMutations>()
                        .AddType<GraphQL.Mutations.StudentStockMutations>()
                        .AddType<GraphQL.Mutations.TransactionMutations>()
                        .AddType<GraphQL.Mutations.UserMutations>()
                    .AddAuthorizeDirectiveType()
                    .AddType<GraphQL.ObjectTypes.GroupType>()
                    .AddType<GraphQL.ObjectTypes.InstanceType>()
                    .AddType<GraphQL.ObjectTypes.ProductType>()
                    .AddType<GraphQL.ObjectTypes.PurchaseType>()
                    .AddType<GraphQL.ObjectTypes.ShareType>()
                    .AddType<GraphQL.ObjectTypes.ShareTypeType>()
                    .AddType<GraphQL.ObjectTypes.StockHistoryType>()
                    .AddType<GraphQL.ObjectTypes.StockType>()
                    .AddType<GraphQL.ObjectTypes.StudentType>()
                    .AddType<GraphQL.ObjectTypes.TransactionTypes>()
                    .AddType<GraphQL.ObjectTypes.UserTypes>()
                    .AddType(new PaginationAmountType(100))
                    .BindClrType<int, IntType>()
                    .BindClrType<Money, MoneyType>()
                    .BindClrType<Rate, RateType>()
                    .Create(),
                new QueryExecutionOptions { ForceSerialExecution = true }
            );

            services.AddControllers();

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config => {
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

            // Add an authz handler that validates the authenticated user has a permission.
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // Add an authz handler that ensures users can only access their own data.
            services
                .AddScoped<IAuthorizationHandler, DataOwnerAuthorizationHandler>()
                .AddScoped<IAuthorizationHandler, DataOwnerAuthorizationHandlerGraphQL>();

            services.AddAuthorization();

            services
                .AddScoped<IUserService, UserService>()
                .AddScoped<IStudentService, StudentService>()
                .AddScoped<IRoleService, RoleService>()
                .AddScoped<ITransactionService, TransactionService>();

            services.AddErrorFilter<GraphQL.ErrorFilter>();

            #if DEBUG
                services.AddSwaggerGen(options => {
                    options.SwaggerDoc("v1",  new Microsoft.OpenApi.Models.OpenApiInfo {
                        Title = "Student Bank API",
                        Description = "API for Student Bank",
                        Version = "v1"
                    });

                    OpenApiSecurityScheme securityDefinition = new OpenApiSecurityScheme()
                    {
                        Name = "Bearer",
                        BearerFormat = "JWT",
                        Scheme = "bearer",
                        Description = "Specify the authorization token.",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                    };

                    options.AddSecurityDefinition("jwt_auth", securityDefinition);

                    OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference()
                        {
                            Id = "jwt_auth",
                            Type = ReferenceType.SecurityScheme
                        }
                    };

                    OpenApiSecurityRequirement securityRequirements = new OpenApiSecurityRequirement()
                    {
                        {securityScheme, new string[] { }},
                    };

                    options.AddSecurityRequirement(securityRequirements);

                    var fileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var filePath = System.IO.Path.Combine(AppContext.BaseDirectory, fileName);
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

            app.UseCors(o => o
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            #if DEBUG
                app.UseSwagger();

                app.UseSwaggerUI(options => {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Bank API");
                });
            #endif

            app.UseAuthorization();

            app.UseGraphQL("/graphql");

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            dbInitializer.Initialize();
            dbInitializer.SeedData();
        }
    }
}
