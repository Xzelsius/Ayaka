// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.DependencyInjection;

using Ayaka.MultiTenancy.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public sealed class MultiTenancyBuilderExtensionsTest
{
    [Fact]
    public void Does_return_same_instance_of_IMultiTenancyBuilder()
    {
        var builder = new TestMultiTenancyBuilder();

        var result = builder.ConfigureRequestTenancy(_ => { });

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void Does_use_specified_IMultiTenancyBuilder_to_configure_request_tenancy()
    {
        var builder = new TestMultiTenancyBuilder();

        builder.ConfigureRequestTenancy(requestTenancy =>
        {
            requestTenancy.MultiTenancy.Should().BeSameAs(builder);
        });
    }

    private sealed class TestMultiTenancyBuilder : IMultiTenancyBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }
}
