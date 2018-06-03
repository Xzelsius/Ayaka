// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System.Linq;

namespace Ayaka.Data
{
    /// <summary>
    ///     Defines functionality to interact with data source objects.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TEntity> where TEntity : IEntity
    {
        /// <summary>
        ///     Provides functionality to evaluate queries against the underlying data source.
        /// </summary>
        /// <value>
        ///     The <see cref="IQueryable{T}" /> to evaluate queries against the underlying data source.
        /// </value>
        IQueryable<TEntity> Table { get; }

        /// <summary>
        ///     Finds an entity by its primary key.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>The <typeparamref name="TEntity" /> found for the specified primary key.</returns>
        TEntity Find(int id);

        /// <summary>
        ///     Inserts the entity into the database
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        void Insert(TEntity entity);

        /// <summary>
        ///     Persists the modification on the entity to the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        void Update(TEntity entity);

        /// <summary>
        ///     Deletes the entity from the database.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void Delete(TEntity entity);
    }
}
