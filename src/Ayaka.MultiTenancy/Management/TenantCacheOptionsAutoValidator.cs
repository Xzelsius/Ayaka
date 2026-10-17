// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

using Microsoft.Extensions.Options;

/// <summary>
///     Validates the data annotations of <see cref="TenantCacheOptions"/>.
/// </summary>
[OptionsValidator]
internal sealed partial class TenantCacheOptionsAutoValidator : IValidateOptions<TenantCacheOptions>;
