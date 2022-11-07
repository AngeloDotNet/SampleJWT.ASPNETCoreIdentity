using System.Reflection;
using System.Text;
using Identity.Authentication;
using Identity.Authentication.Entities;
using Identity.Authentication.Requirements;
using Identity.BusinessLayer.Services;
using Identity.BusinessLayer.Settings;
using Identity.WebApp.Services;
using Identity.WebApp.StartupTasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace Identity.WebApp;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var jwtSettings = Configure<JwtSettings>(nameof(JwtSettings));

        services.AddControllers();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Identity Sample",
                Version = "v1"
            });

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Insert the Bearer Token",
                Name = HeaderNames.Authorization,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference= new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            options.IncludeXmlComments(xmlPath);
        });

        var databaseInMemory = Configuration.GetSection("DatabaseInMemory").GetValue<bool>("enabled");

        if (databaseInMemory)
        {
            services.AddDbContext<AuthenticationDbContext>(option => option.UseInMemoryDatabase("Identity"));
        }
        else
        {
            var connectionString = Configuration.GetSection("ConnectionStrings").GetValue<string>("Default");
            services.AddSqlServer<AuthenticationDbContext>(connectionString);
        }

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
        })
        .AddEntityFrameworkStores<AuthenticationDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddScoped<IAuthorizationHandler, UserActiveHandler>();

        services.AddAuthorization(options =>
        {
            var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
            policyBuilder.Requirements.Add(new UserActiveRequirement());
            options.FallbackPolicy = options.DefaultPolicy = policyBuilder.Build();

            options.AddPolicy("SuperApplication", policy =>
            {
                policy.RequireClaim(CustomClaimTypes.ApplicationId, "42");
            });

            options.AddPolicy("Administrator", policy =>
            {
                policy.RequireRole(RoleNames.Administrator);
            });

            options.AddPolicy("PowerUser", policy =>
            {
                policy.RequireRole(RoleNames.PowerUser);
            });
        });

        // Uncomment if you want to use the old password hashing format for both login and registration.
        //services.Configure<PasswordHasherOptions>(options =>
        //{
        //    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2;
        //});

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserService, HttpUserService>();

        services.AddHostedService<AuthenticationStartupTask>();

        T Configure<T>(string sectionName) where T : class
        {
            var section = Configuration.GetSection(sectionName);
            var settings = section.Get<T>();
            services.Configure<T>(section);

            return settings;
        }
    }

    public void Configure(WebApplication app)
    {
        IWebHostEnvironment env = app.Environment;

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity v1"));
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}