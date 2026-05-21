// Copyright (c) Raphael Strotz. All rights reserved.

using Cake.Frosting;

namespace Ayaka.Cake;

/// <summary>
///     Marker interface for Ayaka Cake build contexts. Capability interfaces such
///     as <c>IHaveSolution</c> derive from this so library tasks can bind to
///     <see cref="FrostingTask{T}" /> at the broadest type and cast to the
///     capability they need.
/// </summary>
public interface IAyakaContext : IFrostingContext
{
}
