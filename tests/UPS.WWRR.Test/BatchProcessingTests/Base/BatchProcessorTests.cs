#nullable enable
using Microsoft.Extensions.Logging;
using Moq;
using UPS.WWRR.Business.Interfaces;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Business.Services;

namespace UPS.WWRR.UnitTests.BatchProcessing.Base;

public abstract class BatchProcessorTests
{
    protected const string Bucket = "test-bucket";
    protected readonly Mock<ILogger<BatchProcessor>> _logger = new();
    protected readonly Mock<IStorageService> _storage = new();
    protected readonly Mock<ICsvValidator> _validator = new();
    protected readonly Mock<ICopyBatchDataService> _copy = new();
    protected readonly Mock<ILoadRepository> _repo = new();

    protected BatchProcessorTests()
    {
        var dict = new Dictionary<string, string?>
        {
            ["LOAD_INTERVAL_MINUTES"] = "1",
            ["GOOGLE_CLOUD_STORAGE_BUCKET_NAME"] = "test-bucket",
            ["TABLE_NAME"] = "ALL"
        };

        foreach (var kv in dict)
        {
            Environment.SetEnvironmentVariable(kv.Key, kv.Value);
        }
    }

    protected BatchProcessor CreateSut() => new(_logger.Object, _storage.Object, _validator.Object, _copy.Object, _repo.Object);

    protected static async Task<T> InvokeAsync<T>(object target, string name, params object[] args)
    {
        var mi = target.GetType().GetMethod(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (mi == null)
        {
            throw new InvalidOperationException($"Method {name} not found on {target.GetType().Name}.");
        }

        var ret = mi.Invoke(target, args);
        if (ret is not Task task)
        {
            return ret is T value ? value : default!;
        }

        await task.ConfigureAwait(false);
        if (task.GetType().IsGenericType)
        {
            return (T)task.GetType().GetProperty("Result")!.GetValue(task)!;
        }

        return default!;
    }
}
