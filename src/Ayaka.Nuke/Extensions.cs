// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

/// <summary>
///     Various extension methods for better experience when creating NUKE builds.
/// </summary>
public static class Extensions
{
    /// <summary>
    ///     Invokes the <paramref name="configurator" /> if the <paramref name="obj" /> is not <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The type of the object to operate on.</typeparam>
    /// <typeparam name="TObject">The type of the object to check for <c>null</c>.</typeparam>
    /// <param name="settings">The object to operate on.</param>
    /// <param name="obj">The object to check for <c>null</c>.</param>
    /// <param name="configurator">The method to execute if <paramref name="obj" /> is not <c>null</c>.</param>
    /// <returns>The same <paramref name="settings" /> instance for chaining.</returns>
    public static T WhenNotNull<T, TObject>(this T settings, TObject? obj, Func<T, TObject, T> configurator) =>
        obj != null ? configurator.Invoke(settings, obj) : settings;
}
