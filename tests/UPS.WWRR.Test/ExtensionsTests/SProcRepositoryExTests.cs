#nullable enable
using System.Data;
using System.Data.Common;
using UPS.WWRR.Business.Extensions;

namespace UPS.WWRR.UnitTests.ExtensionsTests;

/// <summary>
/// Simple POCO used for MapToList / MapToListAsync tests.
/// Property names must match the column names in the DataTable.
/// </summary>
public class MapTestModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// POCO used for ExecuteStoredProcedure tests with SQLite, where INTEGER maps to Int64.
/// </summary>
public class SqliteMapTestModel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SProcRepositoryExTests
{
    #region WithSqlParams Tests

    [Fact]
    public void WithSqlParams_AddsParametersToCommand()
    {
        using var connection = CreateSqliteConnection();
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT 1";

        cmd.WithSqlParams(("@p1", "value1"), ("@p2", 42));

        Assert.Equal(2, cmd.Parameters.Count);
        Assert.Equal("@p1", cmd.Parameters[0].ParameterName);
        Assert.Equal("value1", cmd.Parameters[0].Value);
        Assert.Equal("@p2", cmd.Parameters[1].ParameterName);
        Assert.Equal(42, cmd.Parameters[1].Value);
    }

    [Fact]
    public void WithSqlParams_NullValue_SetsDbNull()
    {
        using var connection = CreateSqliteConnection();
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT 1";

        cmd.WithSqlParams(("@p1", (object)null!));

        Assert.Equal(DBNull.Value, cmd.Parameters[0].Value);
    }

    [Fact]
    public void WithSqlParams_NoParams_ReturnsCommandUnchanged()
    {
        using var connection = CreateSqliteConnection();
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT 1";

        var result = cmd.WithSqlParams();

        Assert.Same(cmd, result);
        Assert.Equal(0, cmd.Parameters.Count);
    }

    [Fact]
    public void WithSqlParams_ReturnsSameCommand_ForChaining()
    {
        using var connection = CreateSqliteConnection();
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT 1";

        var result = cmd.WithSqlParams(("@p1", 1));

        Assert.Same(cmd, result);
    }

    #endregion

    #region MapToList Tests

    [Fact]
    public void MapToList_WithRows_ReturnsMappedList()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, "Alice");
        table.Rows.Add(2, "Bob");

        using var reader = table.CreateDataReader();
        var result = reader.MapToList<MapTestModel>();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(2, result[1].Id);
        Assert.Equal("Bob", result[1].Name);
    }

    [Fact]
    public void MapToList_EmptyResultSet_ReturnsEmptyList()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));

        using var reader = table.CreateDataReader();
        var result = reader.MapToList<MapTestModel>();

        Assert.Empty(result);
    }

    [Fact]
    public void MapToList_WithDbNullValue_SetsPropertyToNull()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, DBNull.Value);

        using var reader = table.CreateDataReader();
        var result = reader.MapToList<MapTestModel>();

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Null(result[0].Name);
    }

    #endregion

    #region MapToListAsync Tests

    [Fact]
    public async Task MapToListAsync_WithRows_ReturnsMappedList()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(10, "Charlie");
        table.Rows.Add(20, "Diana");

        using var reader = table.CreateDataReader();
        var result = await reader.MapToListAsync<MapTestModel>();

        Assert.Equal(2, result.Count);
        Assert.Equal(10, result[0].Id);
        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal(20, result[1].Id);
        Assert.Equal("Diana", result[1].Name);
    }

    [Fact]
    public async Task MapToListAsync_EmptyResultSet_ReturnsEmptyList()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));

        using var reader = table.CreateDataReader();
        var result = await reader.MapToListAsync<MapTestModel>();

        Assert.Empty(result);
    }

    [Fact]
    public async Task MapToListAsync_WithDbNullValue_SetsPropertyToNull()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(5, DBNull.Value);

        using var reader = table.CreateDataReader();
        var result = await reader.MapToListAsync<MapTestModel>();

        Assert.Single(result);
        Assert.Equal(5, result[0].Id);
        Assert.Null(result[0].Name);
    }

    #endregion

    #region ExecuteStoredProcedure Tests

    [Fact]
    public void ExecuteStoredProcedure_ClosedConnection_OpensAndReturnsResults()
    {
        var dbName = $"ExecSP_{Guid.NewGuid():N}";
        var connString = $"DataSource={dbName};Mode=Memory;Cache=Shared";

        // Keeper connection holds the shared in-memory database alive
        using var keeper = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        keeper.Open();
        using var createCmd = keeper.CreateCommand();
        createCmd.CommandText = "CREATE TABLE TestData (Id INTEGER, Name TEXT)";
        createCmd.ExecuteNonQuery();
        using var insertCmd = keeper.CreateCommand();
        insertCmd.CommandText = "INSERT INTO TestData VALUES (1, 'Alice'), (2, 'Bob')";
        insertCmd.ExecuteNonQuery();

        // Second connection starts closed
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM TestData";
        cmd.CommandType = System.Data.CommandType.Text;

        var result = cmd.ExecuteStoredProcedure<SqliteMapTestModel>();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(2, result[1].Id);
        Assert.Equal("Bob", result[1].Name);
    }

    [Fact]
    public void ExecuteStoredProcedure_OpenConnection_ReturnsResults()
    {
        var dbName = $"ExecSPOpen_{Guid.NewGuid():N}";
        var connString = $"DataSource={dbName};Mode=Memory;Cache=Shared";

        using var keeper = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        keeper.Open();
        using var createCmd = keeper.CreateCommand();
        createCmd.CommandText = "CREATE TABLE TestOpen (Id INTEGER, Name TEXT)";
        createCmd.ExecuteNonQuery();
        using var insertCmd = keeper.CreateCommand();
        insertCmd.CommandText = "INSERT INTO TestOpen VALUES (10, 'Charlie')";
        insertCmd.ExecuteNonQuery();

        // Open the connection before creating the command
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM TestOpen";
        cmd.CommandType = System.Data.CommandType.Text;

        var result = cmd.ExecuteStoredProcedure<SqliteMapTestModel>();

        Assert.Single(result);
        Assert.Equal(10, result[0].Id);
        Assert.Equal("Charlie", result[0].Name);
    }

    [Fact]
    public void ExecuteStoredProcedure_EmptyTable_ReturnsEmptyList()
    {
        var dbName = $"ExecSPEmpty_{Guid.NewGuid():N}";
        var connString = $"DataSource={dbName};Mode=Memory;Cache=Shared";

        using var keeper = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        keeper.Open();
        using var createCmd = keeper.CreateCommand();
        createCmd.CommandText = "CREATE TABLE TestEmpty (Id INTEGER, Name TEXT)";
        createCmd.ExecuteNonQuery();

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM TestEmpty";
        cmd.CommandType = System.Data.CommandType.Text;

        var result = cmd.ExecuteStoredProcedure<SqliteMapTestModel>();

        Assert.Empty(result);
    }

    #endregion

    #region ExecuteStoredProcedureAsync Tests

    [Fact]
    public async Task ExecuteStoredProcedureAsync_ClosedConnection_OpensAndReturnsResult()
    {
        var dbName = $"ExecSPAsync_{Guid.NewGuid():N}";
        var connString = $"DataSource={dbName};Mode=Memory;Cache=Shared";

        using var keeper = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        keeper.Open();
        using var createCmd = keeper.CreateCommand();
        createCmd.CommandText = "CREATE TABLE TestAsyncData (Id INTEGER, Name TEXT)";
        createCmd.ExecuteNonQuery();
        using var insertCmd = keeper.CreateCommand();
        insertCmd.CommandText = "INSERT INTO TestAsyncData VALUES (1, 'Alpha'), (2, 'Beta')";
        insertCmd.ExecuteNonQuery();

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM TestAsyncData";
        cmd.CommandType = System.Data.CommandType.Text;

        var result = await cmd.ExecuteStoredProcedureAsync(async reader =>
        {
            return await reader.MapToListAsync<SqliteMapTestModel>();
        });

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Alpha", result[0].Name);
    }

    [Fact]
    public async Task ExecuteStoredProcedureAsync_OpenConnection_ReturnsResult()
    {
        var dbName = $"ExecSPAsyncOpen_{Guid.NewGuid():N}";
        var connString = $"DataSource={dbName};Mode=Memory;Cache=Shared";

        using var keeper = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        keeper.Open();
        using var createCmd = keeper.CreateCommand();
        createCmd.CommandText = "CREATE TABLE TestAsyncOpen (Id INTEGER, Name TEXT)";
        createCmd.ExecuteNonQuery();
        using var insertCmd = keeper.CreateCommand();
        insertCmd.CommandText = "INSERT INTO TestAsyncOpen VALUES (99, 'Gamma')";
        insertCmd.ExecuteNonQuery();

        // Open the connection before creating the command
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM TestAsyncOpen";
        cmd.CommandType = System.Data.CommandType.Text;

        var result = await cmd.ExecuteStoredProcedureAsync(async reader =>
        {
            return await reader.MapToListAsync<SqliteMapTestModel>();
        });

        Assert.Single(result);
        Assert.Equal(99, result[0].Id);
        Assert.Equal("Gamma", result[0].Name);
    }

    [Fact]
    public async Task ExecuteStoredProcedureAsync_EmptyTable_ReturnsEmptyList()
    {
        var dbName = $"ExecSPAsyncEmpty_{Guid.NewGuid():N}";
        var connString = $"DataSource={dbName};Mode=Memory;Cache=Shared";

        using var keeper = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        keeper.Open();
        using var createCmd = keeper.CreateCommand();
        createCmd.CommandText = "CREATE TABLE TestAsyncEmpty (Id INTEGER, Name TEXT)";
        createCmd.ExecuteNonQuery();

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connString);
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM TestAsyncEmpty";
        cmd.CommandType = System.Data.CommandType.Text;

        var result = await cmd.ExecuteStoredProcedureAsync(async reader =>
        {
            return await reader.MapToListAsync<SqliteMapTestModel>();
        });

        Assert.Empty(result);
    }

    #endregion

    #region Helpers

    private static Microsoft.Data.Sqlite.SqliteConnection CreateSqliteConnection()
    {
        return new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
    }

    #endregion
}
