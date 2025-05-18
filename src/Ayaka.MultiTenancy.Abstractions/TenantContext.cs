// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy;

/// <summary>
///     Provides information about the current tenant.
/// </summary>
/// <param name="Id">The identifier of the current tenant.</param>
/// <param name="DisplayName">The optional display name of the current tenant.</param>
public record TenantContext(string Id, string? DisplayName);
