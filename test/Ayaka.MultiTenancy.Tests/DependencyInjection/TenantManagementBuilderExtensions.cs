// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.DependencyInjection;

using Ayaka.MultiTenancy.DependencyInjection;
using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.DependencyInjection;

public sealed class TenantManagementBuilderExtensions
{
    public sealed class UseInMemoryStore
    {
        [Fact]
        public void Does_return_same_instance_of_ITenantManagementBuilder()
        {
            var builder = new TestTenantManagementBuilder();

            var result = builder.UseInMemoryStore();

            result.Should().BeSameAs(builder);
        }

        [Fact]
        public void Does_add_in_memory_tenant_storage()
        {
            var builder = new TestTenantManagementBuilder();

            builder.UseInMemoryStore();

            var storage = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantStorage));

            storage.Should().NotBeNull("ITenantStorage should be registered");
            storage.ImplementationType.Should().Be(typeof(InMemoryTenantStorage));
        }

        [Fact]
        public void Does_not_replace_user_registered_tenant_storage()
        {
            var builder = new TestTenantManagementBuilder();
            builder.Services.AddSingleton<ITenantStorage, TestTenantStorage>();

            builder.UseInMemoryStore();

            var storage = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantStorage));

            storage.Should().NotBeNull("ITenantStorage should be registered");
            storage.ImplementationType.Should().Be(typeof(TestTenantStorage));
        }
    }

    private sealed class TestTenantManagementBuilder : ITenantManagementBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }

    private sealed class TestTenantStorage : ITenantStorage
    {
        public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
