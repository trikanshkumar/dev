using UPS.WWRR.Business.Common.Helper;

namespace UPS.WWRR.UnitTests.HelpersTests;

public class PgDataSourceFactoryTests
{
    #region Parse Tests

    [Fact]
    public void Parse_ValidConnectionString_ReturnsAllComponents()
    {
        // Arrange
        var connectionString = "Host=myhost;Database=mydb;Username=myuser";

        // Act
        var (host, database, iamDbUser) = PgDataSourceFactory.Parse(connectionString);

        // Assert
        Assert.Equal("myhost", host);
        Assert.Equal("mydb", database);
        Assert.Equal("myuser", iamDbUser);
    }

    [Fact]
    public void Parse_WithServerKey_ReturnsHost()
    {
        // Arrange
        var connectionString = "Server=serverhost;Database=mydb;Username=myuser";

        // Act
        var (host, database, iamDbUser) = PgDataSourceFactory.Parse(connectionString);

        // Assert
        Assert.Equal("serverhost", host);
    }

    [Fact]
    public void Parse_WithUserIdKey_ReturnsUsername()
    {
        // Arrange
        var connectionString = "Host=myhost;Database=mydb;User Id=someuser";

        // Act
        var (_, _, iamDbUser) = PgDataSourceFactory.Parse(connectionString);

        // Assert
        Assert.Equal("someuser", iamDbUser);
    }

    [Fact]
    public void Parse_MissingHost_ReturnsEmptyString()
    {
        // Arrange
        var connectionString = "Database=mydb;Username=myuser";

        // Act
        var (host, _, _) = PgDataSourceFactory.Parse(connectionString);

        // Assert
        Assert.Equal("", host);
    }

    [Fact]
    public void Parse_MissingDatabase_ReturnsEmptyString()
    {
        // Arrange
        var connectionString = "Host=myhost;Username=myuser";

        // Act
        var (_, database, _) = PgDataSourceFactory.Parse(connectionString);

        // Assert
        Assert.Equal("", database);
    }

    [Fact]
    public void Parse_MissingUsername_ReturnsEmptyString()
    {
        // Arrange
        var connectionString = "Host=myhost;Database=mydb";

        // Act
        var (_, _, iamDbUser) = PgDataSourceFactory.Parse(connectionString);

        // Assert
        Assert.Equal("", iamDbUser);
    }

    [Fact]
    public void Parse_EmptyConnectionString_ReturnsAllEmpty()
    {
        // Arrange
        var connectionString = "";

        // Act
        var (host, database, iamDbUser) = PgDataSourceFactory.Parse(connectionString);

        // Assert
        Assert.Equal("", host);
        Assert.Equal("", database);
        Assert.Equal("", iamDbUser);
    }

    [Fact]
    public void Parse_HostKeyTakesPriorityOverServer()
    {
        // Arrange - both Host and Server present; Host is checked first
        var connectionString = "Host=primary;Server=secondary;Database=mydb;Username=myuser";

        // Act
        var (host, _, _) = PgDataSourceFactory.Parse(connectionString);

        // Assert
        Assert.Equal("primary", host);
    }

    #endregion

    #region Create (Local) Tests

    [Fact]
    public async Task Create_WithoutSsl_ReturnsDataSource()
    {
        // Arrange
        var connectionString = "Host=localhost;Database=testdb;Username=testuser;Password=testpass";

        // Act
        var dataSource = await PgDataSourceFactory.Create(connectionString, requireSsl: false);

        // Assert
        Assert.NotNull(dataSource);
        dataSource.Dispose();
    }

    [Fact]
    public async Task Create_WithSsl_ReturnsDataSource()
    {
        // Arrange
        var connectionString = "Host=localhost;Database=testdb;Username=testuser;Password=testpass";

        // Act
        var dataSource = await PgDataSourceFactory.Create(connectionString, requireSsl: true);

        // Assert
        Assert.NotNull(dataSource);
        dataSource.Dispose();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithSsl_ReturnsDataSource()
    {
        // Arrange
        var host = "localhost";
        var database = "testdb";
        var iamDbUser = "testuser";
        var requireSsl = true;
        // Act
        var dataSource = await PgDataSourceFactory.CreateAsync(host, database, iamDbUser, requireSsl, CancellationToken.None);
        // Assert
        Assert.NotNull(dataSource);
        dataSource.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithoutSsl_ReturnsDataSource()
    {
        // Arrange
        var host = "localhost";
        var database = "testdb";
        var iamDbUser = "testuser";
        var requireSsl = false;
        // Act
        var dataSource = await PgDataSourceFactory.CreateAsync(host, database, iamDbUser, requireSsl, CancellationToken.None);
        // Assert
        Assert.NotNull(dataSource);
        dataSource.Dispose();
    }

    #endregion
}
