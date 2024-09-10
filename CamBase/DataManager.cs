public class DataManager
{
    private string _directoryPath;
    private const long MaxSizeInBytes = 2L * 1024 * 1024 * 1024; // 2 GB in Bytes
    private int _checkIntervalInMinutes;

    public DataManager(string directoryPath, int checkIntervalInMinutes = 5)
    {
        _directoryPath = directoryPath;
        _checkIntervalInMinutes = checkIntervalInMinutes;
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

    private long CalculateDirectorySize(string directoryPath)
    {
        long totalSize = 0;

        // Rekursives Durchlaufen aller Dateien
        foreach (string file in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
        {
            FileInfo fileInfo = new FileInfo(file);
            totalSize += fileInfo.Length;
        }

        return totalSize;
    }

    private void ClearDirectory(string directoryPath)
    {
        // Alle Dateien und Unterverzeichnisse löschen
        foreach (string file in Directory.GetFiles(directoryPath))
        {
            File.Delete(file);
        }

        foreach (string dir in Directory.GetDirectories(directoryPath))
        {
            Directory.Delete(dir, true); // true, um Unterverzeichnisse rekursiv zu löschen
        }
    }
}