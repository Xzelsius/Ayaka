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

        options.Should().NotBeNull("IOptions<RequestTenancyOptions> should be registered");
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

            options.DetectionStrategies.Should().ContainSingle();

            var strategy = options.DetectionStrategies[0];

            strategy.Should().BeOfType<FromRequestHeaderStrategy>();
            strategy.As<FromRequestHeaderStrategy>().HeaderName.Should().Be("foo");
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

            options.DetectionStrategies.Should().HaveCount(2);
            options.DetectionStrategies.Should().AllBeOfType<FromRequestHeaderStrategy>();

            options.DetectionStrategies[0].As<FromRequestHeaderStrategy>().HeaderName.Should().Be("foo");
            options.DetectionStrategies[1].As<FromRequestHeaderStrategy>().HeaderName.Should().Be("bar");
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

            options.DetectionStrategies.Should().ContainSingle();
            options.DetectionStrategies.Should().AllBeOfType<FromRequestHostStrategy>();
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

            options.DetectionStrategies.Should().ContainSingle();
            options.DetectionStrategies.Should().AllBeOfType<TestStrategy>();
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

            options.DetectionStrategies.Should().ContainSingle();
            options.DetectionStrategies.Should().AllBeOfType<TestStrategyWithDependency>();

            options.DetectionStrategies[0].As<TestStrategyWithDependency>().Dependency.Should().BeSameAs(service);
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

            options.DetectionStrategies.Should().ContainSingle();
            options.DetectionStrategies[0].Should().BeSameAs(strategy);
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
