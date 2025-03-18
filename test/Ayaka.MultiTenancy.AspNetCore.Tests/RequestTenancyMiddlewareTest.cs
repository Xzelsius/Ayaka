// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests;

using Ayaka.MultiTenancy.AspNetCore.Detection;
using Ayaka.MultiTenancy.AspNetCore.Tests.Internal;
using Ayaka.MultiTenancy.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public abstract class RequestTenancyMiddlewareTest
{
    [Fact]
    public async Task Throws_if_tenant_context_was_set_previously()
    {
        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy.DetectUsing(new StaticValueStrategy("test"));
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.Use((httpContext, next) =>
                {
                    var tenantContextAccessor = httpContext
                        .RequestServices
                        .GetRequiredService<ITenantContextAccessor>();

                    tenantContextAccessor.TenantContext = new TenantContext("test", "test");

                    return next(httpContext);
                });

                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();

        var action = () => client.GetAsync("/");

        await action
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Tenant was already set previously in the request pipeline");
    }

    [Fact]
    public async Task Throws_if_no_tenant_detection_strategies_are_configured()
    {
        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(_ => { });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();

        var action = () => client.GetAsync("/tenancy-enabled");

        await action
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("No tenant detection strategies are configured");
    }

    [Fact]
    public async Task Does_skip_detection_if_DisableMultiTenancyAttribute_is_present()
    {
        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy.DetectUsing(new StaticValueStrategy("test"));
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-disabled");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be("no tenant", "because the tenant id should not be set");
    }

    [Fact]
    public async Task Does_set_tenant_context_if_tenant_is_detected()
    {
        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy.DetectUsing(new StaticValueStrategy("test"));
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled");

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be("test", "because the tenant id should be set");
    }

    [Fact]
    public async Task Does_consume_all_available_detection_strategies()
    {
        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy(null))
                            .DetectUsing(new StaticValueStrategy("test"));
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be("test", "because the tenant id should be set");
    }

    [Fact]
    public async Task Does_not_set_tenant_if_unable_to_detect()
    {
        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy.DetectUsing(new StaticValueStrategy(null));
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be("no tenant", "because the tenant id should not be set");
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected virtual void ConfigureApp(IApplicationBuilder app)
    {
    }

    public sealed class WhenUsingEndpoints : RequestTenancyMiddlewareTest
    {
        protected override void ConfigureApp(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/tenancy-enabled", GetTenantId);
                endpoints.MapGet("/tenancy-disabled", GetTenantIdDisabled);
            });

            return;

            static IResult GetTenantId([FromServices] ITenantContextAccessor accessor)
                => TypedResults.Content(accessor.TenantContext?.Id ?? "no tenant");

            [DisableMultiTenancy]
            static IResult GetTenantIdDisabled([FromServices] ITenantContextAccessor accessor)
                => TypedResults.Content(accessor.TenantContext?.Id ?? "no tenant");
        }
    }

    public sealed class WhenUsingControllers : RequestTenancyMiddlewareTest
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddTestControllers(typeof(TestController), typeof(InheritanceTestController));
        }

        protected override void ConfigureApp(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        [Fact]
        public async Task Does_skip_detection_if_controller_has_DisableMultiTenancyAttribute()
        {
            using var host = await CreateHost(
                services =>
                {
                    services
                        .AddMultiTenancy()
                        .ConfigureRequestTenancy(requestTenancy =>
                        {
                            requestTenancy.DetectUsing(new StaticValueStrategy("test"));
                        });

                    ConfigureServices(services);
                },
                app =>
                {
                    app.UseMultiTenancy();

                    ConfigureApp(app);
                });

            using var client = host.GetTestClient();
            using var response = await client.GetAsync("/inherited-tenancy-disabled");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Be("no tenant", "because the tenant id should not be set");
        }

        private sealed class TestController : ControllerBase
        {
            [HttpGet("/tenancy-enabled")]
            public IActionResult GetTenantId([FromServices] ITenantContextAccessor accessor)
                => Ok(accessor.TenantContext?.Id ?? "no tenant");

            [DisableMultiTenancy]
            [HttpGet("/tenancy-disabled")]
            public IActionResult GetTenantIdDisabled([FromServices] ITenantContextAccessor accessor)
                => Ok(accessor.TenantContext?.Id ?? "no tenant");
        }

        [DisableMultiTenancy]
        private sealed class InheritanceTestController : ControllerBase
        {
            [HttpGet("/inherited-tenancy-disabled")]
            public IActionResult GetTenantId([FromServices] ITenantContextAccessor accessor)
                => Ok(accessor.TenantContext?.Id ?? "no tenant");
        }
    }

    private static Task<IHost> CreateHost(
        Action<IServiceCollection> configureServices,
        Action<IApplicationBuilder> configureApp)
        => new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder.UseTestServer();
                webHostBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    configureServices(services);
                });
                webHostBuilder.Configure(app =>
                {
                    app.UseRouting();
                    configureApp(app);
                });
            })
            .StartAsync();

    private sealed class StaticValueStrategy : ITenantDetectionStrategy
    {
        private readonly string? _tenantId;

        public StaticValueStrategy(string? tenantId)
        {
            _tenantId = tenantId;
        }

        public Task<string?> TryDetectAsync(HttpContext context) => Task.FromResult(_tenantId);
    }
}
