using AForge.Vision.Motion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick; // Add reference to Magick.NET

public class Motion
{
    public static int Interval = 1;
    public static int recording_time = 60;
    public static int timeout = recording_time+30;
    public static async Task isDetected(Camera camera)
    {
        if (camera.MotionDetector == null)
            camera.MotionDetector = new MotionDetector(new TwoFramesDifferenceDetector());

        var snap = await camera.getSnapshot(); // Capture image from the camera
        var img = snap.Item1;

        if (snap.Item2)
        {
            var resizedImage = ImageProcessing.ResizeImage(new Bitmap(img), 240, 140);
            var motion = camera.MotionDetector.ProcessFrame(resizedImage);

            if ((double)motion > 0.002) // Adjust this threshold if needed
            {
                // Start recording for this camera if motion is detected
                await StartRecordingSafely(camera);
            }
        }
    }

    public static async Task StartDetection(CancellationToken cts, int inter)
    {
        Interval = inter;
        Console.WriteLine("- Motion Detection started...");

        while (!cts.IsCancellationRequested)
        {
            List<Task> detectionTasks = new List<Task>();

            // Starte für jede Kamera die Bewegungserkennung
            foreach (Camera cam in Cameras.All())
            {
                detectionTasks.Add(isDetected(cam));
            }

            // Warte, bis alle Erkennungstasks abgeschlossen sind
            await Task.WhenAll(detectionTasks);

            // Kurze Pause, bevor erneut geprüft wird
            await Task.Delay(Interval * 1000); // Verwende den festgelegten Intervall
        }
        Console.WriteLine("- Motion Detection stopped...");
    }

    private static async Task StartRecordingSafely(Camera cam)
    {
        // Prüfe, ob die Kamera schon aufnimmt, und starte nur, wenn sie es nicht tut
        if (Program.manager.getDirectory().Contains(":"))
        {
            if (!DataChecker.IsDriveAvailable(Program.manager.getDirectory().Split(':')[0].ToString()))
            {
                return;
            }
        }

        if (!cam.IsRecording)
        {
            // Starte die Aufnahme für diese Kamera
            await record(cam);
        }
    }

    private static async Task record(Camera cam)
    {
        if (cam.IsRecording) return; // Verhindere gleichzeitige Aufnahmen für dieselbe Kamera
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"- {DateTime.Now.ToLongTimeString()}: {cam.CameraName} - Motion Detected");
        CameraStatistics.MotionDetected(cam);
        RecordingMonitor.UpdateLastRecordingTime(cam);
        cam.IsRecording = true;

        try
        {
            // Set a timeout to stop recording after a certain period (e.g. 60 seconds)
            var recordingTask = ImageProcessing.SaveSnapshots(cam, recording_time); // Nehme 30 Bilder auf
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeout)); // Maximal 60 Sekunden Aufnahme

            // Warte, bis die Aufnahme abgeschlossen oder der Timeout erreicht ist
            CameraStatistics.RecordStart(cam);
            var completedTask = await Task.WhenAny(recordingTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                //Console.WriteLine($"{cam.CameraName} - Recording timed out after 60 seconds.");
            }
        }
        catch (Exception ex)
        {
            cam.IsRecording = false;
            //Console.WriteLine($"{cam.CameraName} - Error during recording: {ex.Message}");
        }
        finally
        {
            // Stelle sicher, dass der Aufnahmeprozess beendet wird
            cam.IsRecording = false;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"- {DateTime.Now.ToLongTimeString()}: {cam.CameraName} - recording succeded");
            Console.ForegroundColor= ConsoleColor.White;
            CameraStatistics.RecordEnd(cam);
        }
    }
}
