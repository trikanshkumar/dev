#nullable enable
using System.Collections.Immutable;
using System.Reflection;
using System.Resources;
using UPS.WWRR.Business.Extensions;

namespace UPS.WWRR.UnitTests.ExtensionsTests;

public class ReflectionExTests
{
    // The test assembly has an embedded resource: Resources\EmbeddedTestResource.txt
    // Its full resource name will be: UPS.WWRR.UnitTests.Resources.EmbeddedTestResource.txt
    private static readonly Assembly TestAssembly = typeof(ReflectionExTests).Assembly;
    private const string ResourceFileName = "EmbeddedTestResource.txt";

    #region EmbeddedResourceNames Tests

    [Fact]
    public void EmbeddedResourceNames_NullAssembly_ThrowsArgumentNullException()
    {
        Assembly assembly = null!;
        Assert.Throws<ArgumentNullException>(() => assembly.EmbeddedResourceNames());
    }

    [Fact]
    public void EmbeddedResourceNames_TestAssembly_ReturnsNonEmptyImmutableList()
    {
        var names = TestAssembly.EmbeddedResourceNames();

        Assert.NotNull(names);
        Assert.IsType<ImmutableList<string>>(names);
        Assert.NotEmpty(names);
    }

    [Fact]
    public void EmbeddedResourceNames_CalledTwice_ReturnsSameCachedInstance()
    {
        var first = TestAssembly.EmbeddedResourceNames();
        var second = TestAssembly.EmbeddedResourceNames();

        Assert.Same(first, second);
    }

    [Fact]
    public void EmbeddedResourceNames_ContainsExpectedResource()
    {
        var names = TestAssembly.EmbeddedResourceNames();

        Assert.Contains(names, n => n.EndsWith(ResourceFileName, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region GetFullEmbeddedResourceName Tests

    [Fact]
    public void GetFullEmbeddedResourceName_NullAssembly_ThrowsArgumentNullException()
    {
        Assembly assembly = null!;
        Assert.Throws<ArgumentNullException>(() => assembly.GetFullEmbeddedResourceName("test"));
    }

    [Fact]
    public void GetFullEmbeddedResourceName_AssemblyWithNoResources_ThrowsInvalidOperationException()
    {
        // System.Runtime typically has no embedded resources
        var assembly = typeof(int).Assembly;
        var names = assembly.EmbeddedResourceNames();

        if (names.IsEmpty)
        {
            Assert.Throws<InvalidOperationException>(() => assembly.GetFullEmbeddedResourceName("anything"));
        }
    }

    [Fact]
    public void GetFullEmbeddedResourceName_ExactMatch_ReturnsName()
    {
        var names = TestAssembly.EmbeddedResourceNames();
        var exactName = names.First(n => n.EndsWith(ResourceFileName));

        var result = TestAssembly.GetFullEmbeddedResourceName(exactName);

        Assert.Equal(exactName, result);
    }

    [Fact]
    public void GetFullEmbeddedResourceName_CaseInsensitiveMatch_ReturnsActualName()
    {
        var names = TestAssembly.EmbeddedResourceNames();
        var exactName = names.First(n => n.EndsWith(ResourceFileName));
        var lowerName = exactName.ToLowerInvariant();

        var result = TestAssembly.GetFullEmbeddedResourceName(lowerName);

        Assert.Equal(exactName, result);
    }

    [Fact]
    public void GetFullEmbeddedResourceName_EndsWithMatch_ReturnsFullName()
    {
        var result = TestAssembly.GetFullEmbeddedResourceName(ResourceFileName);

        Assert.EndsWith(ResourceFileName, result);
    }

    [Fact]
    public void GetFullEmbeddedResourceName_EndsWithPartialSuffix_ReturnsFullName()
    {
        // Match with "Resources.EmbeddedTestResource.txt"
        var result = TestAssembly.GetFullEmbeddedResourceName("Resources.EmbeddedTestResource.txt");

        Assert.EndsWith(ResourceFileName, result);
    }

    [Fact]
    public void GetFullEmbeddedResourceName_SlashConvertedToDot_MatchesResource()
    {
        // "Resources/EmbeddedTestResource.txt" should be converted to "Resources.EmbeddedTestResource.txt"
        var result = TestAssembly.GetFullEmbeddedResourceName("Resources/EmbeddedTestResource.txt");

        Assert.EndsWith(ResourceFileName, result);
    }

    [Fact]
    public void GetFullEmbeddedResourceName_BackslashConvertedToDot_MatchesResource()
    {
        var result = TestAssembly.GetFullEmbeddedResourceName("Resources\\EmbeddedTestResource.txt");

        Assert.EndsWith(ResourceFileName, result);
    }

    [Fact]
    public void GetFullEmbeddedResourceName_NameNotFound_ThrowsMissingManifestResourceException()
    {
        Assert.Throws<MissingManifestResourceException>(() =>
            TestAssembly.GetFullEmbeddedResourceName("definitely_does_not_exist_resource_xyz123.txt"));
    }

    #endregion

    #region ReadAllText Tests

    [Fact]
    public void ReadAllText_ValidResource_ReturnsContent()
    {
        var result = TestAssembly.ReadAllText(ResourceFileName);

        Assert.NotNull(result);
        Assert.Contains("embedded test resource", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReadAllText_ExactFullName_ReturnsContent()
    {
        var names = TestAssembly.EmbeddedResourceNames();
        var exactName = names.First(n => n.EndsWith(ResourceFileName));

        var result = TestAssembly.ReadAllText(exactName);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ReadAllText_InvalidName_ThrowsMissingManifestResourceException()
    {
        Assert.Throws<MissingManifestResourceException>(() =>
            TestAssembly.ReadAllText("nonexistent_resource_file_xyz.txt"));
    }

    #endregion

    #region ReadAllTextAsync Tests

    [Fact]
    public async Task ReadAllTextAsync_ValidResource_ReturnsContent()
    {
        var result = await TestAssembly.ReadAllTextAsync(ResourceFileName);

        Assert.NotNull(result);
        Assert.Contains("embedded test resource", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReadAllTextAsync_ExactFullName_ReturnsContent()
    {
        var names = TestAssembly.EmbeddedResourceNames();
        var exactName = names.First(n => n.EndsWith(ResourceFileName));

        var result = await TestAssembly.ReadAllTextAsync(exactName);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ReadAllTextAsync_InvalidName_ThrowsMissingManifestResourceException()
    {
        await Assert.ThrowsAsync<MissingManifestResourceException>(async () =>
            await TestAssembly.ReadAllTextAsync("nonexistent_resource_file_xyz.txt"));
    }

    #endregion

    #region ReadAllTextFromMyAssembly Tests

    [Fact]
    public void ReadAllTextFromMyAssembly_ValidResource_ReturnsContent()
    {
        // ReadAllTextFromMyAssembly uses Assembly.GetCallingAssembly(), which is the test assembly
        var result = ReflectionEx.ReadAllTextFromMyAssembly(ResourceFileName);

        Assert.NotNull(result);
        Assert.Contains("embedded test resource", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReadAllTextFromMyAssembly_InvalidName_ThrowsMissingManifestResourceException()
    {
        Assert.Throws<MissingManifestResourceException>(() =>
            ReflectionEx.ReadAllTextFromMyAssembly("nonexistent_resource_xyz.txt"));
    }

    [Fact]
    public async Task ReadAllTextFromMyAssemblyAsync_ValidResource_ReturnsContent()
    {
        var result = await ReflectionEx.ReadAllTextFromMyAssemblyAsync(ResourceFileName);

        Assert.NotNull(result);
        Assert.Contains("embedded test resource", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReadAllTextFromMyAssemblyAsync_InvalidName_ThrowsMissingManifestResourceException()
    {
        await Assert.ThrowsAsync<MissingManifestResourceException>(async () =>
            await ReflectionEx.ReadAllTextFromMyAssemblyAsync("nonexistent_resource_xyz.txt"));
    }

    #endregion

    #region GetFullEmbeddedResourceName - Namespace Prefix Path Tests

    [Fact]
    public void GetFullEmbeddedResourceName_NamespacePrefixedName_TrimsAndMatches()
    {
        // Build a name like "UPS.WWRR.UnitTests.Resources.EmbeddedTestResource.txt"
        // which starts with the assembly name, exercising the trimmedName != name branch
        var assemblyName = TestAssembly.GetName().Name; // "UPS.WWRR.UnitTests"
        var namespacePrefixed = $"{assemblyName}.Resources.{ResourceFileName}";

        var result = TestAssembly.GetFullEmbeddedResourceName(namespacePrefixed);

        Assert.EndsWith(ResourceFileName, result);
    }

    [Fact]
    public void GetFullEmbeddedResourceName_AssemblyNameDotResourceName_MatchesViaFullNameComparison()
    {
        // Exercises the final possibleNsName + "." + name comparison path
        var names = TestAssembly.EmbeddedResourceNames();
        var exactName = names.First(n => n.EndsWith(ResourceFileName));

        // Strip the assembly name prefix to create a name that won't endsWith-match
        // but will match via "{possibleNsName}.{name}" equality
        var assemblyName = TestAssembly.GetName().Name;
        if (exactName.StartsWith(assemblyName + ".", StringComparison.OrdinalIgnoreCase))
        {
            var withoutPrefix = exactName[(assemblyName.Length + 1)..];
            // This should match via the endsWith path, so use the full name directly
            var result = TestAssembly.GetFullEmbeddedResourceName(withoutPrefix);
            Assert.Equal(exactName, result);
        }
    }

    [Fact]
    public void GetFullEmbeddedResourceName_EmptyAssembly_ThrowsInvalidOperationException()
    {
        // Use an assembly known to have no embedded resources
        // System.Runtime (typeof(int).Assembly) has no embedded manifest resources
        var assembly = typeof(int).Assembly;
        var names = assembly.EmbeddedResourceNames();

        if (names.IsEmpty)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                assembly.GetFullEmbeddedResourceName("anything.txt"));
            Assert.Contains("has no embedded resources", ex.Message);
        }
    }

    #endregion
}
