// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.Resolution;

using Ayaka.MultiTenancy.AspNetCore.Resolution;

public sealed class DetectOnlyTenantContextResolverTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("acme")]
    [InlineData("Acme-123")]
    [InlineData(" acme ")]
    public async Task Wraps_the_id_as_the_tenant_context_id_and_display_name(string id)
    {
        var resolver = new DetectOnlyTenantContextResolver();

        var result = await resolver.ResolveAsync(id, TestContext.Current.CancellationToken);

        result.ShouldBe(new TenantContext(id, id));
    }
}
