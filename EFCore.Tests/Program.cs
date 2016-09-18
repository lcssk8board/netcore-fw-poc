using DryIoc;
using EFCore.Tests.Abstractions;
using EFCore.Tests.Business;
using EFCore.Tests.Models;
using EFCore.Tests.Repositories;
using EFCore.Tests.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EFCore.Tests
{
    public class Program
    {
        public static void Main(string[] args) => MainAsyncTwo(args).Wait();//MainAsync(args).Wait();

        public static async Task MainAsyncTwo(string[] args)
        {
            var container = new Container();

            var loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            var connection = new SqlConnection(
                @"Server=(localdb)\V11.0;Database=EFGetStarted.AspNetCore.NewDb;Trusted_Connection=True;"
            );

            container.RegisterDelegate(resolver => new DbContextOptionsBuilder()
                .UseLoggerFactory(loggerFactory).UseSqlServer(connection).Options
            );

            container.Register<IContextFactory, EntityFrameworkContextFactory>(Reuse.Singleton);
            container.Register<ITransactionManager, EntityFrameworkTransactionManager>(Reuse.Singleton);
            container.Register(serviceType: typeof(IRepository<>), implementationType: typeof(EntityFrameworkRepository<>), reuse: Reuse.Transient);
            container.Register<IItem2Repository, Item2Repository>(Reuse.Transient);
            container.Register(serviceType: typeof(IBaseBusiness<>), implementationType: typeof(BaseBusiness<>), reuse: Reuse.Transient);

            var genericRepository = container.Resolve<IRepository<Item2>>();
            var singleItem = await genericRepository.SelectSingleAsync(w => true);

            var item2Repository = container.Resolve<IItem2Repository>();
            var itemFound = item2Repository.ItemFound();

            var item2Business = container.Resolve<IBaseBusiness<Item2>>();
            var allItens = await item2Business.SelectAsync();
        }

        public static async Task MainAsync(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EntityFrameworkContext<Item2>>();
            var exp = default(Expression<Func<Item2, object>>);
            var loggerFactory = new LoggerFactory().AddConsole().AddDebug();

            optionsBuilder.UseSqlServer(@"Server=(localdb)\V11.0;Database=EFGetStarted.AspNetCore.NewDb;Trusted_Connection=True;");
            optionsBuilder.UseLoggerFactory(loggerFactory);

            var logger = loggerFactory.CreateLogger("console debug");

            using (var efContext = new EntityFrameworkContext<Item2>(optionsBuilder.Options))
            {
                exp = efItem => efItem.Item;
                logger.LogInformation("ASYNC DEFAULT EF PATTERN SELECT");
                var result = await efContext.DbQuery.Include(exp).ToListAsync();
            }

            var context = new EntityFrameworkContextFactory(optionsBuilder.Options);
            var rep = new EntityFrameworkRepository<Item2>(context);

            var item = new Item2
            {
                IdRef = 1,
                Name = "Teste2"
            };

            var watcher = Stopwatch.StartNew();
            
            logger.LogInformation("SYNC INSERT");
            rep.Insert(item);

            logger.LogInformation("SYNC UPDATE");
            item.Name = "123";
            rep.Update(item);

            logger.LogInformation("SYNC DELETE");
            rep.Delete(item);

            logger.LogInformation("SYNC COUNT");
            rep.Count();

            logger.LogInformation("SYNC LONGCOUNT");
            rep.LongCount();

            logger.LogInformation("SYNC MIN");
            rep.Min(w => w.Name.Contains("test"), w => w.Id);

            logger.LogInformation("SYNC MAX");
            rep.Max(w => w.Name.Contains("test"), w => w.Id);

            item = new Item2
            {
                IdRef = 1,
                Name = "Teste2"
            };

            watcher.Stop();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Total ms for sync methods: {0}ms ", watcher.ElapsedMilliseconds);
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");

            watcher.Restart();

            logger.LogInformation("ASYNC INSERT");
            await rep.InsertAsync(item);

            logger.LogInformation("ASYNC UPDATE");
            item.Name = "123";
            await rep.UpdateAsync(item);

            logger.LogInformation("ASYNC DELETE");
            await rep.DeleteAsync(item);

            logger.LogInformation("ASYNC COUNT");
            await rep.CountAsync();

            logger.LogInformation("ASYNC LONGCOUNT");
            await rep.LongCountAsync();

            logger.LogInformation("ASYNC MIN");
            await rep.MinAsync(w => w.Name.Contains("test"), w => w.Id);

            logger.LogInformation("ASYNC MAX");
            await rep.MaxAsync(w => w.Name.Contains("test"), w => w.Id);

            var queryResult = rep.ExecuteQuery(
                sql: "SELECT * FROM ITEM2",
                cmdType: CommandType.Text
            );

            watcher.Stop();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Total ms for async methods: {0}ms", watcher.ElapsedMilliseconds);
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("---------------------------------");


            dynamic perfTests = new ExpandoObject();
            watcher.Restart();
            var itensList = new Item2[100];
            for (int i = 0; i < itensList.Length; i++)
                itensList[i]= new Item2 {
                    IdRef = 1,
                    Name = $"Item{i.ToString()}"
                };

            rep.Insert(itensList);
            watcher.Stop();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("Total ms for sync 10k insert methods: {0} ms ", perfTests.Item1 = watcher.ElapsedMilliseconds);
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");

            watcher.Restart();
            itensList = new Item2[100];
            for (int i = 0; i < itensList.Length; i++)
                itensList[i] = new Item2
                {
                    IdRef = 1,
                    Name = $"Item{i.ToString()}"
                };

            await rep.InsertAsync(itensList);

            watcher.Stop();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("Total ms for sync 10k insert methods: {0} ms ", perfTests.Item2 = watcher.ElapsedMilliseconds);
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");


            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine($"First ms {perfTests.Item1}, second ms {perfTests.Item2}");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");

            var contextOptionsBuilder = new DbContextOptionsBuilder()
                .UseSqlServer(new SqlConnection(@"Server=(localdb)\V11.0;Database=EFGetStarted.AspNetCore.NewDb;Trusted_Connection=True;"))
                .UseLoggerFactory(loggerFactory);

            var contextFactory = new EntityFrameworkContextFactory(contextOptionsBuilder.Options);
            var transactionManager = new EntityFrameworkTransactionManager(contextFactory);

            var rep2 = new EntityFrameworkRepository<Item2>(contextFactory);

            Console.Clear();
            transactionManager.ExecuteWithTransaction(
                IsolationLevel.ReadCommitted,
                new[] { rep2 },
                transaction =>
                {
                    var successExecution = false;
                    try
                    {
                        rep2.Insert(new Item2 { Name = "123", IdRef = 1 });
                        successExecution = true;
                    }
                    catch(Exception ex)
                    {
                        logger.LogError(new EventId(1), ex, "exception");
                        successExecution = false;
                    }

                    return successExecution;
                }
            );



            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}