using System.IO;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using jahndigital.studentbank.server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

namespace jahndigital.studentbank.server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appConfig = Configuration.GetSection("AppConfig");
            if (appConfig.Get<AppConfig>().Secret == null) {
                throw new ArgumentNullException("AppSetting__Secret must be provided as an environment variable.");
            }

            services.Configure<AppConfig>(appConfig);

            var tokenKey = Encoding.ASCII.GetBytes(appConfig.Get<AppConfig>().Secret);

            services.AddDbContext<AppDbContext>(options => {
                options.UseSqlite(Configuration.GetConnectionString("Default"));
            });

            services.AddScoped<IDbInitializer, DbInitializer>();

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
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });

            services.AddScoped<IUserService, UserService>();

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
                    var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                    options.IncludeXmlComments(filePath);
                });
            #endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            #if DEBUG
                app.UseSwagger();

                app.UseSwaggerUI(options => {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Bank API");
                });
            #endif

            dbInitializer.Initialize();
            dbInitializer.SeedData();
        }
    }
}