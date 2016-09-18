using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Tests.Abstractions
{
    public interface IBaseBusiness<TModel>
        where TModel : class, new()
    {
        int Count();
        int Count(Expression<Func<TModel, bool>> where);
        Task<int> CountAsync(CancellationToken cancelToken = default(CancellationToken));
        Task<int> CountAsync(Expression<Func<TModel, bool>> where, CancellationToken cancelToken = default(CancellationToken));
        void Delete(params TModel[] items);
        Task DeleteAsync(TModel[] items, CancellationToken cancelToken = default(CancellationToken));
        Task DeleteAsync(TModel items, CancellationToken cancelToken = default(CancellationToken));
        void Insert(params TModel[] items);
        Task InsertAsync(TModel[] items, CancellationToken cancelToken = default(CancellationToken));
        Task InsertAsync(TModel items, CancellationToken cancelToken = default(CancellationToken));
        long LongCount();
        long LongCount(Expression<Func<TModel, bool>> where);
        Task<long> LongCountAsync(CancellationToken cancelToken = default(CancellationToken));
        Task<long> LongCountAsync(Expression<Func<TModel, bool>> where, CancellationToken cancelToken = default(CancellationToken));
        TResult Max<TResult>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TResult>> maxSelector);
        Task<TResult> MaxAsync<TResult>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TResult>> maxSelector, CancellationToken cancelToken = default(CancellationToken));
        TResult Min<TResult>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TResult>> minSelector);
        Task<TResult> MinAsync<TResult>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TResult>> minSelector, CancellationToken cancelToken = default(CancellationToken));
        IList<TModel> Select();
        IList<TModel> Select(Expression<Func<TModel, object>>[] navigationProperties);
        IList<TModel> Select(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order);
        IList<TModel> Select(Expression<Func<TModel, bool>> where);
        IList<TModel> Select(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order);
        IList<TModel> Select(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties);
        IList<TModel> Select(Expression<Func<TModel, bool>> where, Expression<Func<TModel, object>>[] navigationProperties);
        IList<TModel> Select(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties);
        Task<IList<TModel>> SelectAsync(CancellationToken cancelToken = default(CancellationToken));
        Task<IList<TModel>> SelectAsync(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, CancellationToken cancelToken = default(CancellationToken));
        Task<IList<TModel>> SelectAsync(Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken));
        Task<IList<TModel>> SelectAsync(Expression<Func<TModel, bool>> where, CancellationToken cancelToken = default(CancellationToken));
        Task<IList<TModel>> SelectAsync(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken));
        Task<IList<TModel>> SelectAsync(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, CancellationToken cancelToken = default(CancellationToken));
        Task<IList<TModel>> SelectAsync(Expression<Func<TModel, bool>> where, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken));
        Task<IList<TModel>> SelectAsync(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken));
        TModel SelectSingle(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order);
        TModel SelectSingle(Expression<Func<TModel, bool>> where);
        TModel SelectSingle(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties);
        TModel SelectSingle(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order);
        TModel SelectSingle(Expression<Func<TModel, bool>> where, Expression<Func<TModel, object>>[] navigationProperties);
        TModel SelectSingle(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties);
        Task<TModel> SelectSingleAsync(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, CancellationToken cancelToken = default(CancellationToken));
        Task<TModel> SelectSingleAsync(Expression<Func<TModel, bool>> where, CancellationToken cancelToken = default(CancellationToken));
        Task<TModel> SelectSingleAsync(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, CancellationToken cancelToken = default(CancellationToken));
        Task<TModel> SelectSingleAsync(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken));
        Task<TModel> SelectSingleAsync(Expression<Func<TModel, bool>> where, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken));
        Task<TModel> SelectSingleAsync(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken));
        void Update(params TModel[] items);
        void Update(TModel item, Expression<Func<TModel, object>>[] properties);
        Task UpdateAsync(TModel[] items, CancellationToken cancelToken = default(CancellationToken));
        Task UpdateAsync(TModel items, CancellationToken cancelToken = default(CancellationToken));
        Task UpdateAsync(TModel item, Expression<Func<TModel, object>>[] properties, CancellationToken cancelToken = default(CancellationToken));
    }
}
