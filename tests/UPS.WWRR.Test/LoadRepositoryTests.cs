#nullable enable
using Microsoft.EntityFrameworkCore;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;

namespace UPS.WWRR.UnitTests
{
    public class LoadRepositoryTests
    {
        private DataContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new DataContext(options);
        }

        private LoadRepository CreateRepo(DataContext ctx) => new LoadRepository(ctx);

        [Fact]
        public async Task AddLoadsAsync_InsertsAndSetsCreatedOnUtc()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var loads = new[]
            {
                new DataLoad { LoadTableName = "taltccy", LoadVersionNumber = 1, LoadStatusCode = LoadStatus.ReadyForValidation.ToString(), FileLocation = "f1", DataSource = "src", CreatedOn = default, LogFileLocation = "log", TotalBatchNumber = 0, BatchSize = 10 },
                new DataLoad { LoadTableName = "taltccy", LoadVersionNumber = 2, LoadStatusCode = LoadStatus.ReadyForValidation.ToString(), FileLocation = "f2", DataSource = "src", CreatedOn = DateTime.UtcNow, LogFileLocation = "log", TotalBatchNumber = 0, BatchSize = 10 }
            };

            var result = await repo.AddLoadsAsync(loads);
            Assert.Equal(2, result.Count);
            Assert.All(result, l => Assert.Equal(DateTimeKind.Utc, l.CreatedOn.Kind));
            Assert.Equal(2, await ctx.DataLoads.CountAsync());
        }

        [Fact]
        public async Task UpdateStatusAsync_UpdatesExisting()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = new DataLoad { LoadTableName = "taltccy", LoadVersionNumber = 1, LoadStatusCode = LoadStatus.ReadyForValidation.ToString(), FileLocation = "f1", DataSource = "src", CreatedOn = DateTime.UtcNow, LogFileLocation = "log", TotalBatchNumber = 0, BatchSize = 10 };
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            await repo.UpdateStatusAsync(dl.Id, LoadStatus.Processing, DateTime.UtcNow);
            var updated = await ctx.DataLoads.FindAsync(dl.Id);
            Assert.Equal(LoadStatus.Processing.ToString(), updated!.LoadStatusCode);
            Assert.NotNull(updated.ProcessedOn);
            Assert.Equal(DateTimeKind.Utc, updated.ProcessedOn!.Value.Kind);
        }

        [Fact]
        public async Task AddDetailAsync_InsertsDetail()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = new DataLoad { LoadTableName = "taltccy", LoadVersionNumber = 1, LoadStatusCode = LoadStatus.ReadyForValidation.ToString(), FileLocation = "f1", DataSource = "src", CreatedOn = DateTime.UtcNow, LogFileLocation = "log", TotalBatchNumber = 0, BatchSize = 10 };
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            var detail = new DataLoadDetail { DataLoadId = dl.Id, DataLoadType = "STG", ErrorIndicator = 0, TimeProcessValue = 1, TimePeriodTypeCode = "SECONDS", RecordsInserted = 1, BatchNumber = 1 };
            var ret = await repo.AddDetailAsync(detail);
            Assert.True(ret.Id > 0);
            Assert.Equal(1, await ctx.DataLoadDetails.CountAsync());
        }

        [Fact]
        public async Task AddErrorsAsync_InsertsAndSetsCreatedOnUtc()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var err = new DataLoadError { DataLoadDetailId = 1, ErrorCode = 1, ErrorStoredProcedureName = "sp", ErrorMessage = "msg", CreatedOn = default };
            await repo.AddErrorsAsync(new[] { err });
            var saved = await ctx.DataLoadErrors.FirstAsync();
            Assert.Equal(DateTimeKind.Utc, saved.CreatedOn.Kind);
        }

        [Fact]
        public async Task AddExceptionsAsync_InsertsAndSetsCreatedOnUtc()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var ex = new DataLoadException { DataLoadDetailId = 1, TableName = "taltccy", TableKey = "k", ErrorFieldName = "f", ErrorFieldValue = "v", CreatedOn = default };
            await repo.AddExceptionsAsync(new[] { ex });
            var saved = await ctx.DataLoadExceptions.FirstAsync();
            Assert.Equal(DateTimeKind.Utc, saved.CreatedOn.Kind);
        }

        [Fact]
        public async Task AnyProcessingAsync_ReturnsTrueWhenProcessingExists()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            ctx.DataLoads.Add(new DataLoad { LoadTableName = "taltccy", LoadVersionNumber = 1, LoadStatusCode = LoadStatus.Processing.ToString(), FileLocation = "f1", DataSource = "src", CreatedOn = DateTime.UtcNow, LogFileLocation = "log", TotalBatchNumber = 0, BatchSize = 10 });
            await ctx.SaveChangesAsync();
            Assert.True(await repo.AnyProcessingAsync());
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrueWhenMatch()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            ctx.DataLoads.Add(new DataLoad { LoadTableName = "taltccy", LoadVersionNumber = 2, LoadVersion = "2026_02_18_2", LoadStatusCode = LoadStatus.ReadyForValidation.ToString(), FileLocation = "f1", DataSource = "src", CreatedOn = DateTime.UtcNow, LogFileLocation = "log", TotalBatchNumber = 0, BatchSize = 10 });
            await ctx.SaveChangesAsync();
            Assert.True(await repo.ExistsAsync("taltccy", "2026_02_18_2"));
            Assert.False(await repo.ExistsAsync("taltccy", "2026_02_18_3"));
        }

        [Fact]
        public async Task GetLoadsByStatusAsync_FiltersByStatus()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            ctx.DataLoads.AddRange(
                new DataLoad { LoadTableName = "taltccy", LoadVersionNumber = 1, LoadStatusCode = LoadStatus.ReadyForValidation.ToString(), FileLocation = "f1", DataSource = "src", CreatedOn = DateTime.UtcNow, LogFileLocation = "log", TotalBatchNumber = 0, BatchSize = 10 },
                new DataLoad { LoadTableName = "taltccy", LoadVersionNumber = 2, LoadStatusCode = LoadStatus.Processing.ToString(), FileLocation = "f2", DataSource = "src", CreatedOn = DateTime.UtcNow, LogFileLocation = "log", TotalBatchNumber = 0, BatchSize = 10 }
            );
            await ctx.SaveChangesAsync();
            var list = await repo.GetLoadsByStatusAsync(LoadStatus.Processing);
            Assert.Single(list);
            Assert.Equal(2, list[0].LoadVersionNumber);
        }
    }
}
