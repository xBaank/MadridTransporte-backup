using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;

using ConsoleAppFramework;

using Merger_exporter;

List<(string name, string url)> files =
[
    (
        "google_transit_M9",
        "https://www.arcgis.com/sharing/rest/content/items/357e63c2904f43aeb5d8a267a64346d8/data"
    ),
    (
        "google_transit_M89",
        "https://www.arcgis.com/sharing/rest/content/items/885399f83408473c8d815e40c5e702b7/data"
    ),
    (
        "google_transit_M4",
        "https://www.arcgis.com/sharing/rest/content/items/5c7f2951962540d69ffe8f640d94c246/data"
    ),
    (
        "google_transit_M6",
        "https://www.arcgis.com/sharing/rest/content/items/868df0e58fca47e79b942902dffd7da0/data"
    ),
    (
        "google_transit_M10",
        "https://www.arcgis.com/sharing/rest/content/items/aaed26cc0ff64b0c947ac0bc3e033196/data"
    ),
    (
        "google_transit_M5",
        "https://www.arcgis.com/sharing/rest/content/items/1a25440bf66f499bae2657ec7fb40144/data"
    ),
];

await ConsoleApp.RunAsync(
    args,
    async (CancellationToken token, string destinationFolder) =>
    {
        var sw = new Stopwatch();
        sw.Start();
        using var httpClient = new HttpClient();

        async IAsyncEnumerable<GtfsFile> GetGtfsFilesAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            foreach (var (name, url) in files)
            {
                var gtfsFile = new GtfsFile(
                    name,
                    await httpClient.GetStreamAsync(url, cancellationToken)
                );
                yield return gtfsFile;
            }
        }

        var gtfsFiles = await GetGtfsFilesAsync(token).ToListAsync(cancellationToken: token);

        try
        {
            List<Task> tasks = [];

            foreach (var item in gtfsFiles)
            {
                var task = Task.Run(
                    async () =>
                    {
                        var files = await item.GetFoldersFilesAsync(token)
                            .ToListAsync(cancellationToken: token);
                        var grouped = files.GroupBy(i => Path.GetFileName(i)).ToList();

                        var folder = Directory.CreateDirectory(
                            Path.Combine(destinationFolder, item.Name)
                        );

                        using var merger = await Merger.CreateMergerAsync(
                            grouped,
                            folder.FullName,
                            token
                        );
                        await merger.MergeAsync(token);

                        Console.WriteLine($"Compressing {folder.FullName}");

                        ZipFile.CreateFromDirectory(
                            folder.FullName,
                            folder.FullName + ".zip",
                            CompressionLevel.Optimal,
                            includeBaseDirectory: false
                        );
                    },
                    token
                );
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            sw.Stop();
            Console.WriteLine($"Time: {sw.Elapsed}");
        }
        finally
        {
            foreach (var item in gtfsFiles)
            {
                await item.DisposeAsync();
            }
        }
    }
);
