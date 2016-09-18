using EFCore.Tests.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Tests
{
    public class EntityFrameworkContextFactory : IContextFactory
    {
        private readonly DbContextOptions _options;

        public EntityFrameworkContextFactory(
            DbContextOptions options
        )
        {
            _options = options;
        }

        public IContext GetContext()
        {
            return new EntityFrameworkContext(_options);
        }

        public IContext GetContext(
            DbContextOptions options
        )
        {
            return new EntityFrameworkContext(options);
        }

        public IContext<TModel> GetContext<TModel>() 
            where TModel : class, new()
        {
            return new EntityFrameworkContext<TModel>(_options);
        }

        public IContext<TModel> GetContext<TModel>(DbContextOptions options) 
            where TModel : class, new()
        {
            return new EntityFrameworkContext<TModel>(_options);
        }
    }
}
