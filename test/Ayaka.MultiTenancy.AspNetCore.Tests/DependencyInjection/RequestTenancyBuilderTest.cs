// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.DependencyInjection;

using Ayaka.MultiTenancy.AspNetCore.Detection;
using Ayaka.MultiTenancy.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public sealed class RequestTenancyBuilderTest
{
    [Fact]
    public void Does_add_options()
    {
        var builder = new TestMultiTenancyBuilder();

        builder.ConfigureRequestTenancy(_ => { });

        var sp = builder.Services.BuildServiceProvider();
        var options = sp.GetService<IOptions<RequestTenancyOptions>>();

        options.ShouldNotBeNull("IOptions<RequestTenancyOptions> should be registered");
    }

    public sealed class DetectFromRequestHeader
    {
        [Fact]
        public void Does_add_FromRequestHeaderStrategy()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.ConfigureRequestTenancy(opts => opts.DetectFromRequestHeader("foo"));

            var sp = builder.Services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<RequestTenancyOptions>>().Value;

            options.DetectionStrategies.ShouldHaveSingleItem();

            var strategy = options.DetectionStrategies[0];

            strategy.ShouldBeOfType<FromRequestHeaderStrategy>()
                .HeaderName.ShouldBe("foo");
        }

        [Fact]
        public void Allows_multiple_to_be_added()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.ConfigureRequestTenancy(opts =>
            {
                opts.DetectFromRequestHeader("foo");
                opts.DetectFromRequestHeader("bar");
            });

            var sp = builder.Services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<RequestTenancyOptions>>().Value;

            options.DetectionStrategies.Count.ShouldBe(2);
            options.DetectionStrategies.ShouldAllBe(x => x is FromRequestHeaderStrategy);

            ((FromRequestHeaderStrategy)options.DetectionStrategies[0]).HeaderName.ShouldBe("foo");
            ((FromRequestHeaderStrategy)options.DetectionStrategies[1]).HeaderName.ShouldBe("bar");
        }
    }

    public sealed class DetectFromRequestHost
    {
        [Fact]
        public void Does_add_FromRequestHostStrategy()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.ConfigureRequestTenancy(opts => opts.DetectFromRequestHost());

            var sp = builder.Services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<RequestTenancyOptions>>().Value;

            options.DetectionStrategies.ShouldHaveSingleItem().ShouldBeOfType<FromRequestHostStrategy>();
        }
    }

    public sealed class DetectUsingType
    {
        [Fact]
        public void Does_add_strategy()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.ConfigureRequestTenancy(opts => opts.DetectUsing<TestStrategy>());

            var sp = builder.Services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<RequestTenancyOptions>>().Value;

            options.DetectionStrategies.ShouldHaveSingleItem().ShouldBeOfType<TestStrategy>();
        }

        [Fact]
        public void Does_resolve_dependencies_from_service_provider()
        {
            var builder = new TestMultiTenancyBuilder();

            var service = new Dependency();
            builder.Services.AddSingleton<IDependency>(service);

            builder.ConfigureRequestTenancy(opts => opts.DetectUsing<TestStrategyWithDependency>());

            var sp = builder.Services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<RequestTenancyOptions>>().Value;

            var strategy = options.DetectionStrategies.ShouldHaveSingleItem();

            strategy.ShouldBeOfType<TestStrategyWithDependency>()
                .Dependency.ShouldBeSameAs(service);
        }
    }

    public sealed class DetectUsingInstance
    {
        [Fact]
        public void Does_add_strategy()
        {
            var builder = new TestMultiTenancyBuilder();

            var strategy = new TestStrategy();
            builder.ConfigureRequestTenancy(opts => opts.DetectUsing(strategy));

            var sp = builder.Services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<RequestTenancyOptions>>().Value;

            options.DetectionStrategies.ShouldHaveSingleItem();
            options.DetectionStrategies[0].ShouldBeSameAs(strategy);
        }
    }

    public sealed class ActivityTagName
    {
        [Fact]
        public void Does_have_default_activity_tag_name()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.ConfigureRequestTenancy(opts => { });

            var sp = builder.Services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<RequestTenancyOptions>>().Value;

            options.ActivityTagName.ShouldBe("tenant");
        }

        [Fact]
        public void Does_set_activity_tag_name()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.ConfigureRequestTenancy(opts => opts.UseCustomActivityTagName("custom"));

            var sp = builder.Services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<RequestTenancyOptions>>().Value;

            options.ActivityTagName.ShouldBe("custom");
        }
    }

    private sealed class TestMultiTenancyBuilder : IMultiTenancyBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }

    private sealed class TestStrategy : ITenantDetectionStrategy
    {
        public Task<string?> TryDetectAsync(HttpContext context) => throw new NotImplementedException();
    }

    private sealed class TestStrategyWithDependency : ITenantDetectionStrategy
    {
        public TestStrategyWithDependency(IDependency dependency)
        {
            Dependency = dependency;
        }

        public IDependency Dependency { get; }

        public Task<string?> TryDetectAsync(HttpContext context) => throw new NotImplementedException();
    }

    private interface IDependency;

    private sealed class Dependency : IDependency;
}
