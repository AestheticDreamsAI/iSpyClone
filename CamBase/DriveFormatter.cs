using System;
using System.Management;

public class DriveFormatter
{
    public static void FormatDrive(string driveLetter, string fileSystem = "NTFS", bool quickFormat = true, string label = "")
    {
        try
        {
            string query = $"SELECT * FROM Win32_Volume WHERE DriveLetter = '{driveLetter}:'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject volume in searcher.Get())
                {
                    ManagementBaseObject inParams = volume.GetMethodParameters("Format");
                    inParams["FileSystem"] = fileSystem;
                    inParams["QuickFormat"] = quickFormat;
                    inParams["Label"] = label;
                    inParams["EnableCompression"] = false;

                    ManagementBaseObject outParams = volume.InvokeMethod("Format", inParams, null);

                    uint returnValue = (uint)outParams["ReturnValue"];
                    if (returnValue == 0)
                    {
                        Console.WriteLine($"Drive {driveLetter} formatted successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to format the drive. Error Code: {returnValue}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while formatting the drive: {ex.Message}");
        }
    }
}
