// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.Detection;

using Ayaka.MultiTenancy.AspNetCore.Detection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

public sealed class FromRequestHeaderStrategyTest
{
    [Fact]
    public void Getting_HeaderName_returns_the_header_name()
    {
        var actual = new FromRequestHeaderStrategy("X-Tenant-Id");

        actual.HeaderName.Should().Be("X-Tenant-Id");
    }

    [Fact]
    public async Task Does_return_null_when_no_header_is_available()
    {
        var httpRequest = A.Fake<HttpRequest>();
        A.CallTo(() => httpRequest.Headers).Returns(new HeaderDictionary());

        var httpContext = A.Fake<HttpContext>();
        A.CallTo(() => httpContext.Request).Returns(httpRequest);

        var actual = await new FromRequestHeaderStrategy("X-Tenant-Id").TryDetectAsync(httpContext);

        actual.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Does_return_null_when_header_has_empty_value(string? headerValue)
    {
        var httpRequest = A.Fake<HttpRequest>();
        A.CallTo(() => httpRequest.Headers)
            .Returns(
                new HeaderDictionary(
                    new Dictionary<string, StringValues>
                    {
                        { "X-Tenant-Id", headerValue }
                    }));

        var httpContext = A.Fake<HttpContext>();
        A.CallTo(() => httpContext.Request).Returns(httpRequest);

        var actual = await new FromRequestHeaderStrategy("X-Tenant-Id").TryDetectAsync(httpContext);

        actual.Should().BeNull();
    }

    [Fact]
    public async Task Does_return_header_value_as_tenant_identifier()
    {
        var httpRequest = A.Fake<HttpRequest>();
        A.CallTo(() => httpRequest.Headers)
            .Returns(
                new HeaderDictionary(
                    new Dictionary<string, StringValues>
                    {
                        { "X-Tenant-Id", "example" }
                    }));

        var httpContext = A.Fake<HttpContext>();
        A.CallTo(() => httpContext.Request).Returns(httpRequest);

        var actual = await new FromRequestHeaderStrategy("X-Tenant-Id").TryDetectAsync(httpContext);

        actual.Should().Be("example");
    }

    [Fact]
    public async Task Does_return_first_header_value_as_tenant_identifier_when_multiple_values_are_available()
    {
        var httpRequest = A.Fake<HttpRequest>();
        A.CallTo(() => httpRequest.Headers)
            .Returns(
                new HeaderDictionary(
                    new Dictionary<string, StringValues>
                    {
                        { "X-Tenant-Id", new StringValues(["example", "example2"]) }
                    }));

        var httpContext = A.Fake<HttpContext>();
        A.CallTo(() => httpContext.Request).Returns(httpRequest);

        var actual = await new FromRequestHeaderStrategy("X-Tenant-Id").TryDetectAsync(httpContext);

        actual.Should().Be("example");
    }
}
