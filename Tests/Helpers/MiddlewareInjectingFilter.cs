namespace Tests.Helpers;

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

public class MiddlewareInjectingFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<FakeRemoteIpAddressMiddleware>();
            next(app);
        };
    }
}