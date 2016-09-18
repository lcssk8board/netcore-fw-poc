using EFCore.Tests.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Data.Common;

namespace EFCore.Tests
{
    public class EntityFrameworkTransactionManager 
        : ITransactionManager
    {
        private readonly IContextFactory _contextFactory;
        private DbContextOptions contextOptions = null;

        public EntityFrameworkTransactionManager(IContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void ExecuteWithTransaction(
            IsolationLevel isolation,
            IConfigurableTransaction[] DAOs,
            Func<DbTransaction, bool> transactionExecution
        )
        {
            ExecuteWithTransaction(isolation, DAOs, transactionExecution, null);
        }

        public void ExecuteWithTransaction(
            IsolationLevel isolation,
            IConfigurableTransaction[] DAOs,
            Func<DbTransaction, bool> transactionExecution,
            DbContextOptions contextOptions
        )
        {
            using (var context = contextOptions != null 
                ? _contextFactory.GetContext(contextOptions) 
                : _contextFactory.GetContext())
            {
                var contextTransaction = context.Database.BeginTransaction(isolation);
                var dbTransaction = contextTransaction.GetDbTransaction();

                foreach (var dao in DAOs)
                    dao.SetContextTransaction(dbTransaction);

                var successExecution = transactionExecution(dbTransaction);

                if (dbTransaction.Connection.State != ConnectionState.Closed)
                {
                    if (successExecution)
                        contextTransaction.Commit();
                    else
                        contextTransaction.Rollback();
                }

                foreach (var dao in DAOs)
                    dao.FreeContextTransaction();
            }
        }
    }
}
