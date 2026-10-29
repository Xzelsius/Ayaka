// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests;

using System.Net;
using System.Net.Http.Json;
using Ayaka.MultiTenancy.AspNetCore.Detection;
using Ayaka.MultiTenancy.AspNetCore.Resolution;
using Ayaka.MultiTenancy.AspNetCore.Tests.Internal;
using Ayaka.MultiTenancy.DependencyInjection;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HttpGet = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

public abstract class RequestTenancyMiddlewareTest
{
    [Fact]
    public async Task Skips_detection_if_no_endpoint_matches()
    {
        var strategy = A.Fake<ITenantDetectionStrategy>();
        var resolver = A.Fake<ITenantContextResolver>();

        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(strategy)
                            .ResolveUsing(resolver);
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/does-not-exist", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        A.CallTo(() => strategy.TryDetectAsync(A<HttpContext>._)).MustNotHaveHappened();
        A.CallTo(() => resolver.ResolveAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
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
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy("test"))
                            .ResolveUsing(new StaticContextResolver(
                                new TenantContext("test", "Acme"),
                                predicate: id => id == "test"));
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-disabled", TestContext.Current.CancellationToken);

        var content = await response.Content.ReadFromJsonAsync<TenancyResponse>(TestContext.Current.CancellationToken);
        content.ShouldBeEquivalentTo(new TenancyResponse(null));
    }

    [Fact]
    public async Task Throws_if_tenant_context_is_already_set()
    {
        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy("test"))
                            .ResolveUsing(new StaticContextResolver(
                                new TenantContext("test", "Acme"),
                                predicate: id => id == "test"));
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

                    tenantContextAccessor.TenantContext = new TenantContext("test", "Acme");

                    return next(httpContext);
                });

                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();

        var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken));

        exception.Message.ShouldBe(
            "Tenant was already set previously in the request pipeline");
    }

    [Fact]
    public async Task Does_set_tenant_context_if_tenant_is_detected_and_resolved()
    {
        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy("test"))
                            .ResolveUsing(new StaticContextResolver(
                                new TenantContext("test", "Acme"),
                                predicate: id => id == "test"));
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken);

        var content = await response.Content.ReadFromJsonAsync<TenancyResponse>(TestContext.Current.CancellationToken);
        content.ShouldBeEquivalentTo(new TenancyResponse(new TenantContext("test", "Acme")));
    }

    [Fact]
    public async Task Does_clear_tenant_context_after_request_completed()
    {
        var release = new TaskCompletionSource();
        Task<TenantContext?>? detachedWork = null;

        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy("test"))
                            .ResolveUsing(new StaticContextResolver(new TenantContext("test", "Acme")));
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();
                app.Use((httpContext, next) =>
                {
                    var accessor = httpContext.RequestServices.GetRequiredService<ITenantContextAccessor>();

                    detachedWork = Task.Run(async () =>
                    {
                        await release.Task;
                        return accessor.TenantContext;
                    });

                    return next(httpContext);
                });

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken);

        var content = await response.Content.ReadFromJsonAsync<TenancyResponse>(TestContext.Current.CancellationToken);
        content.ShouldBeEquivalentTo(new TenancyResponse(new TenantContext("test", "Acme")));

        release.SetResult(); // Allow the detached work to run
        var tenantContext = await detachedWork.ShouldNotBeNull();
        tenantContext.ShouldBeNull();
    }

    [Fact]
    public async Task Does_clear_tenant_context_if_request_pipeline_throws()
    {
        var release = new TaskCompletionSource();
        Task<TenantContext?>? detachedWork = null;

        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy("test"))
                            .ResolveUsing(new StaticContextResolver(new TenantContext("test", "Acme")));
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();
                app.Use((HttpContext httpContext, RequestDelegate _) =>
                {
                    var accessor = httpContext.RequestServices.GetRequiredService<ITenantContextAccessor>();

                    detachedWork = Task.Run(async () =>
                    {
                        await release.Task;
                        return accessor.TenantContext;
                    });

                    throw new InvalidOperationException("request failed");
                });

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken));
        exception.Message.ShouldBe("request failed");

        release.SetResult(); // Allow the detached work to run
        var tenantContext = await detachedWork.ShouldNotBeNull();
        tenantContext.ShouldBeNull();
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
                            .DetectUsing(new StaticValueStrategy("test"))
                            .ResolveUsing(
                                new StaticContextResolver(
                                    new TenantContext("test", "Acme"),
                                    predicate: id => id == "test"));
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken);

        var content = await response.Content.ReadFromJsonAsync<TenancyResponse>(TestContext.Current.CancellationToken);
        content.ShouldBeEquivalentTo(new TenancyResponse(new TenantContext("test", "Acme")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rejects_request_with_400_if_tenant_cannot_be_detected(bool useProblemDetails)
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
                            .ResolveUsing(new StaticContextResolver(null));
                    });

                if (useProblemDetails)
                {
                    services.AddProblemDetails();
                }

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        if (useProblemDetails)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

            problemDetails.ShouldNotBeNull();
            problemDetails.Status.ShouldBe((int)HttpStatusCode.BadRequest);
            problemDetails.Title.ShouldBe("Tenant could not be detected.");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rejects_request_with_404_if_tenant_is_unknown(bool useProblemDetails)
    {
        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy("test"))
                            .ResolveUsing(new StaticContextResolver(null));
                    });

                if (useProblemDetails)
                {
                    services.AddProblemDetails();
                }

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        if (useProblemDetails)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

            problemDetails.ShouldNotBeNull();
            problemDetails.Status.ShouldBe((int)HttpStatusCode.NotFound);
            problemDetails.Title.ShouldBe("Tenant is unknown.");
        }
    }

    [Fact]
    public async Task Propagates_exception_from_resolver()
    {
        var resolver = A.Fake<ITenantContextResolver>();
        A.CallTo(() => resolver.ResolveAsync(A<string>._, A<CancellationToken>._))
            .ThrowsAsync(new InvalidOperationException("resolution failed"));

        using var host = await CreateHost(
            services =>
            {
                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy("test"))
                            .ResolveUsing(resolver);
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken));

        exception.Message.ShouldBe("resolution failed");
    }

    [Fact]
    public async Task Does_tag_current_request_Activity_if_tenant_is_detected()
    {
        using var tracker = new ActivityTracker();

        using var host = await CreateHost(
            services =>
            {
                services.AddSingleton(tracker.Source); // override ASP.NET Core's ActivitySource

                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy.DetectUsing(new StaticValueStrategy("test"));
                        requestTenancy.ResolveUsing(
                            new StaticContextResolver(
                                new TenantContext("test", "Acme"),
                                predicate: id => id == "test"));
                        requestTenancy.UseCustomActivityTagName("custom");
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        tracker.StoppedActivities.ShouldHaveSingleItem();
        tracker.StoppedActivities[0].Tags.ShouldContain(x => x.Key == "custom" && x.Value == "test", 1);
    }

    [Fact]
    public async Task Does_not_tag_Activity_if_DisableMultiTenancyAttribute_is_present()
    {
        using var tracker = new ActivityTracker();

        using var host = await CreateHost(
            services =>
            {
                services.AddSingleton(tracker.Source); // override ASP.NET Core's ActivitySource

                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy("test"))
                            .ResolveUsing(new StaticContextResolver(
                                new TenantContext("test", "Acme"),
                                predicate: id => id == "test"));
                        requestTenancy.UseCustomActivityTagName("custom");
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-disabled", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        tracker.StoppedActivities.ShouldNotBeEmpty();
        tracker.StoppedActivities.ShouldNotContain(activity => activity.Tags.Any(tag => tag.Key == "custom"));
    }

    [Fact]
    public async Task Does_not_tag_Activity_if_tenant_cannot_be_detected()
    {
        using var tracker = new ActivityTracker();

        using var host = await CreateHost(
            services =>
            {
                services.AddSingleton(tracker.Source); // override ASP.NET Core's ActivitySource

                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy(null))
                            .ResolveUsing(new StaticContextResolver(null));

                        requestTenancy.UseCustomActivityTagName("custom");
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        tracker.StoppedActivities.ShouldNotBeEmpty();
        tracker.StoppedActivities.ShouldNotContain(activity => activity.Tags.Any(tag => tag.Key == "custom"));
    }

    [Fact]
    public async Task Does_not_tag_Activity_if_tenant_is_unknown()
    {
        using var tracker = new ActivityTracker();

        using var host = await CreateHost(
            services =>
            {
                services.AddSingleton(tracker.Source); // override ASP.NET Core's ActivitySource

                services
                    .AddMultiTenancy()
                    .ConfigureRequestTenancy(requestTenancy =>
                    {
                        requestTenancy
                            .DetectUsing(new StaticValueStrategy("test"))
                            .ResolveUsing(new StaticContextResolver(null));

                        requestTenancy.UseCustomActivityTagName("custom");
                    });

                ConfigureServices(services);
            },
            app =>
            {
                app.UseMultiTenancy();

                ConfigureApp(app);
            });

        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/tenancy-enabled", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        tracker.StoppedActivities.ShouldNotBeEmpty();
        tracker.StoppedActivities.ShouldNotContain(activity => activity.Tags.Any(tag => tag.Key == "custom"));
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
                endpoints.MapGet("/tenancy-enabled", GetTenant);
                endpoints.MapGet("/tenancy-disabled", GetTenantDisabled);
            });

            return;

            static IResult GetTenant([FromServices] ITenantContextAccessor accessor)
                => TypedResults.Json(new TenancyResponse(accessor.TenantContext));

            [DisableMultiTenancy]
            static IResult GetTenantDisabled([FromServices] ITenantContextAccessor accessor)
                => TypedResults.Json(new TenancyResponse(accessor.TenantContext));
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
                            requestTenancy
                                .DetectUsing(new StaticValueStrategy("test"))
                                .ResolveUsing(new StaticContextResolver(new TenantContext("test", "test")));
                        });

                    ConfigureServices(services);
                },
                app =>
                {
                    app.UseMultiTenancy();

                    ConfigureApp(app);
                });

            using var client = host.GetTestClient();
            using var response = await client.GetAsync("/inherited-tenancy-disabled", TestContext.Current.CancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TenancyResponse>(TestContext.Current.CancellationToken);
            content.ShouldBeEquivalentTo(new TenancyResponse(null));
        }

        private sealed class TestController : ControllerBase
        {
            [HttpGet("/tenancy-enabled")]
            public IActionResult GetTenant([FromServices] ITenantContextAccessor accessor)
                => Ok(new TenancyResponse(accessor.TenantContext));

            [DisableMultiTenancy]
            [HttpGet("/tenancy-disabled")]
            public IActionResult GetTenantDisabled([FromServices] ITenantContextAccessor accessor)
                => Ok(new TenancyResponse(accessor.TenantContext));
        }

        [DisableMultiTenancy]
        private sealed class InheritanceTestController : ControllerBase
        {
            [HttpGet("/inherited-tenancy-disabled")]
            public IActionResult GetTenant([FromServices] ITenantContextAccessor accessor)
                => Ok(new TenancyResponse(accessor.TenantContext));
        }
    }

    public sealed class WhenUsingFastEndpoints : RequestTenancyMiddlewareTest
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddFastEndpoints(opts =>
            {
                opts.DisableAutoDiscovery = true;
                opts.Assemblies = [typeof(WhenUsingFastEndpoints).Assembly];
            });
        }

        protected override void ConfigureApp(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapFastEndpoints();
            });
        }

        private sealed class GetTenantEndpoint : EndpointWithoutRequest
        {
            private readonly ITenantContextAccessor _accessor;

            public GetTenantEndpoint(ITenantContextAccessor accessor)
            {
                _accessor = accessor;
            }

            public override void Configure()
            {
                Get("/tenancy-enabled");
                AllowAnonymous();
            }

            public override Task HandleAsync(CancellationToken ct)
                => Send.OkAsync(new TenancyResponse(_accessor.TenantContext), cancellation: ct);
        }

        private sealed class GetTenantDisabledEndpoint : EndpointWithoutRequest
        {
            private readonly ITenantContextAccessor _accessor;

            public GetTenantDisabledEndpoint(ITenantContextAccessor accessor)
            {
                _accessor = accessor;
            }

            public override void Configure()
            {
                Get("/tenancy-disabled");
                AllowAnonymous();
                Options(o => o.WithMetadata(new DisableMultiTenancyAttribute()));
            }

            public override Task HandleAsync(CancellationToken ct)
                => Send.OkAsync(new TenancyResponse(_accessor.TenantContext), cancellation: ct);
        }
    }

    private static Task<IHost> CreateHost(
        Action<IServiceCollection> configureServices,
        Action<IApplicationBuilder> configureApp,
        bool useRouting = true)
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
                    if (useRouting)
                    {
                        app.UseRouting();
                    }

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

    private sealed class StaticContextResolver : ITenantContextResolver
    {
        private readonly TenantContext? _context;
        private readonly Func<string, bool>? _predicate;

        public StaticContextResolver(TenantContext? context, Func<string, bool>? predicate = null)
        {
            _context = context;
            _predicate = predicate;
        }

        public ValueTask<TenantContext?> ResolveAsync(string id, CancellationToken cancellationToken)
            => _predicate == null || _predicate.Invoke(id)
                ? ValueTask.FromResult(_context)
                : ValueTask.FromResult<TenantContext?>(null);
    }

    private sealed record TenancyResponse(TenantContext? TenantContext);
}
