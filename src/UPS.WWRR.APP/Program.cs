using Google.Cloud.PubSub.V1;
using Google.Cloud.Storage.V1;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using System.Diagnostics.CodeAnalysis;
using UPS.WWRR.API.Infrastructure;
using UPS.WWRR.Business.Common.Helper;
using UPS.WWRR.Business.Interfaces;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Business.Services;
using UPS.WWRR.Data.Common;
using UPS.WWRR.Data.Models;

namespace UPS.WWRR.App;

[ExcludeFromCodeCoverage]
class Program
{
    static async Task Main(string[] args)
    {
        // Configure Serilog — CsvLoadLogEntry and CsvLoadSummaryLogEntry go as JSON to Log console,
        // all other logs use plain-text console output.
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    Matching.WithProperty("CsvLoadLogEntry")(e) ||
                    Matching.WithProperty("CsvLoadSummaryLogEntry")(e))
                .WriteTo.Console(new RenderedCompactJsonFormatter()))
            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(e =>
                    Matching.WithProperty("CsvLoadLogEntry")(e) ||
                    Matching.WithProperty("CsvLoadSummaryLogEntry")(e))
                .WriteTo.Console())
            .CreateLogger();

        // Direct environment variable reads (no helper methods)
        var connectionString = Environment.GetEnvironmentVariable("ALLOYDB_CONNECTION")
            ?? throw new InvalidOperationException("Required environment variable 'ALLOYDB_CONNECTION' not set.");
        var tableName = Environment.GetEnvironmentVariable("TABLE_NAME")
            ?? throw new InvalidOperationException("Required environment variable 'TABLE_NAME' not set.");
        var batchSize = int.TryParse(Environment.GetEnvironmentVariable("BATCH_SIZE"), out var bs) ? bs : 100;
        var chunkSize = int.TryParse(Environment.GetEnvironmentVariable("BatchLoad_ChunkSize"), out var cs) ? cs : 10000; 
        var delimiter = Environment.GetEnvironmentVariable("BatchLoad_Delimiter") ?? ",";
        var hasHeader = bool.TryParse(Environment.GetEnvironmentVariable("BatchLoad_HasHeader"), out var hh) ? hh : true;
        var bucket = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_STORAGE_BUCKET_NAME") ?? string.Empty;
        var bucketSubName = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_STORAGE_BUCKET_SUB_NAME") ?? string.Empty;
        var enableIAMTokenAuth = bool.TryParse(Environment.GetEnvironmentVariable("EnableIAMTokenAuth"), out var iam_tok) ? iam_tok : true;
        var pubSubProjectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID") ?? string.Empty;
        var pubSubTopicId = Environment.GetEnvironmentVariable("PUBSUB_TOPIC_ID") ?? string.Empty;


        // GCS download chunk size (separate from CSV batch processing)
        var gcsDownloadChunkSize = int.TryParse(Environment.GetEnvironmentVariable("GCS_DOWNLOAD_CHUNK_SIZE"), out var gcs) ? gcs : 64 * 1024 * 1024; // 64 MB default

        NpgsqlDataSource dataSource = null;

        if (enableIAMTokenAuth)
        {
            // Pull stable values from configuration / Secret Manager(NOT the token)
            var (db_host, database, iamDbUser) = PgDataSourceFactory.Parse(connectionString);


            // Create a single shared NpgsqlDataSource for pooling
            dataSource = await PgDataSourceFactory.CreateAsync(
                    host: db_host,
                    database: database,
                    iamDbUser: iamDbUser,
                    requireSsl: true);
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

        var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(dataSource);

                    services.AddDbContext<DataContext>(options =>
                        options.UseNpgsql(dataSource, npgSqlOptions =>
                            npgSqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DataConstants.defaultSchema)));
     
                    services.AddScoped<INpgsqlConnectionHelper, NpgsqlConnectionHelper>();
                    services.AddScoped<ICsvSplitterService, CsvSplitterService>();
                    services.AddScoped<ICopyBatchDataService, CopyBatchDataService>();
                    services.AddScoped<ICsvValidator, CsvValidator>();
                    services.AddScoped<ILoadRepository, LoadRepository>();
                    // Worker is scoped (so it can use DbContext safely)
                    services.AddScoped<IBatchProcessorWorker, BatchProcessorWorker>();

                    services.AddHostedService<BatchProcessor>();

                    var storageClient = StorageClient.Create();
                    services.AddSingleton<IStorageService>(_ => new GoogleCloudStorageService(storageClient, bucket, bucketSubName, gcsDownloadChunkSize));
                    services.AddSingleton(new LocalRuntimeSettings(connectionString, tableName, batchSize, chunkSize, delimiter, hasHeader, bucket));

                    if (!string.IsNullOrEmpty(pubSubProjectId) && !string.IsNullOrEmpty(pubSubTopicId))
                    {
                        var topicName = TopicName.FromProjectTopic(pubSubProjectId, pubSubTopicId);
                        var publisherClient = PublisherClient.CreateAsync(topicName).GetAwaiter().GetResult();
                        services.AddSingleton(publisherClient);
                        services.AddSingleton<IGooglePubSubService, GooglePubSubService>();
                    }
                })
                .UseSerilog()
                .Build();

        await host.RunAsync();
    }
}

public record LocalRuntimeSettings(string ConnectionString, string TableName, int BatchSize, int ChunkSize, string Delimiter, bool HasHeader, string BucketName);