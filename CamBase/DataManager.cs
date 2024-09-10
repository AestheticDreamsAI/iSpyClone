public class DataManager
{
    public string _directoryPath;
    public long MaxSizeInBytes = 1L * 50 * 1024 * 1024 * 1024; // 2 GB in Bytes
    private int _checkIntervalInMinutes;

    public string getDirectory()
    {
        return _directoryPath;
    }
    public DataManager(string directoryPath, int checkIntervalInMinutes = 5)
    {
        _directoryPath = directoryPath;
        _checkIntervalInMinutes = checkIntervalInMinutes;
        Directory.CreateDirectory(directoryPath);
    }

    public long getMaxSize()
    {
        return MaxSizeInBytes; 
    }

    public async Task StartMonitoring(CancellationToken cancellationToken)
    {
                Console.WriteLine("- File Manager started...");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                CheckAndClearIfExceedsLimit();

                // Warte die angegebene Zeit (in Minuten) vor der nächsten Überprüfung
                await Task.Delay(TimeSpan.FromMinutes(_checkIntervalInMinutes), cancellationToken);
            }
            catch { }
        }
        Console.WriteLine("- File Manager stopped...");
    }

    private void CheckAndClearIfExceedsLimit()
    {
        try
        {
            long totalSize = CalculateDirectorySize(_directoryPath);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Snapshots-Size: {totalSize / (1024 * 1024)} MB");

            if (totalSize > MaxSizeInBytes)
            {
                ClearDirectory(_directoryPath);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()}: Snapshots deleted.");
                totalSize = CalculateDirectorySize(_directoryPath);
                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Snapshots-Size: {totalSize / (1024 * 1024)} MB");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
        }
    }

    public long CalculateDirectorySize(string directoryPath)
    {
        long totalSize = 0;

        // Rekursives Durchlaufen aller Dateien
        foreach (string file in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
            }
            catch { }
        }

        return totalSize;
    }

    private void ClearDirectory(string directoryPath)
    {
        // Alle Dateien und Unterverzeichnisse löschen
        foreach (string file in Directory.GetFiles(directoryPath))
        {
            try
            {
                File.Delete(file);
            }
            catch { }
        }

        foreach (string dir in Directory.GetDirectories(directoryPath))
        {
            try
            {
                Directory.Delete(dir, true); // true, um Unterverzeichnisse rekursiv zu löschen
            }
            catch { }
        }
    }
}