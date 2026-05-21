// Copyright (c) Raphael Strotz. All rights reserved.

using Cake.Core;
using Cake.Frosting;

namespace Ayaka.Cake;

/// <summary>
///     Default base class for Ayaka Cake build contexts. Consumers derive from
///     this and implement the <c>IHaveX</c> capability interfaces they need.
/// </summary>
public abstract class DefaultAyakaContext : FrostingContext, IAyakaContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultAyakaContext" /> class.
    /// </summary>
    /// <param name="context">The underlying Cake context.</param>
    protected DefaultAyakaContext(ICakeContext context)
        : base(context)
    {
    }
}
