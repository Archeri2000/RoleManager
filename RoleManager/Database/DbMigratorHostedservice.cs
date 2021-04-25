using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoleManager.Service;

namespace RoleManager.Database
{
    public class DbMigratorHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        //private readonly IHostApplicationLifetime _lifetime;
        private readonly SourcedLoggingService _logging;

        public DbMigratorHostedService(IServiceProvider serviceProvider,
            IHostApplicationLifetime lifetime, ILoggingService logger)
        {
            Console.WriteLine("Initialising hosted service");
            _serviceProvider = serviceProvider;
            //_lifetime = lifetime;
            _logging = new SourcedLoggingService(logger, "dbmigrator");
        }
        
        public DbMigratorHostedService(IServiceProvider serviceProvider,
            ILoggingService logger)
        {
            Console.WriteLine("Initialising hosted service");
            _serviceProvider = serviceProvider;
            //_lifetime = lifetime;
            _logging = new SourcedLoggingService(logger, "dbmigrator");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("start async");
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
                var timeout = 5;
                var tries = 0;
                var canConnect = false;
                while (!canConnect)
                {
                    if (tries > timeout)
                    {
                        _logging.Fatal("Failed to contact DB, exiting application");
                        Environment.ExitCode = 1;
                        //_lifetime.StopApplication();
                        break;
                    }

                    _logging.Info($"Attempting to contact database: Attempt {tries}");
                    try
                    {
                        canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _logging.Info($"Cannot Connect: {e.Message}");
                    }

                    tries++;
                }

                _logging.Info("Contacted Database Successfully!");

                await dbContext.Database.MigrateAsync(cancellationToken);
                _logging.Info("Database migrated successfully.");
            }
            catch (Exception e)
            {
                _logging.Fatal($"Database migration failed: {e.Message}");
                _logging.Fatal(e.StackTrace);
                Environment.ExitCode = 1;
                //_lifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}