// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests;

using Ayaka.MultiTenancy.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public sealed class ServiceCollectionExtensionsTest
{
    public sealed class AddMultiTenancy
    {
        [Fact]
        public void Does_return_instance_of_MultiTenancyBuilder()
        {
            var services = new ServiceCollection();

            var builder = services.AddMultiTenancy();

            builder.ShouldBeOfType<MultiTenancyBuilder>();
        }

        [Fact]
        public void Does_use_specified_service_collection()
        {
            var services = new ServiceCollection();

            var builder = services.AddMultiTenancy();

            builder.Services.ShouldBeSameAs(services);
        }

        [Fact]
        public void Does_add_default_services()
        {
            var services = new ServiceCollection();

            services.AddMultiTenancy();

            var accessor = services.FirstOrDefault(x => x.ServiceType == typeof(ITenantContextAccessor));
            accessor.ShouldNotBeNull("ITenantContextAccessor should be registered");
        }

        [Fact]
        public void Does_not_add_default_services_twice()
        {
            var services = new ServiceCollection();

            services.AddMultiTenancy();
            services.AddMultiTenancy();

            services.Count(x => x.ServiceType == typeof(ITenantContextAccessor)).ShouldBe(1);
        }
    }

    public sealed class AddTenantContextAccessor
    {
        [Fact]
        public void Does_add_default_TenantContextAccessor()
        {
            var services = new ServiceCollection();

            services.AddTenantContextAccessor();

            var accessor = services.FirstOrDefault(x => x.ServiceType == typeof(ITenantContextAccessor));

            accessor.ShouldNotBeNull();
            accessor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }

        [Fact]
        public void Does_not_replace_user_registered_service()
        {
            var services = new ServiceCollection();
            services.AddScoped<ITenantContextAccessor, TestTenantContextAccessor>();

            services.AddTenantContextAccessor();

            var accessor = services.FirstOrDefault(x => x.ServiceType == typeof(ITenantContextAccessor));
            accessor.ShouldNotBeNull();
            accessor.Lifetime.ShouldBe(ServiceLifetime.Scoped);

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider
                .GetRequiredService<ITenantContextAccessor>()
                .ShouldBeOfType<TestTenantContextAccessor>();
        }

        [Fact]
        public void Does_allow_chaining()
        {
            var services = new ServiceCollection();

            services.AddTenantContextAccessor().ShouldBeSameAs(services);
        }

        private sealed class TestTenantContextAccessor : ITenantContextAccessor
        {
            public TenantContext? TenantContext { get; set; }
        }
    }
}
