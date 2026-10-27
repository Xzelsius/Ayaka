// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests;

using Ayaka.MultiTenancy.AspNetCore.Detection;
using Ayaka.MultiTenancy.AspNetCore.Resolution;

public sealed class RequestTenancyOptionsAutoValidatorTest
{
    [Fact]
    public void Sets_DetectOnlyTenantContextResolver_as_default_resolver()
    {
        var options = new RequestTenancyOptions();

        options.ContextResolver.ShouldNotBeNull();
        options.ContextResolver.ShouldBeOfType<DetectOnlyTenantContextResolver>();
    }

    public sealed class Validate
    {
        [Fact]
        public void Rejects_null_detection_strategies()
        {
            var validator = new RequestTenancyOptionsAutoValidator();
            var options = new RequestTenancyOptions { DetectionStrategies = null! };

            var result = validator.Validate(null, options);

            result.Failed.ShouldBeTrue();
            result.FailureMessage.ShouldContain(nameof(RequestTenancyOptions.DetectionStrategies));
        }

        [Fact]
        public void Rejects_zero_detection_strategies()
        {
            var validator = new RequestTenancyOptionsAutoValidator();
            var options = new RequestTenancyOptions();

            var result = validator.Validate(null, options);

            result.Failed.ShouldBeTrue();
            result.FailureMessage.ShouldContain(nameof(RequestTenancyOptions.DetectionStrategies));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Accepts_one_or_more_detection_strategies(int strategyCount)
        {
            var validator = new RequestTenancyOptionsAutoValidator();
            var options = new RequestTenancyOptions();

            for (var i = 0; i < strategyCount; i++)
            {
                options.DetectionStrategies.Add(A.Fake<ITenantDetectionStrategy>());
            }

            var result = validator.Validate(null, options);

            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Rejects_a_null_context_resolver()
        {
            var validator = new RequestTenancyOptionsAutoValidator();
            var options = new RequestTenancyOptions
            {
                DetectionStrategies = [A.Fake<ITenantDetectionStrategy>()],
                ContextResolver = null!,
            };

            var result = validator.Validate(null, options);

            result.Failed.ShouldBeTrue();
            result.FailureMessage.ShouldContain(nameof(RequestTenancyOptions.ContextResolver));
        }
    }
}
