// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.DependencyInjection;

using Ayaka.MultiTenancy.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public sealed class MultiTenancyBuilderExtensionsTest
{
    public sealed class AddTenantManagement
    {
        [Fact]
        public void Does_return_instance_of_TenantManagementBuilder()
        {
            var services = new ServiceCollection();
            var builder = services.AddMultiTenancy();

            var tenantManagementBuilder = builder.AddTenantManagement();

            tenantManagementBuilder.Should().NotBeNull();
            tenantManagementBuilder.Should().BeOfType<TenantManagementBuilder>();
        }

        [Fact]
        public void Does_use_service_collection_from_MultiTenancyBuilder()
        {
            var services = new ServiceCollection();
            var builder = services.AddMultiTenancy();

            var tenantManagementBuilder = builder.AddTenantManagement();

            tenantManagementBuilder.Services.Should().BeSameAs(services);
        }
    }
}
