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
        public void Does_add_in_memory_tenant_store()
        {
            var builder = new TestTenantManagementBuilder();

            builder.UseInMemoryStore();

            var store = builder.MultiTenancy.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantStore));

            store.Should().NotBeNull("ITenantStore should be registered");
            store.ImplementationType.Should().Be(typeof(InMemoryTenantStore));
        }

        [Fact]
        public void Does_not_replace_user_registered_tenant_store()
        {
            var builder = new TestTenantManagementBuilder();
            builder.MultiTenancy.Services.AddSingleton<ITenantStore, TestTenantStore>();

            builder.UseInMemoryStore();

            var store = builder.MultiTenancy.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantStore));

            store.Should().NotBeNull("ITenantStore should be registered");
            store.ImplementationType.Should().Be(typeof(TestTenantStore));
        }
    }

    private sealed class TestMultiTenancyBuilder : IMultiTenancyBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }

    private sealed class TestTenantManagementBuilder : ITenantManagementBuilder
    {
        public IMultiTenancyBuilder MultiTenancy { get; } = new TestMultiTenancyBuilder();
    }

    private sealed class TestTenantStore : ITenantStore
    {
        public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
