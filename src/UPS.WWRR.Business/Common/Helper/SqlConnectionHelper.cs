#nullable enable
using Npgsql;

namespace UPS.WWRR.Business.Common.Helper
{
    /// <summary>
    /// Helper that ensures a single NpgsqlConnection is opened once during its lifetime.
    /// Dispose to close the connection. Intended for one request/operation scope.
    /// </summary>
    public class SqlConnectionHelper : IAsyncDisposable
    {
        private readonly string _connectionString;
        private NpgsqlConnection? _connection;
        private bool _disposed;

        public SqlConnectionHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Returns an open connection, opening it lazily on first call.
        /// </summary>
        public async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken ct = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SqlConnectionHelper));
            if (_connection is { State: System.Data.ConnectionState.Open }) return _connection;
            if (_connection == null)
            {
                _connection = new NpgsqlConnection(_connectionString);
                await _connection.OpenAsync(ct);
            }
            else if (_connection.State != System.Data.ConnectionState.Open)
            {
                await _connection.OpenAsync(ct);
            }
            return _connection;
        }

        /// <summary>
        /// Disposes the helper and closes the connection, Automatically gets called on "await using"
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            if (_connection != null)
            {
                try { await _connection.CloseAsync(); } catch { }
                await _connection.DisposeAsync();
            }
        }
    }
}
