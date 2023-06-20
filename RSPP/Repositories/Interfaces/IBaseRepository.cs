using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Repositories.Interfaces
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "", int? take = null, int? skip = null);
        TEntity GetById([AllowNull] object Id);
        TEntity Add(TEntity entity);
        void Update(TEntity entity);
        void Delete(object Id);
        void Delete(TEntity entity);
        void DeleteMany(IEnumerable<TEntity> entities);
        void DeleteMany(Expression<Func<TEntity, bool>> filter);


        //Task<TEntity> AddAsync(TEntity entity);
        //Task DeleteAsync(object id);
        //Task DeleteAsync(TEntity entityToDelete);
        //Task DeleteManyAsync(Expression<Func<TEntity, bool>> filter);
        //Task<IEnumerable<TEntity>> Get(Expression<Func<TEntity, bool>>? filter = null, 
        //    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, 
        //    string includeProperties = "", int? take = null, int? skip = null);
        //Task<TEntity?> GetByIdAsync([AllowNull] object id);
        //Task Update(TEntity entityToUpdate);
    }
}
