using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using RSPP.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RSPP.Repositories
{
    ///<summary>
    /// Implements contract for basic database operations on all entities
    ///</summary>
    ///<typeparam name="TEntity">The Type of Entity to operate on</typeparam>
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {

        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        ///<summary>
        ///Repository constructor
        /// </summary>
        /// <param name="dbContext"> The Database Context </param>
        public BaseRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }
        public TEntity Add(TEntity entity)
        {
            _dbSet.Add(entity);
            return entity;
        }

        /// <summary>
        /// Determines which object to delete & deletes the object
        /// </summary>
        /// <param name="object">The entity to delete</param>
        public void Delete(object id)
        {
            TEntity entityToDelete = _dbSet.Find(id);
            if (entityToDelete != null)
                Delete(entityToDelete);
        }

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        public void Delete(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Deletes entities provided they exist
        /// </summary>
        /// <param name="entities">The entities to be deleted</param>
        public void DeleteMany(IEnumerable<TEntity> entities)
        {
            if (entities.Count() > 0)
                _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// Deletes entities based on a filter
        /// </summary>
        /// <param name="filter">The condition the entities must fulfil to be deleted</param>
        public void DeleteMany(Expression<Func<TEntity, bool>> filter)
        {
            var entities = _dbSet.Where(filter);
            _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// Get a collection of entities based on some specified criteria
        /// </summary>
        /// <param name="filter"> The condition the entities must fulfil to be returned</param>
        /// <param name="orderBy"> The function used to order the entities</param>
        /// <param name="includeProperties"> Any other navigation properties to include when returning the collection</param>
        /// <param name="take"> The number of records to limit the results to</param>
        /// <param name="skip"> The number of records to skip</param>
        /// <returns> A collection of entities </returns>
        public IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "",
            int? take = null,
            int? skip = null
            )
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return query.ToList();
        }


        /// <summary>
        /// Get an entity by id
        /// </summary>
        /// <param name="id">The id of the entity to retrieve</param>
        /// <returns> The entity object if found, otherwise null </returns>
        public TEntity GetById([AllowNull] object id)
        {
            if (id != null)
                return _dbSet.Find(id);
            else
                return null;
        }

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        public void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
