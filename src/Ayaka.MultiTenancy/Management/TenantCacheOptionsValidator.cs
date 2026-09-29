// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

using Microsoft.Extensions.Options;

/// <summary>
///     Validates <see cref="TenantCacheOptions"/>.
/// </summary>
internal sealed class TenantCacheOptionsValidator : IValidateOptions<TenantCacheOptions>
{
    private static readonly TimeSpan _minExpiration = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan _maxExpiration = TimeSpan.FromHours(1);

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, TenantCacheOptions options)
        => options.Expiration >= _minExpiration && options.Expiration <= _maxExpiration
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(
                $"{nameof(TenantCacheOptions)}.{nameof(TenantCacheOptions.Expiration)} must be between {_minExpiration} and {_maxExpiration}, but was {options.Expiration}.");
}
