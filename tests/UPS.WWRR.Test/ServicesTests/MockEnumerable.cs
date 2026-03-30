using Google.Api.Gax;

namespace UPS.WWRR.UnitTests.ServicesTests;

internal class MockEnumerable<TResponse, TResource>(List<TResource> values) : PagedEnumerable<TResponse, TResource>
{
    private int index = 0;

    public override IEnumerator<TResource> GetEnumerator() => values.GetEnumerator();

    public override Page<TResource> ReadPage(int pageSize)
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
