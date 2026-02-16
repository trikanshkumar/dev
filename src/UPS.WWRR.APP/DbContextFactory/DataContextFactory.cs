using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using UPS.WWRR.Business.Common.Helper;
using UPS.WWRR.Data.Common;
using UPS.WWRR.Data.Models;

namespace UPS.WWRR.API.DbContextFactory
{
    [ExcludeFromCodeCoverage]
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ALLOYDB_CONNECTION")
                ?? throw new InvalidOperationException("ALLOYDB_CONNECTION environment variable is not set.");

            var enableIAMTokenAuth = bool.TryParse(Environment.GetEnvironmentVariable("EnableIAMTokenAuth"), out var iam_tok) ? iam_tok : true;


            NpgsqlDataSource dataSource = null;

            if (enableIAMTokenAuth)
            {
                // Parse host, db, user from connection string
                var (db_host, database, iamDbUser) = PgDataSourceFactory.Parse(connectionString);

                // Create DataSource with IAM token provider
                dataSource = PgDataSourceFactory
                    .CreateAsync(db_host, database, iamDbUser, requireSsl: true)
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                // Local dev: use the full connection string with Username/Password
                // e.g., Host=localhost;Port=5432;Database=mydb;Username=myuser;Password=mypwd;
                dataSource = PgDataSourceFactory.Create(
                    localConnectionString: connectionString,  // includes user & password
                    requireSsl: false)
                    .GetAwaiter()
                    .GetResult();
            }

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            optionsBuilder
                .UseNpgsql(
                    dataSource,
                    npgSqlOptions =>
                        npgSqlOptions.MigrationsHistoryTable(
                            "__EFMigrationsHistory",
                            DataConstants.defaultSchema))
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableDetailedErrors();

            return new DataContext(optionsBuilder.Options);
        }
    }
}