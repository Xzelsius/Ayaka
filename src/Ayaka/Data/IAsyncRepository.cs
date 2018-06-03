// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System.Linq;
using System.Threading.Tasks;

namespace Ayaka.Data
{
    /// <summary>
    ///     Defines functionality to asynchronously interact with data source objects.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IAsyncRepository<TEntity> where TEntity : IEntity
    {
        /// <summary>
        ///     Provides functionality to evaluate queries against the underlying data source.
        /// </summary>
        /// <value>
        ///     The <see cref="IQueryable{T}" /> to evaluate queries against the underlying data source.
        /// </value>
        IQueryable<TEntity> Table { get; }

        /// <summary>
        ///     Finds an entity by its primary key asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>The <typeparamref name="TEntity" /> found for the specified primary key.</returns>
        Task<TEntity> FindAsync(int id);

        /// <summary>
        ///     Inserts the entity into the data source asynchronously.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task InsertAsync(TEntity entity);

        /// <summary>
        ///     Persists the modification on the entity to the data source asynchronously.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        ///     Deletes the entity from the data source asynchronously.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task DeleteAsync(TEntity entity);
    }
}
