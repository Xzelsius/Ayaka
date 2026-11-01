// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public sealed class HybridCacheTenantCacheOptionsAutoValidatorTest
{
    public sealed class Validate
    {
        [Theory]
        [InlineData(60)]
        [InlineData(300)]
        [InlineData(3600)]
        public void Accepts_an_expiration_between_one_minute_and_one_hour(int seconds)
        {
            var validator = new HybridCacheTenantCacheOptionsAutoValidator();
            var options = new HybridCacheTenantCacheOptions { Expiration = TimeSpan.FromSeconds(seconds) };

            var result = validator.Validate(null, options);

            result.Succeeded.ShouldBeTrue();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(59)]
        [InlineData(3601)]
        public void Rejects_an_expiration_outside_one_minute_to_one_hour(int seconds)
        {
            var validator = new HybridCacheTenantCacheOptionsAutoValidator();
            var options = new HybridCacheTenantCacheOptions { Expiration = TimeSpan.FromSeconds(seconds) };

            var result = validator.Validate(null, options);

            result.Failed.ShouldBeTrue();
            result.FailureMessage.ShouldContain(nameof(HybridCacheTenantCacheOptions.Expiration));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("billing-api")]
        [InlineData("Billing_API.v2")]
        [InlineData("tenant")]
        [InlineData("tenants")]
        [InlineData("my-tenant")]
        [InlineData("0123456789012345678901234567890123456789012345678901234567890123")]
        public void Accepts_a_valid_key_prefix(string? keyPrefix)
        {
            var validator = new HybridCacheTenantCacheOptionsAutoValidator();
            var options = new HybridCacheTenantCacheOptions { KeyPrefix = keyPrefix };

            var result = validator.Validate(null, options);

            result.Succeeded.ShouldBeTrue();
        }

        [Theory]
        [InlineData("billing:api")]
        [InlineData("billing api")]
        [InlineData("billing/api")]
        [InlineData("bílling")]
        [InlineData("01234567890123456789012345678901234567890123456789012345678901234")]
        public void Rejects_an_invalid_key_prefix(string keyPrefix)
        {
            var validator = new HybridCacheTenantCacheOptionsAutoValidator();
            var options = new HybridCacheTenantCacheOptions { KeyPrefix = keyPrefix };

            var result = validator.Validate(null, options);

            result.Failed.ShouldBeTrue();
            result.FailureMessage.ShouldContain(nameof(HybridCacheTenantCacheOptions.KeyPrefix));
        }
    }
}
