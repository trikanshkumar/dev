using Microsoft.Extensions.Configuration;
using Moq;
using Npgsql;
using UPS.WWRR.Business.Common.Helper;

namespace UPS.WWRR.UnitTests.HelpersTests;

public class NpgsqlConnectionHelperTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_MissingEnvVariable_ThrowsInvalidOperationException()
    {
        // Arrange - ensure the env variable is not set
        var original = Environment.GetEnvironmentVariable("ALLOYDB_CONNECTION");
        try
        {
            Environment.SetEnvironmentVariable("ALLOYDB_CONNECTION", null);

            var dataSource = new NpgsqlDataSourceBuilder("Host=localhost;Database=test;Username=user;Password=pass").Build();
            var configuration = new Mock<IConfiguration>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                new NpgsqlConnectionHelper(dataSource, configuration.Object));

            dataSource.Dispose();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ALLOYDB_CONNECTION", original);
        }
    }

    [Fact]
    public void Constructor_WithEnvVariable_CreatesInstance()
    {
        // Arrange
        var original = Environment.GetEnvironmentVariable("ALLOYDB_CONNECTION");
        try
        {
            Environment.SetEnvironmentVariable("ALLOYDB_CONNECTION", "Host=localhost;Database=test;Username=user;Password=pass");

            var dataSource = new NpgsqlDataSourceBuilder("Host=localhost;Database=test;Username=user;Password=pass").Build();
            var configuration = new Mock<IConfiguration>();

            // Act
            var helper = new NpgsqlConnectionHelper(dataSource, configuration.Object);

            // Assert
            Assert.NotNull(helper);

            dataSource.Dispose();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ALLOYDB_CONNECTION", original);
        }
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void NpgsqlConnectionHelper_ImplementsINpgsqlConnectionHelper()
    {
        Assert.True(typeof(INpgsqlConnectionHelper).IsAssignableFrom(typeof(NpgsqlConnectionHelper)));
    }

    #endregion
}
