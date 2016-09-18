using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Tests.Abstractions
{
    public interface IContextFactory
    {
        IContext GetContext();

        IContext GetContext(
            DbContextOptions options
        );

        IContext<TModel> GetContext<TModel>() 
            where TModel : class, new();


        IContext<TModel> GetContext<TModel>(
            DbContextOptions options
        ) where TModel : class, new();
    }
}
