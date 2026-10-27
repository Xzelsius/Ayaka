// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore;

using Microsoft.Extensions.Options;

/// <summary>
///     Validates the data annotations of <see cref="RequestTenancyOptions"/>.
/// </summary>
[OptionsValidator]
internal sealed partial class RequestTenancyOptionsAutoValidator : IValidateOptions<RequestTenancyOptions>;
