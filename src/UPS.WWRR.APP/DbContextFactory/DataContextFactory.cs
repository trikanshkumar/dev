using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
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

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder
                .UseNpgsql(connectionString)
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableDetailedErrors();

            return new DataContext(optionsBuilder.Options);
        }
    }
}
