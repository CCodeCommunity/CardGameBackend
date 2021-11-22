using System;
using System.Text;
using Api.Authorization;
using Api.Authorization.Handlers;
using Api.Authorization.Requirements;
using Api.Enums;
using Api.Models;
using Api.Services;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Api;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddDistributedMemoryCache();
            
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); 
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // Note: https://stackoverflow.com/a/48278882
        services.AddHttpContextAccessor();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo {Title = "Api", Version = "v1"});
            options.CustomSchemaIds(type => type.ToString());
        });
        
        // services.Configure<ForwardedHeadersOptions>(options =>
        // {
        //     options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        // });

        var usePsql = Configuration["DatabaseSettings:DbType"] == "Psql";
        
        if (usePsql)
            services.AddDbContext<DatabaseContext>(options =>
                options.UseNpgsql(Configuration["DatabaseSettings:ConnectionString"]));
        else
            services.AddDbContext<DatabaseContext>(builder =>
            {
                // @link: https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases
                var connection = new SqliteConnection($"Data Source=TestSomething;Mode=Memory;Cache=Shared;");

                connection.Open();
                        
                var dbOptions = builder.UseSqlite(connection).Options;
                
                using var context = new DatabaseContext(dbOptions);

                context.Database.EnsureCreated();

                context.Accounts.Add(new Account
                {
                    Id = Nanoid.Nanoid.Generate(),
                    Name = "admin",
                    Email = "admin@cardgame.cc",
                    Password = Argon2.Hash("admin"),
                    Role = AccountRole.Admin,
                    State = AccountState.Active
                });

                context.SaveChangesAsync();
            });
            
        services.AddScoped<AuthTokenService, AuthTokenService>();

        services.AddScoped<AccessTokenTrackingService, AccessTokenTrackingService>();
        
        services.AddScoped<IAuthorizationHandler, DefaultAuthorizationHandler>();
        
        // services.AddScoped<IAuthorizationHandler, AccountIdentityHandler>();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Issuer"],
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.DefaultPolicy, p => p
                .RequireAuthenticatedUser()
                .AddRequirements(new DefaultAuthorization())
            );

            options.AddPolicy(AuthorizationPolicies.RequireAdminOnly, p => p
                .RequireAuthenticatedUser()
                .AddRequirements(new DefaultAuthorization())
                .RequireRole(AuthorizationRoles.Admin));

            options.AddPolicy(AuthorizationPolicies.RequireMatchingAccountId, p => p
                .RequireAuthenticatedUser()
                .AddRequirements(new DefaultAuthorization())
                .AddRequirements(new AccountIdentityAuthorization()));
            
            options.DefaultPolicy = options.GetPolicy(AuthorizationPolicies.DefaultPolicy)!;
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                //.WithOrigins("https://localhost:44351"));
                .AllowCredentials()); // allow credentials
            
            app.UseExceptionHandler("/error-local-development");
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CC Card Game Api v1"));
        }
        else
        {
            app.UseExceptionHandler("/error");
        }
            
        app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSession();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}