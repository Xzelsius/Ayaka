// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.Detection;

using Ayaka.MultiTenancy.AspNetCore.Detection;
using Microsoft.AspNetCore.Http;

public sealed class FromRequestHostStrategyTest
{
    [Fact]
    public async Task Does_return_null_when_no_host_is_available()
    {
        var httpRequest = A.Fake<HttpRequest>();
        A.CallTo(() => httpRequest.Host).Returns(new HostString(string.Empty));

        var httpContext = A.Fake<HttpContext>();
        A.CallTo(() => httpContext.Request).Returns(httpRequest);

        var actual = await new FromRequestHostStrategy().TryDetectAsync(httpContext);

        actual.Should().BeNull();
    }

    [Fact]
    public async Task Does_return_null_when_host_has_insufficient_parts()
    {
        var httpRequest = A.Fake<HttpRequest>();
        A.CallTo(() => httpRequest.Host).Returns(new HostString("localhost"));

        var httpContext = A.Fake<HttpContext>();
        A.CallTo(() => httpContext.Request).Returns(httpRequest);

        var actual = await new FromRequestHostStrategy().TryDetectAsync(httpContext);

        actual.Should().BeNull();
    }

    [Theory]
    [InlineData("example.com", "example")]
    [InlineData("sub.example.com", "sub")]
    [InlineData("deep-sub.sub.example.com", "deep-sub")]
    public async Task Does_return_left_most_part_as_tenant_identifier(string hostValue, string expectedTenantId)
    {
        var httpRequest = A.Fake<HttpRequest>();
        A.CallTo(() => httpRequest.Host).Returns(new HostString(hostValue));

        var httpContext = A.Fake<HttpContext>();
        A.CallTo(() => httpContext.Request).Returns(httpRequest);

        var actual = await new FromRequestHostStrategy().TryDetectAsync(httpContext);

        actual.Should().Be(expectedTenantId);
    }
}
