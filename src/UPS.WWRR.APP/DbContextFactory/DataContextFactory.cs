using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using UPS.WWRR.Data.Models;
using UPS.WWRR.Data.Common;
using UPS.WWRR.Business.Common.Helper;

namespace UPS.WWRR.API.DbContextFactory
{
    [ExcludeFromCodeCoverage]
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ALLOYDB_CONNECTION")
                ?? throw new InvalidOperationException("ALLOYDB_CONNECTION environment variable is not set.");

            // Parse host, db, user from connection string
            var (db_host, database, iamDbUser) = PgDataSourceFactory.Parse(connectionString);

            // Create DataSource with IAM token provider
            var dataSource = PgDataSourceFactory
                .CreateAsync(db_host, database, iamDbUser, requireSsl: true)
                .GetAwaiter()
                .GetResult();

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