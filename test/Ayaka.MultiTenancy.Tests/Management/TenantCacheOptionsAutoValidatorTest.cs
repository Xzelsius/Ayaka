// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public sealed class TenantCacheOptionsAutoValidatorTest
{
    public sealed class Validate
    {
        [Theory]
        [InlineData(60)]
        [InlineData(300)]
        [InlineData(3600)]
        public void Accepts_an_expiration_between_one_minute_and_one_hour(int seconds)
        {
            var validator = new TenantCacheOptionsAutoValidator();
            var options = new TenantCacheOptions { Expiration = TimeSpan.FromSeconds(seconds) };

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
            var validator = new TenantCacheOptionsAutoValidator();
            var options = new TenantCacheOptions { Expiration = TimeSpan.FromSeconds(seconds) };

            var result = validator.Validate(null, options);

            result.Failed.ShouldBeTrue();
            result.FailureMessage.ShouldContain(nameof(TenantCacheOptions.Expiration));
        }
    }
}
