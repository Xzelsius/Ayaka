// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests;

using Microsoft.Extensions.DependencyInjection;

public sealed class ServiceCollectionExtensionsTest
{
    public sealed class AddTenantContextAccessor
    {
        [Fact]
        public void Does_add_default_TenantContextAccessor()
        {
            var services = new ServiceCollection();

            services.AddTenantContextAccessor();

            var accessor = services.FirstOrDefault(x => x.ServiceType == typeof(ITenantContextAccessor));

            accessor.Should().NotBeNull();
            accessor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void Does_not_replace_user_registered_service()
        {
            var services = new ServiceCollection();
            services.AddScoped<ITenantContextAccessor, TestTenantContextAccessor>();

            services.AddTenantContextAccessor();

            var accessor = services.FirstOrDefault(x => x.ServiceType == typeof(ITenantContextAccessor));
            accessor.Should().NotBeNull();
            accessor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider
                .GetRequiredService<ITenantContextAccessor>()
                .Should()
                .BeOfType<TestTenantContextAccessor>();
        }

        [Fact]
        public void Does_allow_chaining()
        {
            var services = new ServiceCollection();

            services.AddTenantContextAccessor().Should().BeSameAs(services);
        }

        private sealed class TestTenantContextAccessor : ITenantContextAccessor
        {
            public TenantContext? TenantContext { get; set; }
        }
    }
}
