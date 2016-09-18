using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Tests.Abstractions
{
    public interface IContext : IDisposable
    {
        ChangeTracker ChangeTracker { get; }

        DatabaseFacade Database { get; }

        EntityEntry Entry(object entity);

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        Type GetType();

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }

    public interface IContext<TModel> : IContext
        where TModel : class, new()
    {
        DbSet<TModel> DbObject { get; set; }

        IQueryable<TModel> DbQuery { get; set; }

        void SetState(TModel entity, EntityState state);
    }
}
