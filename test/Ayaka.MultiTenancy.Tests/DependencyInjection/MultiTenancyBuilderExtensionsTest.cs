// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.DependencyInjection;

using Ayaka.MultiTenancy.DependencyInjection;
using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            var cache = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantCache));
            manager.ShouldNotBeNull("ITenantManager should be registered");
            cache.ShouldNotBeNull("ITenantCache should be registered");
        }

        [Fact]
        public void Does_not_add_default_services_twice()
        {
            var builder = new TestMultiTenancyBuilder();

            builder.AddTenantManagement();
            builder.AddTenantManagement();

            builder.Services.Count(x => x.ServiceType == typeof(ITenantManager)).ShouldBe(1);
        }

        [Fact]
        public void Does_reject_an_invalid_cache_expiration_on_startup()
        {
            var builder = new TestMultiTenancyBuilder();
            builder.AddTenantManagement();
            builder.Services.Configure<TenantCacheOptions>(options => options.Expiration = TimeSpan.FromSeconds(30));
            using var services = builder.Services.BuildServiceProvider();

            var act = () => services.GetRequiredService<IStartupValidator>().Validate();

            Should.Throw<OptionsValidationException>(act);
        }
    }

    private sealed class TestMultiTenancyBuilder : IMultiTenancyBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }
}
