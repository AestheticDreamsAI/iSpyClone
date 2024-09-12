public class DataManager
{
    private readonly string _primaryDirectoryPath;
    private readonly string _fallbackDirectoryPath = @".\media";
    private readonly long _maxSizeInBytes;
    private readonly int _checkIntervalInMinutes;

    public DataManager(string directoryPath, int checkIntervalInMinutes = 5, long maxSizeInBytes = 2) // 50 GB in Bytes
    {
        maxSizeInBytes = maxSizeInBytes * 1024 * 1024 * 1024;
        _primaryDirectoryPath = directoryPath;
        _checkIntervalInMinutes = checkIntervalInMinutes;
        _maxSizeInBytes = maxSizeInBytes;

        // Verzeichnis erstellen
        EnsureDirectoryExists(getDirectory());
    }

    public long getMaxSize()
    {
        return _maxSizeInBytes;
    }



    // Methode, um sicherzustellen, dass das Verzeichnis existiert
    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    // Methode zum Abrufen des Verzeichnisses (entweder Primär oder Fallback)
    public string getDirectory()
    {
        // Überprüfen, ob das Primärverzeichnis auf einem bestimmten Laufwerk (z.B. F:) liegt
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        if (_primaryDirectoryPath == _fallbackDirectoryPath)
            return _primaryDirectoryPath;
        else
        {
            if (IsPrimaryDirectoryAvailable())
            {
                return _primaryDirectoryPath;
            }
            else
            {
                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Primary directory not available. Using fallback directory.");
                Console.ForegroundColor = ConsoleColor.White;
                return _fallbackDirectoryPath;
            }
        }

    }

    // Methode zur Überprüfung, ob das Primärverzeichnis auf einem Laufwerk liegt und ob es verfügbar ist
    private bool IsPrimaryDirectoryAvailable()
    {
        // Prüfen, ob der Pfad mit einem Laufwerksbuchstaben endet (z.B. "F:\" oder "C:\")
        if (_primaryDirectoryPath.Length >= 2 && _primaryDirectoryPath[1] == ':')
        {
            string driveLetter = _primaryDirectoryPath.Substring(0, 2); // z.B. "F:"

            // Prüfen, ob das Laufwerk verfügbar ist
            return DriveInfo.GetDrives().Any(drive => drive.Name.Equals(driveLetter + "\\", StringComparison.OrdinalIgnoreCase));
        }

        return false; // Kein gültiger Laufwerkspfad
    }

    // Synchronisierungsmethode, wenn das Primärverzeichnis wieder verfügbar ist
    public async Task SyncFilesWhenPrimaryAvailableAsync()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        if (_primaryDirectoryPath != _fallbackDirectoryPath && Directory.GetDirectories(_fallbackDirectoryPath).Length>0)
        {
            if (IsPrimaryDirectoryAvailable())
            {
                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Primary directory available again. Synchronizing files...");

                // Dateien vom Fallback-Verzeichnis in das Primärverzeichnis verschieben
                foreach (string file in Directory.GetFiles(_fallbackDirectoryPath))
                {
                    string destFile = Path.Combine(_primaryDirectoryPath, Path.GetFileName(file));
                    File.Move(file, destFile);
                }

                // Verzeichnisse vom Fallback-Verzeichnis in das Primärverzeichnis verschieben
                foreach (string dir in Directory.GetDirectories(_fallbackDirectoryPath))
                {
                    string destDir = Path.Combine(_primaryDirectoryPath, Path.GetFileName(dir));
                    Directory.Move(dir, destDir);
                }

                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Files successfully synchronized.");
            }
            else
            {
                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Primary directory not available yet. Continuing to wait...");
            }
        }
        Console.ForegroundColor = ConsoleColor.White;
    }

    public async Task StartMonitoring(CancellationToken cancellationToken)
    {
        Console.WriteLine("- File Manager started...");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Prüfen, ob Primärverzeichnis wieder verfügbar ist und synchronisieren
                await SyncFilesWhenPrimaryAvailableAsync();

                // Überprüfe, ob die Größe des Verzeichnisses das Limit überschreitet und lösche bei Bedarf
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
                //Console.WriteLine($"Fehler beim Monitoring: {ex.Message}");
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
                await ClearDirectoryAsync(getDirectory());
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()}: Snapshots deleted.");

                totalSize = CalculateDirectorySize(Cameras.All());
                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Snapshots-Size: {totalSize / (1024 * 1024)} MB");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Fehler beim Prüfen der Größe: {ex.Message}");
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
        if (!getDirectory().Contains(".\\"))
        {
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
                        //Console.WriteLine($"Fehler beim Löschen von {file}: {ex.Message}");
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
                        //Console.WriteLine($"Fehler beim Löschen von Verzeichnis {dir}: {ex.Message}");
                    }
                }));
            }

            await Task.WhenAll(tasks); // Warten, bis alle Löschvorgänge abgeschlossen sind
        }
    }
}
