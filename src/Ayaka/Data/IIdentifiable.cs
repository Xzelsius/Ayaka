// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

namespace Ayaka.Data
{
    /// <summary>
    ///     Defines functionality of an identifiable object.
    /// </summary>
    /// <typeparam name="TKey">The type of the identifier.</typeparam>
    public interface IIdentifiable<TKey>
    {
        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     A <typeparamref name="TKey">value</typeparamref> representing the identifier.
        /// </value>
        TKey Id { get; set; }
    }
}
