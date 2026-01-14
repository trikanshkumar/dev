#nullable enable
using System.Text;
using UPS.WWRR.Business.Interfaces;

namespace UPS.WWRR.Business.Services
{
    /// <summary>
    /// Csv Splitter implementation that reads a CSV file and yields chunks of specified number of data lines.
    /// </summary>
    public class CsvSplitterService : ICsvSplitterService
    {
        /// <summary>
        /// It Splits a CSV file and yields successive stream of string chunks of up to configured chunkSize.
        /// </summary>
        public async IAsyncEnumerable<string> SplitAsync(string csvFilePath, int chunkSize, bool hasHeader, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var stream = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);
            string? headerLine = null;
            if (hasHeader)
            {
                headerLine = await reader.ReadLineAsync();
            }
            var buffer = new List<string>(chunkSize);
            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync();
                if (line == null) break;
                buffer.Add(line);
                if (buffer.Count >= chunkSize)
                {
                    yield return ComposeChunk(buffer, headerLine);
                    buffer.Clear();
                }
            }
            if (buffer.Count > 0)
            {
                yield return ComposeChunk(buffer, headerLine);
            }
        }
        private static string ComposeChunk(List<string> lines, string? header)
        {
            var sb = new StringBuilder();
            if (header != null) sb.AppendLine(header);
            foreach (var l in lines)
                sb.AppendLine(l);
            return sb.ToString();
        }
    }
}
