using EFCore.Tests.Abstractions;
using EFCore.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Tests
{
    public class EntityFrameworkRepository<TModel> 
        : IRepository<TModel>, IConfigurableTransaction
        where TModel : class, new()
    {
        public Enumerators.RepositoryType Type
        {
            get
            {
                return Enumerators.RepositoryType.EntityFrameworkCore;
            }
        }

        private readonly IContextFactory _contextFactory;

        #region Fields
        /// <summary>
        /// Nome de conexão ou a string propriamente dita
        /// </summary>
        private string nameOrConnectionString = string.Empty;

        /// <summary>
        /// Field responsável por guardar o estado de uma conexão
        /// </summary>
        private DbContextOptions<EntityFrameworkContext<TModel>> contextOptions = null;

        /// <summary>
        /// Field responsável por guardar o estado de uma transação
        /// </summary>
        private DbTransaction transaction = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Construtor padrão, inicializa a string de conexão com o parametrizado em ConnectionStrings.config (Name: DefaultConnection)
        /// </summary>
        public EntityFrameworkRepository(IContextFactory contextFactory)
        {
            //TODO: Modificar a atribuição de nome default para um configurador
            nameOrConnectionString = "DefaultConnection";
            _contextFactory = contextFactory;
        }
        #endregion

        #region Methods
        #region Private Methods
        private IContext<TModel> CreateContext()
        {
            var context = default(IContext<TModel>);

            if (contextOptions != null)
                context = _contextFactory.GetContext<TModel>(contextOptions);
            else
                context = _contextFactory.GetContext<TModel>();

            if (transaction != null)
            {
                context.Database.UseTransaction(transaction);
            }

            return context;
        }

        /// <summary>
        /// Método auxiliar destinado a incluir referências a classes com propriedades de chasves estrangeiras. 
        /// </summary>
        /// <param name="dbSet">Objeto de Entity Framework o qual permite acesso a uma tabela vinculada a uma Model pelo attribute "TableAttribute"</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Objeto utilizado na query já possuindo a referencia relacional de chave estrangeira</returns>
        private IQueryable<TModel> IncludeReference(
            DbSet<TModel> dbSet,
            Expression<Func<TModel, object>>[] navigationProperties
        )
        {
            IQueryable<TModel> query = dbSet;

            foreach (Expression<Func<TModel, object>> navigationProperty in navigationProperties)
                query = query.Include(navigationProperty) ?? query;

            return query;
        }


        /// <summary>
        /// Método auxiliar destinado a abrir uma conexão com o banco e devolvê-la.
        /// </summary>
        /// <param name="context">Contexto do EntityFramework</param>
        /// <returns>Objeto com conexão aberta</returns>
        private DbConnection OpenConnection(IContext<TModel> context)
        {
            DbConnection conn = null;

            if (context != null && context.Database != null)
            {
                conn = context.Database.GetDbConnection();
                conn.Open();
            }

            return conn;
        }

        /// <summary>
        /// Método auxiliar destinado a criar um command baseado em um objeto de connection.
        /// </summary>
        /// <param name="conn">Objeto de conexão aberta</param>
        /// <param name="sql">Comando a ser executado (Query ou procedure)</param>
        /// <param name="cmdType">Tipo do comando</param>
        /// <returns>Objeto de command</returns>
        private DbCommand CreateCommand(
            DbConnection conn,
            string sql,
            CommandType cmdType = CommandType.Text
        )
        {
            if (conn == null)
            {
                throw new ArgumentNullException("conn");
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException("sql");
            }

            DbCommand comm = null;

            if (conn.State == ConnectionState.Open)
            {
                comm = conn.CreateCommand();
                comm.CommandText = sql;
                comm.CommandType = cmdType;
                comm.CommandTimeout = 99999;
                if (transaction != null)
                {
                    comm.Transaction = transaction;
                }
            }

            return comm;
        }

        /// <summary>
        /// Método auxiliar destinado a incluir parametros em um command.
        /// </summary>
        /// <param name="comm">Command a inserir os paramtros</param>
        /// <param name="parameters">Dicionario de parametros a inserir</param>
        private void SetParameter(
            DbCommand comm,
            Dictionary<string, object> parameters
        )
        {
            if (comm == null)
            {
                throw new ArgumentNullException("comm");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            if (parameters.Count <= 0)
            {
                throw new ArgumentException("The dictionary can't be empty", "parameters");
            }

            foreach (var item in parameters)
            {
                var param = comm.CreateParameter();
                param.ParameterName = item.Key;
                param.Value = item.Value;
                comm.Parameters.Add(param);
            }
        }

        /// <summary>
        /// Método auxiliar destinado a incluir parametros em um command.
        /// </summary>
        /// <param name="comm">Command a inserir os paramtros</param>
        /// <param name="parameters">Lista de parametros a inserir</param>
        private void SetParameter(
            DbCommand comm,
            List<ParameterData> parameters
        )
        {
            if (comm == null)
            {
                throw new ArgumentNullException("comm");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            if (parameters.Count <= 0)
            {
                throw new ArgumentException("The list of tuples can't be empty", "parameters");
            }

            foreach (var item in parameters)
            {
                var param = comm.CreateParameter();
                param.Direction = item.Direction;
                param.ParameterName = item.ParameterName;
                param.Value = item.Value;
                param.Size = item.Size;
                param.DbType = item.Type;
                comm.Parameters.Add(param);
            }
        }

        /// <summary>
        /// Implementação de método para retornar um count da tabela vinculada ao objeto
        /// </summary>
        /// <returns>Quantidade de registros</returns>
        private int Count(
            Expression<Func<TModel, bool>> where,
            IContext<TModel> context
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return context.DbQuery
                   .AsNoTracking()
                   .Count(where);
        }

        private async Task<int> CountAsync(
            Expression<Func<TModel, bool>> where,
            IContext<TModel> context,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return await context.DbQuery
                   .AsNoTracking()
                   .CountAsync(where, cancelToken);
        }


        /// <summary>
        /// Método auxiliar para retornar um count da tabela vinculada ao objeto e contexto
        /// </summary>
        /// <param name="where">Filtro de busca</param>
        /// <param name="context">Contexto de repositorio para a execução da query</param>
        /// <returns>Quantidade de registros</returns>
        private long LongCount(
            Expression<Func<TModel, bool>> where,
            IContext<TModel> context
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return context.DbQuery
                   .AsNoTracking()
                   .LongCount(where);
        }

        /// <summary>
        /// Método auxiliar para retornar um count da tabela vinculada ao objeto e contexto
        /// </summary>
        /// <param name="where">Filtro de busca</param>
        /// <param name="context">Contexto de repositorio para a execução da query</param>
        /// <returns>Quantidade de registros</returns>
        private async Task<long> LongCountAsync(
            Expression<Func<TModel, bool>> where,
            IContext<TModel> context,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return await context.DbQuery
                   .AsNoTracking()
                   .LongCountAsync(where, cancelToken);
        }
        #endregion

        #region Public Sync Methods

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma Model.
        /// Há possibilidade de incluir objetos referenciais a chaves estrangeiras
        /// </summary>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public IList<TModel> Select()
        {
            return Select(null, null, null);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma Model.
        /// Há possibilidade de incluir objetos referenciais a chaves estrangeiras
        /// </summary>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public IList<TModel> Select(
            Expression<Func<TModel, object>>[] navigationProperties
        )
        {
            return Select(null, null, navigationProperties);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma Model.
        /// Há possibilidade de incluir objetos referenciais a chaves estrangeiras
        /// </summary>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public IList<TModel> Select(
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order
        )
        {
            return Select(null, order, null);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma Model.
        /// Há possibilidade de incluir objetos referenciais a chaves estrangeiras
        /// </summary>
        /// <param name="order">Delegate contendo parâmetros de ordenação</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public IList<TModel> Select(
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            Expression<Func<TModel, object>>[] navigationProperties
        )
        {
            return Select(null, order, navigationProperties);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public IList<TModel> Select(Expression<Func<TModel, bool>> where)
        {
            return Select(where, null, null);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public IList<TModel> Select(
            Expression<Func<TModel, bool>> where,
            Expression<Func<TModel, object>>[] navigationProperties
        )
        {
            return Select(where, null, navigationProperties);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public IList<TModel> Select(
            Expression<Func<TModel, bool>> where,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order
        )
        {
            return Select(where, order, null);
        }

        /// <summary>
        /// Método auxiliar de construção de estruturas IQueryable<>
        /// </summary>
        /// <param name="context">Contexto do EntityFramework aberto para execuções de comandos</param>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="order">Delegate contendo parâmetros de ordenação</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns></returns>
        private IQueryable<TModel> PrepareQueryable(
            IContext<TModel> context,
            Expression<Func<TModel, bool>> where,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            Expression<Func<TModel, object>>[] navigationProperties
        )
        {
            var query = context.DbQuery;
            Expression<Func<TModel, bool>> whereClause = p => true;

            if (navigationProperties != null && navigationProperties.Length > 0)
                query = IncludeReference(context.DbObject, navigationProperties);

            if (order != null)
                query = order(query);

            if (where != null)
                whereClause = where;

            query = query
               .AsNoTracking()
               .Where(whereClause);

            return query;
        }


        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <param name="order">Delegate contendo parâmetros de ordenação</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public IList<TModel> Select(
            Expression<Func<TModel, bool>> where,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            Expression<Func<TModel, object>>[] navigationProperties
        )
        {
            List<TModel> list;

            using (var context = CreateContext())
            {
                var queryable = PrepareQueryable(
                    context: context,
                    where: where,
                    order: order,
                    navigationProperties: navigationProperties
                );

                list = queryable.ToList();
            }

            return list;
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar o valor máximo de uma tabela vinculada a uma model. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="maxSelector">Delegate contendo a propriedade a se encontrar o Max e retornar o tipo definido por TResult</param>
        /// <returns></returns>
        public TResult Max<TResult>(
            Expression<Func<TModel, bool>> where,
            Expression<Func<TModel, TResult>> maxSelector
        )
        {
            TResult item;

            using (var context = CreateContext())
            {
                var queryable = PrepareQueryable(
                    context: context,
                    where: where,
                    order: null,
                    navigationProperties: null
                );

                item = queryable.Max(maxSelector);
            }

            return item;
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar o valor mínimo de uma tabela vinculada a uma model. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="maxSelector">Delegate contendo a propriedade a se encontrar o Max e retornar o tipo definido por TResult</param>
        /// <returns></returns>
        public TResult Min<TResult>(
            Expression<Func<TModel, bool>> where,
            Expression<Func<TModel, TResult>> minSelector
        )
        {
            TResult item;

            using (var context = CreateContext())
            {
                var queryable = PrepareQueryable(
                    context: context,
                    where: where,
                    order: null,
                    navigationProperties: null
                );

                item = queryable.Min(minSelector);
            }
            return item;
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public TModel SelectSingle(
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order
        )
        {
            return SelectSingle(null, order, null);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public TModel SelectSingle(
            Expression<Func<TModel, bool>> where
        )
        {
            return SelectSingle(where, null, null);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public TModel SelectSingle(
            Expression<Func<TModel, bool>> where,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order
        )
        {
            return SelectSingle(where, order, null);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public TModel SelectSingle(
            Expression<Func<TModel, bool>> where,
            Expression<Func<TModel, object>>[] navigationProperties
        )
        {
            return SelectSingle(where, null, navigationProperties);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public TModel SelectSingle(
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            Expression<Func<TModel, object>>[] navigationProperties
        )
        {
            return SelectSingle(null, order, navigationProperties);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public TModel SelectSingle(
            Expression<Func<TModel, bool>> where,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            Expression<Func<TModel, object>>[] navigationProperties
        )
        {
            TModel item = null;

            using (var context = CreateContext())
            {
                var queryable = PrepareQueryable(
                    context: context,
                    where: where,
                    order: order,
                    navigationProperties: null
                );

                item = queryable.FirstOrDefault();
            }

            return item;
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a inserir uma coleção de registros.
        /// </summary>
        /// <param name="items">Coleção de registros a inserir na base</param>
        public void Insert(params TModel[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (items.Length <= 0)
            {
                throw new ArgumentException("The array of itens is empty ", "items");
            }

            using (var context = CreateContext())
            {
                foreach (TModel item in items)
                {
                    context.SetState(item, EntityState.Added);
                }

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a atualizar uma coleção de registros.
        /// </summary>
        /// <param name="items">Coleção de registros a inserir na base</param>
        public void Update(params TModel[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (items.Length <= 0)
            {
                throw new ArgumentException("The array of itens is empty ", "items");
            }

            using (var context = CreateContext())
            {
                foreach (TModel item in items)
                {
                    context.SetState(item, EntityState.Modified);
                }

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a atualizar propriedades específicas de um objeto.
        /// </summary>
        /// <param name="item">Item a atualizar na base</param>
        /// <param name="properties">Propriedades do objeto a atualizar na base</param>
        public void Update(
            TModel item,
            Expression<Func<TModel, object>>[] properties
        )
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            using (var context = CreateContext())
            {
                foreach (var property in properties)
                    context.Entry(item).Property(property).IsModified = true;

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a excluir (logicamente ou fisicamente) uma coleção de registros.
        /// </summary>
        /// <param name="items">Coleção de registros a inserir na base</param>
        public void Delete(params TModel[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (items.Length <= 0)
            {
                throw new ArgumentException("The array of itens is empty ", "items");
            }

            using (var context = CreateContext())
            {
                foreach (TModel item in items)
                    context.SetState(item, EntityState.Deleted);

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Implementação de método para retornar um count da tabela vinculada ao objeto
        /// </summary>
        /// <returns>Quantidade de registros</returns>
        public int Count()
        {
            return Count(m => true);
        }

        /// <summary>
        /// Implementação de método para retornar um count da tabela vinculada ao objeto
        /// </summary>
        /// <param name="where">Filtro de busca</param>
        /// <returns>Quantidade de registros</returns>
        public int Count(Expression<Func<TModel, bool>> where)
        {
            int count;

            using (var context = CreateContext())
                count = Count(where, context);

            return count;
        }

        /// <summary>
        /// Implementação de método para retornar um count da tabela vinculada ao objeto
        /// </summary>
        /// <returns>Quantidade de registros</returns>
        public long LongCount()
        {
            return LongCount(m => true);
        }

        /// <summary>
        /// Implementação de método para retornar um count da tabela vinculada ao objeto
        /// </summary>
        /// <param name="where">Filtro de busca</param>
        /// <returns>Quantidade de registros</returns>
        public long LongCount(Expression<Func<TModel, bool>> where)
        {
            long count;

            using (var context = CreateContext())
                count = LongCount(where, context);

            return count;
        }

        public IList<TModel> ExecuteQuery(
            string sql,
            Dictionary<string, object> parameters = null,
            CommandType cmdType = CommandType.Text
        )
        {
            List<TModel> list = null;

            if (!string.IsNullOrEmpty(sql))
                using (var context = CreateContext())
                {
                    var conn = OpenConnection(context);

                    var comm = CreateCommand(conn, sql, cmdType);

                    if (parameters != null && parameters.Count > 0)
                    {
                        SetParameter(comm, parameters);
                    }

                    var reader = comm.ExecuteReader();

                    var or = new ORM();
                    var props = or.GetProperties(typeof(TModel));
                    list = or.GetModel<TModel>(reader, props);
                }

            return list;
        }

        public object ExecuteScalar(
            string sql,
            Dictionary<string, object> parameters = null,
            CommandType cmdType = CommandType.Text
        )
        {
            object result = null;

            if (!string.IsNullOrEmpty(sql))
            {
                using (var context = CreateContext())
                {
                    var conn = OpenConnection(context);
                    var comm = CreateCommand(conn, sql, cmdType);

                    if (parameters != null && parameters.Count > 0)
                        SetParameter(comm, parameters);

                    result = comm.ExecuteScalar();
                }
            }

            return result;
        }

        public void ExecuteReader(
            string sql,
            Action<DbDataReader> callback,
            Action<DbCommand> setupCommand,
            CommandType cmdType = CommandType.Text,
            CommandBehavior behaviour = CommandBehavior.Default
        )
        {
            if (!string.IsNullOrEmpty(sql))
            {
                using (var context = CreateContext())
                {
                    var conn = OpenConnection(context);
                    var comm = CreateCommand(conn, sql, cmdType);

                    setupCommand?.Invoke(comm);

                    var result = comm.ExecuteReader(behaviour);

                    if (callback != null && result != null && result.HasRows)
                    {
                        while (result.Read())
                            callback(result);
                    }
                }
            }
        }

        public void ExecuteReader(
            string sql,
            Action<DbDataReader> callback,
            Dictionary<string, object> parameters = null,
            CommandType cmdType = CommandType.Text,
            CommandBehavior behaviour = CommandBehavior.Default
        )
        {
            ExecuteReader(
                sql,
                callback,
                command => {
                    if (parameters != null && parameters.Count > 0)
                        SetParameter(command, parameters);
                },
                cmdType,
                behaviour
            );
        }

        public void ExecuteReader(
            string sql,
            Action<DbDataReader> callback,
            List<ParameterData> parametersWithDirection,
            CommandType cmdType = CommandType.Text,
            CommandBehavior behaviour = CommandBehavior.Default
        )
        {
            ExecuteReader(
                sql,
                callback,
                command => {
                    if (parametersWithDirection != null && parametersWithDirection.Count > 0)
                        SetParameter(command, parametersWithDirection);
                },
                cmdType,
                behaviour
            );
        }

        #endregion

        #region Public Async Methods
        public async Task<IList<TModel>> ExecuteQueryAsync(
            string sql,
            Dictionary<string, object> parameters = null,
            CommandType cmdType = CommandType.Text
        )
        {
            List<TModel> list = null;

            if (!string.IsNullOrEmpty(sql))
            {
                using (var context = CreateContext())
                {
                    var conn = OpenConnection(context);

                    var comm = CreateCommand(conn, sql, cmdType);

                    if (parameters != null && parameters.Count > 0)
                    {
                        SetParameter(comm, parameters);
                    }

                    var reader = await comm.ExecuteReaderAsync();

                    var or = new ORM();
                    var props = or.GetProperties(typeof(TModel));
                    list = or.GetModel<TModel>(reader, props);
                }
            }

            return list;
        }

        public async Task<object> ExecuteScalarAsync(
            string sql,
            Dictionary<string, object> parameters = null,
            CommandType cmdType = CommandType.Text
        )
        {
            object result = null;

            if (!string.IsNullOrEmpty(sql))
            {
                using (var context = CreateContext())
                {
                    var conn = OpenConnection(context);
                    var comm = CreateCommand(conn, sql, cmdType);

                    if (parameters != null && parameters.Count > 0)
                        SetParameter(comm, parameters);

                    result = await comm.ExecuteScalarAsync();
                }
            }

            return result;
        }

        public async Task ExecuteReaderAsync(
            string sql,
            Action<DbDataReader> callback,
            Action<DbCommand> setupCommand,
            CommandType cmdType = CommandType.Text,
            CommandBehavior behaviour = CommandBehavior.Default
        )
        {
            if (!string.IsNullOrEmpty(sql))
            {
                using (var context = CreateContext())
                {
                    var conn = OpenConnection(context);
                    var comm = CreateCommand(conn, sql, cmdType);

                    setupCommand?.Invoke(comm);

                    var result = await comm.ExecuteReaderAsync(behaviour);

                    if (callback != null && result != null && result.HasRows)
                    {
                        while (result.Read())
                            callback(result);
                    }
                }
            }
        }

        public async Task ExecuteReaderAsync(
            string sql,
            Action<DbDataReader> callback,
            Dictionary<string, object> parameters = null,
            CommandType cmdType = CommandType.Text,
            CommandBehavior behaviour = CommandBehavior.Default
        )
        {
            await ExecuteReaderAsync(
                sql,
                callback,
                command => {
                    if (parameters != null && parameters.Count > 0)
                        SetParameter(command, parameters);
                },
                cmdType,
                behaviour
            );
        }

        public async Task ExecuteReaderAsync(
            string sql,
            Action<DbDataReader> callback,
            List<ParameterData> parametersWithDirection,
            CommandType cmdType = CommandType.Text,
            CommandBehavior behaviour = CommandBehavior.Default
        )
        {
            await ExecuteReaderAsync(
                sql,
                callback,
                command => {
                    if (parametersWithDirection != null && parametersWithDirection.Count > 0)
                        SetParameter(command, parametersWithDirection);
                },
                cmdType,
                behaviour
            );
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma Model.
        /// Há possibilidade de incluir objetos referenciais a chaves estrangeiras
        /// </summary>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public async Task<IList<TModel>> SelectAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            return await SelectAsync(null, null, null, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma Model.
        /// Há possibilidade de incluir objetos referenciais a chaves estrangeiras
        /// </summary>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public async Task<IList<TModel>> SelectAsync(
            Expression<Func<TModel, object>>[] navigationProperties,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            return await SelectAsync(null, null, navigationProperties, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma Model.
        /// Há possibilidade de incluir objetos referenciais a chaves estrangeiras
        /// </summary>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public async Task<IList<TModel>> SelectAsync(
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            return await SelectAsync(null, order, null, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma Model.
        /// Há possibilidade de incluir objetos referenciais a chaves estrangeiras
        /// </summary>
        /// <param name="order">Delegate contendo parâmetros de ordenação</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public async Task<IList<TModel>> SelectAsync(
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            Expression<Func<TModel, object>>[] navigationProperties,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            return await SelectAsync(null, order, navigationProperties, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public async Task<IList<TModel>> SelectAsync(
            Expression<Func<TModel, bool>> where,
            CancellationToken cancelToken = default(CancellationToken))
        {
            return await SelectAsync(where, null, null, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public async Task<IList<TModel>> SelectAsync(
            Expression<Func<TModel, bool>> where,
            Expression<Func<TModel, object>>[] navigationProperties,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            return await SelectAsync(where, null, navigationProperties, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public async Task<IList<TModel>> SelectAsync(
            Expression<Func<TModel, bool>> where,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            return await SelectAsync(where, order, null, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar todos os registros de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <param name="order">Delegate contendo parâmetros de ordenação</param>
        /// <returns>Implementação de IList com os registros encontrados.</returns>
        public async Task<IList<TModel>> SelectAsync(
            Expression<Func<TModel, bool>> where,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            Expression<Func<TModel, object>>[] navigationProperties,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            var list = default(List<TModel>);

            using (var context = CreateContext())
            {
                var queryable = PrepareQueryable(
                    context: context,
                    where: where,
                    order: order,
                    navigationProperties: navigationProperties
                );

                list = await queryable.ToListAsync(cancelToken);
            }

            return list;
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar o valor máximo de uma tabela vinculada a uma model. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="maxSelector">Delegate contendo a propriedade a se encontrar o Max e retornar o tipo definido por TResult</param>
        /// <returns></returns>
        public async Task<TResult> MaxAsync<TResult>(
            Expression<Func<TModel, bool>> where,
            Expression<Func<TModel, TResult>> maxSelector,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            var item = default(TResult);

            using (var context = CreateContext())
            {
                var queryable = PrepareQueryable(
                    context: context,
                    where: where,
                    order: null,
                    navigationProperties: null
                );

                item = await queryable.MaxAsync(maxSelector, cancelToken);
            }

            return item;
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar o valor mínimo de uma tabela vinculada a uma model. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="maxSelector">Delegate contendo a propriedade a se encontrar o Max e retornar o tipo definido por TResult</param>
        /// <returns></returns>
        public async Task<TResult> MinAsync<TResult>(
            Expression<Func<TModel, bool>> where,
            Expression<Func<TModel, TResult>> minSelector,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            var item = default(TResult);

            using (var context = CreateContext())
            {
                var queryable = PrepareQueryable(
                    context: context,
                    where: where,
                    order: null,
                    navigationProperties: null
                );

                item = await queryable.MinAsync(minSelector, cancelToken);
            }

            return item;
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public async Task<TModel> SelectSingleAsync(
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            return await SelectSingleAsync(null, order, null, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public async Task<TModel> SelectSingleAsync(
            Expression<Func<TModel, bool>> where,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            return await SelectSingleAsync(where, null, null, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public async Task<TModel> SelectSingleAsync(
            Expression<Func<TModel, bool>> where,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            return await SelectSingleAsync(where, order, null, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public async Task<TModel> SelectSingleAsync(
            Expression<Func<TModel, bool>> where,
            Expression<Func<TModel, object>>[] navigationProperties,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            return await SelectSingleAsync(where, null, navigationProperties, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public async Task<TModel> SelectSingleAsync(
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            Expression<Func<TModel, object>>[] navigationProperties,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            return await SelectSingleAsync(null, order, navigationProperties, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a encontrar um unico registro de uma tabela vinculada a uma model. 
        /// </summary>
        /// <param name="where">Delegate contendo parâmetros para composição de WHERE</param>
        /// <param name="navigationProperties">Objetos de uma Model referentes a chaves estrangeiras no database</param>
        /// <returns>Objeto de classe modelo preenchido com registro encontrado</returns>
        public async Task<TModel> SelectSingleAsync(
            Expression<Func<TModel, bool>> where,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> order,
            Expression<Func<TModel, object>>[] navigationProperties,
            CancellationToken cancelToken = default(CancellationToken)
        )
        {
            var item = default(TModel);

            using (var context = CreateContext())
            {
                var queryable = PrepareQueryable(
                    context: context,
                    where: where,
                    order: order,
                    navigationProperties: null
                );

                item = await queryable.FirstOrDefaultAsync(cancelToken);
            }

            return item;
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a inserir uma coleção de registros.
        /// </summary>
        /// <param name="items">Coleção de registros a inserir na base</param>
        public async Task InsertAsync(
            TModel items,
            CancellationToken cancelToken = default(CancellationToken))
        {
            await InsertAsync(new TModel[] { items }, cancelToken);
        }


        /// <summary>
        /// Implementação de método de IBaseDAO destinado a inserir uma coleção de registros.
        /// </summary>
        /// <param name="items">Coleção de registros a inserir na base</param>
        public async Task InsertAsync(
            TModel[] items,
            CancellationToken cancelToken = default(CancellationToken))
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (items.Length <= 0)
            {
                throw new ArgumentException("The array of itens is empty ", "items");
            }

            using (var context = CreateContext())
            {
                foreach (TModel item in items)
                    context.SetState(item, EntityState.Added);

                await context.SaveChangesAsync(cancelToken);
            }
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a atualizar uma coleção de registros.
        /// </summary>
        /// <param name="items">Coleção de registros a inserir na base</param>
        public async Task UpdateAsync(
            TModel items,
            CancellationToken cancelToken = default(CancellationToken))
        {
            await UpdateAsync(new TModel[] { items }, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a atualizar uma coleção de registros.
        /// </summary>
        /// <param name="items">Coleção de registros a inserir na base</param>
        public async Task UpdateAsync(
            TModel[] items,
            CancellationToken cancelToken = default(CancellationToken))
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (items.Length <= 0)
            {
                throw new ArgumentException("The array of itens is empty ", "items");
            }

            using (var context = CreateContext())
            {
                foreach (TModel item in items)
                    context.SetState(item, EntityState.Modified);

                await context.SaveChangesAsync(cancelToken);
            }
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a atualizar propriedades específicas de um objeto.
        /// </summary>
        /// <param name="item">Item a atualizar na base</param>
        /// <param name="properties">Propriedades do objeto a atualizar na base</param>
        public async Task UpdateAsync(
            TModel item,
            Expression<Func<TModel, object>>[] properties,
            CancellationToken cancelToken = default(CancellationToken))
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            using (var context = CreateContext())
            {
                foreach (var property in properties)
                    context.Entry(item).Property(property).IsModified = true;

                await context.SaveChangesAsync(cancelToken);
            }
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a excluir (logicamente ou fisicamente) uma coleção de registros.
        /// </summary>
        /// <param name="items">Coleção de registros a inserir na base</param>
        public async Task DeleteAsync(TModel items,
            CancellationToken cancelToken = default(CancellationToken))
        {
            await DeleteAsync(new TModel[] { items }, cancelToken);
        }

        /// <summary>
        /// Implementação de método de IBaseDAO destinado a excluir (logicamente ou fisicamente) uma coleção de registros.
        /// </summary>
        /// <param name="items">Coleção de registros a inserir na base</param>
        public async Task DeleteAsync(TModel[] items,
            CancellationToken cancelToken = default(CancellationToken))
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (items.Length <= 0)
            {
                throw new ArgumentException("The array of itens is empty ", "items");
            }

            using (var context = CreateContext())
            {
                foreach (TModel item in items)
                    context.SetState(item, EntityState.Deleted);

                await context.SaveChangesAsync(cancelToken);
            }
        }

        /// <summary>
        /// Implementação de método para retornar um count da tabela vinculada ao objeto
        /// </summary>
        /// <returns>Quantidade de registros</returns>
        public async Task<int> CountAsync(CancellationToken cancelToken = default(CancellationToken)) =>
            await CountAsync(m => true, cancelToken);

        /// <summary>
        /// Implementação de método para retornar um count da tabela vinculada ao objeto
        /// </summary>
        /// <param name="where">Filtro de busca</param>
        /// <returns>Quantidade de registros</returns>
        public async Task<int> CountAsync(
            Expression<Func<TModel, bool>> where,
            CancellationToken cancelToken = default(CancellationToken))
        {
            var result = 0;

            using (var context = CreateContext())
                result = await CountAsync(where, context, cancelToken);

            return result;
        }

        /// <summary>
        /// Implementação de método para retornar um count da tabela vinculada ao objeto
        /// </summary>
        /// <returns>Quantidade de registros</returns>
        public async Task<long> LongCountAsync(CancellationToken cancelToken = default(CancellationToken)) =>
            await LongCountAsync(m => true);

        /// <summary>
        /// Implementação de método para retornar um count da tabela vinculada ao objeto
        /// </summary>
        /// <param name="where">Filtro de busca</param>
        /// <returns>Quantidade de registros</returns>
        public async Task<long> LongCountAsync(Expression<Func<TModel, bool>> where,
            CancellationToken cancelToken = default(CancellationToken))
        {
            using (var context = CreateContext())
                return await LongCountAsync(where, context, cancelToken);
        }

        public void SetContextTransaction(DbTransaction transaction)
        {
            this.transaction = transaction;
        }

        public void FreeContextTransaction()
        {
            this.transaction = null;
        }
        #endregion
        #endregion
    }
}
