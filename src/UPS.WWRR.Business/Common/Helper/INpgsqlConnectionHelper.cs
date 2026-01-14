using Npgsql;
namespace UPS.WWRR.Business.Common.Helper
{
    /// <summary>
    /// Abstraction for obtaining an open NpgsqlConnection and executing commands.
    /// </summary>
    public interface INpgsqlConnectionHelper
    {
        /// <summary>
        /// Returns an open NpgsqlConnection (opens lazily if needed).
        /// </summary>
        Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken ct = default);

        /// <summary>
        /// Executes a non-query SQL command (e.g., TRUNCATE).
        /// </summary>
        Task ExecuteNonQueryAsync(NpgsqlConnection connection, string sql, CancellationToken ct = default);

        /// <summary>
        /// Begins a text import (COPY) operation.
        /// </summary>
        Task<TextWriter> BeginTextImportAsync(NpgsqlConnection connection, string copySql, CancellationToken ct = default);
    }
}
