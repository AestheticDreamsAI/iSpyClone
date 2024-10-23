using System;
using System.IO;
using System.Security;

public class DataChecker
{
    public static bool IsDirectoryAccessible(string path)
    {
        try
        {
            // Überprüfen, ob das Verzeichnis existiert
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Verzeichnis existiert nicht.");
                return false;
            }

            // Überprüfen, ob Zugriff auf das Verzeichnis möglich ist
            var files = Directory.GetFiles(path);
            return true; // Wenn wir die Dateien auflisten konnten, ist der Zugriff möglich
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Keine Berechtigung, auf das Verzeichnis zuzugreifen.");
            return false;
        }
        catch (PathTooLongException)
        {
            Console.WriteLine("Der Pfad ist zu lang.");
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("Verzeichnis wurde nicht gefunden.");
            return false;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Fehler beim Zugriff auf das Verzeichnis: {ex.Message}");
            return false;
        }
        catch (SecurityException)
        {
            Console.WriteLine("Sicherheitsfehler beim Zugriff auf das Verzeichnis.");
            return false;
        }
    }
    public static bool IsDriveAvailable(string driveLetter)
    {
        // Füge das ":\" hinzu, um das vollständige Laufwerk zu erstellen, z.B. "D:\"
        string drivePath = driveLetter + @":\";

        // Hol dir Informationen über das Laufwerk
        DriveInfo drive = new DriveInfo(drivePath);

        // Prüfen, ob das Laufwerk bereit ist
        return drive.IsReady;
    }
    public static bool IsFileCorrupt(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false; // File does not exist, considered corrupt
        }

        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[512]; // Read first 512 bytes
                int bytesRead = fs.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine(filePath + " is empty.");
                    return true; // File is empty, considered corrupt
                }
            }

            return false; // No exceptions, file is not corrupt
        }
        catch (Exception ex)
        {
            // Log or handle the error if necessary
            Console.WriteLine(filePath + " is corrupt: " + ex.Message);
            return true; // File is corrupt or can't be opened
        }
    }
}
