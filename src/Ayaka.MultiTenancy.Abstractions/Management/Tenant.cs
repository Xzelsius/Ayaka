// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

using System.Collections.Immutable;

/// <summary>
///     Represents a tenant.
/// </summary>
/// <param name="Id">The unique identifier of the tenant.</param>
/// <param name="DisplayName">The optional display name of the tenant.</param>
/// <param name="Attributes">
///     The optional attributes of the tenant.
///     Normally used to store additional information about a tenant.
/// </param>
public record Tenant(
    string Id,
    string? DisplayName = null,
    IImmutableDictionary<string, string>? Attributes = null);
