using System;
using System.IO;

public class DataChecker
{

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
