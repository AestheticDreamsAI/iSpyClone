public class DataManager
{
    private readonly string _directoryPath;
    private readonly long _maxSizeInBytes;
    private readonly int _checkIntervalInMinutes;

    public DataManager(string directoryPath, int checkIntervalInMinutes = 5, long maxSizeInBytes = 2) // 50 GB in Bytes
    {
        maxSizeInBytes = maxSizeInBytes * 1024 * 1024 * 1024;
        _directoryPath = directoryPath;
        _checkIntervalInMinutes = checkIntervalInMinutes;
        _maxSizeInBytes = maxSizeInBytes;
        Directory.CreateDirectory(directoryPath);
    }

    public string getDirectory()
    {
        return _directoryPath;
    }

    public long getMaxSize()
    {
        return MaxSizeInBytes;
    }


    public string DirectoryPath => _directoryPath;
    public long MaxSizeInBytes => _maxSizeInBytes;

    public async Task StartMonitoring(CancellationToken cancellationToken)
    {
        Console.WriteLine("- File Manager started...");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndClearIfExceedsLimitAsync();

                // Warte die angegebene Zeit (in Minuten) vor der nächsten Überprüfung
                await Task.Delay(TimeSpan.FromMinutes(_checkIntervalInMinutes), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Task wurde abgebrochen
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Monitoring: {ex.Message}");
            }
        }
        Console.WriteLine("- File Manager stopped...");
    }

    private async Task CheckAndClearIfExceedsLimitAsync()
    {
        try
        {
            long totalSize = CalculateDirectorySize(Cameras.All());
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Snapshots-Size: {totalSize / (1024 * 1024)} MB");

            if (totalSize > _maxSizeInBytes)
            {
                await ClearDirectoryAsync(_directoryPath);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()}: Snapshots deleted.");

                totalSize = CalculateDirectorySize(Cameras.All());
                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Snapshots-Size: {totalSize / (1024 * 1024)} MB");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Prüfen der Größe: {ex.Message}");
        }
    }

    public long CalculateDirectorySize(List<Camera> cameras)
    {
        long totalSize = 0;
        try
        {
            foreach (Camera camera in cameras)
            {
                //totalSize += camera.CalculateTotalFrameSize();
                foreach (Recording rec in camera.GetRecordings())
                {
                    totalSize += rec.Size; // Annahme: Die Aufnahme hat eine `Size`-Eigenschaft
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return totalSize;
    }

    private async Task ClearDirectoryAsync(string directoryPath)
    {
        // Alle Dateien und Unterverzeichnisse asynchron löschen
        var tasks = new List<Task>();

        foreach (string file in Directory.GetFiles(directoryPath))
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Löschen von {file}: {ex.Message}");
                }
            }));
        }

        foreach (string dir in Directory.GetDirectories(directoryPath))
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    Directory.Delete(dir, true); // true, um Unterverzeichnisse rekursiv zu löschen
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Löschen von Verzeichnis {dir}: {ex.Message}");
                }
            }));
        }

        await Task.WhenAll(tasks); // Warten, bis alle Löschvorgänge abgeschlossen sind
    }
}
