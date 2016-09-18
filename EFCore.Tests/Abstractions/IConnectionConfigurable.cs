using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Tests.Abstractions
{
    public interface IConfigurableTransaction
    {
        void SetContextTransaction(DbTransaction transaction);

        void FreeContextTransaction();
    }
}
