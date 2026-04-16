#nullable enable
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace UPS.WWRR.Business.Common.Helper
{
    /// <summary>
    /// Helper providing a lazily opened NpgsqlConnection based on configuration.
    /// </summary>
    public class NpgsqlConnectionHelper : INpgsqlConnectionHelper
    {
        private readonly NpgsqlDataSource _dataSource;

        /// <summary>
        /// constructor that reads connection string from configuration
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="configuration"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public NpgsqlConnectionHelper(NpgsqlDataSource dataSource, IConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ALLOYDB_CONNECTION")))
                throw new InvalidOperationException("Required environment variable 'ALLOYDB_CONNECTION' not set.");
            _dataSource = dataSource;
        }

        /// <summary>
        /// Opens and returns an open NpgsqlConnection (opens lazily if needed).
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken ct = default)
        {
            return await _dataSource.OpenConnectionAsync(ct);
        }

        /// <summary>
        /// Executes a non-query SQL command (e.g., TRUNCATE).
        /// </summary>
        public async Task ExecuteNonQueryAsync(NpgsqlConnection connection, string sql, CancellationToken ct = default)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync(ct);
        }

        /// <summary>
        /// Begins a text import (COPY) operation.
        /// </summary>
        public async Task<TextWriter> BeginTextImportAsync(NpgsqlConnection connection, string copySql, CancellationToken ct = default)
        {
            return await connection.BeginTextImportAsync(copySql, ct);
        }
    }
}
