// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.DependencyInjection;

using Ayaka.MultiTenancy.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public sealed class ApplicationBuilderExtensionsTest
{
    [Fact]
    public void Throws_InvalidOperationException_if_services_are_missing()
    {
        using var host = new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder.UseTestServer();
                webHostBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                });
                webHostBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseMultiTenancy();
                });
            })
            .Build();

        var action = () => host.Start();

        action
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "Unable to find the required services. " +
                "Please add all the required services by calling 'IServiceCollection.AddMultiTenancy()' in the application startup code.");
    }
}
