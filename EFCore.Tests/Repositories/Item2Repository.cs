using EFCore.Tests.Abstractions;
using EFCore.Tests.Models;
using EFCore.Tests.Repositories.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Tests.Repositories
{
    public class Item2Repository : 
        EntityFrameworkRepository<Item2>, IItem2Repository
    {
        private readonly IContextFactory _contextFactory;

        public Item2Repository(IContextFactory contextFactory) 
            : base(contextFactory)
        {
            if (contextFactory == null)
                throw new ArgumentNullException(nameof(contextFactory));

            _contextFactory = contextFactory;
        }

        public bool ItemFound()
        {
            var singleItem = SelectSingle(w => w.Name.Contains("123"));

            return singleItem != null;
        }
    }
}
