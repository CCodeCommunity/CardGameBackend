namespace Tests.Helpers;

using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public record ScopedService<T>(T Service, IServiceScope Scope) : IDisposable
{
    public static ScopedService<T> GetService<S>(WebApplicationFactory<S> appFactory) where S : class
    {
        var scope = appFactory.Services.CreateScope();
        return new (scope.ServiceProvider.GetRequiredService<T>(), scope);
    }

    public void Dispose()
    {
        if (Service is IDisposable s) s.Dispose();
        Scope.Dispose();
    }
}