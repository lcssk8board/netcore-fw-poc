using System;
using System.Data;
using System.Data.Common;
using EFCore.Tests.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Tests
{
    public interface ITransactionManager
    {
        void ExecuteWithTransaction(
            IsolationLevel isolation, 
            IConfigurableTransaction[] DAOs, 
            Func<DbTransaction, bool> transactionExecution);

        void ExecuteWithTransaction(
            IsolationLevel isolation, 
            IConfigurableTransaction[] DAOs, 
            Func<DbTransaction, bool> transactionExecution, 
            DbContextOptions contextOptions);
    }
}