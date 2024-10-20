public class RecordingMonitor
{
    private static Dictionary<string, DateTime> lastRecordingTimes = new Dictionary<string, DateTime>();
    private static DateTime noRecordingsSinceStart;
    public static int CheckIntervalInSeconds = 60;
    public static int MaxIdleTimeInSeconds = 300; // 5 Minuten Leerlaufzeit
    public static int MaxInactiveCameras = 2; // Anzahl der inaktiven Kameras, die das Programm beenden soll
    public static int MaxNoRecordingTimeInMinutes = 120; // 30 Minuten ohne Aufnahme

    public static void UpdateLastRecordingTime(Camera cam)
    {
        if (lastRecordingTimes.ContainsKey(cam.CameraName))
        {
            lastRecordingTimes[cam.CameraName] = DateTime.Now;
        }
        else
        {
            lastRecordingTimes.Add(cam.CameraName, DateTime.Now);
        }
    }

    public static bool CheckIdleCameras()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        int inactiveCamerasCount = 0;

        if (lastRecordingTimes.Count == 0)
        {
            if (noRecordingsSinceStart != null && (DateTime.Now - noRecordingsSinceStart).TotalMinutes >= 10)
            {
                Console.WriteLine($"- {DateTime.Now.ToLongTimeString()}: All Cameras - No recording for 10 minutes.");
                return true;
            }
        }

        foreach (var cam in Cameras.All())
        {
            if (lastRecordingTimes.ContainsKey(cam.CameraName))
            {
                var lastRecordingTime = lastRecordingTimes[cam.CameraName];
                var timeSinceLastRecording = DateTime.Now - lastRecordingTime;

                if (timeSinceLastRecording.TotalMinutes >= MaxNoRecordingTimeInMinutes)
                {
                    Console.WriteLine($"- {DateTime.Now.ToLongTimeString()}: {cam.CameraName} - No recording for {Math.Round(timeSinceLastRecording.TotalMinutes)} minutes. Exceeded 30 minutes.");
                    return true; // Kamera war länger als 30 Minuten inaktiv
                }

                if (timeSinceLastRecording.TotalSeconds > MaxIdleTimeInSeconds)
                {
                    inactiveCamerasCount++;
                    Console.WriteLine($"- {DateTime.Now.ToLongTimeString()}: {cam.CameraName} - No recording for {Math.Round(timeSinceLastRecording.TotalMinutes)} minutes.");
                }
            }

            if (inactiveCamerasCount >= MaxInactiveCameras)
            {
                return true;
            }
        }

        Console.ForegroundColor = ConsoleColor.White;
        return false;
    }

    public static async Task StartMonitoring(CancellationTokenSource cts, int MaxCamFails = 2)
    {
        MaxInactiveCameras = MaxCamFails;
        Console.WriteLine("- Recording Monitor started...");
        noRecordingsSinceStart = DateTime.Now;

        while (!cts.Token.IsCancellationRequested)
        {
            if (CheckIdleCameras())
            {
                if (!Cameras.RecordingState())
                {
                    cts.Cancel();
                    break;
                }
            }

            await Task.Delay(CheckIntervalInSeconds * 1000,cts.Token);
        }
        Console.WriteLine("- Recording Monitor stopped...");
    }
}
