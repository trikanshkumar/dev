namespace UPS.WWRR.UnitTests
{
    /// <summary>
    /// Central store for mock / test data helpers to keep test classes lean.
    /// </summary>
    public static class MockDataStore
    {
        /// <summary>
        /// Creates a temporary CSV file with optional header and provided data lines.
        /// </summary>
        public static string CreateTempCsv(bool includeHeader, IEnumerable<string> dataLines, string header = "Id,Name")
        {
            var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
            using var writer = new StreamWriter(file);
            if (includeHeader) writer.WriteLine(header);
            foreach (var l in dataLines) writer.WriteLine(l);
            return file;
        }

        /// <summary>
        /// Collects an async enumerable into a list.
        /// </summary>
        public static async Task<List<string>> CollectAsync(IAsyncEnumerable<string> source, CancellationToken ct = default)
        {
            var list = new List<string>();
            await foreach (var item in source.WithCancellation(ct))
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// Splits a chunk string into its individual lines removing empty entries.
        /// </summary>
        public static IEnumerable<string> ChunkLines(string chunk) =>
            chunk.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Creates an async enumerable from a set of items (used for mocking).
        /// </summary>
        public static async IAsyncEnumerable<string> TestAsync(IEnumerable<string> items, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var i in items)
            {
                ct.ThrowIfCancellationRequested();
                yield return i;
                await Task.Yield();
            }
        }

        /// <summary>
        /// Builds sequential CSV data lines in format: index,valuePrefix+index.
        /// </summary>
        public static string[] BuildSequentialLines(int count, string valuePrefix = "Val") =>
            Enumerable.Range(1, count).Select(i => $"{i},{valuePrefix}{i}").ToArray();
    }
}
