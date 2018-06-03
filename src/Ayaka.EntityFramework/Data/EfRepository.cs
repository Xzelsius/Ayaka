// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ayaka.Data
{
    /// <summary>
    ///     Provides functionality to interact with data source objects using EntityFramework.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class EfRepository<TEntity> : IAsyncRepository<TEntity>, IRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        /// <summary>
        ///     Initializes a new instance of <see cref="EfRepository{TEntity}" /> class.
        /// </summary>
        /// <param name="dbContext">The underlying data context.</param>
        public EfRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
        }

        /// <summary>
        ///     Provides functionality to evaluate queries against the underlying data source.
        /// </summary>
        /// <value>
        ///     The <see cref="IQueryable{T}" /> to evaluate queries against the underlying data source.
        /// </value>
        public virtual IQueryable<TEntity> Table => _dbSet;

        /// <summary>
        ///     Finds an entity by its primary key.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>The <typeparamref name="TEntity" /> found for the specified primary key.</returns>
        public virtual TEntity Find(int id)
        {
            return _dbSet.Find(id);
        }

        /// <summary>
        ///     Finds an entity by its primary key asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>The <typeparamref name="TEntity" /> found for the specified primary key.</returns>
        public virtual Task<TEntity> FindAsync(int id)
        {
            return _dbSet.FindAsync(id);
        }

        /// <summary>
        ///     Inserts the entity into the database
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public virtual void Insert(TEntity entity)
        {
            _dbSet.Add(entity);
            _dbContext.SaveChanges();
        }

        /// <summary>
        ///     Inserts the entity into the data source asynchronously.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public virtual Task InsertAsync(TEntity entity)
        {
            _dbSet.Add(entity);
            return _dbContext.SaveChangesAsync();
        }

        /// <summary>
        ///     Persists the modification on the entity to the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public virtual void Update(TEntity entity)
        {
            if (_dbContext.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }

            _dbContext.SaveChanges();
        }

        /// <summary>
        ///     Persists the modification on the entity to the data source asynchronously.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public virtual Task UpdateAsync(TEntity entity)
        {
            if (_dbContext.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }

            return _dbContext.SaveChangesAsync();
        }

        /// <summary>
        ///     Deletes the entity from the database.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public virtual void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
            _dbContext.SaveChanges();
        }

        /// <summary>
        ///     Deletes the entity from the data source asynchronously.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public virtual Task DeleteAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            return _dbContext.SaveChangesAsync();
        }
    }
}
