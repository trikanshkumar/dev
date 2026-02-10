#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UPS.WWRR.Business.Interfaces;

namespace UPS.WWRR.Business.Services
{
	/// <summary>
	/// background service for processing data loads
	/// </summary>
	public class BatchProcessor : BackgroundService
	{
		private readonly ILogger<BatchProcessor> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
		private readonly int _intervalMinutes;

		public BatchProcessor(ILogger<BatchProcessor> logger, IServiceScopeFactory scopeFactory)
		{
			_logger = logger;
            _scopeFactory = scopeFactory;
            _intervalMinutes = int.TryParse(Environment.GetEnvironmentVariable("LOAD_INTERVAL_MINUTES"), out var m) ? m : 5;
		}

		/// <summary>
		/// Processor execution loop
		/// </summary>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var worker = scope.ServiceProvider.GetRequiredService<IBatchProcessorWorker>();

                    await worker.ProcessAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in BatchProcessor loop.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // shutdown
                }

                await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
			}
		}

	}
}