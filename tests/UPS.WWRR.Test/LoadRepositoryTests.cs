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

        private static DataLoad MakeLoad(
            string table = "taltccy",
            long ver = 1,
            string loadVersion = "",
            LoadStatus status = LoadStatus.ReadyForValidation,
            DateTime? createdOn = null) => new()
        {
            LoadTableName = table,
            LoadVersionNumber = ver,
            LoadVersion = loadVersion,
            LoadStatusCode = status.ToString(),
            FileLocation = "f1",
            DataSource = "src",
            CreatedOn = createdOn ?? DateTime.UtcNow,
            LogFileLocation = "log",
            TotalBatchNumber = 0,
            BatchSize = 10
        };

        #region AddLoadsAsync

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
        public async Task AddLoadsAsync_DefaultCreatedOn_SetsUtcNow()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var load = MakeLoad(createdOn: default);

            var result = await repo.AddLoadsAsync(new[] { load });

            Assert.Single(result);
            Assert.Equal(DateTimeKind.Utc, result[0].CreatedOn.Kind);
            Assert.True(result[0].CreatedOn > DateTime.MinValue);
        }

        [Fact]
        public async Task AddLoadsAsync_CreatedOnAlreadyUtc_PreservesValue()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var specificTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);
            var load = MakeLoad(createdOn: specificTime);

            var result = await repo.AddLoadsAsync(new[] { load });

            Assert.Single(result);
            Assert.Equal(specificTime, result[0].CreatedOn);
            Assert.Equal(DateTimeKind.Utc, result[0].CreatedOn.Kind);
        }

        [Fact]
        public async Task AddLoadsAsync_CreatedOnUnspecifiedKind_ConvertsToUtc()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var unspecifiedTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Unspecified);
            var load = MakeLoad(createdOn: unspecifiedTime);

            var result = await repo.AddLoadsAsync(new[] { load });

            Assert.Single(result);
            Assert.Equal(DateTimeKind.Utc, result[0].CreatedOn.Kind);
        }

        [Fact]
        public async Task AddLoadsAsync_CreatedOnLocalKind_ConvertsToUtc()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var localTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Local);
            var load = MakeLoad(createdOn: localTime);

            var result = await repo.AddLoadsAsync(new[] { load });

            Assert.Single(result);
            Assert.Equal(DateTimeKind.Utc, result[0].CreatedOn.Kind);
        }

        [Fact]
        public async Task AddLoadsAsync_EmptyCollection_ReturnsEmpty()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            var result = await repo.AddLoadsAsync(Array.Empty<DataLoad>());

            Assert.Empty(result);
            Assert.Equal(0, await ctx.DataLoads.CountAsync());
        }

        #endregion

        #region UpdateStatusAsync

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
        public async Task UpdateStatusAsync_NonExistentLoad_DoesNothing()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            await repo.UpdateStatusAsync(999, LoadStatus.Failed, DateTime.UtcNow);

            Assert.Equal(0, await ctx.DataLoads.CountAsync());
        }

        [Fact]
        public async Task UpdateStatusAsync_NullProcessedOn_DoesNotSetProcessedOn()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            await repo.UpdateStatusAsync(dl.Id, LoadStatus.Processing);

            var updated = await ctx.DataLoads.FindAsync(dl.Id);
            Assert.Equal(LoadStatus.Processing.ToString(), updated!.LoadStatusCode);
            Assert.Null(updated.ProcessedOn);
        }

        [Fact]
        public async Task UpdateStatusAsync_ProcessedOnUnspecifiedKind_ConvertsToUtc()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            var unspecifiedTime = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Unspecified);
            await repo.UpdateStatusAsync(dl.Id, LoadStatus.Processed, unspecifiedTime);

            var updated = await ctx.DataLoads.FindAsync(dl.Id);
            Assert.NotNull(updated!.ProcessedOn);
            Assert.Equal(DateTimeKind.Utc, updated.ProcessedOn!.Value.Kind);
        }

        [Fact]
        public async Task UpdateStatusAsync_ProcessedOnLocalKind_ConvertsToUtc()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            var localTime = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Local);
            await repo.UpdateStatusAsync(dl.Id, LoadStatus.Failed, localTime);

            var updated = await ctx.DataLoads.FindAsync(dl.Id);
            Assert.NotNull(updated!.ProcessedOn);
            Assert.Equal(DateTimeKind.Utc, updated.ProcessedOn!.Value.Kind);
        }

        [Fact]
        public async Task UpdateStatusAsync_ProcessedOnAlreadyUtc_PreservesValue()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            var utcTime = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
            await repo.UpdateStatusAsync(dl.Id, LoadStatus.Processed, utcTime);

            var updated = await ctx.DataLoads.FindAsync(dl.Id);
            Assert.Equal(utcTime, updated!.ProcessedOn!.Value);
            Assert.Equal(DateTimeKind.Utc, updated.ProcessedOn!.Value.Kind);
        }

        [Theory]
        [InlineData(LoadStatus.ReadyForValidation)]
        [InlineData(LoadStatus.FailedValidation)]
        [InlineData(LoadStatus.ReadyToProcess)]
        [InlineData(LoadStatus.Processing)]
        [InlineData(LoadStatus.Failed)]
        [InlineData(LoadStatus.Processed)]
        [InlineData(LoadStatus.MissingRequiredPair)]
        [InlineData(LoadStatus.FailedMissingData)]
        public async Task UpdateStatusAsync_AllStatusValues_UpdatesCorrectly(LoadStatus status)
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            await repo.UpdateStatusAsync(dl.Id, status);

            var updated = await ctx.DataLoads.FindAsync(dl.Id);
            Assert.Equal(status.ToString(), updated!.LoadStatusCode);
        }

        #endregion

        #region UpdateTotalBatchNumber

        [Fact]
        public async Task UpdateTotalBatchNumber_ExistingLoad_UpdatesValue()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            await repo.UpdateTotalBatchNumber(dl.Id, 5);

            var updated = await ctx.DataLoads.FindAsync(dl.Id);
            Assert.Equal(5, updated!.TotalBatchNumber);
        }

        [Fact]
        public async Task UpdateTotalBatchNumber_NonExistentLoad_DoesNothing()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            await repo.UpdateTotalBatchNumber(999, 10);

            Assert.Equal(0, await ctx.DataLoads.CountAsync());
        }

        [Fact]
        public async Task UpdateTotalBatchNumber_ZeroValue_SetsToZero()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            dl.TotalBatchNumber = 5;
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            await repo.UpdateTotalBatchNumber(dl.Id, 0);

            var updated = await ctx.DataLoads.FindAsync(dl.Id);
            Assert.Equal(0, updated!.TotalBatchNumber);
        }

        #endregion

        #region UpdateFileLocation

        [Fact]
        public async Task UpdateFileLocation_ExistingLoad_UpdatesValue()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            await repo.UpdateFileLocation(dl.Id, "gs://new-bucket/newfile.csv");

            var updated = await ctx.DataLoads.FindAsync(dl.Id);
            Assert.Equal("gs://new-bucket/newfile.csv", updated!.FileLocation);
        }

        [Fact]
        public async Task UpdateFileLocation_NonExistentLoad_DoesNothing()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            await repo.UpdateFileLocation(999, "gs://bucket/file.csv");

            Assert.Equal(0, await ctx.DataLoads.CountAsync());
        }

        [Fact]
        public async Task UpdateFileLocation_EmptyString_SetsEmpty()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            await repo.UpdateFileLocation(dl.Id, "");

            var updated = await ctx.DataLoads.FindAsync(dl.Id);
            Assert.Equal("", updated!.FileLocation);
        }

        #endregion

        #region AddDetailAsync

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
        public async Task AddDetailAsync_DefaultCreatedOn_SetsUtcNow()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            var detail = new DataLoadDetail
            {
                DataLoadId = dl.Id, DataLoadType = "STG", ErrorIndicator = 0,
                TimeProcessValue = 10, TimePeriodTypeCode = "SECONDS",
                RecordsInserted = 100, BatchNumber = 1, CreatedOn = default
            };

            var result = await repo.AddDetailAsync(detail);

            Assert.Equal(DateTimeKind.Utc, result.CreatedOn.Kind);
            Assert.True(result.CreatedOn > DateTime.MinValue);
        }

        [Fact]
        public async Task AddDetailAsync_CreatedOnAlreadyUtc_PreservesValue()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            var specificTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);
            var detail = new DataLoadDetail
            {
                DataLoadId = dl.Id, DataLoadType = "ACL", ErrorIndicator = 0,
                TimeProcessValue = 5, TimePeriodTypeCode = "SECONDS",
                RecordsInserted = 50, BatchNumber = 1, CreatedOn = specificTime
            };

            var result = await repo.AddDetailAsync(detail);

            Assert.Equal(specificTime, result.CreatedOn);
            Assert.Equal(DateTimeKind.Utc, result.CreatedOn.Kind);
        }

        [Fact]
        public async Task AddDetailAsync_CreatedOnUnspecifiedKind_ConvertsToUtc()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            var unspecifiedTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Unspecified);
            var detail = new DataLoadDetail
            {
                DataLoadId = dl.Id, DataLoadType = "NRM", ErrorIndicator = 1,
                TimeProcessValue = 3, TimePeriodTypeCode = "SECONDS",
                RecordsInserted = 0, BatchNumber = 1, CreatedOn = unspecifiedTime
            };

            var result = await repo.AddDetailAsync(detail);

            Assert.Equal(DateTimeKind.Utc, result.CreatedOn.Kind);
        }

        #endregion

        #region UpdateDetailAsync

        [Fact]
        public async Task UpdateDetailAsync_SetsUpdatedOnAndSaves()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            var detail = new DataLoadDetail
            {
                DataLoadId = dl.Id, DataLoadType = "STG", ErrorIndicator = 0,
                TimeProcessValue = 10, TimePeriodTypeCode = "SECONDS",
                RecordsInserted = 100, BatchNumber = 1, CreatedOn = DateTime.UtcNow
            };
            ctx.DataLoadDetails.Add(detail);
            await ctx.SaveChangesAsync();

            detail.RecordsInserted = 200;
            detail.ErrorIndicator = 1;
            await repo.UpdateDetailAsync(detail);

            var updated = await ctx.DataLoadDetails.FindAsync(detail.Id);
            Assert.Equal(200, updated!.RecordsInserted);
            Assert.Equal(1, updated.ErrorIndicator);
            Assert.NotNull(updated.UpdatedOn);
            Assert.Equal(DateTimeKind.Utc, updated.UpdatedOn!.Value.Kind);
        }

        [Fact]
        public async Task UpdateDetailAsync_UpdatedOnIsRecentUtcNow()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad();
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            var detail = new DataLoadDetail
            {
                DataLoadId = dl.Id, DataLoadType = "STG", ErrorIndicator = 0,
                TimeProcessValue = 5, TimePeriodTypeCode = "SECONDS",
                RecordsInserted = 50, BatchNumber = 1, CreatedOn = DateTime.UtcNow
            };
            ctx.DataLoadDetails.Add(detail);
            await ctx.SaveChangesAsync();

            var before = DateTime.UtcNow;
            await repo.UpdateDetailAsync(detail);
            var after = DateTime.UtcNow;

            Assert.NotNull(detail.UpdatedOn);
            Assert.InRange(detail.UpdatedOn.Value, before, after);
        }

        #endregion

        #region AddErrorsAsync

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
        public async Task AddErrorsAsync_CreatedOnAlreadyUtc_PreservesValue()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var specificTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);
            var err = new DataLoadError
            {
                DataLoadDetailId = 1, ErrorCode = 1,
                ErrorStoredProcedureName = "sp", ErrorMessage = "msg",
                CreatedOn = specificTime
            };

            await repo.AddErrorsAsync(new[] { err });

            var saved = await ctx.DataLoadErrors.FirstAsync();
            Assert.Equal(specificTime, saved.CreatedOn);
            Assert.Equal(DateTimeKind.Utc, saved.CreatedOn.Kind);
        }

        [Fact]
        public async Task AddErrorsAsync_CreatedOnUnspecifiedKind_ConvertsToUtc()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var unspecifiedTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Unspecified);
            var err = new DataLoadError
            {
                DataLoadDetailId = 1, ErrorCode = 1,
                ErrorStoredProcedureName = "sp", ErrorMessage = "msg",
                CreatedOn = unspecifiedTime
            };

            await repo.AddErrorsAsync(new[] { err });

            var saved = await ctx.DataLoadErrors.FirstAsync();
            Assert.Equal(DateTimeKind.Utc, saved.CreatedOn.Kind);
        }

        [Fact]
        public async Task AddErrorsAsync_MultipleErrors_AllInserted()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var errors = new[]
            {
                new DataLoadError { DataLoadDetailId = 1, ErrorCode = 1, ErrorMessage = "e1", CreatedOn = default },
                new DataLoadError { DataLoadDetailId = 1, ErrorCode = 2, ErrorMessage = "e2", CreatedOn = DateTime.UtcNow },
                new DataLoadError { DataLoadDetailId = 1, ErrorCode = 3, ErrorMessage = "e3", CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }
            };

            await repo.AddErrorsAsync(errors);

            Assert.Equal(3, await ctx.DataLoadErrors.CountAsync());
            Assert.All(await ctx.DataLoadErrors.ToListAsync(), e => Assert.Equal(DateTimeKind.Utc, e.CreatedOn.Kind));
        }

        #endregion

        #region AddExceptionsAsync

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
        public async Task AddExceptionsAsync_CreatedOnAlreadyUtc_PreservesValue()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var specificTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);
            var ex = new DataLoadException
            {
                DataLoadDetailId = 1, TableName = "taltccy", TableKey = "k",
                ErrorFieldName = "f", ErrorFieldValue = "v",
                CreatedOn = specificTime
            };

            await repo.AddExceptionsAsync(new[] { ex });

            var saved = await ctx.DataLoadExceptions.FirstAsync();
            Assert.Equal(specificTime, saved.CreatedOn);
            Assert.Equal(DateTimeKind.Utc, saved.CreatedOn.Kind);
        }

        [Fact]
        public async Task AddExceptionsAsync_CreatedOnUnspecifiedKind_ConvertsToUtc()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var unspecifiedTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Unspecified);
            var ex = new DataLoadException
            {
                DataLoadDetailId = 1, TableName = "taltccy", TableKey = "k",
                ErrorFieldName = "f", ErrorFieldValue = "v",
                CreatedOn = unspecifiedTime
            };

            await repo.AddExceptionsAsync(new[] { ex });

            var saved = await ctx.DataLoadExceptions.FirstAsync();
            Assert.Equal(DateTimeKind.Utc, saved.CreatedOn.Kind);
        }

        [Fact]
        public async Task AddExceptionsAsync_MultipleExceptions_AllInserted()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var exceptions = new[]
            {
                new DataLoadException { DataLoadDetailId = 1, TableName = "t1", TableKey = "k1", ErrorFieldName = "f1", CreatedOn = default },
                new DataLoadException { DataLoadDetailId = 1, TableName = "t2", TableKey = "k2", ErrorFieldName = "f2", CreatedOn = DateTime.UtcNow },
                new DataLoadException { DataLoadDetailId = 1, TableName = "t3", TableKey = "k3", ErrorFieldName = "f3", CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Local) }
            };

            await repo.AddExceptionsAsync(exceptions);

            Assert.Equal(3, await ctx.DataLoadExceptions.CountAsync());
            Assert.All(await ctx.DataLoadExceptions.ToListAsync(), e => Assert.Equal(DateTimeKind.Utc, e.CreatedOn.Kind));
        }

        #endregion

        #region AnyProcessingAsync

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
        public async Task AnyProcessingAsync_ReturnsFalseWhenNoProcessingExists()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            Assert.False(await repo.AnyProcessingAsync());
        }

        [Fact]
        public async Task AnyProcessingAsync_ReturnsFalseWhenOnlyOtherStatuses()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            ctx.DataLoads.AddRange(
                MakeLoad(status: LoadStatus.ReadyForValidation),
                MakeLoad(table: "tdecode", ver: 2, status: LoadStatus.Processed),
                MakeLoad(table: "tasybrl", ver: 3, status: LoadStatus.Failed)
            );
            await ctx.SaveChangesAsync();

            Assert.False(await repo.AnyProcessingAsync());
        }

        #endregion

        #region ExistsAsync

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
        public async Task ExistsAsync_DifferentTableName_ReturnsFalse()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            ctx.DataLoads.Add(MakeLoad(table: "taltccy", loadVersion: "2026_02_18_1"));
            await ctx.SaveChangesAsync();

            Assert.False(await repo.ExistsAsync("tdecode", "2026_02_18_1"));
        }

        [Fact]
        public async Task ExistsAsync_EmptyDatabase_ReturnsFalse()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            Assert.False(await repo.ExistsAsync("taltccy", "2026_02_18_1"));
        }

        #endregion

        #region GetLoadsByStatusAsync

        [Fact]
        public async Task GetLoadsByStatusAsync_FiltersByStatus()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            ctx.DataLoads.AddRange(
                new DataLoad { LoadTableName = "taltccy", LoadVersionNumber = 1, LoadStatusCode = LoadStatus.ReadyForValidation.ToString(), FileLocation = "f1", DataSource = "src", CreatedOn = DateTime.UtcNow, LogFileLocation = "log", TotalBatchNumber = 0, BatchSize = 10, LoadVersion = "2026_02_18_2" },
                new DataLoad { LoadTableName = "taltccy", LoadVersionNumber = 2, LoadStatusCode = LoadStatus.Processing.ToString(), FileLocation = "f2", DataSource = "src", CreatedOn = DateTime.UtcNow, LogFileLocation = "log", TotalBatchNumber = 0, BatchSize = 10, LoadVersion = "2026_02_18_2" }
            );
            await ctx.SaveChangesAsync();
            var list = await repo.GetLoadsByStatusAsync(LoadStatus.Processing, "log");
            Assert.Single(list);
            Assert.Equal(2, list[0].LoadVersionNumber);
        }

        [Fact]
        public async Task GetLoadsByStatusAsync_NoMatch_ReturnsEmpty()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            ctx.DataLoads.Add(MakeLoad(status: LoadStatus.ReadyForValidation));
            await ctx.SaveChangesAsync();

            var result = await repo.GetLoadsByStatusAsync(LoadStatus.Processing, "log");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLoadsByStatusAsync_DifferentLogFileLocation_ReturnsEmpty()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            var dl = MakeLoad(status: LoadStatus.Processing);
            dl.LogFileLocation = "log1";
            ctx.DataLoads.Add(dl);
            await ctx.SaveChangesAsync();

            var result = await repo.GetLoadsByStatusAsync(LoadStatus.Processing, "log2");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLoadsByStatusAsync_EmptyDatabase_ReturnsEmpty()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            var result = await repo.GetLoadsByStatusAsync(LoadStatus.Processing, "log");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLoadsByStatusAsync_MultipleMatching_ReturnsAll()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);
            ctx.DataLoads.AddRange(
                MakeLoad(table: "taltccy", ver: 1, status: LoadStatus.Processing),
                MakeLoad(table: "tdecode", ver: 2, status: LoadStatus.Processing),
                MakeLoad(table: "tasybrl", ver: 3, status: LoadStatus.ReadyToProcess)
            );
            await ctx.SaveChangesAsync();

            var result = await repo.GetLoadsByStatusAsync(LoadStatus.Processing, "log");

            Assert.Equal(2, result.Count);
            Assert.All(result, l => Assert.Equal(LoadStatus.Processing.ToString(), l.LoadStatusCode));
        }

        #endregion

        #region MarkStagingCompletedForMultipleTablesAsync

        [Fact]
        public async Task MarkStagingCompletedForMultipleTablesAsync_EmptyCollection_ReturnsZero()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            var result = await repo.MarkStagingCompletedForMultipleTablesAsync(Array.Empty<string>());

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task MarkStagingCompletedForMultipleTablesAsync_NoStgSuffix_ReturnsZero()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            var result = await repo.MarkStagingCompletedForMultipleTablesAsync(new[] { "taltccy", "tdecode" });

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task MarkStagingCompletedForMultipleTablesAsync_MixedSuffixes_FiltersToStgOnly()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            // None have _stg suffix so returns 0 immediately
            var result = await repo.MarkStagingCompletedForMultipleTablesAsync(new[] { "taltccy", "tdecode_staging" });

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task MarkStagingCompletedForMultipleTablesAsync_CaseInsensitiveStgSuffix_Filters()
        {
            using var ctx = CreateContext();
            var repo = CreateRepo(ctx);

            // Names without _stg at all — all filtered out
            var result = await repo.MarkStagingCompletedForMultipleTablesAsync(new[] { "table1", "table2" });

            Assert.Equal(0, result);
        }

        #endregion

        #region MergeResult

        [Fact]
        public void MergeResult_HasError_WithErrorMessage_ReturnsTrue()
        {
            var result = new MergeResult(0, 0, 0, "42000", "ERROR", "sp_merge", "10", "Merge failed");

            Assert.True(result.HasError);
        }

        [Fact]
        public void MergeResult_HasError_NullErrorMessage_ReturnsFalse()
        {
            var result = new MergeResult(100, 50, 10, null, null, null, null, null);

            Assert.False(result.HasError);
        }

        [Fact]
        public void MergeResult_HasError_EmptyErrorMessage_ReturnsFalse()
        {
            var result = new MergeResult(100, 50, 10, null, null, null, null, "");

            Assert.False(result.HasError);
        }

        [Fact]
        public void MergeResult_HasError_WhitespaceErrorMessage_ReturnsFalse()
        {
            var result = new MergeResult(100, 50, 10, null, null, null, null, "   ");

            Assert.False(result.HasError);
        }

        [Fact]
        public void MergeResult_Properties_SetCorrectly()
        {
            var result = new MergeResult(100, 50, 25, "42000", "ERROR", "sp_test", "15", "Test error");

            Assert.Equal(100, result.Inserted);
            Assert.Equal(50, result.Updated);
            Assert.Equal(25, result.Deleted);
            Assert.Equal("42000", result.ErrorNumber);
            Assert.Equal("ERROR", result.ErrorState);
            Assert.Equal("sp_test", result.ErrorProcedure);
            Assert.Equal("15", result.ErrorLine);
            Assert.Equal("Test error", result.ErrorMessage);
        }

        [Fact]
        public void MergeResult_ZeroCounts_NoError()
        {
            var result = new MergeResult(0, 0, 0, null, null, null, null, null);

            Assert.Equal(0, result.Inserted);
            Assert.Equal(0, result.Updated);
            Assert.Equal(0, result.Deleted);
            Assert.False(result.HasError);
        }

        [Fact]
        public void MergeResult_OnlyErrorMessage_HasErrorTrue()
        {
            var result = new MergeResult(0, 0, 0, null, null, null, null, "Some error occurred");

            Assert.True(result.HasError);
            Assert.Null(result.ErrorNumber);
            Assert.Null(result.ErrorState);
            Assert.Null(result.ErrorProcedure);
            Assert.Null(result.ErrorLine);
        }

        #endregion
    }
}
