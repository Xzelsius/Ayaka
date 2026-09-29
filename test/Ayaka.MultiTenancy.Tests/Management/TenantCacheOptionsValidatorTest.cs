// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public sealed class TenantCacheOptionsValidatorTest
{
    public sealed class Validate
    {
        [Theory]
        [InlineData(60)]
        [InlineData(300)]
        [InlineData(3600)]
        public void Does_accept_an_expiration_between_one_minute_and_one_hour(int seconds)
        {
            var validator = new TenantCacheOptionsValidator();
            var options = new TenantCacheOptions { Expiration = TimeSpan.FromSeconds(seconds) };

            var result = validator.Validate(null, options);

            result.Succeeded.ShouldBeTrue();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(59)]
        [InlineData(3601)]
        public void Does_reject_an_expiration_outside_one_minute_to_one_hour(int seconds)
        {
            var validator = new TenantCacheOptionsValidator();
            var options = new TenantCacheOptions { Expiration = TimeSpan.FromSeconds(seconds) };

            var result = validator.Validate(null, options);

            result.Failed.ShouldBeTrue();
            result.FailureMessage.ShouldBe(
                $"TenantCacheOptions.Expiration must be between 00:01:00 and 01:00:00, but was {options.Expiration}.");
        }
    }
}
