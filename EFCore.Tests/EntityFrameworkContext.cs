using EFCore.Tests.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Tests
{
    public class EntityFrameworkContext 
        : DbContext, IContext
    {
        public EntityFrameworkContext(DbContextOptions options) : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }
    }

    public class EntityFrameworkContext<TModel> 
        : EntityFrameworkContext, IContext<TModel>, IContext
        where TModel : class, new()
    {
        /// <summary>
        /// Objeto de DbSet para acessar as funções de model do Entity Framework
        /// </summary>
        public virtual DbSet<TModel> DbObject { get; set; }

        /// <summary>
        /// Objeto de IQuereyable, utilizado para incluir objetos de foreign key em querys do Entity Framework
        /// </summary>
        public IQueryable<TModel> DbQuery { get; set; }

        /// <summary>
        /// Constructor padrão, iniciando o Entity Framework com um DbContextOptions
        /// </summary>
        /// <param name="nameOrConnectionString"></param>
        public EntityFrameworkContext(DbContextOptions options)
            : base(options)

        {
            DbQuery = DbObject;
        }

        public virtual void SetState(TModel entity, EntityState state)
        {
            Entry(entity).State = state;
        }
    }
}
