using System;
using Microsoft.AspNetCore.Hosting;
using System.Data.Common;
using System.Linq;
using Api;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Helpers;

namespace Tests;

public class IntegrationTestEnvironment : IDisposable
{
    public readonly string DatabaseName = Guid.NewGuid().ToString().Replace("-", "");
    public readonly WebApplicationFactory<Startup> App;
    public readonly HttpClient Client;
    
    public IntegrationTestEnvironment()
    {
        App = new WebApplicationFactory<Startup>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IStartupFilter, MiddlewareInjectingFilter>();
                    
                    // Remove the app's DatabaseContext registration. @link: https://stackoverflow.com/a/59463192
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DatabaseContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<DatabaseContext>(options =>
                    {
                        // @link: https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases
                        var connection = new SqliteConnection($"Data Source={DatabaseName};Mode=Memory;Cache=Shared;");

                        connection.Open();
                        
                        var dbOptions = options.UseSqlite(connection).Options;

                        using var context = new DatabaseContext(dbOptions);

                        context.Database.EnsureCreated();
                    });
                });
            });

        Client = App.CreateClient();
    }

    public void Dispose()
    {
        App.Dispose();
        Client.Dispose();
    }
}