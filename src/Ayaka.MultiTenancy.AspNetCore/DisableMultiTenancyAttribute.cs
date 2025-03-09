// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore;

/// <summary>
///     Specifies that automatic tenant detection should be disabled for the endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class DisableMultiTenancyAttribute : Attribute;
