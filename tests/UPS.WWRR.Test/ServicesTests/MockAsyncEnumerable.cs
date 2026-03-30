using Google.Api.Gax;

namespace UPS.WWRR.UnitTests.ServicesTests;

internal class MockAsyncEnumerable<TResponse, TResource>(List<TResource> values) : PagedAsyncEnumerable<TResponse, TResource>
{
    private int index = 0;

    private static async IAsyncEnumerable<T> CreateAsyncEnumerable<T>(IEnumerable<T> input)
    {
        foreach (var value in input)
        {
            yield return value;
        }
    }

    public override IAsyncEnumerator<TResource> GetAsyncEnumerator(CancellationToken cancellationToken = default) => CreateAsyncEnumerable(values).GetAsyncEnumerator();

    public override async Task<Page<TResource>> ReadPageAsync(int pageSize, CancellationToken cancellationToken = default)
    {
        var pageItems = new List<TResource>();
        for (var i = 0; i < pageSize && index + i < values.Count; ++i)
        {
            var item = values[index + i];
            pageItems.Add(item);
        }
        index += pageSize;
        return new Page<TResource>(pageItems, "");
    }
}
