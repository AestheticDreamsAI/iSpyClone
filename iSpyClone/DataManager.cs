public class DataManager
{
    private readonly string _primaryDirectoryPath;
    private readonly string _fallbackDirectoryPath = @".\media";
    private readonly long _maxSizeInBytes;
    private readonly int _checkIntervalInMinutes;
    private readonly SemaphoreSlim _syncSemaphore = new SemaphoreSlim(1, 1); // Maximal 1 gleichzeitiger Zugriff

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
        if (_primaryDirectoryPath.Length >= 2 && _primaryDirectoryPath[1] == ':')
        {
            string driveLetter = _primaryDirectoryPath.Substring(0, 2);
            return DriveInfo.GetDrives().Any(drive => drive.Name.Equals(driveLetter + "\\", StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    // Synchronisierungsmethode, wenn das Primärverzeichnis wieder verfügbar ist
    public async Task SyncFilesWhenPrimaryAvailableAsync()
    {
        // Versuche, das Semaphore zu betreten
        if (await _syncSemaphore.WaitAsync(0)) // 0 bedeutet sofortige Rückgabe ohne Wartezeit
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                if (_primaryDirectoryPath != _fallbackDirectoryPath && Directory.GetDirectories(_fallbackDirectoryPath).Length > 0)
                {
                    if (IsPrimaryDirectoryAvailable())
                    {
                        Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Primary directory available again. Synchronizing files...");

                        // Dateien kopieren
                        foreach (string file in Directory.GetFiles(_fallbackDirectoryPath))
                        {
                            string destFile = Path.Combine(_primaryDirectoryPath, Path.GetFileName(file));

                            try
                            {
                                // Falls die Datei bereits existiert, wird sie überschrieben
                                if (File.Exists(destFile))
                                {
                                    File.Delete(destFile);
                                }

                                File.Copy(file, destFile);
                                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Copied file {file} to {destFile}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Error copying file {file}: {ex.Message}");
                            }
                        }

                        // Verzeichnisse kopieren
                        foreach (string dir in Directory.GetDirectories(_fallbackDirectoryPath))
                        {
                            string destDir = Path.Combine(_primaryDirectoryPath, Path.GetFileName(dir));

                            try
                            {
                                // Falls das Verzeichnis bereits existiert, wird es gelöscht
                                if (Directory.Exists(destDir))
                                {
                                    Directory.Delete(destDir, true); // true, um rekursiv zu löschen
                                }

                                DirectoryCopy(dir, destDir, true);
                                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Copied directory {dir} to {destDir}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Error copying directory {dir}: {ex.Message}");
                            }
                        }

                        Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Files and directories successfully synchronized.");

                        // Fallback-Verzeichnis leeren
                        await ClearFallbackDirectoryAsync();
                    }
                    else
                    {
                        Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Primary directory not available yet. Continuing to wait...");
                    }
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            finally
            {
                // Gib das Semaphore frei
                _syncSemaphore.Release();
            }
        }
        else
        {
            // Wenn bereits eine Synchronisation läuft, gib eine Meldung aus
            Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Sync already in progress. Skipping this run.");
        }
    }

    // Methode zum rekursiven Kopieren von Verzeichnissen
    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
        DirectoryInfo[] dirs = dir.GetDirectories();

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
        }

        Directory.CreateDirectory(destDirName);

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, true);
        }

        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }

    // Methode zum Leeren des Fallback-Verzeichnisses
    private async Task ClearFallbackDirectoryAsync()
    {
        Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Clearing fallback directory...");
        var tasks = new List<Task>();

        foreach (string file in Directory.GetFiles(_fallbackDirectoryPath))
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    File.Delete(file);
                    Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Deleted file {file}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Error deleting file {file}: {ex.Message}");
                }
            }));
        }

        foreach (string dir in Directory.GetDirectories(_fallbackDirectoryPath))
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    Directory.Delete(dir, true); // true, um Unterverzeichnisse rekursiv zu löschen
                    Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Deleted directory {dir}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Error deleting directory {dir}: {ex.Message}");
                }
            }));
        }

        await Task.WhenAll(tasks); // Warten, bis alle Löschvorgänge abgeschlossen sind
        Console.WriteLine($"- {DateTime.Now.ToLongTimeString()} Fallback directory cleared.");
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
