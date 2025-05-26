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

            tenantManagementBuilder.Should().NotBeNull();
            tenantManagementBuilder.Should().BeOfType<TenantManagementBuilder>();
        }

        [Fact]
        public void Does_use_specified_IMultiTenancyBuilder_to_configure_tenant_management()
        {
            var builder = new TestMultiTenancyBuilder();

            var tenantManagementBuilder = builder.AddTenantManagement();

            tenantManagementBuilder.MultiTenancy.Should().BeSameAs(builder);
        }

        [Fact]
        public void Does_add_default_services()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.AddTenantManagement();

            var manager = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantManager));
            manager.Should().NotBeNull("ITenantManager should be registered");
        }

        [Fact]
        public void Does_not_add_default_services_twice()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.AddTenantManagement();
            builder.AddTenantManagement();

            builder.Services.Count(x => x.ServiceType == typeof(ITenantManager)).Should().Be(1);
        }
    }

    private sealed class TestMultiTenancyBuilder : IMultiTenancyBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }
}
