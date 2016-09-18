using EFCore.Tests.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Threading;

namespace EFCore.Tests.Business
{
    public class BaseBusiness<TModel> : IBaseBusiness<TModel>
        where TModel : class, new()
    {
        private readonly IRepository<TModel> _repository;

        public BaseBusiness(IRepository<TModel> repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            _repository = repository;
        }

        public int Count()
            => _repository.Count();

        public int Count(Expression<Func<TModel, bool>> where)
            => _repository.Count(where);

        public Task<int> CountAsync(CancellationToken cancelToken = default(CancellationToken))
            => _repository.CountAsync(cancelToken);

        public Task<int> CountAsync(Expression<Func<TModel, bool>> where, CancellationToken cancelToken = default(CancellationToken))
            => _repository.CountAsync(where, cancelToken);

        public void Delete(params TModel[] items)
            => _repository.Delete(items);

        public Task DeleteAsync(TModel items, CancellationToken cancelToken = default(CancellationToken))
            => _repository.DeleteAsync(items, cancelToken);

        public Task DeleteAsync(TModel[] items, CancellationToken cancelToken = default(CancellationToken))
            => _repository.DeleteAsync(items, cancelToken);

        public void Insert(params TModel[] items)
            => _repository.Insert(items);

        public Task InsertAsync(TModel items, CancellationToken cancelToken = default(CancellationToken))
            => _repository.InsertAsync(items, cancelToken);

        public Task InsertAsync(TModel[] items, CancellationToken cancelToken = default(CancellationToken))
            => _repository.InsertAsync(items, cancelToken);

        public long LongCount()
            => _repository.LongCount();

        public long LongCount(Expression<Func<TModel, bool>> where)
            => _repository.LongCount(where);

        public Task<long> LongCountAsync(CancellationToken cancelToken = default(CancellationToken))
            => _repository.LongCountAsync(cancelToken);

        public Task<long> LongCountAsync(Expression<Func<TModel, bool>> where, CancellationToken cancelToken = default(CancellationToken))
            => _repository.LongCountAsync(where, cancelToken);

        public TResult Max<TResult>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TResult>> maxSelector)
            => _repository.Max(where, maxSelector);

        public Task<TResult> MaxAsync<TResult>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TResult>> maxSelector, CancellationToken cancelToken = default(CancellationToken))
            => _repository.MaxAsync(where, maxSelector, cancelToken);

        public TResult Min<TResult>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TResult>> minSelector)
            => _repository.Min(where, minSelector);

        public Task<TResult> MinAsync<TResult>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TResult>> minSelector, CancellationToken cancelToken = default(CancellationToken))
            => _repository.MinAsync(where, minSelector, cancelToken);

        public IList<TModel> Select()
            => _repository.Select();

        public IList<TModel> Select(Expression<Func<TModel, bool>> where) 
            => _repository.Select(where);

        public IList<TModel> Select(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order)
            => _repository.Select(order);

        public IList<TModel> Select(Expression<Func<TModel, object>>[] navigationProperties)
            => _repository.Select(navigationProperties);

        public IList<TModel> Select(Expression<Func<TModel, bool>> where, Expression<Func<TModel, object>>[] navigationProperties)
            => _repository.Select(navigationProperties);

        public IList<TModel> Select(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties)
            => _repository.Select(order, navigationProperties);

        public IList<TModel> Select(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order)
            => _repository.Select(where, order);

        public IList<TModel> Select(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties)
            => _repository.Select(where, order, navigationProperties);

        public Task<IList<TModel>> SelectAsync(CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectAsync(cancelToken);

        public Task<IList<TModel>> SelectAsync(Expression<Func<TModel, bool>> where, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectAsync(where, cancelToken);

        public Task<IList<TModel>> SelectAsync(Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectAsync(navigationProperties, cancelToken);

        public Task<IList<TModel>> SelectAsync(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectAsync(order, cancelToken);

        public Task<IList<TModel>> SelectAsync(Expression<Func<TModel, bool>> where, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectAsync(where, navigationProperties, cancelToken);

        public Task<IList<TModel>> SelectAsync(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectAsync(where, order, cancelToken);

        public Task<IList<TModel>> SelectAsync(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectAsync(order, navigationProperties, cancelToken);

        public Task<IList<TModel>> SelectAsync(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectAsync(where, order, navigationProperties, cancelToken);

        public TModel SelectSingle(Expression<Func<TModel, bool>> where)
            => _repository.SelectSingle(where);

        public TModel SelectSingle(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order)
            => _repository.SelectSingle(order);

        public TModel SelectSingle(Expression<Func<TModel, bool>> where, Expression<Func<TModel, object>>[] navigationProperties)
            => _repository.SelectSingle(where, navigationProperties);

        public TModel SelectSingle(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order)
            => _repository.SelectSingle(where, order);

        public TModel SelectSingle(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties)
            => _repository.SelectSingle(order, navigationProperties);

        public TModel SelectSingle(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties)
            => _repository.SelectSingle(where, order, navigationProperties);

        public Task<TModel> SelectSingleAsync(Expression<Func<TModel, bool>> where, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectSingleAsync(where, cancelToken);

        public Task<TModel> SelectSingleAsync(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectSingleAsync(order, cancelToken);

        public Task<TModel> SelectSingleAsync(Expression<Func<TModel, bool>> where, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectSingleAsync(where, navigationProperties, cancelToken);

        public Task<TModel> SelectSingleAsync(Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectSingleAsync(order, navigationProperties, cancelToken);

        public Task<TModel> SelectSingleAsync(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectSingleAsync(where, order, cancelToken);

        public Task<TModel> SelectSingleAsync(Expression<Func<TModel, bool>> where, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order, Expression<Func<TModel, object>>[] navigationProperties, CancellationToken cancelToken = default(CancellationToken))
            => _repository.SelectSingleAsync(where, order, navigationProperties, cancelToken);

        public void Update(params TModel[] items)
            => _repository.Update(items);

        public void Update(TModel item, Expression<Func<TModel, object>>[] properties)
            => _repository.Update(item, properties);

        public Task UpdateAsync(TModel items, CancellationToken cancelToken = default(CancellationToken))
            => _repository.UpdateAsync(items, cancelToken);

        public Task UpdateAsync(TModel[] items, CancellationToken cancelToken = default(CancellationToken))
            => _repository.UpdateAsync(items, cancelToken);

        public Task UpdateAsync(TModel item, Expression<Func<TModel, object>>[] properties, CancellationToken cancelToken = default(CancellationToken))
            => _repository.UpdateAsync(item, properties, cancelToken);
    }
}
