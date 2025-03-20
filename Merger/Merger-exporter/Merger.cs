using System.Threading.Channels;

using nietras.SeparatedValues;

namespace Merger_exporter;

class Merger(
    List<IGrouping<string, string>> gtfsFilesByName,
    string destinationFolder,
    List<SepReader> readers
) : IAsyncDisposable
{
    Channel<string> WriteChannel { get; set; } = Channel.CreateUnbounded<string>();
;    /*º
     * Before reading, the files should be grouped by their name
     * Then read the files in each group and write them into a channel, the channel reads from another process and writes into the final file
     *
     * */
    public static async Task<Merger> CreateMergerAsync(
        List<IGrouping<string, string>> gtfsFilesByName,
        string destinationFolder,
        CancellationToken cancellationToken
    )
    {
        List<SepReader> readers = [];

        foreach (var gtfsFiles in gtfsFilesByName)
        {
            foreach (var file in gtfsFiles)
            {
                var reader = await Sep.Reader().FromFileAsync(file, cancellationToken);
                readers.Add(reader);
                var a = reader.ParallelEnumerate(row =>
                {
                    for (var i = 0; i < row.ColCount; i++)
                    {
                        var colName = row[i];
                        WriteChannel.Writer.WriteAsync(row.Span.ToString());
                    }
                });
            }
        }

        return new Merger(gtfsFilesByName, destinationFolder, readers);
    }

    public async Task<string> MergeAsync(CancellationToken cancellationToken)
    {
        foreach (var item in gtfsFilesByName)
        {
            Console.WriteLine($"Merging {item}");
            await MergeAsync(item, cancellationToken);
        }
        return "";
    }

    private async Task MergeAsync(string gtfsFolder, CancellationToken cancellationToken) { }

    public ValueTask DisposeAsync()
    {
        foreach (var item in readers)
        {
            item.Dispose();
        }
        return ValueTask.CompletedTask;
    }
}
