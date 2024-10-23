using AForge.Vision.Motion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class Camera
{
    public string CameraName { get; set; }
    public int CameraIndex { get; set; }
    public string CameraUrl { get; set; }
    public string CameraUser { get; set; }
    public string CameraPass { get; set; }
    public bool IsRecording { get; set; } // Neue Eigenschaft hinzugefügt
    public int altDir { get; set; } // Neue Eigenschaft hinzugefügt
    public MotionDetector MotionDetector { get; set; }

    public Camera Clone()
    {
        return new Camera
        {
            CameraName = this.CameraName,
            CameraIndex = this.CameraIndex,
            CameraUrl = this.CameraUrl,
            CameraUser = this.CameraUser,
            CameraPass = this.CameraPass,
            IsRecording = false,
            MotionDetector = null, // Clone MotionDetector if not null
            altDir = 0
        };
    }

    public string getDir(string type)
    {
        string baseDirectory = Program.manager.getDirectory();

        // Build directory paths based on type (images or video)
        string originalDir = $@"{baseDirectory}\{type}\{this.CameraIndex}";
        string altDirPath = $@"{baseDirectory}\{type}\{this.CameraIndex}.{this.altDir}";

        // Check if altDir is set and the alternative directory exists
        if (altDir > 0 && Directory.Exists(altDirPath))
        {
            return altDirPath;
        }
        else
        {
            // Return the original directory if no valid altDir is found
            return originalDir;
        }
    }
    public byte[] GetRecording(string path)
    {
        // Replace the path as per the original logic
        var f = Path.GetFileNameWithoutExtension(path);

        string filePath = $"{this.getDir("video")}\\{f}\\animated.gif";

        // Check if the file is corrupt before proceeding
        if (DataChecker.IsFileCorrupt(filePath))
        {
            return null; // Return null or handle accordingly when file is corrupt
        }

        // Load image and convert it to byte array in a using block to dispose the resource properly
        if (File.Exists(filePath))
        {
            using (Image image = Image.FromFile(filePath))
            {
                return this.ImageToByteArray(image);
            }
        }
        return null;
    }

    public string GetRecordingFrames(string path)
    {
        // Replace the path as per the original logic
        var f = Path.GetFileNameWithoutExtension(path);
        string filePath = $"{getDir("video")}\\{f}\\animated.gif";

        // Check if the file is corrupt before proceeding
        if (DataChecker.IsFileCorrupt(filePath))
        {
            return null; // Return null or handle accordingly when file is corrupt
        }

            return filePath;
    }


    public long CalculateTotalFrameSize()
    {
        long totalSize = 0;

        // Hole das Verzeichnis für die Frame-Dateien der aktuellen Kamera
        string directoryPath = getDir("images");

        // Prüfe, ob das Verzeichnis existiert
        if (Directory.Exists(directoryPath))
        {
            // Gehe durch alle Unterverzeichnisse
            foreach (var dir in Directory.GetDirectories(directoryPath))
            {
                try
                {
                    // Hole den Pfad zu den Frames (z.B. als PNG oder andere Bildformate)
                    var frames = Directory.GetFiles(dir, "*.jpg"); // oder entsprechendes Format wie "*.bmp" etc.

                    // Gehe durch jede Frame-Datei
                    foreach (var frame in frames)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(frame);

                            // Prüfe, ob die Datei existiert und ob sie nicht defekt ist
                            if (fileInfo.Exists && !DataChecker.IsFileCorrupt(frame))
                            {
                                totalSize += fileInfo.Length; // Größe der Datei in Bytes
                            }
                        }
                        catch (Exception ex)
                        {
                            // Fehlerbehandlung falls nötig
                            Console.WriteLine($"Fehler beim Überprüfen der Datei {frame}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Fehlerbehandlung falls nötig
                    Console.WriteLine($"Fehler beim Zugriff auf Frames in {dir}: {ex.Message}");
                }
            }
        }

        return totalSize;
    }




    public List<Recording> GetRecordings()
    {
        var recordings = new List<Recording>();
        foreach (var f in Directory.GetDirectories(getDir("video"), "*.*"))
        {
            try
            {
                var animated = $@"{f}\\animated.gif";
                if (!DataChecker.IsFileCorrupt(animated) && File.Exists(animated))
                {
                    var recording = new Recording();
                    var p = Path.GetFileName(f).Replace("_", " ").Replace("-", ".").Split(' ');

                    recording.CameraIndex = this.CameraIndex;
                    recording.Date = p[0];
                    recording.Time = p[1].Replace(".", ":");
                    recording.Path = $"{this.CameraIndex}/{Path.GetFileName(f)}";
                    recording.Size = new FileInfo(animated).Length; // Size in bytes

                    recordings.Add(recording);
                }
            }
            catch (Exception ex)
            {
                // Handle exception if needed
            }
        }
        recordings.Reverse();
        return recordings;
    }

    public async Task<(Image,bool)> getSnapshot()
    {
        var cameraUrl = CameraUrl
            .Replace("[username]", CameraUser)
            .Replace("[password]", CameraPass);

        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5); // Timeout configuration

                using (Stream stream = await client.GetStreamAsync(cameraUrl))
                {
                    return (Image.FromStream(stream),true);
                }
            }
        }
        catch (TaskCanceledException ex)
        {
        }
        catch (Exception ex)
        {
        }

        // Fallback to GIF
        try
        {
            using (var img = Image.FromFile(".\\nosignal.gif"))
            {
                return (img, false); // This will load the entire GIF including animation
            }
        }
        catch (Exception ex)
        {
            return (null,false); // Return null if even the fallback fails
        }
    }

    public byte[] ImageToByteArray(Image image)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            // Save the image to the MemoryStream in the original format
            image.Save(ms, image.RawFormat);
            return ms.ToArray();
        }
    }
}
