// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.DependencyInjection;

using Ayaka.MultiTenancy.DependencyInjection;
using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.DependencyInjection;

public sealed class MultiTenancyBuilderExtensionsTest
{
    public sealed class AddTenantManagement
    {
        [Fact]
        public void Does_return_instance_of_TenantManagementBuilder()
        {
            var builder = new TestMultiTenancyBuilder();

            var tenantManagementBuilder = builder.AddTenantManagement();

            tenantManagementBuilder.ShouldBeOfType<TenantManagementBuilder>();
        }

        [Fact]
        public void Does_use_specified_IMultiTenancyBuilder_to_configure_tenant_management()
        {
            var builder = new TestMultiTenancyBuilder();

            var tenantManagementBuilder = builder.AddTenantManagement();

            tenantManagementBuilder.MultiTenancy.ShouldBeSameAs(builder);
        }

        [Fact]
        public void Does_add_default_services()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.AddTenantManagement();

            var manager = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantManager));
            manager.ShouldNotBeNull("ITenantManager should be registered");

            var cache = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantCache));
            cache.ShouldNotBeNull("ITenantCache should be registered");
            cache.ImplementationType.ShouldBe(typeof(NoopTenantCache));
        }

        [Fact]
        public void Does_not_add_default_services_twice()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.AddTenantManagement();
            builder.AddTenantManagement();

            builder.Services.Where(x => x.ServiceType == typeof(ITenantManager)).ShouldHaveSingleItem();
            builder.Services.Where(x => x.ServiceType == typeof(ITenantCache)).ShouldHaveSingleItem();
        }
    }

    private sealed class TestMultiTenancyBuilder : IMultiTenancyBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }
}
