// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

using Microsoft.Extensions.Options;

/// <summary>
///     Validates the data annotations of <see cref="HybridCacheTenantCacheOptions"/>.
/// </summary>
[OptionsValidator]
internal sealed partial class HybridCacheTenantCacheOptionsAutoValidator : IValidateOptions<HybridCacheTenantCacheOptions>;
