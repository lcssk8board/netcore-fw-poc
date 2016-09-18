using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EFCore.Tests.Abstractions;
using EFCore.Tests.Models;

namespace EFCore.Tests.Abstractions
{
    public interface IRepository<TModel> : IBaseBusiness<TModel>
        where TModel : class, new()
    {
        Enumerators.RepositoryType Type { get; }
        IList<TModel> ExecuteQuery(string sql, Dictionary<string, object> parameters = null, CommandType cmdType = CommandType.Text);
        Task<IList<TModel>> ExecuteQueryAsync(string sql, Dictionary<string, object> parameters = null, CommandType cmdType = CommandType.Text);
        void ExecuteReader(string sql, Action<DbDataReader> callback, List<ParameterData> parametersWithDirection, CommandType cmdType = CommandType.Text, CommandBehavior behaviour = CommandBehavior.Default);
        void ExecuteReader(string sql, Action<DbDataReader> callback, Action<DbCommand> setupCommand, CommandType cmdType = CommandType.Text, CommandBehavior behaviour = CommandBehavior.Default);
        void ExecuteReader(string sql, Action<DbDataReader> callback, Dictionary<string, object> parameters = null, CommandType cmdType = CommandType.Text, CommandBehavior behaviour = CommandBehavior.Default);
        Task ExecuteReaderAsync(string sql, Action<DbDataReader> callback, List<ParameterData> parametersWithDirection, CommandType cmdType = CommandType.Text, CommandBehavior behaviour = CommandBehavior.Default);
        Task ExecuteReaderAsync(string sql, Action<DbDataReader> callback, Action<DbCommand> setupCommand, CommandType cmdType = CommandType.Text, CommandBehavior behaviour = CommandBehavior.Default);
        Task ExecuteReaderAsync(string sql, Action<DbDataReader> callback, Dictionary<string, object> parameters = null, CommandType cmdType = CommandType.Text, CommandBehavior behaviour = CommandBehavior.Default);
        object ExecuteScalar(string sql, Dictionary<string, object> parameters = null, CommandType cmdType = CommandType.Text);
        Task<object> ExecuteScalarAsync(string sql, Dictionary<string, object> parameters = null, CommandType cmdType = CommandType.Text);
    }
}