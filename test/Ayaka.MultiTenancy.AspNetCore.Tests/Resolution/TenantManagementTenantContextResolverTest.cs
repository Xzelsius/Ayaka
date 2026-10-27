// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.Resolution;

using Ayaka.MultiTenancy.AspNetCore.Resolution;
using Ayaka.MultiTenancy.Management;

public sealed class TenantManagementTenantContextResolverTest
{
    [Fact]
    public async Task Returns_null_when_the_tenant_is_unknown()
    {
        var manager = A.Fake<ITenantManager>();
        var cancellationToken = TestContext.Current.CancellationToken;
        A.CallTo(() => manager.GetAsync("unknown", cancellationToken)).Returns((Tenant?)null);
        var resolver = new TenantManagementTenantContextResolver(manager);

        var result = await resolver.ResolveAsync("unknown", cancellationToken);

        result.ShouldBeNull();
        A.CallTo(() => manager.GetAsync("unknown", cancellationToken)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Returns_a_context_with_the_known_tenants_id_and_display_name()
    {
        var manager = A.Fake<ITenantManager>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenant = new Tenant("acme", "Acme Corporation");
        A.CallTo(() => manager.GetAsync("ACME", cancellationToken)).Returns(tenant);
        var resolver = new TenantManagementTenantContextResolver(manager);

        var result = await resolver.ResolveAsync("ACME", cancellationToken);

        result.ShouldBe(new TenantContext(tenant.Id, tenant.DisplayName));
        A.CallTo(() => manager.GetAsync("ACME", cancellationToken)).MustHaveHappenedOnceExactly();
    }
}
